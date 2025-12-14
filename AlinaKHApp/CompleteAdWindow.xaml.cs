using System;
using System.Windows;

namespace AlinaKHApp
{
    public partial class CompleteAdWindow : Window
    {
        public decimal FinalPrice { get; private set; }
        private int adId;

        public CompleteAdWindow(int adId, string adTitle, decimal currentPrice)
        {
            InitializeComponent();
            this.adId = adId;

            TitleText.Text = $"Завершить: {adTitle}";
            CurrentPriceText.Text = $"Текущая цена: {currentPrice} руб.";
            FinalPriceTextBox.Text = currentPrice.ToString();
        }

        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(FinalPriceTextBox.Text, out decimal finalPrice))
            {
                FinalPrice = finalPrice;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Введите корректную цену", "Ошибка");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}