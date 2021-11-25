using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// Класс собирает из модели суммарную пожарную нагрузку по всем, находящимся внутри помещения семействам, определяет высоту от пожарной нагрузки до потолка
// и возвращает все это в контейнере, совместно с номером помещения, его названием и площадью.

namespace TX_CategoriesCalculation
{
    public class RvtGetCalculationData
    {
        public static GetResult GetDataFromRooms(FilteredElementCollector familyesFilter, List<List<Element>> allLinkedRooms, double feetToMM, string parameterName, bool isUndreground)
        {
            // Основной метод возвращающий все необходимые данные

            bool result = true;
            
            List<GetResult> rvtIterimResults = new List<GetResult>();

            foreach (List<Element> listRooms in allLinkedRooms)  // на случай, если архитектурных файлов в проекте несколько
            {
                if (!result)
                {
                    break;
                }
                else
                {                   
                    //Parallel.ForEach(listRooms, (roomElement, state) =>   // генерирует внутренне исключение в фильтре семейств (из-за запроса к одном и тем же объектам?)
                    foreach (Element roomElement in listRooms)
                    {
                        if (!result)
                        {
                            //state.Break();
                            break;
                        }
                        else
                        {
                            Room room = roomElement as Room;
                            BoundingBoxXYZ roomBoundingBox = roomElement.get_BoundingBox(null);

                            double roomHeigth = Math.Ceiling((roomBoundingBox.Max.Z - roomBoundingBox.Min.Z) * feetToMM);

                            GetResult familyResult = GetFamily(familyesFilter, room, parameterName, feetToMM);  // переход к работе с следующей коллекцией
                            result = familyResult.result;

                            double heigthToStructures = (roomHeigth - familyResult.height);
                            double Feet2ToM2 = 10.764;
                            int mmToM = 1000;
                            string name = room.Name;
                            if (room.Name.Contains(room.Number))
                            {
                                name = room.Name.Replace(room.Number, string.Empty);
                            } 

                            if (familyResult.height != 0 && !isUndreground )                                
                            {
                                if(name.ToLower().Contains("санузел") || name.ToLower().Contains("тамбур")  
                                   || name.ToLower().Contains("бытовые помещения") || name.ToLower().Contains("коридор"))  
                                {
                                    // при использовании !name.Tolower().Contains(...); пропускает иногда некоторые помещения, вероятно связано с ошибками заполнения параметра в модели, однако так работает нормально
                                }
                                else
                                {
                                    GetResult rvtResult = new GetResult(familyResult.materials, result, familyResult.height / mmToM, int.Parse(room.Number), name, Math.Round(room.Area / Feet2ToM2, 2, MidpointRounding.AwayFromZero));
                                    rvtIterimResults.Add(rvtResult);
                                }
                                    
                            }
                            if(isUndreground)
                            {
                                GetResult rvtResult = new GetResult(familyResult.materials, result, familyResult.height/mmToM, int.Parse(room.Number), name, Math.Round(room.Area / Feet2ToM2, 2, MidpointRounding.AwayFromZero));
                                rvtIterimResults.Add(rvtResult);
                            }
                            
                        }
                    }//);
                }
            }  
            return new GetResult(result, rvtIterimResults);
        }


        public static GetResult GetFamily(FilteredElementCollector familyesFilter, Room room, string parameterName, double feetToMM)
        {
            // Метод обеспечивает проход по коллекции семейств, получение удовлетворяющих условиям принадлежности к конкретной категории,
            // специальности и нахождения внутри помещения. Далее объект передается дальше для дальнейших вычислений.

            bool result = true;

            Dictionary<string, double> materials = new Dictionary<string, double>();
            double familyHeight=0;            

            foreach (FamilyInstance family in familyesFilter)
            {
                if (!result)
                {
                    break;
                }
                else
                {
                    if (family.Name.Contains("ТХ"))
                    {
                        BoundingBoxXYZ boundingBoxXYZ = family.get_BoundingBox(null);
                        XYZ centerPoint = (boundingBoxXYZ.Max + boundingBoxXYZ.Min) * 0.5;
                        if (room.IsPointInRoom(centerPoint))
                        {
                            familyHeight = RvtFamilyHaight.IsHigher(boundingBoxXYZ, familyHeight, feetToMM); // определение самого высокого семейства в помещении

                            GetResult parameterResult = GetParameter(family, parameterName); // проход по параметрам семейства
                            result = parameterResult.result;

                            foreach(KeyValuePair<string, double> material in parameterResult.materials)
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
            }
            return new GetResult(materials, result, familyHeight);
        }

        public static GetResult GetParameter(FamilyInstance family, string parameterName)
        {
            // Метод получает коллекцию параметров типа из сообщенного семейства, находит необходимый параметр и передает его дальше

            bool result = true;
            Dictionary<string, double> materials = new Dictionary<string, double>();

            ParameterSet parametersOfType = family.Symbol.Parameters;

            foreach (Parameter parameterOfType in parametersOfType)  // получение значения параметра типа "многострочный текст" из коллекции параметров семейства
            {
                if (!result)
                {
                    break;
                }
                else
                {
                    if (parameterOfType.Definition.Name == parameterName)
                    {
                        GetResult parameterValueResult = GetParameterValue(parameterOfType, family, parameterName); // переход к работе с следующей коллекцией
                        result = parameterValueResult.result;
                        materials = parameterValueResult.materials;
                    }
                }

            }
            return new GetResult(materials, result);

        }


        public static GetResult GetParameterValue(Parameter parameterOfType, FamilyInstance family, string parameterName)
        {
            // Метод разбирает многострочный текст полученный из сообщенного параметра на строки, разделенные ';', и подстроки, разделенные '-'. 
            // Преобразует их в необходимые типы и заполняет ссылочный словарь для дальнейшего использования.
            // Если параметр заполнен не правильно пользователю выводится соответствующее сообщение.
            // Пример правильно заполненного параметра: ДСП - 24;
            //                                          Картон - 5;
            //                                          Резина - 14;
            //                                          Бумага - 30

            Dictionary<string, double> materials = new Dictionary<string, double>();

            string[] lineSeparation = parameterOfType.AsString().Split(';');    // разбиение многострочного текста на строки

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
                        materialAndWeight[0]=materialAndWeight[0].Replace("\r\n", "");
                    }                    
                   
                    if (!materials.ContainsKey(materialAndWeight[0].Trim(' ')) &&
                        double.TryParse(materialAndWeight[materialAndWeight.Length - 1], out parseOut) &&
                        materialAndWeight[0] != string.Empty)  // защита от дурака
                    {
                        materials.Add(materialAndWeight[0].Trim(' '), parseOut);
                    }
                    else if (!string.IsNullOrEmpty(materialAndWeight[0]))
                    {
                        ShowWarningMessage showMessage = new ShowWarningMessage("Error", $"Неверно заполнен параметр {parameterName}\nв семействе: {family.Name}");
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
