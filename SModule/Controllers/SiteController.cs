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
        public async System.Threading.Tasks.Task<ActionResult> clearNotyCount(String notiToken)
        {
            await SUtils.getInstance().modifyCurrentNotyCount(notiToken, 0);
            return Json(new { message = "success" },  JsonRequestBehavior.AllowGet);
        }
    }
}