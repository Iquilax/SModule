using FireSharp.Interfaces;
using FireSharp.Response;
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
        public static Boolean titleComparing(String firebaseTitle, String crawlTitle)
        {
            return crawlTitle.ToLower().Replace(" ", "").Contains(firebaseTitle.ToLower().Replace(" ",""));
        }
        public void initalFacebookCrawl()
        {

        }
        public async System.Threading.Tasks.Task<Dictionary<String, ProductTrack>> getFirebase()
        {
            IFirebaseClient client = FirebaseClientProvider.getFirebaseClient();
            var todo = new ProductTrack();
            FirebaseResponse response = await client.GetAsync("products");
            Dictionary<String, ProductTrack> dicResult = response.ResultAs<Dictionary<String, ProductTrack>>();
            return dicResult;
        }
        public async void mapParseObject2FirebaseObject(PostDetailParsed parseObject, String trackedPlaced)
        {
            IFirebaseClient client = FirebaseClientProvider.getFirebaseClient();
            Dictionary<String, ProductTrack> trackedProducts = await getFirebase();
            foreach (ProductTrack trackProduct in trackedProducts.Values)
            {
                if (SUtils.titleComparing(trackProduct.title, parseObject.product) && trackProduct.updates.Values.Where(a => a.id == parseObject.id).Count() == 0)
                {

                    TrackedUpdate update = new TrackedUpdate();
                    update.trackedPlaces = trackedPlaced;
                    update.price = Double.Parse(parseObject.price);
                    update.lastUpdate = DateTime.Now;
                    update.id = parseObject.id;
                    update.fullPicture = parseObject.fullPicture;
                    update.description = parseObject.description;
                    update.url = String.Format("http://www.facebook.com/{0}/posts/{1}", update.id.Split('_').FirstOrDefault(), update.id.Split('_').LastOrDefault());
                    foreach (var trackedAttempt in trackProduct.trackedAttempts.Values)
                    {
                        if (SUtils.checkMatchRequirement(trackedAttempt, update))
                        {
                            SUtils.getInstance().SendNotification(trackedAttempt.id, "Your tracked matchhh !!!");
                        }
                    }
                    List<TrackedUpdate> trackUpdates = trackProduct.updates.Values.ToList();
                    trackUpdates.Add(update);
                    trackUpdates.Sort((b, a) => (int)(a.price - b.price));
                    Dictionary<String, TrackedUpdate> updatesDictionary = new Dictionary<string, TrackedUpdate>();
                    foreach (var item in trackUpdates)
                    {
                        updatesDictionary.Add(item.id, item);
                    }
                    SetResponse setResponse = await client.SetAsync("products/" + trackedProducts.FirstOrDefault(x => x.Value == trackProduct).Key + "/updates", updatesDictionary);
                    //PushResponse response = await client.PushAsync("products/" + trackedProducts.FirstOrDefault(x => x.Value == trackProduct).Key + "/updates", update);
                }
            }
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

                string postData = "data.message=" + value + "&data.title=" + System.DateTime.Now.ToString() + "&registration_id=" + deviceId + "";

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