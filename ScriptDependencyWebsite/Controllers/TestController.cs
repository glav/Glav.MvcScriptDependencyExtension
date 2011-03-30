using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ScriptDependencyExtensionWebsite.Controllers
{
    public class TestController : Controller
    {
        //
        // GET: /Test/

        public ActionResult Index()
        {
            return View();
        }

		public ActionResult HeadInclude()
		{
			return View("StdHeadInclude");
		}

		public ActionResult Deferred()
		{
			return View("DeferredScripts");
		}

		public ActionResult Complex()
		{
			return View();
		}

    }
}
