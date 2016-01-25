

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using FileNet.Api.Collection;
using FileNet.Api.Core;
using FileNet.Api.Util;
using SirruXCloud.Helpers;
using SirruXCloud.Models;

namespace SirruXCloud.Controllers
{

    public class DashboardController : Controller
    {
        


        public ActionResult Index()
        {
            var userInfo = (UserSessionInfo) Session["userCredentials"];
            Debug.WriteLine("{0}::{1}::{2}",userInfo.RetrieveUserName(),userInfo.RetrievePassword(),userInfo.RetrieveDesktop());

            return View();
        }

        public JsonResult Test(Guid? id)
        {
           
            List<ContentListFolderModel> folder = GetFolders(id);
          
            var items = folder.Where(e => id.HasValue ? e.ParentID == id : e.ParentID == null).Select(e => new
            {
                id = e.Id,
                Name = e.FolderName,
                hasChildren = e.children
            });
               
            return Json(items,JsonRequestBehavior.AllowGet);
        }

        
        /// <summary>
        /// Gets a list of FolderModels for telerik tree controls, using filenet api
        /// </summary>
        /// <param name="id">GUID of parent folder</param>
        /// <returns>A list of folderModel items, if GUID is null, 
        /// it is set as a root folder, otherwise, it will be a
        /// child of a root folder.</returns>
        private List<ContentListFolderModel> GetFolders(Guid? id)
        {
            List<ContentListFolderModel> folderList = new List<ContentListFolderModel>();
            sxAPIHelper filenet = Session["filenet"] as sxAPIHelper;
            if (filenet != null)
            {
                if (id == null)
                {
                    IFolderSet folderSet = FolderHelper.GetSubFolders(FolderHelper.FetchFolder(filenet.GetObjectStore(), "/"));
                    folderList.AddRange(from IFolder folder in folderSet
                        select new ContentListFolderModel()
                        {
                            Id = new Guid(folder.Id.GetBytes()),
                            FolderName = folder.FolderName,
                            children = !folder.SubFolders.IsEmpty(),
                            ParentID = null
                        });
                }
                else
                {
                    IFolder targetFolder = Factory.Folder.FetchInstance(filenet.GetObjectStore(),new Id(id.ToString()),null );
                    IFolderSet folderSet = FolderHelper.GetSubFolders(targetFolder);
                    folderList.AddRange(from IFolder folder in folderSet
                                        select new ContentListFolderModel()
                                        {
                                            Id = new Guid(folder.Id.GetBytes()),
                                            FolderName = folder.FolderName,
                                            children = !folder.SubFolders.IsEmpty(),
                                            ParentID = new Guid(folder.Parent.Id.ToString())
                                        });
                }
            }

            return folderList;

        }
    }
}