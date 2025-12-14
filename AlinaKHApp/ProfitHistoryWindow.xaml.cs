using System;
using System.Collections.Generic;
using System.Windows;
using AlinaKHApp.Models;

namespace AlinaKHApp
{
    public partial class ProfitHistoryWindow : Window
    {
        private string userLogin;
        private DatabaseHelper dbHelper = new DatabaseHelper();

        public ProfitHistoryWindow(string userLogin)
        {
            InitializeComponent();
            this.userLogin = userLogin;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProfitData();
        }

        private void LoadProfitData()
        {
            try
            {
                
                List<ProfitItem> profitItems = dbHelper.GetProfitHistory(userLogin);

                
                List<ProfitDisplayItem> displayItems = new List<ProfitDisplayItem>();
                decimal totalProfit = 0;

                foreach (var item in profitItems)
                {
                    displayItems.Add(new ProfitDisplayItem
                    {
                        AdTitle = item.Title,
                        Price = item.OriginalPrice,
                        FinalPrice = item.FinalPrice,
                        Profit = item.Profit,
                        CompletionDate = item.Date
                    });

                    totalProfit += item.Profit;
                }

             
                ProfitListView.ItemsSource = displayItems;

               
                TotalProfitText.Text = $"{totalProfit:N0} руб.";
                CompletedCountText.Text = $"Завершено сделок: {displayItems.Count}";

                this.Title = $"Прибыль (Всего: {totalProfit:N0} руб.)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

   
    public class ProfitDisplayItem
    {
        public string AdTitle { get; set; }
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal Profit { get; set; }
        public DateTime CompletionDate { get; set; }
    }
}