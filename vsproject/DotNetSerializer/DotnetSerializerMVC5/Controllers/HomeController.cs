using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DotnetSerializerMVC5.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            return View();
        }

        public ViewResult LeafDefaultExample()
        {
            return View();
        }

        public ViewResult ReferenceSupportExample()
        {
            return View();
        }


	}
}