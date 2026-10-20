using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.SectionSolvers;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class PsiFactorComponent : GpcComponent
    {
        public PsiFactorComponent()
            : base("Psi coefficient", "FC", "Psi coefficient")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Concrete Material", "CM", "The Concrete Material", GH_ParamAccess.item);
            pManager.AddGenericParameter("Steel material", "SM", "The Concrete Material", GH_ParamAccess.item);
            pManager.AddNumberParameter("Homogenized factor", "HF", "Concrete Material", GH_ParamAccess.item, 15);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("ψ factor", "ψ", "The ψ factor", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_ConcreteMaterial gH_ConcreteMaterial = null;
            GH_SteelMaterial gH_SteelMaterial = null;
            double nfactor = 0;

            if (DA.GetData(0, ref gH_ConcreteMaterial) && DA.GetData(1, ref gH_SteelMaterial) && DA.GetData(2, ref nfactor))
            {
                DA.SetData(0, GPC.Model.Sections.Concrete.ReinforcedConcreteSection.CalculateHomogenizedFactorPhi(nfactor, gH_SteelMaterial.Value, gH_ConcreteMaterial.Value));
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("7480e84d-4e82-4be1-a021-425dff4803e1");

    }
}
