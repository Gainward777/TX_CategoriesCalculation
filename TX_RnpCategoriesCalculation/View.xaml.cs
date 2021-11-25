using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TX_CategoriesCalculation
{
    /// <summary>
    /// Логика взаимодействия для View.xaml
    /// </summary>
    public partial class View : Window, IView
    {
        IController controller=null;
        List<IViewObserver> viewObservers = new List<IViewObserver>();
        string projectCodeAsString;
        string buildingNumberAsString;

        public View(IController controller)
        {
            this.controller = controller;
            InitializeComponent();
        }

        private void Button_OnGround(object sender, RoutedEventArgs e)
        {
            controller.GetChoiseResult(false, projectCodeAsString, buildingNumberAsString);
            this.Close();
            NotifyObservers();
        }

        private void Button_Underground(object sender, RoutedEventArgs e)
        {            
            controller.GetChoiseResult(true, projectCodeAsString, buildingNumberAsString);
            this.Close();
            NotifyObservers();
            
        }



        public void NotifyObservers()
        {
            for (int i = 0; i < viewObservers.Count; i++)
            {
                IViewObserver viewObserver = viewObservers[i];
                viewObserver.UpdateFromView();
            }
        }

        public void RegisterObserver(IViewObserver observer)
        {
            viewObservers.Add(observer);
        }

        public void RemoveObserver(IViewObserver observer)
        {
            if (viewObservers.Contains(observer))
                viewObservers.Remove(observer);
        }

        public bool SetValue()
        {
            throw new NotImplementedException();
        }

        void IView.ShowDialog()
        {
            Show();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            projectCodeAsString = projectCode.Text;
        }

        private void buildingNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            buildingNumberAsString = buildingNumber.Text;
        }
    }
}
