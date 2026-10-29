using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper
{
    /// <summary>
    /// Long calculations off the Rhino UI thread. Inputs are read and validated on the UI thread (<see cref="Prepare"/>); the
    /// returned work runs as a task per iteration with a cancellation token, then the component expires itself and writes the
    /// outputs. A new solution (changed inputs), removal from the document or the context menu cancel the running work.
    /// The work must use only the data it captured; checkers must be private copies (the legacy checker is not thread-safe).
    /// Without a document (console tests, example generator) or with REBEAR_SYNCHRONOUS=1 the work runs synchronously.
    /// </summary>
    public abstract class ReBearAsyncComponent : ReBearComponent
    {
        protected ReBearAsyncComponent(string name, string nick, string description) : base(name, nick, description) { }

        /// <summary>Read inputs on the UI thread; null when there is nothing to compute (for example Run = false).</summary>
        protected abstract Func<CancellationToken, Action<IGH_DataAccess>> Prepare(IGH_DataAccess d);

        private CancellationTokenSource cancellation = new();
        private Dictionary<int, Task<Action<IGH_DataAccess>>> tasks = new();
        private Dictionary<int, Action<IGH_DataAccess>> results;
        private int generation;
        private bool applying;

        /// <summary>True while a background calculation is running (read by the Rhino acceptance script).</summary>
        public bool IsComputing { get; private set; }

        public static bool Synchronous => Environment.GetEnvironmentVariable("REBEAR_SYNCHRONOUS") == "1";

        protected override void BeforeSolveInstance()
        {
            base.BeforeSolveInstance();
            if (applying) return;
            Cancel(false);
            cancellation = new CancellationTokenSource();
            tasks = new Dictionary<int, Task<Action<IGH_DataAccess>>>();
            generation++;
        }

        protected sealed override void SolveGpc(IGH_DataAccess d)
        {
            var work = Prepare(d); // validation errors appear in both passes
            if (work == null) return;
            if (applying)
            {
                if (results != null && results.TryGetValue(d.Iteration, out var apply)) apply?.Invoke(d);
                return;
            }
            if (Synchronous || OnPingDocument() == null) { work(CancellationToken.None)?.Invoke(d); return; }
            var token = cancellation.Token;
            tasks[d.Iteration] = Task.Run(() => work(token), token);
        }

        protected override void AfterSolveInstance()
        {
            base.AfterSolveInstance();
            if (applying) { applying = false; results = null; IsComputing = false; Message = null; return; }
            if (tasks.Count == 0) { IsComputing = false; Message = null; return; }
            IsComputing = true; Message = "Calcolo in corso…";
            int current = generation; var running = tasks; var token = cancellation.Token;
            Task.WhenAll(running.Values).ContinueWith(_ =>
            {
                var done = running.ToDictionary(p => p.Key, p => Outcome(p.Value));
                Rhino.RhinoApp.InvokeOnUiThread((Action)(() =>
                {
                    if (current != generation || token.IsCancellationRequested) return;
                    results = done; applying = true;
                    ExpireSolution(true);
                }));
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Default);
        }

        private static Action<IGH_DataAccess> Outcome(Task<Action<IGH_DataAccess>> task)
        {
            if (task.IsCanceled) return _ => throw new OperationCanceledException();
            if (task.IsFaulted)
            {
                var error = task.Exception.Flatten().InnerExceptions.First();
                return _ => ExceptionDispatchInfo.Capture(error).Throw();
            }
            return task.Result;
        }

        private void Cancel(bool visible)
        {
            if (!IsComputing && !visible) return;
            cancellation.Cancel();
            if (!IsComputing) return;
            IsComputing = false;
            if (visible) { Message = "Annullato"; OnDisplayExpired(true); }
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "Annulla calcolo", (_, _) => Cancel(true), IsComputing);
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            Cancel(false);
            base.RemovedFromDocument(document);
        }

        public override void DocumentContextChanged(GH_Document document, GH_DocumentContext context)
        {
            if (context is GH_DocumentContext.Close or GH_DocumentContext.Unloaded) Cancel(false);
            base.DocumentContextChanged(document, context);
        }
    }
}
