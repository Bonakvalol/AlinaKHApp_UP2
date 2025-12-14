
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace AlinaKHApp.Services
{
    [Serializable]
    public class ProfitRecord
    {
        public int AdId { get; set; }
        public string AdTitle { get; set; }
        public decimal ProfitAmount { get; set; }
        public DateTime CompletionDate { get; set; }
        public string UserLogin { get; set; }
        public decimal SalePrice { get; set; } 
    }

    public static class ProfitService
    {
        private static List<ProfitRecord> _profits = new List<ProfitRecord>();
        private static readonly string _filePath = "profits.xml";

        // Статический конструктор - загружает данные при первом обращении
        static ProfitService()
        {
            LoadFromFile();
        }

        // 1. Добавить запись о прибыли
        public static void AddProfit(int adId, string adTitle, decimal profit, string userLogin, decimal salePrice)
        {
            var record = new ProfitRecord
            {
                AdId = adId,
                AdTitle = adTitle,
                ProfitAmount = profit,
                CompletionDate = DateTime.Now,
                UserLogin = userLogin,
                SalePrice = salePrice
            };

            _profits.Add(record);
            SaveToFile();
        }

        // 2. Получить прибыль по ID объявления
        public static decimal GetProfitByAdId(int adId)
        {
            return _profits
                .Where(p => p.AdId == adId)
                .Sum(p => p.ProfitAmount);
        }

        // 3. Получить общую прибыль пользователя
        public static decimal GetTotalProfitByUser(string userLogin)
        {
            return _profits
                .Where(p => p.UserLogin == userLogin)
                .Sum(p => p.ProfitAmount);
        }

        // 4. Получить историю прибыли пользователя
        public static List<ProfitRecord> GetProfitHistoryByUser(string userLogin)
        {
            return _profits
                .Where(p => p.UserLogin == userLogin)
                .OrderByDescending(p => p.CompletionDate)
                .ToList();
        }

        // 5. Получить количество завершённых сделок пользователя
        public static int GetCompletedCountByUser(string userLogin)
        {
            return _profits
                .Where(p => p.UserLogin == userLogin)
                .Count();
        }

        // 6. Проверить, завершено ли объявление
        public static bool IsAdCompleted(int adId)
        {
            return _profits.Any(p => p.AdId == adId);
        }

        // 7. Удалить запись о прибыли (если нужно)
        public static bool RemoveProfit(int adId)
        {
            var record = _profits.FirstOrDefault(p => p.AdId == adId);
            if (record != null)
            {
                _profits.Remove(record);
                SaveToFile();
                return true;
            }
            return false;
        }

        // --- Вспомогательные методы для работы с файлом ---

        private static void SaveToFile()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(List<ProfitRecord>));
                using (var writer = new StreamWriter(_filePath))
                {
                    serializer.Serialize(writer, _profits);
                }
            }
            catch (Exception ex)
            {
                // Можно добавить логирование
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения файла прибыли: {ex.Message}");
            }
        }

        private static void LoadFromFile()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var serializer = new XmlSerializer(typeof(List<ProfitRecord>));
                    using (var reader = new StreamReader(_filePath))
                    {
                        _profits = (List<ProfitRecord>)serializer.Deserialize(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                // Если файл повреждён, начинаем с чистого листа
                _profits = new List<ProfitRecord>();
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки файла прибыли: {ex.Message}");
            }
        }

        // 8. Очистить все данные (для тестирования)
        public static void ClearAllData()
        {
            _profits.Clear();
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
        }
    }
}