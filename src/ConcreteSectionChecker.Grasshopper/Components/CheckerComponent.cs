using System;
using ConcreteSectionChecker.Adapters;
using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Geometry;
using GPC.Model.Sections.Concrete;
using GPC.Model.Standards;
using Grasshopper.Kernel;

namespace ConcreteSectionChecker.Grasshopper.Components
{
    public sealed class CheckerOptionsComponent : ReBearComponent
    {
        public CheckerOptionsComponent() : base("Concrete Checker Options", "CCO",
            "Opzioni del checker GPC. Default: dominio plastico, strategia Iterative, analisi lineare, 64 direzioni del dominio.") { }
        public override Guid ComponentGuid => new("b7e2c3a0-0300-4e1b-9d1a-000000000003");
        protected override Port[] Inputs => new[] { G("Coordinate System", "CS", "Riferimento delle azioni del checker."),
            I("Failure Domain Types", "FDT", "0 elastico, 1 plastico.", 1, Named("Elastic", "Plastic")),
            I("Failure Analysis Types", "FAT", "Criterio di ricerca del punto resistente.", 1,
                Named("Constant N", "Constant eccentricity", "Constant Mx - My", "Constant N and Mx", "Constant N and My")),
            I("Domain Point Strategy Types", "DPST", "0 Iterative (default, 2–4 ms per checker); 1 Intersection (dominio discretizzato precalcolato, 0.4–0.6 s per checker).", 0,
                Named("Iterative", "Intersection")),
            I("Stress Analysis Types", "SAT", "0 non lineare, 1 lineare.", 1, Named("Non Linear", "Linear")),
            N("ψ rebar", "ψ r", "Coefficiente ψ delle barre per l'analisi lineare.", 1.5), N("ψ tendon", "ψ t", "Coefficiente ψ dei trefoli.", 0),
            B("Consider tensile concrete", "TC", "Considera il calcestruzzo teso."),
            I("Theta discretization", "θn", "Direzioni del dominio 3D (8–360); usata dalla strategia Intersection e dal dominio 3D.", 64) };
        protected override Port[] Outputs => new[] { G("Checker Options", "CO", "Opzioni GPC.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var axes = Required<CoordinateSystem>(d, 0);
            int theta = Required<int>(d, 8);
            if (theta < 8 || theta > 360) throw new ArgumentException($"{Label(8)}: valore fra 8 e 360 richiesto.");
            double psiRebar = Finite(d, 5, nonNegative: true), psiTendon = Finite(d, 6, nonNegative: true);
            d.SetData(0, new GH_CheckerOptions(new SectionCheckerModelCode2010.SectionOptionsModelCode2010(new CoordinateSystem(axes),
                (SectionSolver.FailureAnalysisTypes)Choice(d, 2, 5), (SectionSolver.FailureDomainTypes)Choice(d, 1, 2),
                (SectionSolver.StressAnalysisTypes)Choice(d, 4, 2), psiRebar, psiTendon, Read(d, 7, false), theta)
            { DomainPointStrategy = (SectionSolver.DomainPointStrategyTypes)Choice(d, 3, 2) }));
        }
    }

    public sealed class CheckerComponent : ReBearComponent
    {
        public CheckerComponent() : base("Concrete Checker", "CC", "Checker GPC di una sezione con norma e opzioni (copia privata delle opzioni).") { }
        public override Guid ComponentGuid => new("c485957b-3d8e-4f19-8176-f30ffcda952e");
        protected override Port[] Inputs => new[] { G("Reinforced Concrete Section", "RCS", "Sezione GPC."), G("Standard", "S", "Norma GPC."),
            G("Options", "O", "Da Concrete Checker Options.") };
        protected override Port[] Outputs => new[] { G("Concrete Checker", "CC", "Checker GPC.") };
        protected override void SolveGpc(IGH_DataAccess d)
        {
            var options = Required<SectionCheckerModelCode2010.SectionOptionsModelCode2010>(d, 2);
            // The GPC solver keeps a reference to its options: a private copy isolates the checker from later changes.
            d.SetData(0, new GH_SectionChecker(new SectionCheckerModelCode2010(new SectionCheckerAttribute(Required<ReinforcedConcreteSection>(d, 0)),
                SectionAnalyses.Copy(options), Required<StandardModelCode2010>(d, 1))));
        }
    }
}
