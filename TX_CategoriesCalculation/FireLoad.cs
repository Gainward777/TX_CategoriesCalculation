using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Класс - контейнер, определающий значение пожарной нагрузки

namespace TX_CategoriesCalculation
{
    public class FireLoad
    {
        public string fireLoadAsString { get; private set; }
        public double fireLoad { get; private set; }

        public FireLoad(string fireLoadAsString, double fireLoad)
        {
            this.fireLoadAsString = fireLoadAsString;
            this.fireLoad = fireLoad;
        }

        public static FireLoad GetFireLoad(Dictionary<string, double> materials, Dictionary<string, double> netCalorificValues)
        {
            // Метод расчитывает суммарную пожарную нагрузку, возвращает контейнер с числовым значением пожарной нагрузки 
            // возвращает формулу с подставленными значениями в формате строки (используется именно полное выражение
            // т.к. заранее не известна наменклатура материалов)

            string fireLoadAsString = "Q=";
            double fireLoad = 0;

            foreach (KeyValuePair<string, double> material in materials)
            {
               fireLoadAsString += $"{material.Value}x{netCalorificValues[material.Key]}+";
               fireLoad = fireLoad + material.Value * netCalorificValues[material.Key];
            }

            fireLoadAsString = fireLoadAsString.TrimEnd('+');
            fireLoadAsString = fireLoadAsString + $"={fireLoad}";

            return new FireLoad(fireLoadAsString, fireLoad);
        }

        public static double GetSpecificFireLoad(double roomArea, double fireLoad)
        {
            // Метод рассчитывает и возвращает удельное значение пожарной нагрузки

            double specificFireLoad = Math.Round(fireLoad / roomArea, 2, MidpointRounding.AwayFromZero);
            return specificFireLoad;
        }
    }
}
