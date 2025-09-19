using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Coursework.Entities;


using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;

namespace Coursework.Pages
{
    public partial class OrdersPage : Page
    {
        private Order _selectedOrder;
        private ObservableCollection<Order> _allOrders;
        private ObservableCollection<Order> _filteredOrders;

        public OrdersPage()
        {
            InitializeComponent();
            LoadOrders();
            SetupFiltering();
        }

        private void LoadOrders()
        {
            try
            {
                var db = ApplicationDbContext.Instance;
                _allOrders = new ObservableCollection<Order>(db.Orders.OrderBy(o => o.OrderID).ToList());
                _filteredOrders = new ObservableCollection<Order>(_allOrders);
                OrdersDataGrid.ItemsSource = _filteredOrders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupFiltering()
        {
            try
            {
                var sortOptions = new[] {
                    new { Display = "No sorting", Value = 0 },
                    new { Display = "By Order ID (↑)", Value = 1 },
                    new { Display = "By Order ID (↓)", Value = 2 },
                    new { Display = "By Customer ID (↑)", Value = 3 },
                    new { Display = "By Customer ID (↓)", Value = 4 }
                };
                SortOrderComboBox.ItemsSource = sortOptions;
                SortOrderComboBox.DisplayMemberPath = "Display";
                SortOrderComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error setting up filters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateOrders()
        {
            try
            {
                var searchText = SearchOrderTextBox.Text?.ToLower() ?? "";
                var selectedItem = (dynamic)SortOrderComboBox.SelectedItem;

                var filtered = _allOrders
                    .Where(o => string.IsNullOrEmpty(searchText) ||
                                o.OrderID.ToString().Contains(searchText) ||
                                o.CustomerID.ToString().Contains(searchText) ||
                                o.EmployeeID.ToString().Contains(searchText) ||
                                o.TotalAmount.ToString().ToLower().Contains(searchText)); 

                if (selectedItem != null)
                {
                    switch (selectedItem.Value)
                    {
                        case 1: filtered = filtered.OrderBy(o => o.OrderID); break;
                        case 2: filtered = filtered.OrderByDescending(o => o.OrderID); break;
                        case 3: filtered = filtered.OrderBy(o => o.CustomerID); break;
                        case 4: filtered = filtered.OrderByDescending(o => o.CustomerID); break;
                    }
                }

                _filteredOrders.Clear();
                foreach (var order in filtered)
                {
                    _filteredOrders.Add(order);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating order list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SearchOrderTextBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateOrders();

        private void SortOrderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateOrders();

        private void CleanFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SearchOrderTextBox.Clear();
            SortOrderComboBox.SelectedIndex = 0;
            UpdateOrders();
        }

        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            var db = ApplicationDbContext.Instance;
            try
            {
                int totalAmount;
                if (!int.TryParse(TotalAmountTextBox.Text.Trim(), out totalAmount))
                {
                    MessageBox.Show("Invalid total amount! Must be a valid number.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var order = new Order
                {
                    CustomerID = int.Parse(CustomerIDTextBox.Text),
                    EmployeeID = int.Parse(EmployeeIDTextBox.Text),
                    TotalAmount = totalAmount  
                };

                db.Orders.Add(order);
                db.SaveChanges();

                _allOrders.Add(order);
                _filteredOrders.Add(order);
                UpdateOrders();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SaveOrderChanges_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder == null)
            {
                MessageBox.Show("Select an order to edit!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!ValidateInputs()) return;

            var db = ApplicationDbContext.Instance;
            try
            {
                var order = db.Orders.FirstOrDefault(o => o.OrderID == _selectedOrder.OrderID);
                if (order != null)
                {
                    order.CustomerID = int.Parse(CustomerIDTextBox.Text);
                    order.EmployeeID = int.Parse(EmployeeIDTextBox.Text);


                    int totalAmount;
                    if (!int.TryParse(TotalAmountTextBox.Text.Trim(), out totalAmount))
                    {
                        MessageBox.Show("Invalid total amount! Must be a valid integer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    order.TotalAmount = totalAmount;

                    db.SaveChanges();
                    var index = _allOrders.IndexOf(_selectedOrder);
                    if (index >= 0)
                    {
                        _allOrders[index] = order;
                        _filteredOrders[index] = order;
                    }
                    UpdateOrders();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ClearFields();
            _selectedOrder = null;
        }


        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(CustomerIDTextBox.Text))
            {
                MessageBox.Show("Customer ID cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!int.TryParse(CustomerIDTextBox.Text, out _))
            {
                MessageBox.Show("Invalid Customer ID! Must be an integer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmployeeIDTextBox.Text))
            {
                MessageBox.Show("Employee ID cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!int.TryParse(EmployeeIDTextBox.Text, out _))
            {
                MessageBox.Show("Invalid Employee ID! Must be an integer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(TotalAmountTextBox.Text))
            {
                MessageBox.Show("Order total cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder == null)
            {
                MessageBox.Show("Select an order to delete!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this order?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                var db = ApplicationDbContext.Instance;
                var order = db.Orders.FirstOrDefault(o => o.OrderID == _selectedOrder.OrderID);
                if (order != null)
                {
                    db.Orders.Remove(order);
                    db.SaveChanges();
                    _allOrders.Remove(order);
                    _filteredOrders.Remove(order);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            ClearFields();
            _selectedOrder = null;
        }

        private void OrdersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is Order selectedOrder)
            {
                _selectedOrder = selectedOrder;
                CustomerIDTextBox.Text = selectedOrder.CustomerID.ToString();
                EmployeeIDTextBox.Text = selectedOrder.EmployeeID.ToString();


                TotalAmountTextBox.Text = selectedOrder.TotalAmount.ToString();
            }
        }


        private void ClearFields()
        {
            CustomerIDTextBox.Clear();
            EmployeeIDTextBox.Clear();
            TotalAmountTextBox.Clear();
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new NavigatePage());
        }

        private void ExportToExcel()
        {
            try
            {
                var excelApp = new Excel.Application();
                var workBook = excelApp.Workbooks.Add();
                var workSheet = (Excel.Worksheet)workBook.Worksheets[1];


                workSheet.Cells[1, 1] = "Order ID";
                workSheet.Cells[1, 2] = "Customer ID";
                workSheet.Cells[1, 3] = "Employee ID";
                workSheet.Cells[1, 4] = "Total Amount";


                for (int i = 0; i < _filteredOrders.Count; i++)
                {
                    workSheet.Cells[i + 2, 1] = _filteredOrders[i].OrderID;
                    workSheet.Cells[i + 2, 2] = _filteredOrders[i].CustomerID;
                    workSheet.Cells[i + 2, 3] = _filteredOrders[i].EmployeeID;
                    workSheet.Cells[i + 2, 4] = _filteredOrders[i].TotalAmount;
                }


                excelApp.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            ExportToExcel();
        }

        private void ExportToWord()
        {
            try
            {
                var wordApp = new Word.Application();
                var document = wordApp.Documents.Add();

                var range = document.Range();
                range.Text = "Orders Report\n\n";

                var table = document.Tables.Add(range, _filteredOrders.Count + 1, 4);
                table.Borders.Enable = 1;


                table.Cell(1, 1).Range.Text = "Order ID";
                table.Cell(1, 2).Range.Text = "Customer ID";
                table.Cell(1, 3).Range.Text = "Employee ID";
                table.Cell(1, 4).Range.Text = "Total Amount";

                
                for (int i = 0; i < _filteredOrders.Count; i++)
                {
                    table.Cell(i + 2, 1).Range.Text = _filteredOrders[i].OrderID.ToString();
                    table.Cell(i + 2, 2).Range.Text = _filteredOrders[i].CustomerID.ToString();
                    table.Cell(i + 2, 3).Range.Text = _filteredOrders[i].EmployeeID.ToString();
                    table.Cell(i + 2, 4).Range.Text = _filteredOrders[i].TotalAmount.ToString();
                }

                wordApp.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Word: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToWord_Click(object sender, RoutedEventArgs e)
        {
            ExportToWord();
        }



    }
}
