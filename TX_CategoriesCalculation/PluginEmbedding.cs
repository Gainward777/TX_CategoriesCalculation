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
using System.IO;
using System.Threading;

// Плагин собирает значения из модели, производит расчет, после чего составляет ПЗ и формирует строительное задание

namespace TX_CategoriesCalculation
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PluginEmbedding : IExternalCommand, IModelObserver
    {
        const double feetToMM = 304.8;
        string parameterName = "ТХ_Пожарная нагрузка";
        bool isExist = true;
        bool modelResult;
        IModel model;        
        const string tableAdress= @"C:\Revit Plugins\_TH\TX_CategoriesCalculation\materials\Низшая теплота сгорания.csv";
        const string vCategorySampleAdress = @"C:\Revit Plugins\_TH\TX_CategoriesCalculation\samples\vSample.doc";
        const string generalSampleAdress= @"C:\Revit Plugins\_TH\TX_CategoriesCalculation\samples\generalSample.doc";
        string[] adresses = new string[3] { tableAdress, vCategorySampleAdress , generalSampleAdress };

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Result result = Result.Failed;
            
            foreach (string adress in adresses)
            {
                if (!File.Exists(adress))
                {
                    TaskDialog.Show("Error", $"Отсутствует файл: {adress}");
                    isExist = false;
                }

            }            

            if (isExist)
            {
                model = new RvtModel(commandData, tableAdress, vCategorySampleAdress, generalSampleAdress, parameterName, feetToMM);
                model.RegisterObserver(this);
                IController controller = new Controller(model);               

                /*if (modelResult) 
                {
                    result = Result.Succeeded;
                }*/
            }

            /*if (!isExist || !modelResult)      // вызов Result.Succeeded сохраняет объект до конца работы Revit, т.к. в модель не вносятся изменения, это не нужно
            {                                    // можно спокойно вызывать Result.Failed, приложение все равно отработает
                result = Result.Failed;
            }     */

            return result;
        }

        public void UpdateFromModel()   // закладка на будущее. сейчас не нужен т.к. в модель сооружения не вносятся изменения
        {
            modelResult = model.GetResult();  
        } 
    }
}
