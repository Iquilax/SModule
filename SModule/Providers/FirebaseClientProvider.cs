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
        public static String APIKey = "AIzaSyDbMxSJtLlpRdr9iwg0UQYeiIIoG_3yFd4";
        public static String AuthSecret = "BBPhctSv5RCfbrPveFBJVELilPD3A1GRz9cHJpBP";
        public static String BasePath = "https://trakky-d5c00.firebaseio.com";
        public static String SenderID = "907201675905";
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