using Coursework.Pages;
using System.Windows;
using System.Windows.Controls;

namespace Coursework
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new WelcomePage()); 
        }

        public void NavigateToPage(Page page)
        {
            MainFrame.Navigate(page);
        }

    }
}
