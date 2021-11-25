using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;
using Word = Microsoft.Office.Interop.Word;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

// Класс принимает словарь с материалами и их весом, находящимися в помещении. Находит в таблице соответствующие значения. 
// Производит с их помощью рассчет и заполняет ПЗ

namespace TX_CategoriesCalculation
{
    public class RoomCalculate
    {       
        public CalculationResult Write(List<string> materialsTable, string sapmleAdress, GetResult rvtResult, Dictionary<string, double> materials, double heightFromFireLoad)
        {     
            Word.Application application = new Word.Application();

            string currentDateTime=string.Empty;
            string correctCurrentDateTime=string.Empty;
            string copyAdress=string.Empty;
            Word.Document doc = null;

            try
            {
                currentDateTime = DateTime.Now.ToLocalTime().ToString("mm:ss:fffffff");
                correctCurrentDateTime = currentDateTime.Replace(':', '.');
                copyAdress = $@"{Path.GetDirectoryName(sapmleAdress)}\temp\{correctCurrentDateTime}.doc";                
                File.Copy(sapmleAdress, copyAdress);
                doc = application.Documents.Open(copyAdress);
            }
            catch(Exception ex)
            {
                ShowWarningMessage showMessage = new ShowWarningMessage("Error", ex.Message + "\n\n" + ex.ToString());
                Thread newError = new Thread(new ThreadStart(showMessage.Run));
                newError.Start();

                if(doc !=null)
                doc.Close(false);
                application.Quit();
                Marshal.ReleaseComObject(application);
                GC.Collect();
            }                                                  

            Dictionary<string, double> netCalorificValues = MaterialSearch.GetNetCalorificValue(materialsTable, materials);

            AddTable(doc, materials, netCalorificValues); 


            FireLoad fireLoad = FireLoad.GetFireLoad(materials, netCalorificValues);    
            double specificFireLoad = FireLoad.GetSpecificFireLoad(rvtResult.roomArea, fireLoad.fireLoad);  
            FireLoadCategory category = FireLoadCategory.GetRoughCategory(rvtResult.roomArea, doc, specificFireLoad); 
            double checkResult = FireLoadCategory.GetCheckResult(heightFromFireLoad, category.ultimateSpecificFireLoad); 
            FireLoadCategory finalCategoryContainer=FireLoadCategory.GetFinalCategory(fireLoad.fireLoad, category.categoryToCheck, checkResult, category.roughCategory);  
            string finalCategory = finalCategoryContainer.finalCategory;

            Dictionary<string, string> items = new Dictionary<string, string>() // - словарь с тегами в шаблоне и значениями, на которые они заменяются
            {
                //{ "<Building>", "Сооружение номер 1"}, вынести за пределы класса, т.к. заполняется 1 раз
                { "<RoomNumber>", $"{rvtResult.roomNumber}" },
                { "<RoomName>", $"{rvtResult.roomName}" },
                { "<Table>", "" }, // - пустая строка для корректного удаления тега из конечного файла
                { "<Area>", $"{rvtResult.roomArea}"},
                { "<FireLoadAsString>", fireLoad.fireLoadAsString },
                { "<SpecificFireLoad>", specificFireLoad.ToString() },
                { "<RoughCategory>", category.roughCategory },
                { "<CategoryToCheck>", category.categoryToCheck },
                { "<UltimateSpecificFireLoad>", category.ultimateSpecificFireLoad.ToString() },
                { "<FireLoad>", fireLoad.fireLoad.ToString() },
                { "<HeightFromFireLoad>", heightFromFireLoad.ToString() },                
                { "<CheckResult>", checkResult.ToString() },
                { "<ParticleNot>", finalCategoryContainer.particaleNot},
                { "<FinalCategory>", finalCategory }
            };

            foreach (var item in items)
                TagseReplace(application, item.Key, item.Value);   // - заменяет теги в шаблоне на занчения из словаря  

            doc.Close(true);
            application.Quit();
            Marshal.ReleaseComObject(application);
            GC.Collect();

            
            if (string.IsNullOrEmpty(finalCategory))
                finalCategory = category.roughCategory;

            return new CalculationResult(rvtResult.roomNumber, copyAdress, finalCategory);
        }

        public static void TagseReplace(Word.Application application, string whatFind, string valueToReplece)
        {
            // Метод заменяет теги в файле-шаблоне на заданные значения

            object missing = System.Reflection.Missing.Value;

            Word.Find find = application.Selection.Find;
            find.Text = whatFind;
            find.Replacement.Text = valueToReplece;

            Word.WdReplace replace = Word.WdReplace.wdReplaceAll;

            find.Execute(FindText: Type.Missing, MatchSoundsLike: missing, ReplaceWith: missing, Replace: replace);
        }

        public void AddTable(Word.Document doc, Dictionary<string, double> materials, Dictionary<string, double> netCalorificValues)
        {
            // Метод добавляет в документ таблицу с материалами, их весом и низшей теплотой сгорания

            List<string> valuesForTable = ParseForWordTable(materials, netCalorificValues); 

            string[] RowOne = new string[3] { "Наименование пожарной нагрузки", "Масса пожарной нагрузки, кг", "Низшая теплота сгорания, МДж/кг" };

            Word.Range tableLocation = doc.Content;
            Word.Find find = tableLocation.Find;
            find.Text = "<Table>";            
            find.ClearFormatting();
            find.Execute();
            tableLocation.Start = tableLocation.End;
            Word.Table table = doc.Tables.Add(tableLocation, materials.Count+1, RowOne.Length);

            for(int i=1; i<=RowOne.Length; ++i)
            {
                table.Columns[i].Width = 153;
            }
            
            table.Rows.Alignment = Word.WdRowAlignment.wdAlignRowCenter;
            table.Borders.Enable = 23;            

            int count = 0;
            foreach (Word.Row row in table.Rows)
            {       
                for(int i=0; i<row.Cells.Count; ++i)
                {
                    if (row.Cells[i + 1].RowIndex == 1)
                    {
                        row.Cells[i + 1].Range.Text = RowOne[i];
                        //row.Cells[i + 1].Range.Bold = 1;
                    }
                    else
                    {
                        row.Cells[i + 1].Range.Text = valuesForTable[count];
                        count++;
                    }
                }                
            }
            tableLocation.Delete();
        }
        

        public List<string> ParseForWordTable(Dictionary<string, double> materials, Dictionary<string, double> netCalorificValues)
        {
            // - вспомогательный метод, преобразующий два словаря в один массив формата
            //  {материал1, вес1, низшая теплота1, материал2, вес2, низшая теплота2...} 

            List<string> values = new List<string>();
            
            foreach(KeyValuePair<string, double> material in materials)
            {
                values.Add(material.Key);
                values.Add(material.Value.ToString());
                values.Add(netCalorificValues[material.Key].ToString());
            }            

            return values;
        }        
    }
}
