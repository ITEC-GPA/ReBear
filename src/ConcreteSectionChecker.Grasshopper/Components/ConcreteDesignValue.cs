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
    public class ConcreteDesignValue : GpcComponent
    {
        public ConcreteDesignValue()
            : base("Concrete Material Design Value", "CD", "Concrete Material Design Value")
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
            pManager.AddTextParameter("fck", "fck", "fck", GH_ParamAccess.item);
            pManager.AddTextParameter("fcm", "fcm", "fcm", GH_ParamAccess.item);
            pManager.AddTextParameter("Elastic Modulus", "E", "Elastic Modulus", GH_ParamAccess.item);
            pManager.AddTextParameter("εy", "εy", "Yelding strain in compression", GH_ParamAccess.item);
            pManager.AddTextParameter("εu", "εu", "Ultimate strain in compression", GH_ParamAccess.item);
            pManager.AddTextParameter("fcd", "fcd", "fcd", GH_ParamAccess.item);
            pManager.AddTextParameter("fctd", "fcd", "fcd", GH_ParamAccess.item);
        }

        protected override void SolveGpc(IGH_DataAccess DA)
        {
            GH_ConcreteMaterial gH_ConcreteMaterial = null;
            GH_Standard gH_Standard = null;

            if (DA.GetData(0, ref gH_ConcreteMaterial) && (DA.GetData(1, ref gH_Standard)))
            {
                ConcreteMaterialEuropeanCommon concrete = gH_ConcreteMaterial.Value;
                StandardModelCode2010 standard = gH_Standard.Value;

                var count = 0;
                DA.SetData(count++, concrete.Name);
                DA.SetData(count++, Math.Abs(concrete.Fck));
                DA.SetData(count++, Math.Abs(concrete.Fcm));
                DA.SetData(count++, concrete.ElasticModulusCompression);
                DA.SetData(count++, concrete.StrainYCompression);
                DA.SetData(count++, concrete.StrainUCompression);
                DA.SetData(count++, Math.Abs(concrete.CalculateFcd(standard)));
                DA.SetData(count++, Math.Abs(concrete.CalculateFctd(standard)));
            }
        }

        //protected override Bitmap Icon => Resources.material;

        public override Guid ComponentGuid => new Guid("a8098614-15d9-4789-bffe-ecb9e694b888");

    }
}
