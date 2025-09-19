using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Coursework.Entities;
using System.Data.SqlClient;
using System.Data;
using System.IO;


namespace Coursework.Pages
{
    public partial class CategoriesPage : Page
    {
        private Category _selectedCategory;
        private ObservableCollection<Category> _allCategories;
        private ObservableCollection<Category> _filteredCategories;

        public CategoriesPage()
        {
            InitializeComponent();
            LoadCategories();
            SetupFiltering();
        }

        private void LoadCategories()
        {
            try
            {
                var db = ApplicationDbContext.Instance;
                _allCategories = new ObservableCollection<Category>(db.Categories.OrderBy(c => c.CategoryName).ToList());

                foreach (var category in _allCategories)
                {
                    if (!string.IsNullOrEmpty(category.ProductImage))
                    {
                        try
                        {
                            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, category.ProductImage);
                            if (File.Exists(imagePath))
                            {
                                category.ImageSource = new BitmapImage(new Uri(imagePath));
                            }
                            else
                            {
                                category.ImageSource = null;
                            }
                        }
                        catch
                        {
                            category.ImageSource = null;
                        }
                    }
                }


                _filteredCategories = new ObservableCollection<Category>(_allCategories);
                CategoriesListView.ItemsSource = _filteredCategories;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}", "Ошибка",
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
            new { Display = "По названию (A-Z)", Value = 1 },
            new { Display = "По названию (Z-A)", Value = 2 }
        };

                SortProductCategory.ItemsSource = sortOptions;
                SortProductCategory.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при настройке фильтрации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCategories()
        {
            try
            {
                var nameFilter = SearchProductName.Text?.ToLower() ?? "";
                var selectedItem = (dynamic)SortProductCategory.SelectedItem;

                var filtered = _allCategories
                    .Where(c => string.IsNullOrEmpty(nameFilter) ||
                               c.CategoryName.ToLower().Contains(nameFilter));

                if (selectedItem != null)
                {
                    switch (selectedItem.Value)
                    {
                        case 1: 
                            filtered = filtered.OrderBy(c => c.CategoryName);
                            break;
                        case 2: 
                            filtered = filtered.OrderByDescending(c => c.CategoryName);
                            break;
                        default:

                            filtered = filtered.OrderBy(c => c.CategoryName);
                            break;
                    }
                }

                _filteredCategories.Clear();
                foreach (var cat in filtered)
                {
                    _filteredCategories.Add(cat);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении списка категорий: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchProductName_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCategories();
        }

        private void SortProductCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCategories();
        }

        private void CleanFilter_OnClick(object sender, RoutedEventArgs e)
        {
            SearchProductName.Clear();
            SortProductCategory.SelectedIndex = 0;
            UpdateCategories();
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CategoryNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
                {
                    MessageBox.Show("Введите название и описание категории!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string name = CategoryNameTextBox.Text.Trim();
                string description = DescriptionTextBox.Text.Trim();
                string imageUrl = string.IsNullOrWhiteSpace(ImagePathTextBox.Text) ? null : ImagePathTextBox.Text.Trim();

                int newCategoryId = AddCategory(name, description, imageUrl);

                if (newCategoryId > 0)
                {
                    var category = new Category
                    {
                        CategoryID = newCategoryId,
                        CategoryName = name,
                        Description = description,
                        ProductImage = imageUrl
                    };

                    _allCategories.Add(category);
                    _filteredCategories.Add(category);
                    UpdateCategories(); 
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Не удалось добавить категорию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении категории: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public int AddCategory(string name, string description, string imageUrl)
        {
            using (SqlConnection connection = new SqlConnection("Your_Connection_String"))
            {
                SqlCommand command = new SqlCommand("sp_AddCategory", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@CategoryName", name);
                command.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                command.Parameters.AddWithValue("@ProductImage", string.IsNullOrEmpty(imageUrl) ? (object)DBNull.Value : imageUrl);

                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }


        private void SaveCategoryChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedCategory == null)
                {
                    MessageBox.Show("Выберите категорию для редактирования!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var db = ApplicationDbContext.Instance;
                var category = db.Categories.FirstOrDefault(c => c.CategoryID == _selectedCategory.CategoryID);
                if (category != null)
                {
                    category.CategoryName = CategoryNameTextBox.Text.Trim();
                    category.Description = DescriptionTextBox.Text.Trim();
                    category.ProductImage = string.IsNullOrWhiteSpace(ImagePathTextBox.Text) ? null : ImagePathTextBox.Text.Trim();
                    db.SaveChanges();

                    var index = _allCategories.IndexOf(_selectedCategory);
                    if (index >= 0)
                    {
                        _allCategories[index] = category;
                        _filteredCategories[index] = category;
                    }

                    UpdateCategories(); 
                }

                ClearFields();
                _selectedCategory = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedCategory == null)
                {
                    MessageBox.Show("Выберите категорию для удаления!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show("Вы уверены, что хотите удалить категорию?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                var db = ApplicationDbContext.Instance;
                var categoryId = _selectedCategory.CategoryID;

                var relatedProducts = db.Products.Where(p => p.CategoryID == categoryId).ToList();
                foreach (var product in relatedProducts)
                {
                    product.CategoryID = null;
                }
                db.SaveChanges();

                DeleteCategory(categoryId);


                _allCategories.Remove(_selectedCategory);
                _filteredCategories.Remove(_selectedCategory);

                ClearFields();
                _selectedCategory = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}\n\n{ex.InnerException?.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public void DeleteCategory(int categoryId)
        {
            using (SqlConnection connection = new SqlConnection("Your_Connection_String"))
            {
                SqlCommand command = new SqlCommand("sp_DeleteCategory", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@CategoryID", categoryId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }


        private void CategoriesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoriesListView.SelectedItem is Category selectedCategory)
            {
                _selectedCategory = selectedCategory;
                CategoryNameTextBox.Text = selectedCategory.CategoryName;
                DescriptionTextBox.Text = selectedCategory.Description;
                ImagePathTextBox.Text = selectedCategory.ProductImage;
            }
        }

        private void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                    Title = "Выберите изображение"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string sourcePath = openFileDialog.FileName;
                    string imagesFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

                    // Создай папку, если не существует
                    if (!Directory.Exists(imagesFolder))
                        Directory.CreateDirectory(imagesFolder);

                    // Имя файла
                    string fileName = System.IO.Path.GetFileName(sourcePath);
                    string destPath = System.IO.Path.Combine(imagesFolder, fileName);

                    // Копируем файл, если он ещё не существует
                    if (!File.Exists(destPath))
                        File.Copy(sourcePath, destPath);

                    ImagePathTextBox.Text = System.IO.Path.Combine("Images", fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выборе изображения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCategory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedCategory == null)
                {
                    MessageBox.Show("Выберите категорию для изменения!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(CategoryNameTextBox.Text))
                {
                    MessageBox.Show("Введите название категории!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string name = CategoryNameTextBox.Text.Trim();
                string description = string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ? null : DescriptionTextBox.Text.Trim();
                string imageUrl = string.IsNullOrWhiteSpace(ImagePathTextBox.Text) ? null : ImagePathTextBox.Text.Trim();


                UpdateCategory(_selectedCategory.CategoryID, name, description, imageUrl);

                _selectedCategory.CategoryName = name;
                _selectedCategory.Description = description;
                _selectedCategory.ProductImage = imageUrl;

                UpdateCategories();
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении категории: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UpdateCategory(int categoryId, string categoryName, string description, string productImage)
        {
            using (SqlConnection connection = new SqlConnection("Your_Connection_String"))
            {
                SqlCommand command = new SqlCommand("sp_UpdateCategory", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@CategoryID", categoryId);
                command.Parameters.AddWithValue("@CategoryName", categoryName);
                command.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                command.Parameters.AddWithValue("@ProductImage", string.IsNullOrEmpty(productImage) ? (object)DBNull.Value : productImage);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private void ClearFields()
        {
            CategoryNameTextBox.Clear();
            DescriptionTextBox.Clear();
            ImagePathTextBox.Clear();
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new NavigatePage());
        }
    }
}