using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Coursework.Pages
{
    public partial class NavigatePage : Page
    {
        public NavigatePage()
        {
            InitializeComponent();
            LoadStaticTableButtons();
        }

        private void LoadStaticTableButtons()
        {
            var tableNames = new List<string>
            {
                "Shipments",
                "Suppliers",
                "Categories",
                "Orders",
                "Customers",
                "Chart"
            };

            MenuPanel.ItemsSource = tableNames;
        }

        private void TableButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Content is string tableName)
            {
                NavigateToPage(tableName);
            }
        }

        private void NavigateToPage(string tableName)
        {
            switch (tableName)
            {
                case "Shipments":
                    NavigationService.Navigate(new ShipmentsPage());
                    break;
                case "Suppliers":
                    NavigationService.Navigate(new SuppliersPage());
                    break;
                case "Categories":
                    NavigationService.Navigate(new CategoriesPage());
                    break;
                case "Orders":
                    NavigationService.Navigate(new OrdersPage());
                    break;
                case "Customers":
                    NavigationService.Navigate(new CustomersPage());
                    break;
                case "Chart":
                    NavigationService.Navigate(new Page1());
                    break;


                default:
                    MessageBox.Show("Страница не найдена.");
                    break;
            }
        }

        private void ShowLegend_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Легенда:\n\nАдминистратор имеет доступ ко всем таблицам базы данных.\nСотрудник — только к таблицам заказов и клиентов.",
                            "Легенда",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
    }
}
