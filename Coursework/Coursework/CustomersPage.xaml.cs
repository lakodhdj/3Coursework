using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Coursework.Entities;

namespace Coursework.Pages
{
    public partial class CustomersPage : Page
    {
        private Customer _selectedCustomer;
        private ObservableCollection<Customer> _allCustomers;
        private ObservableCollection<Customer> _filteredCustomers;
        private string _previousPage;  // Store information about the previous page

        public CustomersPage(string previousPage = "")
        {
            InitializeComponent();
            _previousPage = previousPage;  // Save the previous page
            LoadCustomers();
            SetupFiltering();
        }

        private void LoadCustomers()
        {
            try
            {
                var db = ApplicationDbContext.Instance;
                _allCustomers = new ObservableCollection<Customer>(db.Customers.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToList());
                _filteredCustomers = new ObservableCollection<Customer>(_allCustomers);
                CustomersDataGrid.ItemsSource = _filteredCustomers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке клиентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupFiltering()
        {
            try
            {
                var sortOptions = new[]
                {
                    new { Display = "Без сортировки", Value = 0 },
                    new { Display = "По фамилии (A-Z)", Value = 1 },
                    new { Display = "По фамилии (Z-A)", Value = 2 },
                    new { Display = "По имени (A-Z)", Value = 3 },
                    new { Display = "По имени (Z-A)", Value = 4 }
                };

                SortCustomerComboBox.ItemsSource = sortOptions;
                SortCustomerComboBox.DisplayMemberPath = "Display";
                SortCustomerComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при настройке фильтрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCustomers()
        {
            try
            {
                var nameFilter = SearchCustomerTextBox.Text?.ToLower() ?? "";
                var selectedItem = (dynamic)SortCustomerComboBox.SelectedItem;

                var filtered = _allCustomers
                    .Where(c => string.IsNullOrEmpty(nameFilter) ||
                               c.LastName.ToLower().Contains(nameFilter) ||
                               c.FirstName.ToLower().Contains(nameFilter) ||
                               c.Email.ToLower().Contains(nameFilter) ||
                               c.Phone.ToLower().Contains(nameFilter));

                if (selectedItem != null)
                {
                    switch (selectedItem.Value)
                    {
                        case 1: // LastName A-Z
                            filtered = filtered.OrderBy(c => c.LastName).ThenBy(c => c.FirstName);
                            break;
                        case 2: // LastName Z-A
                            filtered = filtered.OrderByDescending(c => c.LastName).ThenBy(c => c.FirstName);
                            break;
                        case 3: // FirstName A-Z
                            filtered = filtered.OrderBy(c => c.FirstName).ThenBy(c => c.LastName);
                            break;
                        case 4: // FirstName Z-A
                            filtered = filtered.OrderByDescending(c => c.FirstName).ThenBy(c => c.LastName);
                            break;
                        default:
                            filtered = filtered.OrderBy(c => c.LastName).ThenBy(c => c.FirstName);
                            break;
                    }
                }

                _filteredCustomers.Clear();
                foreach (var customer in filtered)
                {
                    _filteredCustomers.Add(customer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении списка клиентов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchCustomerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCustomers();
        }

        private void SortCustomerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCustomers();
        }

        private void CleanFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SearchCustomerTextBox.Clear();
            SortCustomerComboBox.SelectedIndex = 0;
            UpdateCustomers();
        }

        private void AddCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneTextBox.Text))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var db = ApplicationDbContext.Instance;
                var customer = new Customer
                {
                    FirstName = FirstNameTextBox.Text.Trim(),
                    LastName = LastNameTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    Phone = PhoneTextBox.Text.Trim()
                };

                db.Customers.Add(customer);
                db.SaveChanges();

                _allCustomers.Add(customer);
                _filteredCustomers.Add(customer);
                UpdateCustomers();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveCustomerChanges_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                MessageBox.Show("Выберите клиента для редактирования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var db = ApplicationDbContext.Instance;
                var customer = db.Customers.FirstOrDefault(c => c.CustomerID == _selectedCustomer.CustomerID);
                if (customer != null)
                {
                    customer.FirstName = FirstNameTextBox.Text.Trim();
                    customer.LastName = LastNameTextBox.Text.Trim();
                    customer.Email = EmailTextBox.Text.Trim();
                    customer.Phone = PhoneTextBox.Text.Trim();

                    db.SaveChanges();

                    var index = _allCustomers.IndexOf(_selectedCustomer);
                    if (index >= 0)
                    {
                        _allCustomers[index] = customer;
                        _filteredCustomers[index] = customer;
                    }

                    UpdateCustomers();
                }

                ClearFields();
                _selectedCustomer = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteCustomer_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCustomer == null)
            {
                MessageBox.Show("Выберите клиента для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить клиента?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var db = ApplicationDbContext.Instance;
                var customer = db.Customers.FirstOrDefault(c => c.CustomerID == _selectedCustomer.CustomerID);
                if (customer != null)
                {
                    bool hasOrders = db.Orders.Any(o => o.CustomerID == customer.CustomerID);
                    if (hasOrders)
                    {
                        MessageBox.Show("Невозможно удалить клиента, так как у него есть связанные заказы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    db.Customers.Remove(customer);
                    db.SaveChanges();

                    _allCustomers.Remove(customer);
                    _filteredCustomers.Remove(customer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ClearFields();
            _selectedCustomer = null;
        }

        private void CustomersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomersDataGrid.SelectedItem is Customer selectedCustomer)
            {
                _selectedCustomer = selectedCustomer;
                FirstNameTextBox.Text = selectedCustomer.FirstName;
                LastNameTextBox.Text = selectedCustomer.LastName;
                EmailTextBox.Text = selectedCustomer.Email;
                PhoneTextBox.Text = selectedCustomer.Phone;
            }
        }

        private void ClearFields()
        {
            FirstNameTextBox.Clear();
            LastNameTextBox.Clear();
            EmailTextBox.Clear();
            PhoneTextBox.Clear();
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to the previous page
            if (_previousPage == "NavigatePage")
            {
                NavigationService?.Navigate(new NavigatePage());  // Admin's NavigatePage
            }
            else if (_previousPage == "NavigatePage2")
            {
                NavigationService?.Navigate(new NavigatePage2());  // Employee's NavigatePage2
            }
            else
            {
                NavigationService?.GoBack();  // Go back if no previous page info is available
            }
        }
    }
}
