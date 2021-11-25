using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renga;

namespace TX_CategoriesCalculation
{
    public class GetPropertyId
    {
        public static string Name(string PropertyName, IApplication application)
        {
            string ID = string.Empty;

            IPropertyManager propertyManager = application.Project.PropertyManager;

            for (int i = 0; i < propertyManager.PropertyCount; i++)
            {
                string prId = propertyManager.GetPropertyIdS(i);
                string name = propertyManager.GetPropertyNameS(prId);
                if (name == PropertyName)
                    ID = prId;
            }
            return ID;
        }
    }
}
