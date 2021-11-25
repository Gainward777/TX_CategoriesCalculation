using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Renga;

// Класс собирает из модели суммарную пожарную нагрузку по всем, находящимся внутри помещения семействам, определяет высоту от пожарной нагрузки до потолка
// и возвращает все это в контейнере, совместно с номером помещения, его названием и площадью.

namespace TX_CategoriesCalculation
{
    public class RnpDataValue
    {
        public static GetResult Get(IApplication application, string categoryPropertyName, List<IModelObject> rooms, List<IModelObject> elements)
        {
            // Основной метод возвращающий все необходимые данные
            
            bool result = true;
            double heightToFireLoad = 0;
            GetResult roomResult = null;
            GetResult elementsResult = null;

            Renga.IModel model = application.Project.Model;
            IOperation operation = model.CreateOperation();

            operation.Start(); 

            if (rooms.Count > 1)
            {
                ShowWarningMessage showMessage = new ShowWarningMessage("Error", "Выбрано больше одно помещения");
                Thread newError = new Thread(new ThreadStart(showMessage.Run));
                newError.Start();
                result = false;
            }
            else
            {
                roomResult = GetRooms(rooms);
                elementsResult = GetElements(elements, application, categoryPropertyName);
                result = elementsResult.result;
                heightToFireLoad = roomResult.height - elementsResult.height;

            }

            if (result)
            {
                operation.Apply();
            }
            else
            {
                operation.Rollback();
            }

            return new GetResult(elementsResult.materials, result, heightToFireLoad/1000, roomResult.roomNumber, roomResult.roomName, roomResult.roomArea);
            
        }

        public static GetResult GetRooms(List<IModelObject> rooms)
        {
            // Метод получает значения высоты, названия, номера и площади для выбранного помещения

            int roomNumber=0;
            string roomName=string.Empty;
            double roomHeight=0;
            double roomArea=0;            

            foreach (IModelObject roomObject in rooms)
            {
                IRoom room = roomObject as IRoom;

                IQuantityContainer quantityContainer = roomObject.GetQuantities();

                IParameterContainer parameterContainer = roomObject.GetParameters();

                roomHeight = parameterContainer.Get(ParameterIds.RoomHeight).GetDoubleValue();
                roomArea = quantityContainer.Get(QuantityIds.GrossFloorArea).AsArea(AreaUnit.AreaUnit_Meters2);

                roomNumber = int.Parse(room.RoomNumber);
                roomName = room.RoomName;
            }

            return new GetResult(roomNumber, roomName, roomHeight, roomArea);
        }

        public static GetResult GetElements(List<IModelObject> elements, IApplication application, string propertyName)
        {
            // Метод определяет суммарное значение пожарной нагрузки и высоту самого высокого элемента в помещении

            bool result = true;
            Dictionary<string, double> materials = new Dictionary<string, double>();
            double elementHeight = 0;

            string fireLoadUniqueIdAsString = GetPropertyId.Name(propertyName, application);

            foreach (IModelObject element in elements)
            {
                if (!result)
                {
                    break;
                }
                else
                {                    
                    IQuantityContainer quantityContainer = element.GetQuantities();
                    IPropertyContainer propertyContainer = element.GetProperties();

                    string fireLoadValue = propertyContainer.Get(Guid.Parse(fireLoadUniqueIdAsString)).GetStringValue();

                    double currentElementHeight = quantityContainer.Get(QuantityIds.OverallHeight).AsLength(LengthUnit.LengthUnit_Millimeters);

                    if (currentElementHeight > elementHeight)
                        elementHeight = currentElementHeight;

                    GetResult propertyResult = GetPropertyValue(fireLoadValue, element, propertyName);
                    result = propertyResult.result;

                    if (result)
                    {
                        foreach (KeyValuePair<string, double> material in propertyResult.materials)
                        {
                            if (materials.ContainsKey(material.Key))
                            {
                                double summMaterialWeight = material.Value + materials[material.Key];
                                materials[material.Key] = summMaterialWeight;
                            }
                            else
                            {
                                materials[material.Key] = material.Value;
                            }
                        }
                    }                    
                }  
            }
            return new GetResult(materials, result, elementHeight);

        }

        public static GetResult GetPropertyValue(string fireLoadValue, IModelObject element, string propertyName)
        {
            // Метод разбирает многострочный текст полученный из сообщенного параметра на строки, разделенные ';', и подстроки, разделенные '-'. 
            // Преобразует их в необходимые типы и заполняет ссылочный словарь для дальнейшего использования.
            // Если параметр заполнен не правильно пользователю выводится соответствующее сообщение.
            // Пример правильно заполненного параметра: ДСП - 24;
            //                                          Картон - 5;
            //                                          Резина - 14;
            //                                          Бумага - 30

            Dictionary<string, double> materials = new Dictionary<string, double>();

            string[] lineSeparation = fireLoadValue.Split(';');    // разбиение многострочного текста на строки

            bool result = true;
            foreach (string line in lineSeparation)   // в каждой строке находится значение материала и его веса. сбор этих значенией, 
            {                                        // сложение веса одинакового материала и заполнение словаря, где ключ: название материала+
                if (!result)
                {
                    break;
                }
                else
                {
                    string[] materialAndWeight = line.Split('-');

                    double parseOut = 0;


                    if (materialAndWeight[0].Contains("\r\n"))
                    {
                        materialAndWeight[0] = materialAndWeight[0].Replace("\r\n", "");
                    }                  

                    if (!materials.ContainsKey(materialAndWeight[0].Trim(' ')) &&
                        double.TryParse(materialAndWeight[materialAndWeight.Length - 1], out parseOut) &&
                        materialAndWeight[0] != string.Empty)  // защита от дурака
                    {
                        materials.Add(materialAndWeight[0].Trim(' '), parseOut);
                    }
                    else if (!string.IsNullOrEmpty(materialAndWeight[0]))
                    {
                        ShowWarningMessage showMessage = new ShowWarningMessage("Error", $"Неверно заполнено свойство {propertyName}\nэлемента: {element.Name}");
                        Thread newError = new Thread(new ThreadStart(showMessage.Run));
                        newError.Start();
                        result = false;
                    }
                }
            }
            return new GetResult(materials, result);

        }


    }
}
