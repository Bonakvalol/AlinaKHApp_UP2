using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AlinaKHApp.Models;

namespace AlinaKHApp
{
    public partial class AddAdvertisementWindow : Window
    {
        private string _currentUserLogin;
        private DatabaseHelper _dbHelper;

      
        private List<City> _cities = new List<City>
        {
            new City { CityId = 1, CityName = "Москва" },
            new City { CityId = 2, CityName = "Санкт-Петербург" },
            new City { CityId = 3, CityName = "Екатеринбург" },
            new City { CityId = 4, CityName = "Казань" },
            new City { CityId = 5, CityName = "Новосибирск" }
        };

        private List<Category> _categories = new List<Category>
        {
            new Category { CategoryId = 1, CategoryName = "Авто" },
            new Category { CategoryId = 2, CategoryName = "Недвижимость" },
            new Category { CategoryId = 3, CategoryName = "Одежда" },
            new Category { CategoryId = 4, CategoryName = "Услуги" },
            new Category { CategoryId = 5, CategoryName = "Электроника" }
        };

        private List<AdType> _adTypes = new List<AdType>
        {
            new AdType { TypeId = 1, TypeName = "Аренда" },
            new AdType { TypeId = 2, TypeName = "Продажа" },
            new AdType { TypeId = 3, TypeName = "Услуга" }
        };

        private List<AdStatus> _adStatuses = new List<AdStatus>
        {
            new AdStatus { StatusId = 1, StatusName = "Активно" },
            new AdStatus { StatusId = 2, StatusName = "Завершено" }
        };

        public class City
        {
            public int CityId { get; set; }
            public string CityName { get; set; }
        }

        public class Category
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; }
        }

        public class AdType
        {
            public int TypeId { get; set; }
            public string TypeName { get; set; }
        }

        public class AdStatus
        {
            public int StatusId { get; set; }
            public string StatusName { get; set; }
        }

        public AddAdvertisementWindow(string currentUserLogin)
        {
            InitializeComponent();
            _currentUserLogin = currentUserLogin;
            _dbHelper = new DatabaseHelper();

            LoadComboBoxData();
        }

        private void LoadComboBoxData()
        {
            try
            {
             
                CityComboBox.ItemsSource = _cities;
                CityComboBox.DisplayMemberPath = "CityName";

              
                CategoryComboBox.ItemsSource = _categories;
                CategoryComboBox.DisplayMemberPath = "CategoryName";

               
                AdTypeComboBox.ItemsSource = _adTypes;
                AdTypeComboBox.DisplayMemberPath = "TypeName";

             
                StatusComboBox.ItemsSource = _adStatuses;
                StatusComboBox.DisplayMemberPath = "StatusName";

             
                if (StatusComboBox.Items.Count > 0)
                    StatusComboBox.SelectedIndex = 0; 

                PostDatePicker.SelectedDate = DateTime.Today;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
               
                var selectedCity = CityComboBox.SelectedItem as City;
                var selectedCategory = CategoryComboBox.SelectedItem as Category;
                var selectedAdType = AdTypeComboBox.SelectedItem as AdType;
                var selectedStatus = StatusComboBox.SelectedItem as AdStatus;

               
                var newAd = new Advertisement
                {
                    AdTitle = TitleTextBox.Text.Trim(),
                    AdDescription = DescriptionTextBox.Text.Trim(),
                    AdPostDate = PostDatePicker.SelectedDate.Value,
                    CityName = selectedCity.CityName,
                    CategoryName = selectedCategory.CategoryName,
                    TypeName = selectedAdType.TypeName,
                    StatusName = selectedStatus.StatusName,
                    Price = decimal.Parse(PriceTextBox.Text),
                    FinalPrice = string.IsNullOrEmpty(FinalPriceTextBox.Text) ?
                        null : (decimal?)decimal.Parse(FinalPriceTextBox.Text),
                    UserLogin = _currentUserLogin
                };

                if (SaveAdvertisementToDatabase(newAd, selectedCity.CityId, selectedCategory.CategoryId,
                    selectedAdType.TypeId, selectedStatus.StatusId))
                {
                    MessageBox.Show("Объявление успешно создано!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message, "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool SaveAdvertisementToDatabase(Advertisement ad, int cityId, int categoryId, int typeId, int statusId)
        {
            try
            {
                using (var connection = new SqlConnection(_dbHelper.connectionString))
                {
                    connection.Open();

                    var query = @"INSERT INTO Advertisements 
                                (ad_title, ad_description, ad_post_date, city_id, category_id, type_id, status_id, price, final_price, user_login)
                                VALUES (@title, @description, @post_date, @city_id, @category_id, @type_id, @status_id, @price, @final_price, @user_login)";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@title", ad.AdTitle);
                        command.Parameters.AddWithValue("@description", ad.AdDescription ?? "");
                        command.Parameters.AddWithValue("@post_date", ad.AdPostDate);
                        command.Parameters.AddWithValue("@city_id", cityId);
                        command.Parameters.AddWithValue("@category_id", categoryId);
                        command.Parameters.AddWithValue("@type_id", typeId);
                        command.Parameters.AddWithValue("@status_id", statusId);
                        command.Parameters.AddWithValue("@price", ad.Price);

                        if (ad.FinalPrice.HasValue)
                            command.Parameters.AddWithValue("@final_price", ad.FinalPrice.Value);
                        else
                            command.Parameters.AddWithValue("@final_price", DBNull.Value);

                        command.Parameters.AddWithValue("@user_login", ad.UserLogin);

                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка базы данных: " + ex.Message);
            }
        }

        private bool ValidateForm()
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
                errors.AppendLine("• Укажите заголовок объявления");
            else if (TitleTextBox.Text.Length < 5)
                errors.AppendLine("• Заголовок должен содержать минимум 5 символов");

            if (string.IsNullOrWhiteSpace(PriceTextBox.Text) ||
                !decimal.TryParse(PriceTextBox.Text, out decimal price) || price <= 0)
                errors.AppendLine("• Укажите корректную цену");

            if (CityComboBox.SelectedItem == null)
                errors.AppendLine("• Выберите город");

            if (CategoryComboBox.SelectedItem == null)
                errors.AppendLine("• Выберите категорию");

            if (AdTypeComboBox.SelectedItem == null)
                errors.AppendLine("• Выберите тип объявления");

            if (StatusComboBox.SelectedItem == null)
                errors.AppendLine("• Выберите статус");

   
            if (PostDatePicker.SelectedDate == null)
                errors.AppendLine("• Укажите дату публикации");
            else if (PostDatePicker.SelectedDate > DateTime.Today)
                errors.AppendLine("• Дата не может быть в будущем");

            if (!string.IsNullOrEmpty(FinalPriceTextBox.Text) &&
                !decimal.TryParse(FinalPriceTextBox.Text, out decimal finalPrice))
                errors.AppendLine("• Укажите корректную финальную цену");

            if (errors.Length > 0)
            {
                MessageBox.Show("Исправьте следующие ошибки:\n" + errors.ToString(),
                              "Ошибка заполнения", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Отменить создание объявления?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        private void PriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ValidateNumericInput(sender, e);
        }

        private void FinalPriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ValidateNumericInput(sender, e);
        }

        private void ValidateNumericInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (!char.IsDigit(e.Text, 0) && e.Text != ".")
            {
                e.Handled = true;
                return;
            }

            if (e.Text == "." && textBox.Text.Contains("."))
            {
                e.Handled = true;
            }
        }

        private void FinalPriceTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FinalPriceTextBox.Text))
                FinalPriceTextBox.Text = "";
        }
    }
}