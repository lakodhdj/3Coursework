using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Coursework.Entities;

namespace Coursework.Pages
{
    public partial class ShipmentsPage : Page
    {
        private Shipment _selectedShipment;
        private ObservableCollection<Shipment> _allShipments;
        private ObservableCollection<Shipment> _filteredShipments;
        private readonly ApplicationDbContext _dbContext;

        public ShipmentsPage()
        {
            InitializeComponent();
            _dbContext = ApplicationDbContext.Instance;
            LoadShipments();
            SetupFiltering();
        }

        private void LoadShipments()
        {
            try
            {
                _allShipments = new ObservableCollection<Shipment>(_dbContext.Shipments.OrderBy(s => s.ShipmentID).ToList());
                _filteredShipments = new ObservableCollection<Shipment>(_allShipments);
                ShipmentsDataGrid.ItemsSource = _filteredShipments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке поставок: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupFiltering()
        {
            try
            {
                var sortOptions = new[]
                {
                    new { Display = "Без сортировки", Value = 0 },
                    new { Display = "По ID поставки (↑)", Value = 1 },
                    new { Display = "По ID поставки (↓)", Value = 2 },
                    new { Display = "По ID поставщика (↑)", Value = 3 },
                    new { Display = "По ID поставщика (↓)", Value = 4 }
                };

                SortShipmentComboBox.ItemsSource = sortOptions;
                SortShipmentComboBox.DisplayMemberPath = "Display";
                SortShipmentComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при настройке фильтрации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateShipments()
        {
            try
            {
                var searchText = SearchShipmentTextBox.Text?.ToLower() ?? "";
                var selectedItem = (dynamic)SortShipmentComboBox.SelectedItem;

                var filtered = _allShipments
                    .Where(s => string.IsNullOrEmpty(searchText) ||
                               s.ShipmentID.ToString().Contains(searchText) ||
                               s.SupplierID.ToString().Contains(searchText) ||
                               s.EmployeeID.ToString().Contains(searchText) ||
                               (s.TotalCost != null && s.TotalCost.ToLower().Contains(searchText)));

                if (selectedItem != null)
                {
                    switch (selectedItem.Value)
                    {
                        case 1: // ShipmentID ascending
                            filtered = filtered.OrderBy(s => s.ShipmentID);
                            break;
                        case 2: // ShipmentID descending
                            filtered = filtered.OrderByDescending(s => s.ShipmentID);
                            break;
                        case 3: // SupplierID ascending
                            filtered = filtered.OrderBy(s => s.SupplierID);
                            break;
                        case 4: // SupplierID descending
                            filtered = filtered.OrderByDescending(s => s.SupplierID);
                            break;
                        
                        default:
                            filtered = filtered.OrderBy(s => s.ShipmentID);
                            break;
                    }
                }

                _filteredShipments.Clear();
                foreach (var shipment in filtered)
                {
                    _filteredShipments.Add(shipment);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении списка поставок: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchShipmentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateShipments();
        }

        private void SortShipmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateShipments();
        }

        private void CleanFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SearchShipmentTextBox.Clear();
            SortShipmentComboBox.SelectedIndex = 0;
            UpdateShipments();
        }

        private void AddShipment_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                var shipment = new Shipment
                {
                    SupplierID = int.Parse(SupplierIDTextBox.Text),
                    EmployeeID = int.Parse(EmployeeIDTextBox.Text),
                    TotalCost = TotalCostTextBox.Text.Trim()
                };

                _dbContext.Shipments.Add(shipment);
                _dbContext.SaveChanges();

                _allShipments.Add(shipment);
                _filteredShipments.Add(shipment);
                UpdateShipments();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении поставки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveShipmentChanges_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedShipment == null)
            {
                MessageBox.Show("Выберите поставку для редактирования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!ValidateInputs()) return;

            try
            {
                var shipment = _dbContext.Shipments.Find(_selectedShipment.ShipmentID);
                if (shipment != null)
                {
                    shipment.SupplierID = int.Parse(SupplierIDTextBox.Text);
                    shipment.EmployeeID = int.Parse(EmployeeIDTextBox.Text);
                    shipment.TotalCost = TotalCostTextBox.Text.Trim();

                    _dbContext.SaveChanges();

                    var index = _allShipments.IndexOf(_selectedShipment);
                    if (index >= 0)
                    {
                        _allShipments[index] = shipment;
                        _filteredShipments[index] = shipment;
                    }

                    UpdateShipments();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ClearFields();
            _selectedShipment = null;
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(SupplierIDTextBox.Text))
            {
                MessageBox.Show("ID поставщика не может быть пустым!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!int.TryParse(SupplierIDTextBox.Text, out _))
            {
                MessageBox.Show("Некорректный ID поставщика! Должно быть целое число.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmployeeIDTextBox.Text))
            {
                MessageBox.Show("ID сотрудника не может быть пустым!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!int.TryParse(EmployeeIDTextBox.Text, out _))
            {
                MessageBox.Show("Некорректный ID сотрудника! Должно быть целое число.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(TotalCostTextBox.Text))
            {
                MessageBox.Show("Общая стоимость не может быть пустой!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void DeleteShipment_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedShipment == null)
            {
                MessageBox.Show("Выберите поставку для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить поставку?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                var shipment = _dbContext.Shipments.Find(_selectedShipment.ShipmentID);
                if (shipment != null)
                {
                    _dbContext.Shipments.Remove(shipment);
                    _dbContext.SaveChanges();
                    _allShipments.Remove(shipment);
                    _filteredShipments.Remove(shipment);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ClearFields();
            _selectedShipment = null;
        }

        private void ShipmentsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ShipmentsDataGrid.SelectedItem is Shipment selectedShipment)
            {
                _selectedShipment = selectedShipment;
                SupplierIDTextBox.Text = selectedShipment.SupplierID.ToString();
                EmployeeIDTextBox.Text = selectedShipment.EmployeeID.ToString();
                TotalCostTextBox.Text = selectedShipment.TotalCost ?? "";
            }
        }

        private void ClearFields()
        {
            SupplierIDTextBox.Clear();
            EmployeeIDTextBox.Clear();
            TotalCostTextBox.Clear();
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new NavigatePage());
        }
    }
}