using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TX_CategoriesCalculation
{
    public interface IController
    {
        void GetChoiseResult(bool isUngerground, string projectCode, string buildingNumber);
    }
}
