using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Xml;
using SpatiaX.sxDocs.Interface;
using FileNet.Api.Admin;
using FileNet.Api.Core;
using FileNet.Api.Collection;
using FileNet.Api.Util;
using FileNet.Api.Constants;
using FileNet.Api.Meta;
using FileNet.Api.Property;

namespace SpatiaX.sxDocs.API
{
    public class sxProperty : IsxProperty
    {
        private IsxPropertyDefinition m_propDef;
        object m_origValue;
        object m_value;
        private bool m_req;
        private bool m_hidden;
        private bool m_readOnly;

        public sxProperty (IsxPropertyDefinition propDef, object value)
        {
            m_propDef = propDef;
            m_value = value;
            m_origValue = value;

            m_req = propDef.Required;
            m_hidden = propDef.Hidden;
            m_readOnly = false;
        }

        public sxProperty(IsxPropertyDefinition propDef)
        {
            m_propDef = propDef;
            m_value = null;
            m_origValue = null;

            m_req = propDef.Required;
            m_hidden = propDef.Hidden;
            m_readOnly = false;
        }

        public sxProperty(IsxDocClass docClass, XmlNode xml)
        {
            string symName = xml.SelectSingleNode("symname").InnerText;

            if (symName == "sxOriginalFileName" || symName == "sxHasParent" || symName == "sxHasChild")
            {
                return;
            }

            if (docClass.PropertyDefinitions.Keys.Contains(symName))
            {
                m_propDef = docClass.PropertyDefinitions[symName];

                m_origValue = null;
                string val = xml.SelectSingleNode("propdef").InnerText;
                if (!string.IsNullOrEmpty(val))
                {
                    m_value = val;
                }

                m_req = (xml.SelectSingleNode("isvalreq").InnerText == "1");
                m_hidden = (xml.SelectSingleNode("ishidden").InnerText == "1");
                m_readOnly = (xml.SelectSingleNode("isreadonly").InnerText == "1");
            }
            else
            {
            }
        }

        public sxProperty(IsxDocClass docClass, string symName)
        {
            //if (symName.ToLower() == "sxoriginalfilename" || symName == "sxhasparent" || symName == "sxhaschild")
            //{
            //    return;
            //}

            if (docClass.PropertyDefinitions.Keys.Contains(symName))
            {
                m_propDef = docClass.PropertyDefinitions[symName];

                m_origValue = null;
                string val = "";
                if (!string.IsNullOrEmpty(val))
                {
                    m_value = val;
                }

                m_req = false;
                m_hidden = false;
                m_readOnly = false;
            }
            else
            {
            }
        }
        public string ID
        {
            get { return m_propDef.ID; }
        }

        public string Name
        {
            get { return m_propDef.Name; }
        }

        public string DisplayName
        {
            get { return m_propDef.DisplayName; }
        }

        public IsxPropertyDefinition PropertyDefinition
        {
            get { return m_propDef; }
        }

        public object Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        public bool Required
        {
            get
            {
                return m_req;
            }
        }

        public bool ReadOnly
        {
            get
            {
                return m_readOnly;
            }
        }

        public bool Hidden
        {
            get
            {
                return m_hidden;
            }
        }
        
        public bool IsDirty
        {
            get
            {
                return (m_value != m_origValue);
            }
        }

        public string StringValue
        {
            get
            {
                if (m_propDef != null && m_propDef.Mulivalues)
                {
                    if (m_value == null)
                    {
                        return "";
                    }

                    string ret = "";
                    foreach (string s in (IStringList)m_value)
                    {
                        ret = ret + (ret == "" ? "" : ";") + s;
                    }

                    return ret;
                }
                else
                {
                    return m_value as string;
                }
            }
            set
            {

                switch (m_propDef.DataType.ToString())
                {
                    case "System.String":
                        if (m_propDef != null && m_propDef.Mulivalues)
                        {
                            if (m_value == null)
                            {
                                m_value = Factory.StringList.CreateList();
                            }

                            ((IStringList)m_value).Clear();

                            foreach (string s in value.Split(";".ToCharArray()))
                            {
                                ((IStringList)m_value).Add(s);
                            }
                        }
                        else
                        {
                            m_value = value;
                        }
                        break;
                    case "System.Double":
                        double tempDouble;
                        if (double.TryParse(value, out tempDouble))
                        {
                            m_value = tempDouble;
                        }
                        else
                        {
                            m_value = null;
                        }
                        break;
                    case "System.Int64":
                        int tempLong;
                        if (int.TryParse(value, out tempLong))
                        {
                            m_value = tempLong;
                        }
                        else
                        {
                            m_value = null;
                        }
                        break;
                    case "System.Guid":
                        try
                        {
                            m_value = new Id(new Guid(value));
                        }
                        catch
                        {
                            m_value = null;
                        }
                        break;
                }
            }
        }

        public DateTime? DateValue
        {
            get
            {
                return m_value as DateTime?;
            }
            set
            {
                m_value = value;
            }
        }

        public bool? BoolValue
        {
            get
            {
                return m_value as bool?;
            }
            set
            {
                m_value = value;
            }
        }
     
        public int? LongValue
        {
            get
            {
                return m_value as int?;
            }
            set
            {
                m_value = value;
            }
        }
 
        public double? DoubleValue
        {
            get
            {
                return m_value as double?;
            }
            set
            {
                m_value = value;
            }
        }

        public Guid? GuidValue
        {
            get
            {
                return m_value as Guid?;
            }
            set
            {
                m_value = value;
            }
        }

        public void UpdateForTemplate(IsxProperty tempProp, bool newDoc)
        {
            m_hidden = tempProp.Hidden;
            m_readOnly = tempProp.ReadOnly;
            m_req = tempProp.Required;

            if (newDoc && tempProp.Value != null)
            {
                Value = tempProp.Value;
            }
        }

    }
}
