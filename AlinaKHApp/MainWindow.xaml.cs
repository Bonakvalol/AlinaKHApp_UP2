using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AlinaKHApp.Models;

namespace AlinaKHApp
{
    public partial class MainWindow : Window
    {
        private DatabaseHelper dbHelper = new DatabaseHelper();
        private bool isLoggedIn = false;
        private string currentUser = "";
        private bool isProcessingLogin = false;

        public MainWindow()
        {
            InitializeComponent();

            UpdateLoginUI();

            LoadAdvertisements();
            InitializeFilters();

            SearchButton.Click += SearchButton_Click;
            SearchTextBox.GotFocus += SearchTextBox_GotFocus;
            SearchTextBox.LostFocus += SearchTextBox_LostFocus;
            SearchTextBox.KeyDown += SearchTextBox_KeyDown;
            LoginButton.Click += LoginButton_Click;
            ProfitButton.Click += ProfitButton_Click;
            CreateAdButton.Click += CreateAdButton_Click;
            AdsListView.MouseDoubleClick += AdsListView_MouseDoubleClick;

            CityFilter.SelectionChanged += Filter_SelectionChanged;
            CategoryFilter.SelectionChanged += Filter_SelectionChanged;
            TypeFilter.SelectionChanged += Filter_SelectionChanged;
            StatusFilter.SelectionChanged += Filter_SelectionChanged;
        }

        private void InitializeFilters()
        {
            CityFilter.Items.Add("Все");
            CityFilter.Items.Add("Москва");
            CityFilter.Items.Add("Санкт-Петербург");
            CityFilter.Items.Add("Новосибирск");
            CityFilter.Items.Add("Екатеринбург");
            CityFilter.Items.Add("Казань");
            CityFilter.SelectedIndex = 0;

            CategoryFilter.Items.Add("Все");
            CategoryFilter.Items.Add("Недвижимость");
            CategoryFilter.Items.Add("Авто");
            CategoryFilter.Items.Add("Электроника");
            CategoryFilter.Items.Add("Одежда");
            CategoryFilter.Items.Add("Услуги");
            CategoryFilter.SelectedIndex = 0;

            TypeFilter.Items.Add("Все");
            TypeFilter.Items.Add("Продажа");
            TypeFilter.Items.Add("Аренда");
            TypeFilter.Items.Add("Услуга");
            TypeFilter.SelectedIndex = 0;

            StatusFilter.Items.Add("Все");
            StatusFilter.Items.Add("Активно");
            StatusFilter.Items.Add("Завершено");
            StatusFilter.SelectedIndex = 0;
        }

        private void LoadAdvertisements()
        {
            try
            {
                string search = GetSearchText();
                string city = CityFilter.SelectedItem?.ToString();
                string category = CategoryFilter.SelectedItem?.ToString();
                string adType = TypeFilter.SelectedItem?.ToString();
                string status = StatusFilter.SelectedItem?.ToString();

                var ads = dbHelper.GetAdvertisements(search, city, category, adType, status);
                AdsListView.ItemsSource = ads;

                StatusText.Text = $"Найдено объявлений: {ads.Count}";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (isProcessingLogin) return;

            isProcessingLogin = true;

            try
            {
                if (!isLoggedIn)
                {
                    var loginWindow = new LoginWindow();
                    bool? result = loginWindow.ShowDialog();

                    if (result == true && loginWindow.IsAuthenticated)
                    {
                        isLoggedIn = true;
                        currentUser = loginWindow.UserLogin;
                        UpdateLoginUI();
                    }
                    else
                    {
                        MessageBox.Show("Вход отменен", "Информация",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    var result = MessageBox.Show("Вы действительно хотите выйти?",
                        "Подтверждение выхода",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        isLoggedIn = false;
                        currentUser = "";
                        UpdateLoginUI();

                        MessageBox.Show("Вы успешно вышли из системы", "Информация",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            finally
            {
                isProcessingLogin = false;
            }
        }

        private void UpdateLoginUI()
        {
            if (isLoggedIn && !string.IsNullOrEmpty(currentUser))
            {
                StatusText.Text = $"Вы вошли как: {currentUser}";
                LoginButton.Content = "Выйти";
                LoginButton.Background = Brushes.Red;
                LoginButton.Foreground = Brushes.White;
                CreateAdButton.IsEnabled = true;
                ProfitButton.IsEnabled = true;
            }
            else
            {
                StatusText.Text = "Гость";
                LoginButton.Content = "Войти";
                LoginButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#78909C"));
                LoginButton.Foreground = Brushes.White;
                CreateAdButton.IsEnabled = false;
                ProfitButton.IsEnabled = false;
            }
        }

        private void CreateAdButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isLoggedIn)
            {
                MessageBox.Show("Для создания объявления необходимо войти в систему",
                    "Требуется авторизация",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var addWindow = new AddAdvertisementWindow(currentUser);
            if (addWindow.ShowDialog() == true)
            {
                LoadAdvertisements();
            }
        }

        private string GetSearchText()
        {
            return SearchTextBox.Text == "Поиск..." ? "" : SearchTextBox.Text;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAdvertisements();
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAdvertisements();
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "Поиск...")
            {
                SearchTextBox.Text = "";
                SearchTextBox.Foreground = Brushes.Black;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Поиск...";
                SearchTextBox.Foreground = Brushes.Gray;
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadAdvertisements();
            }
        }

        private void ProfitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isLoggedIn)
            {
                MessageBox.Show("Для просмотра прибыли необходимо войти в систему",
                    "Требуется авторизация",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var profitWindow = new ProfitHistoryWindow(currentUser);
                profitWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AdsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (AdsListView.SelectedItem is Advertisement ad)
            {
                if (isLoggedIn && ad.UserLogin == currentUser)
                {
                    if (ad.StatusName == "Активно")
                    {
                        var completeWindow = new CompleteAdWindow(
                            ad.AdId,
                            ad.AdTitle,
                            ad.Price);

                        if (completeWindow.ShowDialog() == true)
                        {
                            bool success = dbHelper.CompleteAdvertisement(
                                ad.AdId,
                                completeWindow.FinalPrice);

                            if (success)
                            {
                                MessageBox.Show("Объявление завершено!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                                LoadAdvertisements();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Это объявление уже завершено", "Информация",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else if (!isLoggedIn)
                {
                    MessageBox.Show("Войдите в систему, чтобы завершить объявление");
                }
                else if (ad.UserLogin != currentUser)
                {
                    MessageBox.Show("Вы можете завершать только свои объявления");
                }
            }
        }

        public bool IsLoggedIn => isLoggedIn;
    }
}