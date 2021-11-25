using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TX_CategoriesCalculation
{
    public class MaterialSearch
    {
        public static Dictionary<string, double> GetNetCalorificValue(List<string> materialsTable, Dictionary<string, double> materials)
        {
            Dictionary<string, double> netCalorificValues = new Dictionary<string, double>();

            foreach (KeyValuePair<string, double> material in materials)
            {    
                List<string> l = materialsTable.Where(x => x.Split(';')[0] == material.Key).ToList();
                
                    string[] s = l[0].Split(';');

                    double parseResult;

                    if (s[s.Length - 1].Contains('.'))
                        s[s.Length - 1] = s[s.Length - 1].Replace('.', ',');

                    if (l.Count==0 || l.Count > 1 || !double.TryParse(s[s.Length - 1], out parseResult) || string.IsNullOrWhiteSpace(s[s.Length - 1]))
                    {
                        ShowWarningMessage showMessage = new ShowWarningMessage("Error", $"В таблице есть дубликаты материала {material.Key} и(или) значение низшей теплоты для этого материала не является числом");
                        Thread newError = new Thread(new ThreadStart(showMessage.Run));
                        newError.Start();
                        break;
                    }
                    else
                    {
                        netCalorificValues.Add(material.Key, parseResult);
                    }
            }
            return netCalorificValues;

        }
    }
}
