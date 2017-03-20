using Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SModule.Providers
{
    public class FacebookClientProvider
    {
        private static FacebookClient facebookClient;
        public static FacebookClient getFacebookClient()
        {
            if (facebookClient == null)
            {
                facebookClient = new FacebookClient();
                dynamic result = facebookClient.Get("oauth/access_token", new
                {
                    client_id = "1793161387589434",
                    client_secret = "c09476d259d285c548bcf6dee1950a66",
                    grant_type = "client_credentials"
                });
                facebookClient.AccessToken = result.access_token;
            }
            return facebookClient;
        }
    }
}