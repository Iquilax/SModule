using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SModule.Providers
{
    public class FirebaseClientProvider
    {
        private static IFirebaseClient firebaseClient;
        public static IFirebaseClient getFirebaseClient()
        {
            if (firebaseClient == null)
            {
                IFirebaseConfig config = new FirebaseConfig
                {
                    AuthSecret = "BBPhctSv5RCfbrPveFBJVELilPD3A1GRz9cHJpBP",
                    BasePath = "https://trakky-d5c00.firebaseio.com"
                };
                firebaseClient = new FirebaseClient(config);
            }
            return firebaseClient;
        }
    }
}