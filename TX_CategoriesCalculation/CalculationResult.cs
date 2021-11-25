using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TX_CategoriesCalculation
{
    public class CalculationResult
    {
        public int roomNumber { get; private set; }
        public string fileAdress { get; private set; }
        public string finalCategory { get; private set; }        

        public CalculationResult(int roomNumber, string fileAdress, string finalCategory)
        {
            this.fileAdress = fileAdress;
            this.roomNumber = roomNumber;
            this.finalCategory = finalCategory;
        }        
    }
}
