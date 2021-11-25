using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Renga;

namespace TX_CategoriesCalculation
{
    public class RnpModel : IModel
    {
        bool result;

        List<IModelObserver> modelObservers = new List<IModelObserver>();

        IApplication application;
        ISelection selection;
        string tableAdress;
        string vCategorySampleAdress;
        string generalSampleAdress;
        string categoryPropertyName;
        string fireLoadPropertyName;

        public RnpModel(IApplication application, ISelection selection, string tableAdress, string vCategorySampleAdress, string generalSampleAdress, string categoryPropertyName, string fireLoadPropertyName)
        {
            this.application = application;
            this.selection = selection;
            this.tableAdress = tableAdress;
            this.vCategorySampleAdress = vCategorySampleAdress;
            this.generalSampleAdress = generalSampleAdress;
            this.categoryPropertyName = categoryPropertyName;
            this.fireLoadPropertyName = fireLoadPropertyName;
        }

        public bool GetResult()
        {
            return result;
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

        public void Run(bool isUnderground, string projectCode, string buildingNumber)
        {
            result = Work(isUnderground, projectCode, buildingNumber);
        }

        public void NotifyObservers()
        {
            for (int i = 0; i < modelObservers.Count; i++)
            {
                IModelObserver modelObserver = modelObservers[i];
                modelObserver.UpdateFromModel();
            }
        }

        public bool Work(bool isUnderground, string projectCode, string buildingNumber)
        {
            bool result = true;

            // Сбор данных из модели (ВАЖНО: Поскольку на момент написания плагина API Renga не содержит методов для определения положения элемента в пространстве
            // (обещали добавить позже) сейчас выбор подземного или надземного расположения сооружения ничего не меняет, т.к. нет возможности собрать из модели
            // все помещения или только нужные. Для обоих случаев реализуется один механизм интерфейса ISelection, требующий последовательного выбора каждого помещения,
            // объектов в нем и запуска плагина. После добавления соответствующих методов планируется доработка алгоритма)

            // если пользователь ничего не ввел, в графы шифр объекта и номер сооружения передаются значения из проекта
            if (string.IsNullOrEmpty(projectCode) || string.IsNullOrWhiteSpace(projectCode) || projectCode.ToLower().Contains("шифр"))
                projectCode = application.Project.ProjectInfo.Code;
            if (string.IsNullOrEmpty(buildingNumber) || string.IsNullOrWhiteSpace(buildingNumber) || buildingNumber.ToLower().Contains("номер"))
                buildingNumber = application.Project.BuildingInfo.Number;

            GetResult rnpResult = null;
            RnpObjectCollections rnpObjectCollections = null;
            try
            {
                rnpObjectCollections = RnpObjectCollections.Get(application, selection);
                rnpResult = RnpDataValue.Get(application, categoryPropertyName, rnpObjectCollections.rooms, rnpObjectCollections.elements);
            }
            catch (Exception ex)
            {
                ShowWarningMessage showMessage = new ShowWarningMessage("Error", ex.Message + "\n\n" + ex.ToString());
                Thread newError = new Thread(new ThreadStart(showMessage.Run));
                newError.Start();
                result = false;
            }

            result = rnpResult.result;

            // Заполнение ПЗ
            if (result)
            {
                string tempAdress = $@"{Path.GetDirectoryName(generalSampleAdress)}\temp";
                Directory.CreateDirectory($@"{Path.GetDirectoryName(tempAdress)}\temp");

                SortedDictionary<int, CalculationResult> calculationResults = GetExplanatoryNote.Write(rnpResult, projectCode, buildingNumber, tableAdress, vCategorySampleAdress, generalSampleAdress);

                try
                {
                    //Добавление категорий в объекты помещнияб находящиеся в модели
                    SortedDictionary<int, IModelObject> rooms = new SortedDictionary<int, IModelObject>();
                    foreach (IModelObject roomObject in rnpObjectCollections.rooms)
                    {
                        IRoom room = roomObject as IRoom;
                        rooms.Add(int.Parse(room.RoomNumber), roomObject);
                    }

                    SetCategory.Set(application, calculationResults, rooms, fireLoadPropertyName);
                }
                catch(Exception ex)
                {
                    ShowWarningMessage showMessage = new ShowWarningMessage("Error", ex.Message + "\n\n" + ex.ToString());
                    Thread newError = new Thread(new ThreadStart(showMessage.Run));
                    newError.Start();
                }

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
            NotifyObservers();
            return result;
        }
    }
}
