//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using PayPal.Api;

//namespace e_Book.Helpers
//{
//    public static class PayPalConfiguration
//    {
//        // Retrieve configuration from web.config
//        public static Dictionary<string, string> GetConfig()
//        {
//            return ConfigManager.Instance.GetProperties();
//        }

//        // Create APIContext
//        public static APIContext GetAPIContext()
//        {
//            var accessToken = new OAuthTokenCredential(GetConfig()).GetAccessToken();
//            return new APIContext(accessToken) { Config = GetConfig() };
//        }
//    }
//}