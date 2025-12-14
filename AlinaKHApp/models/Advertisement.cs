using System;
using System.Xml.Serialization;

namespace AlinaKHApp.Models
{
    public class Advertisement
    {
        public int AdId { get; set; }
        public string AdTitle { get; set; }
        public string AdDescription { get; set; }
        public DateTime AdPostDate { get; set; }
        public string CityName { get; set; }
        public string CategoryName { get; set; }
        public string TypeName { get; set; }
        public string StatusName { get; set; }
        public decimal Price { get; set; }
        public decimal? FinalPrice { get; set; }
        public string UserLogin { get; set; }

        [XmlIgnore]
        public bool IsOwner
        {
            get
            {
                
                return this.UserLogin == Session.CurrentUser;
            }
        }

        public bool CanComplete => StatusName == "Активно";

        
        public decimal? Profit
        {
            get
            {
                return FinalPrice.HasValue ? FinalPrice.Value - Price : (decimal?)null;
            }
        }
    }
}