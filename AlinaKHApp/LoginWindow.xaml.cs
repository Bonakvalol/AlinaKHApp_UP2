using System.Windows;
using AlinaKHApp.Models;

namespace AlinaKHApp
{
    public partial class LoginWindow : Window
    {
        public bool IsAuthenticated { get; private set; }
        public string UserLogin { get; private set; }

        private DatabaseHelper dbHelper = new DatabaseHelper();

        public LoginWindow()
        {
            InitializeComponent();
            LoginTextBox.Focus();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                StatusText.Text = "Заполните все поля";
                return;
            }

            try
            {
                bool isValid = dbHelper.ValidateUser(login, password);

                if (isValid)
                {
                    UserLogin = login;
                    IsAuthenticated = true;
                    DialogResult = true;
                    this.Close();
                }
                else
                {
                    StatusText.Text = "Неверный логин или пароль";
                    PasswordBox.Password = "";
                    PasswordBox.Focus();
                }
            }
            catch (System.Exception ex)
            {
                StatusText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            if (registerWindow.ShowDialog() == true)
            {
                LoginTextBox.Text = registerWindow.NewUserLogin;
                PasswordBox.Password = registerWindow.NewUserPassword;
                PasswordBox.Focus();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F1)
            {
                LoginTextBox.Text = "user1";
                PasswordBox.Password = "pass1";
            }
            base.OnKeyDown(e);
        }
    }
}