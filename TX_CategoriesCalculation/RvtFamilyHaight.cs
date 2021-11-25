using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Класс определяет выше ли текущее переданное семейство чем предыдущее

namespace TX_CategoriesCalculation
{
    public class RvtFamilyHaight
    {
        public static double IsHigher(BoundingBoxXYZ boundingBoxXYZ, double familyHeight, double feetToMM)
        {
            double isHigher;
            double height = Math.Ceiling((boundingBoxXYZ.Max.Z - boundingBoxXYZ.Min.Z) * feetToMM);
            if (familyHeight < height)
            {
                isHigher = height;
            }
            else
            {
                isHigher = familyHeight;
            }
            return isHigher;

        }
    }
}
