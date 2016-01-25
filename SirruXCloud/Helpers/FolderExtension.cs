using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SirruXCloud.Models;

namespace SirruXCloud.Helpers
{
    public static class FolderExtension
    {
        public static ContentListFolderModel ToCLFM(this ContentListFolderModel folder)
        {
            return new ContentListFolderModel
            {
                Id = folder.Id,
                ParentID = folder.ParentID,
                FolderName = folder.FolderName
            };
        }
    }
}