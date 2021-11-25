using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

// Класс заполняет таблицу в Excel для последующей передачи в качестве строительного задания

namespace TX_CategoriesCalculation
{
    public class GetExcelTable
    {
        public static void Write(SortedDictionary<int, CalculationResult> calculationResults)
        {
            Excel.Application excelApp = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;

            try
            { 
                int excelCount = 1; // для упрощения доступа к ячейкам при их заполнении (заменяет поиск следующей пустой строки)

                excelApp = new Excel.Application();
                excelApp.Visible = false;
                excelApp.SheetsInNewWorkbook = 1;
                excelApp.Workbooks.Add(Type.Missing);
                workbook = excelApp.Workbooks[1];
                worksheet = workbook.Sheets[1];

                foreach (KeyValuePair<int, CalculationResult> calculationResult in calculationResults)
                {
                    worksheet.Cells[excelCount, 1].Value2 = calculationResult.Key;
                    worksheet.Cells[excelCount, 2].Value2 = calculationResult.Value.finalCategory;
                    excelCount++;
                }

                Excel.Dialog excelAppDialog = excelApp.Dialogs[Excel.XlBuiltInDialog.xlDialogSaveAs];
                excelAppDialog.Show();
            }
            catch(Exception ex)
            {
                ShowWarningMessage showMessage = new ShowWarningMessage("Error", ex.Message + "\n\n" + ex.ToString());
                Thread newError = new Thread(new ThreadStart(showMessage.Run));
                newError.Start();
            }
            finally
            {
                if (workbook != null)
                    workbook.Close(false, Type.Missing, Type.Missing);
                    excelApp.Workbooks.Close();

                if (excelApp != null)
                {
                    excelApp.Quit();

                    Marshal.ReleaseComObject(excelApp);
                    GC.Collect();
                }
            }    
        }
    }
}
