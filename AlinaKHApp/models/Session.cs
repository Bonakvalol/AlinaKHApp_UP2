using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlinaKHApp.Models
{
    
        public static class Session
        {
            public static string CurrentUser { get; set; }
            public static bool IsLoggedIn => !string.IsNullOrEmpty(CurrentUser);
        }
    
}
