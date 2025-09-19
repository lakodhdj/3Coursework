using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Coursework.Entities;

namespace Coursework
{
    public partial class RegisterWindow : Window
    {
        private ObservableCollection<User> _users;

        public RegisterWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            var db = ApplicationDbContext.Instance;
            _users = new ObservableCollection<User>(db.Users.ToList());
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(UsernameTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password) ||
                string.IsNullOrWhiteSpace(RepeatPasswordBox.Password))
            {
                MessageBox.Show("Все поля должны быть заполнены!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (PasswordBox.Password != RepeatPasswordBox.Password)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!IsValidPassword(PasswordBox.Password))
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов, хотя бы одну цифру, одну заглавную и одну строчную букву.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string hashedPassword = GetHash(PasswordBox.Password);
            var db = ApplicationDbContext.Instance;

            try
            {
                var existingUser = db.Users.FirstOrDefault(u => u.Username == UsernameTextBox.Text);
                if (existingUser != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var newUser = new User
                {
                    Username = UsernameTextBox.Text,
                    PasswordHash = hashedPassword,
                    FirstName = FirstNameTextBox.Text,
                    LastName = LastNameTextBox.Text
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                MessageBox.Show("Регистрация прошла успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Открыть LoginWindow после успешной регистрации
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                // Закрыть текущее окно регистрации
                this.Close();
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ошибка при регистрации: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nВложенная ошибка: {ex.InnerException.Message}";
                    errorMessage += $"\nСтек вызовов вложенной ошибки: {ex.InnerException.StackTrace}";
                }

                MessageBox.Show(errorMessage, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private bool IsValidPassword(string password)
        {
            return password.Length >= 6 && password.Any(char.IsDigit) && password.Any(char.IsUpper) && password.Any(char.IsLower);
        }

        private string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        private void GoToLogin(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
