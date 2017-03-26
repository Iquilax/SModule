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

namespace SModule.Controllers
{
    public class SiteController : Controller
    {
        // GET: Site
        public async System.Threading.Tasks.Task<ActionResult> Index()
        {
            ChototApiWrapper products = null;
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(@"https://gateway.chotot.com/v1/public/ad-listing?region=13&cg=0&w=1&limit=20&o=0&st=a&q=galaxy%20s6&q=galaxy%20s6");
            if (response.IsSuccessStatusCode)
            {
                products = await response.Content.ReadAsAsync<ChototApiWrapper>();
            }
            return Json(products.ads,JsonRequestBehavior.AllowGet);
        }
    }
}