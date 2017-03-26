using SModule.Models;
using SModule.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace SModule.Utils
{
    public class SUtils
    {
        private static SUtils singleton;
        public static SUtils getInstance()
        {
            if (singleton == null)
            {
                singleton = new SUtils();
            }
            return singleton;
        }
        public static Boolean checkMatchRequirement(TrackedAttempt trackAttempt, TrackedUpdate trackUpdate)
        {
            Boolean result = trackUpdate.price <= trackAttempt.price;
            return result;
        }
        public AndroidFCMPushNotificationStatus SendNotification(string deviceId, string message)
        {
            AndroidFCMPushNotificationStatus result = new AndroidFCMPushNotificationStatus();

            try
            {

                result.Successful = true;
                result.Error = null;

                var value = message;
                WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                tRequest.Method = "post";
                tRequest.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
                tRequest.Headers.Add(string.Format("Authorization: key={0}", FirebaseClientProvider.APIKey));
                tRequest.Headers.Add(string.Format("Sender: id={0}", FirebaseClientProvider.SenderID));

                string postData = "data.message=" + value + "&data.time=" + System.DateTime.Now.ToString() + "&registration_id=" + deviceId + "";

                Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                tRequest.ContentLength = byteArray.Length;

                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);

                    using (WebResponse tResponse = tRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                String sResponseFromServer = tReader.ReadToEnd();
                                result.Response = sResponseFromServer;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Successful = false;
                result.Response = null;
                result.Error = ex;
            }

            return result;
        }
    }
}