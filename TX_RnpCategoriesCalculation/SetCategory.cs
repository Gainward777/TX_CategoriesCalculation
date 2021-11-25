using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renga;

// Класс проставляет значения рассчитанных категорий в объекты помещений в проекте

namespace TX_CategoriesCalculation
{
    public class SetCategory
    {
        public static void Set(IApplication application, SortedDictionary<int, CalculationResult> calculationResults, SortedDictionary<int, IModelObject> rooms, string fireLoadPropertyName)
        {
            Renga.IModel model = application.Project.Model;
            IOperation operation = model.CreateOperation();

            operation.Start();

            string fireLoadUniqueIdAsString = GetPropertyId.Name(fireLoadPropertyName, application);

            foreach (KeyValuePair<int, IModelObject> room in rooms)
            {
                IPropertyContainer propertyContainer = room.Value.GetProperties();

                propertyContainer.Get(Guid.Parse(fireLoadUniqueIdAsString)).SetStringValue(calculationResults[room.Key].finalCategory);
            }

            operation.Apply();
        }        
    }
}
