using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace AlinaKHApp.Models
{
    public class DatabaseHelper
    {
        internal string connectionString = @"Data Source=S\STP;Initial Catalog=IlinaSofaDB;Integrated Security=True";

        public List<Advertisement> GetAdvertisements(string search = "", string city = "", string category = "", string adType = "", string status = "")
        {
            var advertisements = new List<Advertisement>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;

                string query = @"
                    SELECT 
                        a.ad_id,
                        a.ad_title,
                        a.ad_description,
                        a.ad_post_date,
                        c.city_name,
                        cat.category_name,
                        t.type_name,
                        s.status_name,
                        a.price,
                        a.final_price,
                        u.user_login
                    FROM Advertisements a
                    INNER JOIN Cities c ON a.city_id = c.city_id
                    INNER JOIN Categories cat ON a.category_id = cat.category_id
                    INNER JOIN AdTypes t ON a.type_id = t.type_id
                    INNER JOIN AdStatuses s ON a.status_id = s.status_id
                    INNER JOIN Users u ON a.user_login = u.user_login
                    WHERE 1=1";

                if (!string.IsNullOrEmpty(search))
                {
                    query += " AND (a.ad_title LIKE '%' + @search + '%' OR a.ad_description LIKE '%' + @search + '%')";
                    command.Parameters.AddWithValue("@search", search);
                }

                if (!string.IsNullOrEmpty(city) && city != "Все")
                {
                    query += " AND c.city_name = @city";
                    command.Parameters.AddWithValue("@city", city);
                }

                if (!string.IsNullOrEmpty(category) && category != "Все")
                {
                    query += " AND cat.category_name = @category";
                    command.Parameters.AddWithValue("@category", category);
                }

                if (!string.IsNullOrEmpty(adType) && adType != "Все")
                {
                    query += " AND t.type_name = @adType";
                    command.Parameters.AddWithValue("@adType", adType);
                }

                if (!string.IsNullOrEmpty(status) && status != "Все")
                {
                    query += " AND s.status_name = @status";
                    command.Parameters.AddWithValue("@status", status);
                }

                query += " ORDER BY a.ad_post_date DESC";
                command.CommandText = query;

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var ad = new Advertisement
                        {
                            AdId = reader.GetInt32(0),
                            AdTitle = reader.GetString(1),
                            AdDescription = reader.GetString(2),
                            AdPostDate = reader.GetDateTime(3),
                            CityName = reader.GetString(4),
                            CategoryName = reader.GetString(5),
                            TypeName = reader.GetString(6),
                            StatusName = reader.GetString(7),
                            Price = reader.GetDecimal(8),
                            UserLogin = reader.GetString(10)
                        };

                        if (!reader.IsDBNull(9))
                            ad.FinalPrice = reader.GetDecimal(9);

                        advertisements.Add(ad);
                    }
                }
            }

            return advertisements;
        }

        public bool ValidateUser(string login, string password)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "SELECT COUNT(1) FROM Users WHERE user_login = @login AND user_password = @password";
                command.Parameters.AddWithValue("@login", login);
                command.Parameters.AddWithValue("@password", password);

                connection.Open();
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        public bool TestConnection()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateAdStatus(int adId, string statusName)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = @"
            UPDATE Advertisements 
            SET status_id = (SELECT status_id FROM AdStatuses WHERE status_name = @status)
            WHERE ad_id = @adId";

                command.Parameters.AddWithValue("@adId", adId);
                command.Parameters.AddWithValue("@status", statusName);

                connection.Open();
                int result = command.ExecuteNonQuery();
                return result > 0;
            }
        }

        public decimal GetUserProfit(string userLogin)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = @"
                SELECT ISNULL(SUM(final_price), 0)
                FROM Advertisements 
                WHERE user_login = @userLogin 
                  AND final_price IS NOT NULL
                  AND status_id = (SELECT status_id FROM AdStatuses WHERE status_name = 'Завершено')";

                command.Parameters.AddWithValue("@userLogin", userLogin);

                connection.Open();
                var result = command.ExecuteScalar();

                if (result != DBNull.Value && result != null)
                {
                    return Convert.ToDecimal(result);
                }
                return 0;
            }
        }

        public List<ProfitItem> GetProfitHistory(string userLogin)
        {
            var profitList = new List<ProfitItem>();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = @"
                SELECT 
                    a.ad_id,
                    a.ad_title,
                    a.ad_post_date,
                    a.price,
                    a.final_price,
                    a.final_price as profit,
                    cat.category_name,
                    c.city_name
                FROM Advertisements a
                INNER JOIN Categories cat ON a.category_id = cat.category_id
                INNER JOIN Cities c ON a.city_id = c.city_id
                WHERE a.user_login = @userLogin 
                  AND a.final_price IS NOT NULL
                  AND a.status_id = (SELECT status_id FROM AdStatuses WHERE status_name = 'Завершено')
                ORDER BY a.ad_post_date DESC";

                command.Parameters.AddWithValue("@userLogin", userLogin);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new ProfitItem
                        {
                            AdId = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Date = reader.GetDateTime(2),
                            OriginalPrice = reader.GetDecimal(3),
                            FinalPrice = reader.GetDecimal(4),
                            Profit = reader.GetDecimal(5),
                            Category = reader.GetString(6),
                            City = reader.GetString(7)
                        };
                        profitList.Add(item);
                    }
                }
            }

            return profitList;
        }

        public bool CompleteAdvertisement(int adId, decimal? finalPrice = null)
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;

                if (finalPrice.HasValue)
                {
                    command.CommandText = @"
                    UPDATE Advertisements 
                    SET status_id = (SELECT status_id FROM AdStatuses WHERE status_name = 'Завершено'),
                        final_price = @finalPrice
                    WHERE ad_id = @adId";
                    command.Parameters.AddWithValue("@finalPrice", finalPrice.Value);
                }
                else
                {
                    command.CommandText = @"
                    UPDATE Advertisements 
                    SET status_id = (SELECT status_id FROM AdStatuses WHERE status_name = 'Завершено')
                    WHERE ad_id = @adId";
                }

                command.Parameters.AddWithValue("@adId", adId);

                connection.Open();
                int result = command.ExecuteNonQuery();
                return result > 0;
            }
        }
    }

    public class ProfitItem
    {
        public int AdId { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public decimal Profit { get; set; }
        public string Category { get; set; }
        public string City { get; set; }

        public string DisplayProfit
        {
            get { return Profit.ToString("N0") + " руб."; }
        }
    }
}