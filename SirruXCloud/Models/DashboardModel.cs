using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using FileNet.Api.Collection;
using SpatiaX.sxDocs.Interface;

namespace SirruXCloud.Models
{
    public class DashboardModel
    {
    }

    public class ContentListFolderModel
    {
       public string FolderName { get; set; }
       public Guid Id { get; set; }
        public string FolderPath { get; set; }
        public Guid? ParentID { get; set; }
       public bool children { get; set; }

    }



    public class ContentListDocumentModel
    {
        public string DocumentName { get; set; }
        public string DocumentId { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public double DocumentFileSize { get; set; }
        public bool IsNew { get; set; }

    }

}