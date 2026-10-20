using Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Datatypes;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Model.Materials;
using GPC.Model.Standards;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using ConcreteSectionChecker.Grasshopper;
using System;
using System.Collections.Generic;

namespace Fem2Rhino.Grasshopper.Checkers.ConcreteChecker.Components
{
    public class SteelDesignValue : GpcComponent
    {
        public SteelDesignValue()
            : base("Steel Material Design Value", "SD", "Steel Material Design Value")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Concrete Material", "CM", "Concrete Material", GH_ParamAccess.item);
            pManager.AddGenericParameter("Standard", "S", "Standard", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "Name", "Name", GH_ParamAccess.item);
            pManager.AddTextParameter("Elastic Modulus", "E", "Elastic Modulus", GH_ParamAccess.item);
            pManager.AddTextParameter("fyk", "fyk", "fyk", GH_ParamAccess.item);
            pManager.AddTextParameter("fu", "fu", "fu", GH_ParamAccess.item);
            pManager.AddTextParameter("Et", "Et", "Et", GH_ParamAccess.item);
            pManager.AddTextParameter("εy", "εy", "Yelding strain in compression", GH_ParamAccess.item);
            pManager.AddTextParameter("εu", "εu", "Ultimate strain in compression", GH_ParamAccess.item);
            pManager.AddTextParameter("fyd", "fyd", "fyd", GH_ParamAccess.item);
            pManager.AddTextParameter("fud", "fud", "fud", GH_ParamAccess.item);
            pManager.AddTextParameter("εyd", "εyd", "εyd", GH_ParamAccess.item);
            pManager.AddTextParameter("εud", "εud", "εud", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_SteelMaterial gH_SteelMaterial = null;
            GH_Standard gH_Standard = null;

            if (DA.GetData(0, ref gH_SteelMaterial) && (DA.GetData(1, ref gH_Standard)))
            {
                SteelMaterialEN1992 steelMaterialEN1992 = gH_SteelMaterial.Value;
                StandardModelCode2010 standard = gH_Standard.Value;

                var count = 0;
                DA.SetData(count++, steelMaterialEN1992.Name);
                DA.SetData(count++, steelMaterialEN1992.ElasticModulusTension);
                DA.SetData(count++, steelMaterialEN1992.Fyk);
                DA.SetData(count++, steelMaterialEN1992.Fu);
                DA.SetData(count++, steelMaterialEN1992.Et);
                DA.SetData(count++, steelMaterialEN1992.StrainYTension);
                DA.SetData(count++, steelMaterialEN1992.StrainUTension);
                DA.SetData(count++, steelMaterialEN1992.CalculateDesignYieldingStressTension(standard));
                DA.SetData(count++, steelMaterialEN1992.CalculateDesignStress(standard, steelMaterialEN1992.StrainUTension));
                DA.SetData(count++, steelMaterialEN1992.CalculateDesignYieldingStrainTension(standard));
                DA.SetData(count++, steelMaterialEN1992.CalculateDesignUltimateStrainTension(standard));
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("939e852a-e1b4-4b5b-9454-4eb54be70394");

    }
}
