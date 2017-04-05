using Facebook;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Newtonsoft.Json;
using SModule.Models;
using SModule.Providers;
using SModule.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Timers;
using System.Web;
using System.Web.Mvc;

namespace SModule.Controllers
{
    public class HomeController : Controller
    {
        public String index()
        {
            StringBuilder jsonStr = new StringBuilder("Currently crawling page:");
            jsonStr.AppendLine(JsonConvert.SerializeObject(PageTimer.getAllCrawlingPage()));
            jsonStr.AppendLine("To add crawling page : call /home/crawlface?pageid={pageid}");
            jsonStr.AppendLine("To stop crawling page : call /home/StopCrawl?pageid={pageid}");
            return jsonStr.ToString();
        }       
        public ActionResult CrawlFace(String pageId, int crawlInterval)
        {
            ViewBag.Title = "Crawling ...";
            var timer = new System.Threading.Timer(async(e) =>
            {
                await crawlFaceOneTimeAsync(pageId);
            }, null, 0, crawlInterval == 0 ? 60 * 1000 : crawlInterval);
            PageTimer.setTimer(timer, pageId);
            return Json(PageTimer.getAllCrawlingPage(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult StopCrawl(String pageId)
        {
            PageTimer.stopTimer(pageId);
            return Json(PageTimer.getAllCrawlingPage(), JsonRequestBehavior.AllowGet);
        }


        private async System.Threading.Tasks.Task<String> crawlFaceOneTimeAsync(String pageId)
        {
            try
            {

                var fb = FacebookClientProvider.getFacebookClient();
                dynamic parameters = new ExpandoObject();
                Facebook.JsonObject fbResult = fb.Get(pageId + "/feed?fields=full_picture,permalink_url,message,id,place,created_time,updated_time", parameters);
                List<PostDetailRaw> groupPosts = new List<PostDetailRaw>();
                PostDetailRaw raw = new PostDetailRaw();
                groupPosts = JsonConvert.DeserializeObject<List<PostDetailRaw>>(fbResult["data"].ToString());
                List<PostDetailParsed> resultData = new List<PostDetailParsed>();
                Dictionary<String, ProductTrack> trackedProducts = await SUtils.getInstance().getFirebase();
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
                        parseObject.price = splitedString[1].Split('-').FirstOrDefault();
                        if (parseObject.price == "FREE")
                        {
                            parseObject.price = "0";
                        }
                        else if (parseObject.price.Trim().Count() > 0)
                        {
                            parseObject.price = parseObject.price.Remove(0, 1);
                        }
                        parseObject.location = splitedString[1].Split('-').LastOrDefault();
                        parseObject.fullPicture = rawPost.fullPicture;
                    }
                    parseObject.id = rawPost.id;
                    parseObject.description = splitedString.FirstOrDefault();
                    if (splitedString.Count >= 3)
                    {
                        parseObject.description = splitedString.LastOrDefault();
                    }
                    SUtils.getInstance().mapParseObject2FirebaseObject(parseObject, "FB-" + pageId);
                    resultData.Add(parseObject);
                }
            }
            catch (Exception)
            {
                Trace.TraceError("Parse failed at:" + DateTime.Now);
            }
            return "done";
        }
        public async System.Threading.Tasks.Task<String> createProduct(String title)
        {
            IFirebaseClient client = FirebaseClientProvider.getFirebaseClient();
            var todo = new ProductTrack();
            todo.title = title;
            todo.trackedAttempts = new Dictionary<string, TrackedAttempt>();       
            PushResponse response = await client.PushAsync("products", todo);
            return response.Body;
        }
        public async System.Threading.Tasks.Task<String> generateDB(String title)
        {
            IFirebaseClient client = FirebaseClientProvider.getFirebaseClient();
            Dictionary<String, String> data = new Dictionary<string, string>();
            data.Add("FB-193618214469008", "Phones & Gadgets");
            data.Add("FB-841457799229902", "Chợ tai nghe 2hand");
            data.Add("FB-596113770526429", "GEARVN");
            data.Add("FB-351509411541454", "Chợ máy ảnh");
            SetResponse response = await client.SetAsync("locations", data);
            return response.Body;
        }
        public ActionResult sendNotify(string receiverId, string message, int count, String productId)
        {
            var result = SUtils.getInstance().SendNotification(receiverId, message, count, productId);
            return Json(result.Response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult initalCrawl()
        {
            foreach (var facebookpage in SUtils.getInstance().facebookPage)
            {
                CrawlFace(facebookpage.Key, facebookpage.Value);
            }
            
            return Redirect("/");
        }
        public async System.Threading.Tasks.Task<ActionResult> getProducts(String productName)
        {
            Dictionary<String, ProductTrack> trackedProducts = await SUtils.getInstance().getFirebase();
            if (trackedProducts != null)
            {
                foreach (var item in trackedProducts.ToList())
                {

                    if (SUtils.titleComparing(item.Value.title, productName))
                    {
                        return Json(new { name = item.Key }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
           
            String newProduct = await createProduct(productName);
            try
            {
                foreach (var facebookPage in SUtils.getInstance().facebookPage)
                {
                    String faceRes = await crawlFaceOneTimeAsync(facebookPage.Key);
                }
                String chototResult = await updateChototAsync();
            }
            catch (Exception)
            {
                //Do nothing
            }
            //In case not found

            if (newProduct == null)
            {
                return new HttpStatusCodeResult(500);
            }
            return Content(newProduct, "application/json"); ;
        }
        public async System.Threading.Tasks.Task<ActionResult> searchProduct(String productName, ProductTrack productTrack)
        {
            ChototApiWrapper products = null;
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(@"https://gateway.chotot.com/v1/public/ad-listing?region=13&cg=0&w=1&limit=20&o=0&st=a&q=" + productName.Replace(" ", "%20"));
            if (response.IsSuccessStatusCode)
            {
                products = await response.Content.ReadAsAsync<ChototApiWrapper>();
                foreach (var product in products.ads)
                {
                    PostDetailParsed productParsed = new PostDetailParsed();
                    productParsed.fullPicture = product.image;
                    productParsed.price = product.price + "";
                    productParsed.location = product.regionName;
                    productParsed.id = product.adId + "";
                    productParsed.product = product.subject;
                    productParsed.description = product.subject;
                    SUtils.getInstance().mapParseObject2FirebaseObject(productParsed, "CT");
                }
            }
            return Json(products.ads, JsonRequestBehavior.AllowGet);
        }
        public async System.Threading.Tasks.Task<String> updateChototAsync()
        {
            Dictionary<String, ProductTrack> trackedProducts = await SUtils.getInstance().getFirebase();
            foreach (var product in trackedProducts.Values)
            {
                await searchProduct(product.title, product);
            }
            return "Done";
        }
        public ActionResult crawlChotot(int interval)
        {
            ViewBag.Title = "Crawling ...";
            var timer = new System.Threading.Timer(async (e) =>
            {
                await updateChototAsync();
            }, null, 0, interval == 0 ? 60 * 1000 : interval);
            PageTimer.setTimer(timer, "CT");
            return Json(PageTimer.getAllCrawlingPage(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult stopCrawlChotot()
        {
            PageTimer.stopTimer("CT");
            return Json(PageTimer.getAllCrawlingPage(), JsonRequestBehavior.AllowGet);
        }
        //public async System.Threading.Tasks.Task<String> setFirebase()
        //{
        //    IFirebaseClient client = FirebaseClientProvider.getFirebaseClient();
        //    var todo = new ProductTrack();
        //    todo.title = "Iphone6";
        //    //todo.price = 160000000;
        //    todo.tags = new List<string> { "apple", "iphone", "smartphone" };
        //    todo.trackedPlaces = new List<string> { "FB-CHOTAINGHE", "FB-D2Q", "CHOTOT.VN" };
        //    SetResponse response = await client.SetAsync("products", todo);
        //    return response.Body;
        //}

    }
}
