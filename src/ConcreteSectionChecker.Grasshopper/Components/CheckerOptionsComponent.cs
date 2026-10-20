using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.SectionSolvers;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;
using static GPC.Checkers.Concrete.Checkers.SectionCheckerModelCode2010;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class CheckerOptionsComponent : GpcComponent
    {
        protected Dictionary<int, string> _failureAnalysisTypeList = new Dictionary<int, string>() {
            { 0, "Constant N" },
            { 1, "Constant eccentricity" },
            { 2, "Constant Mx - My" },
            { 3, "Constant N and Mx" },
            { 4, "Constant N and My" },
        };

        protected Dictionary<int, string> _domainPointStrategyTypesList = new Dictionary<int, string>() {
            { 0, "Iterative" },
            { 1, "Intersection" },
        };

        protected Dictionary<int, string> _stressAnalysisTypesList = new Dictionary<int, string>() {
            { 0, "Non Linear" },
            { 1, "Linear" },
        };

        protected Dictionary<int, string> _failureDomainTypesList = new Dictionary<int, string>() {
            { 0, "Elastic" },
            { 1, "Plastic" },
        };

        public CheckerOptionsComponent()
            : base("Concrete Checker Options", "CCO", "Concrete Checker Options")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Coordinate System", "CS", "The Analysis Coordinate System", GH_ParamAccess.item);
            int i = pManager.AddIntegerParameter("Failure Domain Types", "FDT", "The Failure Domain Types", GH_ParamAccess.item, 1);
            Param_Integer mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _failureDomainTypesList)
                mtParam.AddNamedValue(v.Value.ToString(), v.Key);
            i = pManager.AddIntegerParameter("Failure Analysis Types", "FAT", "The Failure Analysis Types", GH_ParamAccess.item, 1);
            mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _failureAnalysisTypeList)
                mtParam.AddNamedValue(v.Value.ToString(), v.Key);
            i = pManager.AddIntegerParameter("Domain Point Strategy Types", "DPST", "The Domain Point Strategy Types", GH_ParamAccess.item, 0);
            mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _domainPointStrategyTypesList)
                mtParam.AddNamedValue(v.Value.ToString(), v.Key);
            i = pManager.AddIntegerParameter("Stress Analysis Types", "SAT", "The Stress Analysis Types", GH_ParamAccess.item, 1);
            mtParam = pManager[i] as Param_Integer;
            foreach (KeyValuePair<int, string> v in _stressAnalysisTypesList)
                mtParam.AddNamedValue(v.Value.ToString(), v.Key);
            pManager.AddNumberParameter("ψ rebar", "ψ r", "The ψ coefficient for rebars", GH_ParamAccess.item, 1.5);
            pManager.AddNumberParameter("ψ tendon", "ψ t", "The ψ coefficient for tendons", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Consider tensile concrete", "TC", "Consider tensile concrete", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Checker Options", "CO", "Checker Options", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_CoordinateSystem gH_CoordinateSystem = null;
            int failureDomainType = 0;
            int failureAnalysisType = 0;
            int domainPointStrategyType = 0;
            int stressAnalysisType = 0;
            double psiRebar = 1.5;
            double psiTendon = 0;
            bool considerTensileConcrete = false;

            if (DA.GetData(0, ref gH_CoordinateSystem) && DA.GetData(1, ref failureDomainType) && DA.GetData(2, ref failureAnalysisType) && DA.GetData(3, ref domainPointStrategyType) &&
                DA.GetData(4, ref stressAnalysisType) && DA.GetData(5, ref psiRebar) && DA.GetData(6, ref psiTendon) && DA.GetData(7, ref considerTensileConcrete))
            {                
                SectionSolver.FailureDomainTypes failureDomainTypeEnum = (SectionSolver.FailureDomainTypes)failureDomainType;
                SectionSolver.FailureAnalysisTypes failureAnalysisTypeEnum = (SectionSolver.FailureAnalysisTypes)failureAnalysisType;
                SectionSolver.DomainPointStrategyTypes domainPointStrategyTypeEnum = (SectionSolver.DomainPointStrategyTypes)domainPointStrategyType;
                SectionSolver.StressAnalysisTypes stressAnalysisTypeEnum = (SectionSolver.StressAnalysisTypes)stressAnalysisType;

                SectionOptionsModelCode2010 sectionOptionsModelCode2010 = new SectionOptionsModelCode2010()
                {
                    ForceReferenceCoordinateSystem = gH_CoordinateSystem.Value,
                    FailureAnalysisType = failureAnalysisTypeEnum,
                    FailureDomainType = failureDomainTypeEnum,
                    DomainPointStrategy = domainPointStrategyTypeEnum,
                    StressAnalysisType = stressAnalysisTypeEnum,
                    PsiCoefficientRebar = psiRebar,
                    PsiCoefficientTendon = psiTendon,
                    ConsiderTensileConcrete = considerTensileConcrete
                };

                DA.SetData(0, new GH_CheckerOptions(sectionOptionsModelCode2010));
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("acd21b40-347d-487b-ad3d-c355e19b5315");

    }
}
