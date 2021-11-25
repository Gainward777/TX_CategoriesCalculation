using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TX_CategoriesCalculation
{
    public interface IView
    {
        bool SetValue();
        void ShowDialog();

        void RegisterObserver(IViewObserver observer);
        void RemoveObserver(IViewObserver observer);
    }
}
