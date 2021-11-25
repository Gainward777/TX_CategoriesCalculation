using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TX_CategoriesCalculation
{
    public class ShowWarningMessage
    {
        private string title;
        private string message;

        public ShowWarningMessage(string title, string message)
        {
            this.title = title;
            this.message = message;
        }

        public void Run()
        {
            MessageBox.Show(message, title);
        }
    }
}
