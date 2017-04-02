using HtmlAgilityPack;
using SModule.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SModule.Utils;
using SModule.Providers;

namespace SModule.Controllers
{
    public class SiteController : Controller
    {
        // GET: Site
        public async System.Threading.Tasks.Task<ActionResult> searchProduct(String productName, ProductTrack productTrack)
        {
            ChototApiWrapper products = null;
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(@"https://gateway.chotot.com/v1/public/ad-listing?region=13&cg=0&w=1&limit=20&o=0&st=a&q="+productName.Replace(" ","%20"));
            if (response.IsSuccessStatusCode)
            {
                products = await response.Content.ReadAsAsync<ChototApiWrapper>();
				foreach (var product in products.ads)
				{
					PostDetailParsed productParsed = new PostDetailParsed();
					productParsed.fullPicture = product.image;
					productParsed.price = product.price+"";
					productParsed.location = product.regionName;
					productParsed.id = product.adId+"";
                    productParsed.product = productName;
                    productParsed.description = product.subject;
					SUtils.getInstance().mapParseObject2FirebaseObject(productParsed, "CT");
				}				
			}
            return Json(products.ads,JsonRequestBehavior.AllowGet);
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
	}
}