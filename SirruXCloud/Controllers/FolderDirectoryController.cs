using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FileNet.Api.Core;
using FileNet.Api.Util;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.Ajax.Utilities;
using SirruXCloud.Helpers;
using SirruXCloud.Models;

namespace SirruXCloud.Controllers
{
    public partial class FolderDirectoryController : Controller
    {
        private sxAPIHelper filenetApiHelper;
        private IObjectStore objectStore;

        public FolderDirectoryController()
        {
            filenetApiHelper = (sxAPIHelper) System.Web.HttpContext.Current.Session["filenet"];
            filenetApiHelper.Login();
            objectStore = filenetApiHelper.GetObjectStore();
        }

      

    }
}