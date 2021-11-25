using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Word = Microsoft.Office.Interop.Word;

// Класс - контейнер для расчета и передачи категории помещения и сопутствующих данных

namespace TX_CategoriesCalculation
{
    public class FireLoadCategory
    {
        public string roughCategory { get; private set; }
        public string categoryToCheck { get; private set; }        
        public int ultimateSpecificFireLoad { get; private set; } 

        public string particaleNot { get; private set; }
        public string finalCategory { get; private set; }
        

        public FireLoadCategory(string roughCategory, string categoryToCheck, int ultimateSpecificFireLoad)
        {
            this.roughCategory = roughCategory;
            this.categoryToCheck = categoryToCheck;
            this.ultimateSpecificFireLoad = ultimateSpecificFireLoad;
        }

        public FireLoadCategory(string particaleNot, string finalCategory)
        {
            this.particaleNot = particaleNot;
            this.finalCategory = finalCategory;
        }


        public static FireLoadCategory GetRoughCategory(double roomArea, Word.Document doc, double specificFireLoad)
        {
            // Метод определяет предварительное значение категории и условия для проверки, если она нужна, 
            // если нет, удаляет дальнейшие теги из шаблона.

            string categoryToCheck = string.Empty;
            string roughCategory = string.Empty;
            int ultimateSpecificFireLoad=0;

            if (specificFireLoad < 180 && roomArea < 10)
            {
                roughCategory = "В4";
                ultimateSpecificFireLoad = 180;

                Word.Range location = doc.Content;
                Word.Find find = location.Find;
                find.Text = "<RoughCategory>";
                find.ClearFormatting();
                find.Execute();
                location.Start = location.End;
                location.End = doc.Range().End;

                location.Delete();
            }
            else if (specificFireLoad > 180 && specificFireLoad < 1400 && roomArea < 10 ||
                specificFireLoad > 180 && specificFireLoad < 1400 && roomArea > 10)
            {
                roughCategory = "В3";
                ultimateSpecificFireLoad = 1400;
                categoryToCheck = "В2";
            }
            else if (specificFireLoad > 1400 && specificFireLoad < 2200 && roomArea < 10 ||
                specificFireLoad > 1400 && specificFireLoad < 2000 && roomArea > 10)
            {
                roughCategory = "В2";
                ultimateSpecificFireLoad = 2200;
                categoryToCheck = "В1";
            }
            else if (specificFireLoad < 180 && roomArea > 10)
            {
                roughCategory = "В4, однако площадь размещения пожарной нагрузки больше 10 м2, следовательно, данное помещение следует отнести к категории В3.";
                ultimateSpecificFireLoad = 1400;
                categoryToCheck = "В2";
            }
            else
            {
                roughCategory = "В1";
                ultimateSpecificFireLoad = 2200;

                Word.Range location = doc.Content;
                Word.Find find = location.Find;
                find.Text = "<RoughCategory>";
                find.ClearFormatting();
                find.Execute();
                location.Start = location.End;
                location.End = doc.Range().End;

                location.Delete();
            }

            return new FireLoadCategory(roughCategory, categoryToCheck, ultimateSpecificFireLoad);
        }

        public static double GetCheckResult(double heightFromFireLoad, int ultimateSpecificFireLoad)
        {
            // Метод рассчитывает и возвращает проверочное значение для сравнения при окончательном определении категории

            double checkResult = Math.Round(0.64 * ultimateSpecificFireLoad * Math.Pow(heightFromFireLoad, 2), 2, MidpointRounding.AwayFromZero);
            return checkResult;
        }

        public static FireLoadCategory GetFinalCategory(double fireLoad, string categoryToCheck, double checkResult, string roughCategory)
        {
            // Метод определяет и возвращает окончательное значение категории и частицу "Не", в зависимости от того, удовлетворяет оноусловию или нет 

            string particleNot = string.Empty;
            if (fireLoad <= checkResult)
            {
                if (roughCategory.Length < 3)
                {
                    particleNot = "Не";
                    return new FireLoadCategory(particleNot, roughCategory);
                }
                else
                {
                    particleNot = "Не";
                    return new FireLoadCategory(particleNot, "В3");
                }                
            }
            else
            {
                return new FireLoadCategory(particleNot, categoryToCheck);                
            }
        }
    }
}
