using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Architecture;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections;

namespace TX_CategoriesCalculation
{
    public class RvtModel : IModel
    {
        bool result;     

        List<IModelObserver> modelObservers = new List<IModelObserver>();

        ExternalCommandData commandData=null;
        string parameterName;
        double feetToMM;
        string tableAdress;
        string generalSampleAdress;
        string vCategorySampleAdress;

        public RvtModel(ExternalCommandData commandData, string tableAdress, string vCategorySampleAdress, string generalSampleAdress, string parameterName, double feetToMM)
        {            
            this.commandData = commandData;
            this.tableAdress = tableAdress;
            this.vCategorySampleAdress = vCategorySampleAdress;
            this.generalSampleAdress = generalSampleAdress;
            this.parameterName = parameterName;
            this.feetToMM = feetToMM;
        }       

        public void RegisterObserver(IModelObserver observer)
        {
            modelObservers.Add(observer);
        }

        public void RemoveObserver(IModelObserver observer)
        {
            if (modelObservers.Contains(observer))
                modelObservers.Remove(observer);
        }

        public void Run(bool isUderground, string projectCode, string buildingNumber)
        {
            result = Work(isUderground, commandData, parameterName, feetToMM, projectCode, buildingNumber);            
            NotifyObservers();
        }

        public void NotifyObservers()
        {
            for (int i = 0; i < modelObservers.Count; i++)
            {
                IModelObserver modelObserver = modelObservers[i];
                modelObserver.UpdateFromModel();
            }
        }

        public bool Work(bool isUnderground, ExternalCommandData commandData, string parameterName, double feetToMM, string projectCode, string buildingNumber)
        {            
            bool result=true;           
            
            // Сбор данных из модели
                 
            Document doc = commandData.Application.ActiveUIDocument.Document;

            // если пользователь ничего не ввел, в графы шифр объекта и номер сооружения передаются значения из проекта
            if(string.IsNullOrEmpty(projectCode)||string.IsNullOrWhiteSpace(projectCode)||projectCode.ToLower().Contains("шифр"))
            projectCode = doc.ProjectInformation.get_Parameter(BuiltInParameter.PROJECT_NUMBER).AsString();
            if(string.IsNullOrEmpty(buildingNumber)||string.IsNullOrWhiteSpace(buildingNumber)||buildingNumber.ToLower().Contains("номер"))
            buildingNumber = doc.ProjectInformation.get_Parameter(BuiltInParameter.PROJECT_NAME).AsString();
            

            FilteredElementCollector roomsFilter = new FilteredElementCollector(doc);
            FilteredElementCollector linksFilter = new FilteredElementCollector(doc); // фильтер для ссылочных проектов

            FilteredElementCollector links = linksFilter.OfClass(typeof(RevitLinkInstance)); // получение коллекции ссылочных проектов
                                                                                             
            List<List<Element>> allLinkedRooms = new List<List<Element>>();                    

            foreach (RevitLinkInstance link in links)   // собирает все помещения, которые находятся внутри ссылки с тегом специальности АС
            {
                if (link.Name.Contains("АС") || link.Name.Contains("АР"))
                {
                    Document AS_Doc = link.GetLinkDocument(); // получение объекта ссылочного документа

                    FilteredElementCollector linkedRoomsFilter = new FilteredElementCollector(AS_Doc);   // фильтер для ссылочного документа
                    List<Element> linkedRooms = linkedRoomsFilter.OfCategory(BuiltInCategory.OST_Rooms).WhereElementIsNotElementType().ToList<Element>(); // собирает все помещения, которые находятся внутри ссылочного документа
                    allLinkedRooms.Add(linkedRooms);
                }
            }

            FilteredElementCollector familyesFilter = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType();
            GetResult rvtResult=null;
            try
            {
                rvtResult = RvtGetCalculationData.GetDataFromRooms(familyesFilter, allLinkedRooms, feetToMM, parameterName, isUnderground);
            }
            catch (Exception ex)
            {
                ShowWarningMessage showMessage = new ShowWarningMessage("Error", ex.Message + "\n\n" +ex.ToString()); 
                Thread newError = new Thread(new ThreadStart(showMessage.Run));
                newError.Start();
                result = false;
            }   

            result = rvtResult.result;


            // Заполнение ПЗ
            if (result)
            {
                string tempAdress = $@"{Path.GetDirectoryName(generalSampleAdress)}\temp";
                Directory.CreateDirectory($@"{Path.GetDirectoryName(tempAdress)}\temp");

                SortedDictionary<int, CalculationResult> calculationResults = GetExplanatoryNote.Write(rvtResult, projectCode, buildingNumber, tableAdress, vCategorySampleAdress, generalSampleAdress);
                GetExcelTable.Write(calculationResults);

                try
                {
                    Directory.Delete(tempAdress, true);
                }
                catch (Exception ex)
                {
                    ShowWarningMessage showMessage = new ShowWarningMessage("Error", ex.Message + "\n\n" + ex.ToString());
                    Thread newError = new Thread(new ThreadStart(showMessage.Run));
                    newError.Start();
                }
            }
            return result;  
        }

        public bool GetResult()
        {
            return result;            
        }        
    }
}
