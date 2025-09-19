using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Coursework.Pages;

namespace Coursework
{
    public partial class NavigatePage2 : Page
    {
        public NavigatePage2()
        {
            InitializeComponent();
        }

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new OrdersPage());
        }

        private void CustomersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CustomersPage());
        }

        private void ShowLegend_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Легенда:\n\nАдминистратор имеет доступ ко всем таблицам базы данных.\nСотрудник — только к таблицам заказов и клиентов.",
                            "Легенда",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void ShowRoleInfo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Вы сотрудник, и вам доступны только таблицы: Заказы и Клиенты.",
                            "Информация о роли",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
    }
}
