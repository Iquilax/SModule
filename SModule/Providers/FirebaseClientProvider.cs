using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SModule.Providers
{
    public static class FirebaseClientProvider
    {
        public static String APIKey = "AIzaSyCpxJ_gxjyoYIjQ5yA_CY-DkifK0mKV4Bk";
        public static String AuthSecret = "4T8aj25IqmeOrcwGrXw6RSJjVbLQAFuPt3oPwBvM";
        public static String BasePath = "https://awesomeproject-64f67.firebaseio.com";
        public static String SenderID = "689312660493";
        private static IFirebaseClient firebaseClient;
        public static IFirebaseClient getFirebaseClient()
        {
            if (firebaseClient == null)
            {
                IFirebaseConfig config = new FirebaseConfig
                {
                    AuthSecret = AuthSecret,
                    BasePath = BasePath
                };
                firebaseClient = new FirebaseClient(config);
            }
            return firebaseClient;
        }
    }

}