using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpatiaX.sxDocs.Interface;
using FileNet.Api.Core;
using FileNet.Api.Collection;
using FileNet.Api.Util;
using FileNet.Api.Constants;

namespace SpatiaX.sxDocs.API
{
    public class sxFolder : IsxFolder
    {
        private IFolder m_fld;
        private IObjectStore m_os;

        public sxFolder(object o)
        {
            m_fld = o as IFolder;
            m_os = o as IObjectStore;
        }

        public string ID
        {
            get
            {
                return (m_os == null ? m_fld.Id.ToString() : m_os.Id.ToString());
            }
        }

        public string Name
        {
            get
            {
                return (m_os == null ? m_fld.Name : m_os.Name);
            }
        }

        public string Path
        {
            get
            {
                return (m_os == null ? m_fld.PathName : "/");
            }
        }

        public Collection<IsxFolder> SubFolders
        {
            get
            {
                Collection<IsxFolder> fldColl = new Collection<IsxFolder>();

                IFolderSet fldSet = (m_os == null ? m_fld.SubFolders : m_os.TopFolders);
                foreach (IFolder fld in fldSet)
                {
                    IsxFolder sxFld = new sxFolder(fld);
                    fldColl.Add(sxFld);
                }

                return fldColl;
            }
        }

        public Collection<IsxDocument> Documents
        {
            get
            {
                Collection<IsxDocument> docColl = new Collection<IsxDocument>();

                if (m_os == null)
                {
                    foreach (IDocument doc in m_fld.ContainedDocuments)
                    {
                        IsxDocument sxDoc;
                        if (Util.GetNullBool(doc.IsCurrentVersion))
                        {
                            sxDoc = new sxDocument(doc);
                        }
                        else
                        {
                            sxDoc = new sxDocument((IDocument)doc.CurrentVersion);
                        }

                        docColl.Add(sxDoc);
                    }
                }

                return docColl;
            }
        }

        public void AddDocument(IsxDocument doc)
        {
            IDocument p8doc = Factory.Document.FetchInstance(m_fld.GetObjectStore(), doc.ID, null);
            
            //m_fld.File(p8doc, FileNet.Api.Constants.AutoUniqueName.AUTO_UNIQUE, p8doc.Name, FileNet.Api.Constants.DefineSecurityParentage.DO_NOT_DEFINE_SECURITY_PARENTAGE);
            //m_fld.Save(FileNet.Api.Constants.RefreshMode.REFRESH);

            IDynamicReferentialContainmentRelationship rcr = Factory.DynamicReferentialContainmentRelationship.CreateInstance(m_fld.GetObjectStore(), null, AutoUniqueName.AUTO_UNIQUE,
                       DefineSecurityParentage.DO_NOT_DEFINE_SECURITY_PARENTAGE);

            rcr.Tail = m_fld;
            rcr.Head = p8doc;
            rcr.ContainmentName = Guid.NewGuid().ToString();
            rcr.Save(RefreshMode.REFRESH);
        }
    }
}
