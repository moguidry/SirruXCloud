using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;

using SpatiaX.sxDocs.Interface;
using FileNet.Api.Core;
using FileNet.Api.Collection;
using FileNet.Api.Util;
using FileNet.Api.Constants;
using FileNet.Api.Property;
using FileNet.Api.Meta;
using FileNet.Api.Admin;


namespace SpatiaX.sxDocs.API
{
    public class sxDocument : IsxDocument
    {
        private IDocument m_doc;
        private FileStream m_fs;
        private IsxDocClass m_docClass;
        private string m_filename;
        Dictionary<string, IsxProperty> m_props = null;

        public sxDocument(IDocument doc)
        {
            m_doc = doc;
        }

        internal IDocument GetDocument()
        {
            return m_doc;
        }

        public string ID
        {
            get
            {
                return m_doc.Id.ToString();
            }
        }

        public string Name
        {
            get
            {
                return m_doc.Name;
            }
        }

        public IsxDocClass DocClass
        {
            get
            {
                if (m_docClass == null)
                {
                    IClassDefinition classDef = Util.GetDocClass(m_doc.GetClassName());
                    m_docClass = new sxDocClass(classDef);
                }

                return m_docClass;
            }
        }

        public bool IsNew
        {
            get
            {
                try
                {
                    return (m_doc.ContentElements.Count == 0);
                }
                catch
                {
                    return true;
                }
            }
        }

        public DateTime? LastModified
        {
            get
            {
                return m_doc.DateLastModified;
            }
        }

        public double? FileSize
        {
            get
            {
                return m_doc.ContentSize;
            }
        }

        public Dictionary<string, IsxProperty> Properties
        {
            get
            {
                if (m_props == null || m_props.Count == 0)
                {
                    m_props = new Dictionary<string, IsxProperty>();

                    IClassDefinition classDef = Util.GetDocClass(m_doc.GetClassName());
                    
                    foreach (IPropertyDefinition propdef in classDef.PropertyDefinitions)
                    {
                        if (Util.GetNullBool(propdef.IsSystemOwned))
                        {
                            continue;
                        }

                        if (propdef.Settability == PropertySettability.READ_ONLY)
                        {
                            continue;
                        }

                        if (propdef.DataType == TypeID.OBJECT || propdef.DataType == TypeID.GUID || propdef.DataType == TypeID.BINARY)
                        {
                            continue;
                        }

                        if (Util.GetNullBool(propdef.IsHidden))
                        {
                            continue;
                        }

                        if (propdef.SymbolicName == "sxOriginalFileName" || propdef.SymbolicName == "sxHasParent" || propdef.SymbolicName == "sxHasChild")
                        {
                            continue;
                        }

                        //if (propdef.Cardinality == Cardinality.LIST)
                        //{
                        //    continue;
                        //}

                        IsxPropertyDefinition propDef = new sxPropertyDefinition(propdef);

                        IsxProperty prop = new sxProperty(propDef, GetAttribute(propdef.SymbolicName));
                        m_props.Add(propdef.SymbolicName, prop);
                    }
                }


                return m_props;
            }
        }

        public string Download(bool checkout)
        {
            return Download(checkout, API.Properties.Settings.Default.WorkingFolder);
        }

        public string Download(bool checkout, string folder)
        {
            try
            {
                if (checkout && ! Util.GetNullBool(m_doc.IsCurrentVersion))
                {
                    m_doc = (IDocument)m_doc.CurrentVersion;
                    m_props = null;
                }

                Stream stream = m_doc.AccessContentStream(0);
                IContentTransfer ct = m_doc.ContentElements[0] as IContentTransfer;

                if (ct == null)
                {
                    return "";
                }

                string filename = Path.Combine(folder, ct.RetrievalName);

                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, (int)0, (int)stream.Length);

                File.WriteAllBytes(filename, bytes);

                if (checkout)
                {
                    m_doc.Checkout(ReservationType.EXCLUSIVE, null, null, null);
                    m_doc.Save(RefreshMode.REFRESH);
                }

                return filename;
            }
            catch (Exception ex)
            {
              //  MessageBox.Show (ex.Message, "Error", MessageBoxButtons.OK);
                return "";
            }
        }

        public void Upload(string filename)
        {
            IContentTransfer ct = Factory.ContentTransfer.CreateInstance();
            m_filename = filename;
            m_fs = File.Open(filename, FileMode.Open, FileAccess.Read);
            ct.SetCaptureSource(m_fs);
            ct.RetrievalName = Path.GetFileName(filename);

            IContentElementList cel = Factory.ContentElement.CreateList();
            cel.Add(ct);

            string mimeType = "application/octet-stream";
            switch (Path.GetExtension(filename).ToLower())
            {
                case ".dwg":
                case ".dwt":
                    mimeType = "application/acad";
                    break;
                case ".dgn":
                    mimeType = "application.microstation";
                    break;
            }


            if (m_doc.Properties.IsPropertyPresent("IsReserved") && m_doc.IsReserved.HasValue && m_doc.IsReserved.Value)
            {
                ((IDocument)m_doc.Reservation).ContentElements = cel;
                ((IDocument)m_doc.Reservation).Properties["sxOriginalFileName"] = Path.GetFileName(filename);
                ((IDocument)m_doc.Reservation).Properties["MimeType"] = mimeType;
            }
            else
            {
                m_doc.ContentElements = cel;
                m_doc.Properties["sxOriginalFileName"] = Path.GetFileName(filename);
                m_doc.Properties["MimeType"] = mimeType;
            }


            //fs.Close();
        }

        public void Save(bool major)
        {
            if (m_doc.Properties.IsPropertyPresent("IsReserved") && m_doc.IsReserved.HasValue && m_doc.IsReserved.Value)
            {
                SaveProperties((IDocument)m_doc.Reservation);

                ((IDocument)m_doc.Reservation).Checkin(AutoClassify.DO_NOT_AUTO_CLASSIFY, major ? CheckinType.MAJOR_VERSION : CheckinType.MINOR_VERSION);
                ((IDocument)m_doc.Reservation).Save(RefreshMode.REFRESH);

                m_doc = (IDocument)m_doc.Reservation;
            }
            else
            {
                SaveProperties(m_doc);
                m_doc.Checkin(AutoClassify.DO_NOT_AUTO_CLASSIFY, major ? CheckinType.MAJOR_VERSION : CheckinType.MINOR_VERSION);
                m_doc.Save(RefreshMode.REFRESH);
            }

            sxAPI.ObjectStore.Refresh();


            if (m_fs != null)
            {
                m_fs.Close();
                m_fs = null;
            }

        }

        public bool CancelCheckout()
        {
            try
            {
                m_doc = (IDocument)m_doc.CancelCheckout();
                m_doc.Save(RefreshMode.REFRESH);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RenameCheckout(string newFilename)
        {
            try
            {
                m_doc.Properties["sxOriginalFileName"] = Path.GetFileName(newFilename);
                m_doc.Save(RefreshMode.REFRESH);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public string Filename
        {
            get
            {
                IContentTransfer ct = m_doc.ContentElements[0] as IContentTransfer;
                return ct.RetrievalName;
            }
        }

        public string Extension
        {
            get
            {
                return Path.GetExtension(Filename).ToLower();
            }
        }

        public bool IsCheckedOut
        {
            get
            {
                return (m_doc.IsReserved.HasValue && m_doc.IsReserved.Value);
            }
        }

        public object GetAttribute(string name)
        {
            IDocument doc = null;

            if (m_doc.Properties.IsPropertyPresent("IsReserved") && Util.GetNullBool(m_doc.IsReserved))
            {
                doc = (IDocument)m_doc.Reservation;
            }
            else
            {
                doc = m_doc;
            }

            if (!doc.Properties.IsPropertyPresent(name))
            {
                return null;
            }

            return doc.Properties[name];
        }

        public void SetStringAttribute(string name, string value)
        {
            if (m_doc.Properties.IsPropertyPresent("IsReserved") && m_doc.IsReserved.HasValue && m_doc.IsReserved.Value)
            {
                ((IDocument)m_doc.Reservation).Properties[name] = value;
            }
            else
            {
                m_doc.Properties[name] = value;
            }
        }

        public void AddChildDocument(IsxDocument child)
        {
            m_doc.CompoundDocumentState = CompoundDocumentState.COMPOUND_DOCUMENT;
            m_doc.Save(RefreshMode.REFRESH);
            IDocument p8Child = Factory.Document.FetchInstance(m_doc.GetObjectStore(), child.ID, null);

            IComponentRelationship cr = Factory.ComponentRelationship.CreateInstance(m_doc.GetObjectStore(), null);
            cr.ChildComponent = p8Child;
            cr.ParentComponent = m_doc;
            cr.ComponentPreventDelete = ComponentPreventDeleteAction.PREVENT_CHILD_DELETE;
            cr.ComponentRelationshipType = ComponentRelationshipType.DYNAMIC_CR;
            cr.VersionBindType = VersionBindType.LATEST_MAJOR_VERSION;
            cr.CopyToReservation = false;

            cr.Save(RefreshMode.REFRESH);
        }

        public Collection<IsxDocument> ChildDocuments
        {
            get
            {
                Collection<IsxDocument> docColl = new Collection<IsxDocument>();

                foreach (IDocument doc in m_doc.ChildDocuments)
                {
                    IsxDocument sxDoc = new sxDocument(doc);
                    docColl.Add(sxDoc);
                }

                return docColl;
            }
        }

        public Collection<IsxMarkup> Markups
        {
            get
            {
                Collection<IsxMarkup> muColl = new Collection<IsxMarkup>();

                foreach (IAnnotation ann in m_doc.Annotations)
                {
                    IsxMarkup mu = new sxMarkup(ann);
                    muColl.Add(mu);
                }

                return muColl;
            }
        }

        public IsxDocument Rendition
        {
            get
            {
                try
                {
                    foreach (IIndependentObject o in (IIndependentObjectSet)m_doc.Properties["sxDWF"])
                    {
                        IDocument rDoc = (IDocument)o;
                        return new sxDocument(rDoc);
                    }

                    return null;
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                ((sxDocument)value).P8Doc.Properties["sxDWFBaseDoc"] = m_doc;
            }
        }
            

        public string StringContent
        {
            get
            {
                try
                {
                    Stream stream = m_doc.AccessContentStream(0);
                    StreamReader reader = new StreamReader(stream);
                    string text = reader.ReadToEnd();

                    //File.AppendAllText("d:\\data\\EntryTemplate.txt", text);

                    return text;
                }
                catch (Exception ex)
                {
                   // MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                    return "";
                }
            }
        }

        public string EntryTemplateID
        {
            get
            {
                if (m_doc.Properties["EntryTemplateID"] == null)
                {
                    return "";
                }
                else
                {
                    return m_doc.Properties["EntryTemplateID"].ToString();
                }
            }
            set
            {
                m_doc.Properties["EntryTemplateId"] = new Id(value);
            }
        }

        public string MajorVersion
        {
            get
            {
                return m_doc.MajorVersionNumber.HasValue ? m_doc.MajorVersionNumber.Value.ToString() : "0";
            }
        }

        public string MinorVersion
        {
            get
            {
                return m_doc.MinorVersionNumber.HasValue ? m_doc.MinorVersionNumber.Value.ToString() : "0";
            }
        }

        private void SaveProperties(IDocument doc)
        {
            if (m_props == null)
            {
                return;
            }

            foreach (string key in m_props.Keys)
            {
                IsxProperty prop = m_props[key];

                if (prop.IsDirty)
                {
                    doc.Properties[key] = prop.Value;
                }
            }
        }

        public IDocument P8Doc
        {
            get
            {
                return m_doc;
            }
        }
    }
}
