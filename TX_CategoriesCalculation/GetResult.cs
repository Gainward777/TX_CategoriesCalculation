using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Класс контейнер для результатов сбора данных для расчета и безошибочности выполнения опарций

namespace TX_CategoriesCalculation
{
    public class GetResult
    {
        public Dictionary<string, double> materials { get; private set; }
        public bool result { get; private set; }
        public double height { get; private set; }
        public int roomNumber { get; private set; }
        public string roomName { get; private set; }
        public double roomArea { get; private set; }
        public List<GetResult> results { get; private set; }        

        public GetResult(Dictionary<string, double> materials, bool result, double height, int roomNumber, string roomName, double roomArea)
        {
            this.materials = materials;
            this.result = result;
            this.height = height;
            this.roomNumber = roomNumber;
            this.roomName = roomName;
            this.roomArea = roomArea;            
        }

        public GetResult(Dictionary<string, double> materials, bool result, double height)
        {
            this.materials = materials;
            this.result = result;
            this.height = height;
        }

        public GetResult(Dictionary<string, double> materials, bool result)
        {
            this.materials = materials;
            this.result = result;
        }

        public GetResult(bool result, List<GetResult> results)
        {
            this.result = result;
            this.results = results;
        }

        public GetResult(int roomNumber, string roomName, double height, double roomArea)
        {
            this.roomNumber = roomNumber;
            this.roomName = roomName;
            this.height = height;
            this.roomArea = roomArea;           
        }
    }
}
