using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Geometry;
using GPC.Model.Data.Steel;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class CheckerComponent : GpcComponent
    {
        public CheckerComponent()
            : base("Concrete Checker", "CC", "Concrete Checker")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Reinforced Concrete Section", "RCS", "The reinforced concrete section", GH_ParamAccess.item);
            pManager.AddGenericParameter("Standard", "S", "The standard", GH_ParamAccess.item);
            pManager.AddGenericParameter("Options", "O", "The analysis options", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Concrete Checker", "CC", "Concrete Checker", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_ReinforcedConcreteSection gH_ReinforcedConcreteSection = null;
            GH_Standard gH_Standard = null;
            GH_CheckerOptions gH_SolverOptions = null;

            if (DA.GetData(0, ref gH_ReinforcedConcreteSection) && DA.GetData(1, ref gH_Standard) && DA.GetData(2, ref gH_SolverOptions))
            {
                SectionCheckerAttribute sectionCheckerAttribute = new SectionCheckerAttribute(gH_ReinforcedConcreteSection.Value);
                var checker = new SectionCheckerModelCode2010(sectionCheckerAttribute, gH_SolverOptions.Value, gH_Standard.Value);

                DA.SetData(0, new GH_SectionChecker(checker));
            }
        }

        //protected override Bitmap Icon => Resources.material;
        
        public override Guid ComponentGuid => new Guid("c485957b-3d8e-4f19-8176-f30ffcda952e");

    }
}
