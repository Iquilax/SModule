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
        public List<KeyValuePair<String, int>> facebookPage = new List<KeyValuePair<String, int>>();
        private static SUtils singleton;
        public static SUtils getInstance()
        {
            if (singleton == null)
            {
                singleton = new SUtils();
                singleton.facebookPage.Add(new KeyValuePair<string, int>("193618214469008", 5000));
                singleton.facebookPage.Add(new KeyValuePair<string, int>("841457799229902", 60000));
            }
            return singleton;
        }
        public static Boolean titleComparing(String firebaseTitle, String crawlTitle)
        {
            if (firebaseTitle == null || crawlTitle == null)
            {
                return false;
            }
            return crawlTitle.ToLower().Replace(" ", "").Contains(firebaseTitle.ToLower().Replace(" ", ""));
        }
        public void initalFacebookCrawl()
        {

        }
        public async System.Threading.Tasks.Task<Dictionary<String, ProductTrack>> getFirebase()
        {
            Dictionary<String, ProductTrack> dicResult;
            try
            {
                IFirebaseClient client = FirebaseClientProvider.getFirebaseClient();
                var todo = new ProductTrack();
                FirebaseResponse response = await client.GetAsync("products");
                dicResult = response.ResultAs<Dictionary<String, ProductTrack>>();
            }
            catch (Exception)
            {

                throw;
            }

            return dicResult;
        }
        public async System.Threading.Tasks.Task<int> getCurrentNotyCount(String token)
        {
            NotyCount notyCount = null;
            try
            {
                IFirebaseClient client = FirebaseClientProvider.getFirebaseClient();
                FirebaseResponse response = await client.GetAsync("noties/" + token);
                notyCount = response.ResultAs<NotyCount>();
            }
            catch (Exception)
            {

            }
            if (notyCount == null)
            {
                return 0;
            }
            return notyCount.count;
        }
        public async System.Threading.Tasks.Task<int> modifyCurrentNotyCount(String token, int modifyValue)
        {
            NotyCount notyCount = new NotyCount();
            notyCount.count = modifyValue;
            try
            {
                IFirebaseClient client = FirebaseClientProvider.getFirebaseClient();

                FirebaseResponse response = await client.SetAsync("noties/" + token, notyCount);
                notyCount = response.ResultAs<NotyCount>();
            }
            catch (Exception)
            {

                throw;
            }

            return notyCount.count;
        }
        public async void mapParseObject2FirebaseObject(PostDetailParsed parseObject, String trackedPlaced)
        {
            try
            {
                IFirebaseClient client = FirebaseClientProvider.getFirebaseClient();
                Dictionary<String, ProductTrack> trackedProducts = await getFirebase();
                foreach (KeyValuePair<String, ProductTrack> trackProductPair in trackedProducts.ToList())
                {
                    ProductTrack trackProduct = trackProductPair.Value;
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
                        if (trackedPlaced == "CT")
                        {
                            update.url = "https://www.chotot.com/toan-quoc/mua-ban?page=1&sp=0&suggested=1&q=" + trackProduct.title;
                        }
                        foreach (var trackedAttempt in trackProduct.trackedAttempts.Values)
                        {
                            if (SUtils.checkMatchRequirement(trackedAttempt, update))
                            {
                                int notyCount = await getCurrentNotyCount(trackedAttempt.id);
                                notyCount++;
                                await modifyCurrentNotyCount(trackedAttempt.id, notyCount);
                                SUtils.getInstance().SendNotification(trackedAttempt.id, String.Format("{0} is new sell at {1}₫", trackProduct.title, update.price), notyCount, trackProductPair.Key);
                            }
                        }
                        List<TrackedUpdate> trackUpdates = trackProduct.updates.Values.ToList();
                        trackUpdates.Add(update);
                        trackUpdates.Sort((b, a) => (int)(a.price - b.price));
                        Dictionary<String, TrackedUpdate> updatesDictionary = new Dictionary<string, TrackedUpdate>();
                        int idCount = 0;
                        foreach (var item in trackUpdates)
                        {
                            idCount++;
                            updatesDictionary.Add(idCount.ToString().PadLeft(4, '0') + item.id, item);
                        }
                        if (updatesDictionary.Count != trackProduct.updates.Count)
                        {
                            try
                            {
                                SetResponse setResponse = await client.SetAsync("products/" + trackedProducts.FirstOrDefault(x => x.Value == trackProduct).Key + "/updates", updatesDictionary);
                            }

                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        //PushResponse response = await client.PushAsync("products/" + trackedProducts.FirstOrDefault(x => x.Value == trackProduct).Key + "/updates", update);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public static Boolean checkMatchRequirement(TrackedAttempt trackAttempt, TrackedUpdate trackUpdate)
        {
            Boolean result = trackUpdate.price <= trackAttempt.price;
            return result;
        }
        public AndroidFCMPushNotificationStatus SendNotification(string deviceId, string message, int notyCount, String productId)
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

                string postData = "data.message=" + value + "&data.notyCount=" + notyCount + "&data.productId=" + productId + "&registration_id=" + deviceId + "&notification.click_action=OPEN_ACTIVITY";

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
        //public String getLinkChotot(ChototProduct product)
        //{
        //    String resource = ChototUrlBuilder.resource;
        //    int areaPostion = resource.IndexOf("\"" + product.areaId + "\"");
        //    int targetAreaNamePos = -1;
        //    foreach (var subStringFind in AllIndexesOf(resource,"name_url"))
        //    {
        //        if (subStringFind > areaPostion)
        //        {
        //            targetAreaNamePos = subStringFind;
        //            break;
        //        }
        //    }
        //    //String areaUrl = resource.Substring();
        //    return null;
        //}
        //public List<int> AllIndexesOf(this string str, string value)
        //{
        //    if (String.IsNullOrEmpty(value))
        //        throw new ArgumentException("the string to find may not be empty", "value");
        //    List<int> indexes = new List<int>();
        //    for (int index = 0; ; index += value.Length)
        //    {
        //        index = str.IndexOf(value, index);
        //        if (index == -1)
        //            return indexes;
        //        indexes.Add(index);
        //    }
        //}
    }
}