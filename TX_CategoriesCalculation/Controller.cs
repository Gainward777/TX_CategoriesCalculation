using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TX_CategoriesCalculation
{
    public class Controller : IController, IViewObserver
    {
        bool isUngerground;
        string projectCode;
        string buildingNumber;
        IModel model;

        public object Thrading { get; private set; }

        public Controller(IModel model)
        {
            this.model = model;
            IView view = new View(this);
            view.RegisterObserver(this);
            view.ShowDialog();            
        }

        public void GetChoiseResult(bool isUngerground, string projectCode, string buildingNumber)
        {
            this.isUngerground = isUngerground;
            this.projectCode = projectCode;
            this.buildingNumber = buildingNumber;
        }

        public void UpdateFromView()
        {
            model.Run(isUngerground, projectCode, buildingNumber);
        }                

        void ViewInMainThread()
        {
            IView view = new View(this);
            view.RegisterObserver(this);
            view.ShowDialog();
        }
    }
}
