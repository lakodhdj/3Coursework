using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using Coursework.Entities;
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;
using System.Drawing;


namespace Coursework
{
    public partial class Page1 : Page
    {
        private readonly ApplicationDbContext _context = ApplicationDbContext.Instance;


        public Page1()
        {
            InitializeComponent();
            LoadChart();
        }

        private void LoadChart()
        {
            var products = _context.Products.ToList();
            ProductChart.Series.Clear();

            var series = new Series("Остаток на складе")
            {
                IsValueShownAsLabel = true,
                ChartType = SeriesChartType.Column,
                Font = new System.Drawing.Font("Arial", 8f),
                LabelForeColor = System.Drawing.Color.Black
            };

            foreach (var product in products)
            {
                series.Points.AddXY(product.ProductName, product.StockQuantity);
            }

            ProductChart.Series.Add(series);

            // Настройки осей
            ProductChart.ChartAreas[0].AxisX.LabelStyle.Angle = -45;
            ProductChart.ChartAreas[0].AxisX.Interval = 1;
        }


        private void ExportToExcel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var products = _context.Products.ToList();

                var excelApp = new Excel.Application();
                excelApp.SheetsInNewWorkbook = 1;
                var workbook = excelApp.Workbooks.Add(Type.Missing);
                var worksheet = (Excel.Worksheet)workbook.Sheets[1];
                worksheet.Name = "Товары";

                worksheet.Cells[1, 1] = "ID";
                worksheet.Cells[1, 2] = "Название";
                worksheet.Cells[1, 3] = "Цена";
                worksheet.Cells[1, 4] = "Остаток";

                for (int i = 0; i < products.Count; i++)
                {
                    var p = products[i];
                    worksheet.Cells[i + 2, 1] = p.ProductID;
                    worksheet.Cells[i + 2, 2] = p.ProductName;
                    worksheet.Cells[i + 2, 3] = p.Price;
                    worksheet.Cells[i + 2, 4] = p.StockQuantity;
                }

                worksheet.Columns.AutoFit();
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string fileName = System.IO.Path.Combine(path, "ProductReport.xlsx");
                workbook.SaveAs(fileName);

                excelApp.Visible = true;

                System.Windows.MessageBox.Show("Экспорт в Excel завершён!", "Успех");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }

        private void ExportToWord_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                var products = _context.Products.ToList();
                var wordApp = new Word.Application();
                var document = wordApp.Documents.Add();

                var title = document.Paragraphs.Add();
                title.Range.Text = "Отчет по остаткам товаров";
                title.Range.Font.Size = 16;
                title.Range.Font.Bold = 1;
                title.Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                title.Range.InsertParagraphAfter();

                var table = document.Tables.Add(document.Paragraphs.Add().Range, products.Count + 1, 4);
                table.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                table.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;

                string[] headers = { "ID", "Название", "Цена", "Остаток" };
                for (int i = 0; i < headers.Length; i++)
                {
                    table.Cell(1, i + 1).Range.Text = headers[i];
                    table.Cell(1, i + 1).Range.Font.Bold = 1;
                }

                for (int i = 0; i < products.Count; i++)
                {
                    var p = products[i];
                    table.Cell(i + 2, 1).Range.Text = p.ProductID.ToString();
                    table.Cell(i + 2, 2).Range.Text = p.ProductName;
                    table.Cell(i + 2, 3).Range.Text = p.Price.ToString("C");
                    table.Cell(i + 2, 4).Range.Text = p.StockQuantity.ToString();
                }

                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string fileName = System.IO.Path.Combine(path, "ProductReport.docx");
                wordApp.Visible = true;

                System.Windows.MessageBox.Show("Экспорт в Word завершён!", "Успех");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
    }
}
