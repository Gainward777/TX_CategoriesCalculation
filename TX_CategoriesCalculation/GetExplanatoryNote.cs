using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

// Класс составляет и заполняет ПЗ в соответствии с шаблонами в корне решения

namespace TX_CategoriesCalculation
{
    public class GetExplanatoryNote
    {
        // Метод возвращает коллекцию
        public static SortedDictionary<int, CalculationResult> Write(GetResult rvtResult, string projectCode, string buildingNumber, string tableAdress, string vCategorySampleAdress, string generalSampleAdress)
        {      
            Word.Application application = null;
            Word.Document document = null;

            // словарь для сортировки результатов параллельного заполнения вспомогательных шаблонов по помещениям, сортировка по номеру помещения для правильного заполнения ПЗ
            SortedDictionary<int, CalculationResult> calculationResults = new SortedDictionary<int, CalculationResult>();

            try
            {
                List<string> materialsTable = File.ReadAllLines(tableAdress, Encoding.Default).ToList<string>();                

                // копирование и заполнение вспомогательных шаблонов по помещениям с последующим заполнением calculationResults
                Parallel.ForEach(rvtResult.results, (roomsData, state) =>
                {
                    RoomCalculate roomCalculate = new RoomCalculate();
                    CalculationResult calculationResult = roomCalculate.Write(materialsTable, vCategorySampleAdress, roomsData, roomsData.materials, roomsData.height);

                    if (!calculationResults.ContainsKey(roomsData.roomNumber))
                        calculationResults.Add(roomsData.roomNumber, calculationResult);
                });               

                application = new Word.Application();
                document = application.Documents.Open(generalSampleAdress);

                // замена тэгов в основном шаблоне ПЗ
                Dictionary<string, string> items = new Dictionary<string, string>() // - словарь с тегами в шаблоне и значениями, на которые они заменяются
                    {
                        { "<ProjectCode>", projectCode },
                        { "<Building>", buildingNumber }
                    };
                foreach (KeyValuePair<string, string> item in items)
                {
                    RoomCalculate.TagseReplace(application, item.Key, item.Value);
                }

                // копирование данных из вспомогательных шаблонов в основной
                foreach (KeyValuePair<int, CalculationResult> calculationResult in calculationResults)
                {
                    Word.Document docer = application.Documents.Open(calculationResult.Value.fileAdress);
                    Word.Range range = docer.Content;
                    range.Copy();
                    Word.Range copyTo = document.Content;
                    copyTo.Start = copyTo.End;
                    copyTo.Paste();
                    docer.Close(false);
                    GC.Collect();                                      
                }  

                Word.Dialog wordDialog = application.Dialogs[Word.WdWordDialog.wdDialogFileSaveAs];
                wordDialog.Show();
                
            }
            catch (Exception ex)
            {
                ShowWarningMessage showMessage = new ShowWarningMessage("Error", ex.Message + "\n\n" + ex.ToString());
                Thread newError = new Thread(new ThreadStart(showMessage.Run));
                newError.Start();
                //result = false;
            }
            finally
            {    
                if (document != null)
                    document.Close(false);
                if (application != null)
                    application.Quit();                
            }
            return calculationResults;
        }
    }
}
