using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Geometry;
using GPC.Model.Data.Steel;
using GPC.Model.Materials;
using GPC.Model.Results;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class FailureDomainSummaryComponent : GpcComponent
    {
        public FailureDomainSummaryComponent()
            : base("Secton Resistance Summary", "SRS", "Secton Resistance Summary", Constants.CATEGORY_CHECKS, Constants.SUBCATEGORY_CHECKS_CONCRETECHECKER   )
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Reinforced Concrete Section", "RCS", "The reinforced concrete section", GH_ParamAccess.item);
            pManager.AddGenericParameter("Standard", "S", "The standard", GH_ParamAccess.item);
            pManager.AddNumberParameter("Axial Force [kN]", "N [kN]", "Axial Force [kN]", GH_ParamAccess.item, 0.0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("MxRd+ pl [kNm]", "MxRd+ pl [kNm]", "MxRd+ pl [kNm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("MyRd+ pl [kNm]", "MyRd+ pl [kNm]", "MyRd+ pl [kNm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("MxRd- pl [kNm]", "MxRd- pl [kNm]", "MxRd- pl [kNm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("MyRd- pl [kNm]", "MyRd- pl [kNm]", "MyRd- pl [kNm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("MxRd+ el [kNm]", "MxRd+ el [kNm]", "MxRd+ el [kNm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("MyRd+ el [kNm]", "MyRd+ el [kNm]", "MyRd+ el [kNm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("MxRd- el [kNm]", "MxRd- el [kNm]", "MxRd- el [kNm]", GH_ParamAccess.item);
            pManager.AddNumberParameter("MyRd- el [kNm]", "MyRd- el [kNm]", "MyRd- el [kNm]", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_ReinforcedConcreteSection gH_ReinforcedConcreteSection = null;
            GH_Standard gH_Standard = null;
            double axialForce = 0;

            if (DA.GetData(0, ref gH_ReinforcedConcreteSection) && DA.GetData(1, ref gH_Standard) && DA.GetData(2, ref axialForce))
            {
                int cifreSignificative = 2;
                SectionCheckerAttribute sectionCheckerAttribute = new SectionCheckerAttribute(gH_ReinforcedConcreteSection.Value);
                var coordinateSystem = new GPC.Geometry.CoordinateSystem(gH_ReinforcedConcreteSection.Value.Centroid, new GPC.Geometry.Vector3d(-1, 0, 0), new GPC.Geometry.Vector3d(0, -1, 0));
                SectionCheckerModelCode2010.SectionOptionsModelCode2010 sectionOptionsPlastic = new SectionCheckerModelCode2010.SectionOptionsModelCode2010()
                {
                    FailureDomainType = SectionSolver.FailureDomainTypes.Plastic,
                    FailureAnalysisType = SectionSolver.FailureAnalysisTypes.ConstantN,
                    DomainPointStrategy = SectionSolver.DomainPointStrategyTypes.Intersection,
                    ForceReferenceCoordinateSystem = coordinateSystem,
                };
                SectionCheckerModelCode2010.SectionOptionsModelCode2010 sectionOptionsElastic = new SectionCheckerModelCode2010.SectionOptionsModelCode2010()
                {
                    FailureDomainType = SectionSolver.FailureDomainTypes.Elastic,
                    FailureAnalysisType = SectionSolver.FailureAnalysisTypes.ConstantN,
                    DomainPointStrategy = SectionSolver.DomainPointStrategyTypes.Intersection,
                    ForceReferenceCoordinateSystem = coordinateSystem,
                };
                SectionCheckerModelCode2010 plasticChecker = new SectionCheckerModelCode2010(sectionCheckerAttribute, sectionOptionsPlastic, gH_Standard.Value);
                SectionCheckerModelCode2010 elasticChecker = new SectionCheckerModelCode2010(sectionCheckerAttribute, sectionOptionsElastic, gH_Standard.Value);



                ResultBeamForces f1 = new ResultBeamForces(axialForce * 1000, 0, 0, 0, 100000000, 0, coordinateSystem, 0);
                ResultBeamForces f2 = new ResultBeamForces(axialForce * 1000, 0, 0, 0, 0, 100000000, coordinateSystem, 0);
                ResultBeamForces f3 = new ResultBeamForces(axialForce * 1000, 0, 0, 0, -100000000, 0, coordinateSystem, 0);
                ResultBeamForces f4 = new ResultBeamForces(axialForce * 1000, 0, 0, 0, 0, -100000000, coordinateSystem, 0);

                var rp1 = ConcreteSectionChecker.Adapters.ResistanceChecks.RequirePoint(plasticChecker, f1);
                var rp2 = ConcreteSectionChecker.Adapters.ResistanceChecks.RequirePoint(plasticChecker, f2);
                var rp3 = ConcreteSectionChecker.Adapters.ResistanceChecks.RequirePoint(plasticChecker, f3);
                var rp4 = ConcreteSectionChecker.Adapters.ResistanceChecks.RequirePoint(plasticChecker, f4);

                var re1 = ConcreteSectionChecker.Adapters.ResistanceChecks.RequirePoint(elasticChecker, f1);
                var re2 = ConcreteSectionChecker.Adapters.ResistanceChecks.RequirePoint(elasticChecker, f2);
                var re3 = ConcreteSectionChecker.Adapters.ResistanceChecks.RequirePoint(elasticChecker, f3);
                var re4 = ConcreteSectionChecker.Adapters.ResistanceChecks.RequirePoint(elasticChecker, f4);

                int outCout = 0;
                DA.SetData(outCout++, Math.Round(rp1.M1.Value / 1000000, cifreSignificative));
                DA.SetData(outCout++, Math.Round(rp2.M2.Value / 1000000, cifreSignificative));
                DA.SetData(outCout++, Math.Round(rp3.M1.Value / 1000000, cifreSignificative));
                DA.SetData(outCout++, Math.Round(rp4.M2.Value / 1000000, cifreSignificative));
                DA.SetData(outCout++, Math.Round(re1.M1.Value / 1000000, cifreSignificative));
                DA.SetData(outCout++, Math.Round(re2.M2.Value / 1000000, cifreSignificative));
                DA.SetData(outCout++, Math.Round(re3.M1.Value / 1000000, cifreSignificative));
                DA.SetData(outCout++, Math.Round(re4.M2.Value / 1000000, cifreSignificative));
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("0a07f98a-2c47-4bc6-84c3-5833a4f6b65a");

        public override GH_Exposure Exposure => GH_Exposure.primary;
    }
}
