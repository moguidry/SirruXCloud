using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.CSharp;
using Newtonsoft.Json.Linq;
using SpatiaX.sxDocs.Interface;
using FileNet.Api.Admin;
using FileNet.Api.Core;
using FileNet.Api.Collection;
using FileNet.Api.Util;
using FileNet.Api.Constants;
using FileNet.Api.Meta;

namespace SpatiaX.sxDocs.API
{
    public class sxEntryTemplate : IsxEntryTemplate
    {
        private XmlDocument m_xmlDoc = null;
        private JObject m_jsonDoc = null;
        private IDocument m_document;
        private IsxFolder m_folder;
        private IsxDocClass m_docClass;
        private bool m_browse;
        private bool m_constrainFolder;
        private bool m_useForCheckin;
        private bool m_displayProps;
        private Dictionary<string, IsxProperty> m_props;
        private Dictionary<string, string> m_mappings;
        private bool m_majorVersion;
        private bool m_versionReadOnly;

        public sxEntryTemplate(IDocument document)
        {
            m_document = document;
        }

        public string ID
        {
            get { return m_document.Id.ToString(); }
        }

        public string Name
        {
            get { return m_document.Name; }
        }

        public bool Browse
        {
            get
            {
                LoadTemplate();

                return m_browse;
            }
        }

        public bool ConstrainFolder
        {
            get
            {
                LoadTemplate();

                return m_constrainFolder;
            }
        }

        public bool UseForCheckin
        {
            get
            {
                LoadTemplate();

                return m_useForCheckin;
            }
        }

        public bool DisplayProperties
        {
            get
            {
                LoadTemplate();

                return m_displayProps;
            }
        }

        public IsxFolder Folder
        {
            get
            {
                LoadTemplate();
 
                return m_folder;
            }
        }

        public IsxDocClass DocClass
        {
            get
            {
                LoadTemplate();

                return m_docClass;
            }
        }

        public Dictionary<string, IsxProperty> Properties
        {
            get
            {
                LoadTemplate();

                return m_props;
            }
        }

        public Dictionary<string, string> Mappings
        {
            get
            {
                //if (m_mappings == null)
                //{
                //    m_mappings = new Dictionary<string, string>();

                //    foreach (IVersionable v in m_document.Versions)
                //    {
                //        IDocument doc = (IDocument)v;

                //        foreach (IIndependentObject o in (IIndependentObjectSet)doc.Properties["sxMappings"])
                //        {
                //            ICustomObject custom = (ICustomObject)o;

                //            string dmsAtt = custom.Properties["sxDMSAtt"].ToString();
                //            string expr = custom.Properties["sxCADAtt"].ToString();

                //            if (m_mappings.ContainsKey(dmsAtt))
                //            {
                //                m_mappings[dmsAtt] = expr;
                //            }
                //            else
                //            {
                //                m_mappings.Add(dmsAtt, expr);
                //            }
                //        }
                //    }
                //}

                m_mappings = new Dictionary<string, string>();
                m_mappings.Add("Client", "V(LINE1)");
                m_mappings.Add("Description", "V(LINE2)"); 
                m_mappings.Add("Facility", "V(FACILTY/BLOCK)");
                m_mappings.Add("JobNum", "V(JOBNO)");

                return m_mappings;
            }
        }

        public bool MajorVersion
        {
            get
            {
                LoadTemplate();
                return m_majorVersion;
            }
        }

        public bool VersionReadOnly
        {
            get
            {
                LoadTemplate();
                return m_versionReadOnly;
            }
        }

        private void LoadTemplate()
        {
            IsxDocument doc = new sxDocument(m_document);
            if (doc.StringContent.StartsWith("{"))
            {
                LoadJSON();
            }
            else
            {
                LoadXML();
            }
        }

        private void LoadJSON()
        {
            if (m_jsonDoc != null)
            {
                return;
            }

            IsxDocument doc = new sxDocument(m_document);
            m_jsonDoc = JObject.Parse(doc.StringContent);

            m_browse = false;
            m_constrainFolder = (bool)((JValue)m_jsonDoc["restrictToSelectedFolderOrDescendant"]).Value;
            m_displayProps = (bool)((JValue)m_jsonDoc["allowUserSetPropertyValues"]).Value;
            m_useForCheckin = true;
            m_majorVersion = true;
            m_versionReadOnly = false;
    
            // Get the folder if it exists
            JValue jsonFolder = (JValue)m_jsonDoc["folder"];
            if (jsonFolder == null)
            {
                m_folder = null;
            }
            else
            {
                string fldID = jsonFolder.Value.ToString().Split(",".ToCharArray())[2];
                if (fldID == "")
                {
                    m_folder = null;
                }
                else
                {
                    IFolder fld = Factory.Folder.FetchInstance(sxAPI.ObjectStore, new Id(fldID), null);
                    m_folder = new sxFolder(fld);
                }
            }

            // Get the Class Desc if it exists
            JValue jsonDocClass = (JValue)m_jsonDoc["addClassName"];
            if (jsonDocClass == null)
            {
                m_docClass = null;
            }
            else
            {
                string dcID = jsonDocClass.Value.ToString();

                IClassDefinition cd = Factory.ClassDefinition.FetchInstance(sxAPI.ObjectStore, dcID, null);
                m_docClass = new sxDocClass(cd);
            }

            // Get the property Descriptions
            m_props = new Dictionary<string, IsxProperty>();
            JArray propList = (JArray)m_jsonDoc["propertiesOptions"];

            foreach (JObject jo in propList)
            {
                string symName = ((JValue)jo["id"]).Value.ToString();
                IsxProperty prop = new sxProperty(m_docClass, symName);

                if (prop.PropertyDefinition != null)
                {
                    m_props.Add(prop.Name, prop);
                }
            }
        }

        private void LoadXML()
        {
            if (m_xmlDoc != null)
            {
                return;
            }

            XmlDocument xmlDoc = new XmlDocument();
            IsxDocument doc = new sxDocument(m_document);
            xmlDoc.LoadXml(doc.StringContent);

            m_xmlDoc = new XmlDocument();
            m_xmlDoc.LoadXml(xmlDoc.FirstChild.OuterXml.Replace("xmlns", "sxns"));
 
            // Get the instructions
            XmlNode instructionList = m_xmlDoc.SelectSingleNode("//instructions");

            foreach (XmlNode instruction in instructionList.SelectNodes("instruction"))
            {
                string name = instruction.SelectSingleNode("name").InnerText;
                string value = instruction.SelectSingleNode("value").InnerText;

                switch (name.ToLower())
                {
                    case "selectfolder":
                        m_browse = (value == "1");
                        break;
                    case "setproperties":
                        m_displayProps = (value == "1");
                        break;
                    case "constrainfolder":
                        m_constrainFolder = (value == "1");
                        break;
                    case "useforcheckin":
                        m_useForCheckin = (value == "1");
                        break;
                }
            }

            // Get the major/minor version
            XmlNode versionNode = m_xmlDoc.SelectSingleNode("//version");
            m_majorVersion = (versionNode.SelectSingleNode("value").InnerText == "0");
            m_versionReadOnly = (versionNode.SelectSingleNode("isreadonly").InnerText == "1");


            // Get the folder if it exists
            XmlNode xmlFolder = m_xmlDoc.SelectSingleNode("//folder");
            if (xmlFolder == null)
            {
                m_folder = null;
            }
            else
            {
                string fldID = xmlFolder.SelectSingleNode("id").InnerText;

                if (fldID == "")
                {
                    m_folder = null;
                }
                else
                {
                    IFolder fld = Factory.Folder.FetchInstance(sxAPI.ObjectStore, new Id(fldID), null);
                    m_folder = new sxFolder(fld);
                }
            }

            // Get the Class Desc if it exists
            XmlNode xmlDocClass = m_xmlDoc.SelectSingleNode("//classdesc");
            if (xmlDocClass == null)
            {
                m_docClass = null;
            }
            else
            {
                string dcID = xmlDocClass.SelectSingleNode("id").InnerText;

                IClassDefinition cd = Factory.ClassDefinition.FetchInstance(sxAPI.ObjectStore, dcID, null);
                m_docClass = new sxDocClass(cd);
            }

            // Get the property Descriptions
            m_props = new Dictionary<string, IsxProperty>();

            XmlNode propList = m_xmlDoc.SelectSingleNode("//propdescs");

            foreach (XmlNode xmlProp in propList.SelectNodes("propdesc"))
            {
                IsxProperty prop = new sxProperty(m_docClass, xmlProp);


                if (prop.PropertyDefinition != null)
                {
                    m_props.Add(prop.Name, prop);
                }
            }
        }
    }
}
