using System.Data.SqlClient;
using System.Windows;
using AlinaKHApp.Models;

namespace AlinaKHApp
{
    public partial class RegisterWindow : Window
    {
        public string NewUserLogin { get; private set; }
        public string NewUserPassword { get; private set; }

        private DatabaseHelper dbHelper = new DatabaseHelper();

        public RegisterWindow()
        {
            InitializeComponent();
            LoginTextBox.Focus();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                StatusText.Text = "Заполните все поля";
                return;
            }

            if (password != confirmPassword)
            {
                StatusText.Text = "Пароли не совпадают";
                return;
            }

            if (login.Length < 3)
            {
                StatusText.Text = "Логин должен быть не менее 3 символов";
                return;
            }

            if (password.Length < 4)
            {
                StatusText.Text = "Пароль должен быть не менее 4 символов";
                return;
            }

            
            try
            {
                bool userExists = CheckUserExists(login);
                if (userExists)
                {
                    StatusText.Text = "Пользователь с таким логином уже существует";
                    return;
                }

                bool success = RegisterNewUser(login, password);
                if (success)
                {
                    NewUserLogin = login;
                    NewUserPassword = password;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    StatusText.Text = "Ошибка регистрации";
                }
            }
            catch (System.Exception ex)
            {
                StatusText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private bool CheckUserExists(string login)
        {
            using (var connection = new SqlConnection(dbHelper.connectionString))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "SELECT COUNT(1) FROM Users WHERE user_login = @login";
                command.Parameters.AddWithValue("@login", login);

                connection.Open();
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        private bool RegisterNewUser(string login, string password)
        {
            using (var connection = new SqlConnection(dbHelper.connectionString))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "INSERT INTO Users (user_login, user_password) VALUES (@login, @password)";
                command.Parameters.AddWithValue("@login", login);
                command.Parameters.AddWithValue("@password", password);

                connection.Open();
                int result = command.ExecuteNonQuery();
                return result > 0;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}