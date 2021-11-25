using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renga;

// Плагин собирает значения из модели, производит расчет, после чего составляет ПЗ и формирует строительное задание

namespace TX_CategoriesCalculation
{
    public class PluginEmbedding : IPlugin, IModelObserver
    {
        public ActionEventSource actionEvents;
        public SelectionEventSource selectionEventSource;
        public IApplication application;
        public IAction action;
        IOperation operation;
        IUI ui;
        IModel model;
        bool modelResult;

        public object TaskDialog { get; private set; }

        public bool Initialize(string pluginFolder)
        {
            application = new Renga.Application();
            ui = application.UI;
            action = ui.CreateAction();

            /*string imagePath = pluginFolder + @"\Icon.png";      // относительный путь к картинке в папке плагина
            IImage image = ui.CreateImage(); //работает через раз... НЕ БАГУЕТ, ЕСЛИ ЯВНО ОБЪЯВЛЯТЬ ПЕРЕМЕННУЮ ЧЕРЕЗ ИНТЕРФЕЙС!!!
            image.LoadFromFile(imagePath);
            action.Icon = image;*/

            action.DisplayName = "TX_CategoriesCalculation";
            action.ToolTip = "TX_CategoriesCalculation";


            ISelection selection = application.Selection;
            selectionEventSource = new SelectionEventSource(selection);
            selectionEventSource.ModelSelectionChanged += OnModelSelectionChanged;
            action.Enabled = selection.GetSelectedObjects().Length > 0;

            IUIPanelExtension panelExtension = ui.CreateUIPanelExtension();

            actionEvents = new ActionEventSource(action);
            actionEvents.Triggered += (sender, args) =>
            {


                Run(selection, pluginFolder);


            };
            panelExtension.AddToolButton(action);
            ui.AddExtensionToPrimaryPanel(panelExtension);

            return true;

        }

        private void OnModelSelectionChanged(object sender, EventArgs e)
        {
            ISelection selection = application.Selection;
            action.Enabled = selection.GetSelectedObjects().Length > 0;
        }

        public void Stop()
        {
            actionEvents.Dispose();
            selectionEventSource.Dispose();
        }

        public void Run(ISelection selection, string pluginFolder)
        {
            string categoryPropertyName = "ТХ_Категория";
            string fireLoadPropertyName = "ТХ_Пожарная нагрузка";

            bool isExist = true;
            string tableAdress = $@"{pluginFolder}\materials\Низшая теплота сгорания.csv";
            string vCategorySampleAdress = $@"{pluginFolder}\samples\vSample.doc";
            string generalSampleAdress = $@"{pluginFolder}\samples\generalSample.doc";
            string[] adresses = new string[3] { tableAdress, vCategorySampleAdress, generalSampleAdress };

            foreach (string adress in adresses)
            {
                if (!File.Exists(adress))
                {
                    ui.ShowMessageBox(MessageIcon.MessageIcon_Error,"Error", $"Отсутствует файл: {adress}");
                    isExist = false;
                }

            }

            if (isExist)
            {
                model = new RnpModel(application, selection, tableAdress, vCategorySampleAdress, generalSampleAdress, categoryPropertyName, fireLoadPropertyName);
                model.RegisterObserver(this);
                IController controller = new Controller(model);
            }

        }

        public void UpdateFromModel()
        {
            modelResult = model.GetResult();
        }
    }
}
