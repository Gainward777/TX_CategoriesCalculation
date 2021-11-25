using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renga;

namespace TX_CategoriesCalculation
{
    public class RnpObjectCollections
    {
        public List<IModelObject> rooms { get; private set; }
        public List<IModelObject> elements { get; private set; }

        public RnpObjectCollections(List<IModelObject> rooms, List<IModelObject> elements)
        {
            this.rooms = rooms;
            this.elements = elements;
        }

        public static RnpObjectCollections Get(IApplication application, ISelection selection)
        {
            List<IModelObject> rooms = new List<IModelObject>();
            List<IModelObject> elements = new List<IModelObject>();

            Renga.IModel model = application.Project.Model;
            IOperation operation = model.CreateOperation();

            operation.Start();

            IModelObjectCollection modelObjectCollection = model.GetObjects();

            int[] selectionObjects = (int[])selection.GetSelectedObjects(); // коллекция ID, которые 22046 и т.д.

            foreach (int index in selectionObjects)
            {
                IModelObject modelObject = modelObjectCollection.GetById(index);

                if (modelObject.ObjectType == ObjectTypes.Room)
                {
                    rooms.Add(modelObject);
                }
                if (modelObject.ObjectType == ObjectTypes.Element && modelObject.Name.Contains("ТХ"))
                {
                    elements.Add(modelObject);
                }
            }

            operation.Apply();

            return new RnpObjectCollections(rooms, elements);
        }
    }
}
