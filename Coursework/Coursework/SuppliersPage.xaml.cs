using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Coursework.Entities;

namespace Coursework.Pages
{
    public partial class SuppliersPage : Page
    {
        private ObservableCollection<Supplier> _allSuppliers;
        private ObservableCollection<Supplier> _filteredSuppliers;
        private Supplier _selectedSupplier;

        public SuppliersPage()
        {
            InitializeComponent();
            LoadSuppliers();
            InitializeSortOptions();
        }

        private void LoadSuppliers()
        {
            try
            {
                var db = ApplicationDbContext.Instance;
                _allSuppliers = new ObservableCollection<Supplier>(db.Suppliers.ToList());
                _filteredSuppliers = new ObservableCollection<Supplier>(_allSuppliers);
                SuppliersDataGrid.ItemsSource = _filteredSuppliers;
            }
            catch (Exception ex)
            {
                ShowError("Ошибка при загрузке поставщиков", ex);
            }
        }



        private void InitializeSortOptions()
        {
            SortComboBox.ItemsSource = new[]
            {
                new { Display = "Без сортировки", Value = 0 },
                new { Display = "По названию (А-Я)", Value = 1 },
                new { Display = "По названию (Я-А)", Value = 2 },
                new { Display = "По контакту (А-Я)", Value = 3 },
                new { Display = "По контакту (Я-А)", Value = 4 }
            };

            SortComboBox.DisplayMemberPath = "Display";
            SortComboBox.SelectedValuePath = "Value";
            SortComboBox.SelectedIndex = 0;
        }

        private void UpdateSuppliers()
        {
            try
            {
                var nameFilter = SearchSupplierTextBox.Text?.ToLower() ?? "";
                var contactFilter = SearchContactTextBox.Text?.ToLower() ?? "";
                var sortOption = (dynamic)SortComboBox.SelectedItem;

                var query = _allSuppliers.AsQueryable();

                if (!string.IsNullOrEmpty(nameFilter))
                    query = query.Where(s => s.SupplierName.ToLower().Contains(nameFilter));

                if (!string.IsNullOrEmpty(contactFilter))
                    query = query.Where(s => s.ContactName.ToLower().Contains(contactFilter));

                switch (sortOption?.Value)
                {
                    case 1: query = query.OrderBy(s => s.SupplierName); break;
                    case 2: query = query.OrderByDescending(s => s.SupplierName); break;
                    case 3: query = query.OrderBy(s => s.ContactName); break;
                    case 4: query = query.OrderByDescending(s => s.ContactName); break;
                }

                _filteredSuppliers.Clear();
                foreach (var supplier in query.ToList())
                {
                    _filteredSuppliers.Add(supplier);
                }
            }
            catch (Exception ex)
            {
                ShowError("Ошибка при фильтрации", ex);
            }
        }

        private void AddSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            string name = SupplierNameTextBox.Text.Trim();
            string contact = ContactNameTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();
            string address = AddressTextBox.Text.Trim();

            int id = AddSupplier(name, contact, phone, address);
            if (id > 0)
            {
                var supplier = new Supplier
                {
                    SupplierID = id,
                    SupplierName = name,
                    ContactName = contact,
                    Phone = phone,
                    Address = address
                };

                _allSuppliers.Add(supplier);
                _filteredSuppliers.Add(supplier);
                UpdateSuppliers();
                ClearInputs();
            }
        }

        public int AddSupplier(string name, string contact, string phone, string address)
        {
            using (SqlConnection conn = new SqlConnection("Your_Connection_String"))
            {
                SqlCommand cmd = new SqlCommand("sp_AddSupplier", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SupplierName", name);
                cmd.Parameters.AddWithValue("@ContactName", contact);
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@Address", address);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void SaveSupplierChanges_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSupplier == null || !ValidateInputs()) return;

            string name = SupplierNameTextBox.Text.Trim();
            string contact = ContactNameTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();
            string address = AddressTextBox.Text.Trim();

            UpdateSupplier(_selectedSupplier.SupplierID, name, contact, phone, address);

            _selectedSupplier.SupplierName = name;
            _selectedSupplier.ContactName = contact;
            _selectedSupplier.Phone = phone;
            _selectedSupplier.Address = address;

            UpdateSuppliers();
            ClearInputs();
        }

        public void UpdateSupplier(int id, string name, string contact, string phone, string address)
        {
            using (SqlConnection conn = new SqlConnection("Your_Connection_String"))
            {
                SqlCommand cmd = new SqlCommand("sp_UpdateSupplier", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SupplierID", id);
                cmd.Parameters.AddWithValue("@SupplierName", name);
                cmd.Parameters.AddWithValue("@ContactName", contact);
                cmd.Parameters.AddWithValue("@Phone", phone);
                cmd.Parameters.AddWithValue("@Address", address);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void DeleteSupplier_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSupplier == null) return;

            var result = MessageBox.Show($"Удалить поставщика {_selectedSupplier.SupplierName}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                DeleteSupplier(_selectedSupplier.SupplierID);
                _allSuppliers.Remove(_selectedSupplier);
                _filteredSuppliers.Remove(_selectedSupplier);
                ClearInputs();
            }
        }

        public void DeleteSupplier(int id)
        {
            using (SqlConnection conn = new SqlConnection("DefaultConnection"))
            {
                SqlCommand cmd = new SqlCommand("sp_DeleteSupplier", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@SupplierID", id);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void SuppliersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedSupplier = SuppliersDataGrid.SelectedItem as Supplier;
            if (_selectedSupplier != null)
            {
                SupplierNameTextBox.Text = _selectedSupplier.SupplierName;
                ContactNameTextBox.Text = _selectedSupplier.ContactName;
                PhoneTextBox.Text = _selectedSupplier.Phone;
                AddressTextBox.Text = _selectedSupplier.Address;

                SaveButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
                AddButton.IsEnabled = false;
            }
            else
            {
                ClearInputs();
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(SupplierNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(ContactNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                string.IsNullOrWhiteSpace(AddressTextBox.Text))
            {
                MessageBox.Show("Все поля должны быть заполнены!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void ClearInputs()
        {
            SupplierNameTextBox.Clear();
            ContactNameTextBox.Clear();
            PhoneTextBox.Clear();
            AddressTextBox.Clear();
            SuppliersDataGrid.SelectedItem = null;
            _selectedSupplier = null;

            AddButton.IsEnabled = true;
            SaveButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
        }

        private void ShowError(string message, Exception ex)
        {
            MessageBox.Show($"{message}:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchSupplierTextBox.Clear();
            SearchContactTextBox.Clear();
            SortComboBox.SelectedIndex = 0;
            UpdateSuppliers();
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new NavigatePage());
        }

        private void SearchSupplierTextBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateSuppliers();
        private void SearchContactTextBox_TextChanged(object sender, TextChangedEventArgs e) => UpdateSuppliers();
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateSuppliers();
    }
}
