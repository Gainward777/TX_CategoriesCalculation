using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TX_CategoriesCalculation
{
    public interface IModel
    {
        void Run(bool isUnderground, string projectCode, string buildingNumber);
        bool GetResult();        

        void RegisterObserver(IModelObserver observer);
        void RemoveObserver(IModelObserver observer);
    }
}
