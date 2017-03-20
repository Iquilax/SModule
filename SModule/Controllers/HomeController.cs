using Facebook;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using SModule.Models;
using SModule.Providers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Web;
using System.Web.Mvc;

namespace SModule.Controllers
{
    public class HomeController : Controller
    {
        public async System.Threading.Tasks.Task<ActionResult> FaceParse()
        {
            var fb = FacebookClientProvider.getFacebookClient();
            dynamic parameters = new ExpandoObject();
            Facebook.JsonObject fbResult = fb.Get("841457799229902/feed/", parameters);
            List<PostDetailRaw> groupPosts = new List<PostDetailRaw>();
            PostDetailRaw raw = new PostDetailRaw();
            groupPosts = JsonConvert.DeserializeObject<List<PostDetailRaw>>(fbResult["data"].ToString());
            List<PostDetailParsed> resultData = new List<PostDetailParsed>();
            Dictionary<String, ProductTrack> trackedProducts = await getFirebase();
            foreach (var rawPost in groupPosts)
            {
                var parseObject = new PostDetailParsed();
                String msg = rawPost.message;
                List<String> splitedString = msg.Split('\n').ToList();
                parseObject.product = splitedString[0].Split('₫').FirstOrDefault();
                if (splitedString.Count > 1)
                {
                    parseObject.price = splitedString[1].Split('-').FirstOrDefault();
                    parseObject.location = splitedString[1].Split('-').LastOrDefault();
                }
                parseObject.id = rawPost.id;
                resultData.Add(parseObject);
            }

            return Json(resultData, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CrawlFace(String pageId)
        {
            ViewBag.Title = "Crawling ...";
            var timer = new System.Threading.Timer((e) =>
            {
                crawlFaceOneTime(pageId);
            }, null, 0, 60 * 1000);
            PageTimer.setTimer(timer, pageId);
            return Json(PageTimer.getAllCrawlingPage(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult StopCrawl(String pageId)
        {
            PageTimer.stopTimer(pageId);
            return Json(PageTimer.getAllCrawlingPage(), JsonRequestBehavior.AllowGet);
        }


        private async void crawlFaceOneTime(String pageId)
        {
            IFirebaseClient client = FirebaseClientProvider.getFirebaseClient();
            var fb = FacebookClientProvider.getFacebookClient();
            dynamic parameters = new ExpandoObject();
            Facebook.JsonObject fbResult = fb.Get(pageId + "/feed/", parameters);
            List<PostDetailRaw> groupPosts = new List<PostDetailRaw>();
            PostDetailRaw raw = new PostDetailRaw();
            groupPosts = JsonConvert.DeserializeObject<List<PostDetailRaw>>(fbResult["data"].ToString());
            List<PostDetailParsed> resultData = new List<PostDetailParsed>();
            Dictionary<String, ProductTrack> trackedProducts = await getFirebase();
            foreach (var rawPost in groupPosts)
            {
                var parseObject = new PostDetailParsed();
                String msg = rawPost.message;
                if (msg == null)
                {
                    continue;
                }
                List<String> splitedString = msg.Split('\n').ToList();
                parseObject.product = splitedString[0].Split('₫').FirstOrDefault();
                if (splitedString.Count > 1)
                {
                    parseObject.price = splitedString[1].Split('-').FirstOrDefault().Remove(0, 1);
                    parseObject.location = splitedString[1].Split('-').LastOrDefault();
                }
                parseObject.id = rawPost.id;
                foreach (ProductTrack trackProduct in trackedProducts.Values)
                {
                    if (parseObject.product.ToLower().Replace(" ","").Contains(trackProduct.title.ToLower().Replace(" ","")) && trackProduct.updates.Values.Where(a => a.id == parseObject.id).Count() == 0)
                    {
                        TrackedUpdate update = new TrackedUpdate();
                        update.trackedPlaces = "FB-"+ pageId;
                        update.price = Double.Parse(parseObject.price);
                        update.lastUpdate = DateTime.Now;
                        update.id = parseObject.id;
                        PushResponse response = await client.PushAsync("products/tracking/" + trackedProducts.FirstOrDefault(x => x.Value == trackProduct).Key + "/updates", update);
                    }
                }
                resultData.Add(parseObject);
            }
        }
        public String resultData()
        {
            var fb = new FacebookClient();
            dynamic result = fb.Get("oauth/access_token", new
            {
                client_id = "1793161387589434",
                client_secret = "c09476d259d285c548bcf6dee1950a66",
                grant_type = "client_credentials"
            });
            fb.AccessToken = result.access_token;
            dynamic parameters = new ExpandoObject();
            Facebook.JsonObject fbResult = fb.Get("841457799229902/feed/", parameters);
            return fbResult["data"].ToString();
        }
        public String result()
        {
            var fb = new FacebookClient();
            dynamic result = fb.Get("oauth/access_token", new
            {
                client_id = "1793161387589434",
                client_secret = "c09476d259d285c548bcf6dee1950a66",
                grant_type = "client_credentials"
            });
            fb.AccessToken = result.access_token;
            dynamic parameters = new ExpandoObject();
            Facebook.JsonObject fbResult = fb.Get("841457799229902/feed/", parameters);
            return fbResult.ToString();
        }
        public async System.Threading.Tasks.Task<String> testFirebase(String title)
        {
            IFirebaseClient client = FirebaseClientProvider.getFirebaseClient();
            var todo = new ProductTrack();
            todo.title = title;
            todo.price = 160000000;
            todo.tags = new List<string> { "apple", "iphone", "smartphone" };
            todo.trackedPlaces = new List<string> { "FB-CHOTAINGHE", "FB-D2Q", "CHOTOT.VN" };
            PushResponse response = await client.PushAsync("products/tracking", todo);
            return response.Body;
        }
        public async System.Threading.Tasks.Task<Dictionary<String, ProductTrack>> getFirebase()
        {
            IFirebaseClient client = FirebaseClientProvider.getFirebaseClient();
            var todo = new ProductTrack();
            FirebaseResponse response = await client.GetAsync("products/tracking");
            Dictionary<String,ProductTrack> dicResult = response.ResultAs<Dictionary<String, ProductTrack>>();
            return dicResult;
        }
        public async System.Threading.Tasks.Task<String> getRaw()
        {
            IFirebaseClient client = FirebaseClientProvider.getFirebaseClient();
            var todo = new ProductTrack();
            FirebaseResponse response = await client.GetAsync("products/tracking");
            ProductTrackContainer trackedProducts = response.ResultAs<ProductTrackContainer>();
            return response.Body;
        }
        public async System.Threading.Tasks.Task<String> setFirebase()
        {
            IFirebaseClient client = FirebaseClientProvider.getFirebaseClient();
            var todo = new ProductTrack();
            todo.title = "Iphone6";
            todo.price = 160000000;
            todo.tags = new List<string> { "apple", "iphone", "smartphone" };
            todo.trackedPlaces = new List<string> { "FB-CHOTAINGHE", "FB-D2Q", "CHOTOT.VN" };
            SetResponse response = await client.SetAsync("products/tracking", todo);
            return response.Body;
        }
    }
}
