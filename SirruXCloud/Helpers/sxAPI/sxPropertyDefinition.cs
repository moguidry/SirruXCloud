using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SpatiaX.sxDocs.Interface;
using FileNet.Api.Admin;
using FileNet.Api.Core;
using FileNet.Api.Collection;
using FileNet.Api.Util;
using FileNet.Api.Constants;
using FileNet.Api.Meta;

namespace SpatiaX.sxDocs.API
{
    public class sxPropertyDefinition : IsxPropertyDefinition
    {
        private IPropertyDefinition m_def;
        private IPropertyDescription m_desc;
        private IPropertyTemplate m_temp;

        private string m_id = "";
        private string m_displayName = "";
        private string m_name = "";
        private Cardinality m_cardinality;
        private FileNet.Api.Admin.IChoiceList m_choiceList;
        private TypeID m_typeID;
        private bool? m_hidden;
        private bool? m_required;

        public sxPropertyDefinition(IPropertyDefinition pd)
        {
            m_def = pd;
            m_desc = null;
            m_temp = null;

            m_id = m_def.Id.ToString();
            m_displayName = m_def.DisplayName;
            m_name = m_def.SymbolicName;
            m_cardinality = m_def.Cardinality;
            m_choiceList = m_def.ChoiceList;
            m_typeID = m_def.DataType;
            m_hidden = m_def.IsHidden;
            m_required = m_def.IsValueRequired;
        }

        public sxPropertyDefinition(IPropertyDescription pd)
        {
            m_def = null;
            m_desc = pd;
            m_temp = null;

            m_id = m_desc.Id.ToString();
            m_displayName = m_desc.DisplayName;
            m_name = m_desc.SymbolicName;
            m_cardinality = m_desc.Cardinality;
            m_choiceList = m_desc.ChoiceList;
            m_typeID = m_desc.DataType;
            m_hidden = m_desc.IsHidden;
            m_required = m_desc.IsValueRequired;
        }

        public sxPropertyDefinition(IPropertyTemplate pt)
        {
            m_def = null;
            m_desc = null;
            m_temp = pt;

            m_id = m_temp.Id.ToString();
            m_displayName = m_temp.DisplayName;
            m_name = m_temp.SymbolicName;
            m_cardinality = m_temp.Cardinality;
            m_choiceList = m_temp.ChoiceList;
            m_typeID = m_temp.DataType;
            m_hidden = m_temp.IsHidden;
            m_required = m_temp.IsValueRequired;
        }

        public string ID
        {
            get { return m_id; }
        }

        public string DisplayName
        {
            get { return m_displayName; }
        }

        public string Name
        {
            get { return m_name; }
        }
        public bool Mulivalues
        {
	        get 
            { 
                return (m_cardinality != Cardinality.SINGLE);
            }
        }

        public Collection<object>  ChoiceList
        {
	        get 
            {
                if (m_choiceList == null)
                {
                    return null;
                }

                Collection<object> choices = new Collection<object>();

                foreach (IChoice choice in m_choiceList.ChoiceValues)
                {
                    switch (choice.ChoiceType)
                    {
                        case ChoiceType.INTEGER:
                        case ChoiceType.MIDNODE_INTEGER:
                            choices.Add (choice.ChoiceIntegerValue);
                            break;
                        case ChoiceType.STRING:
                        case ChoiceType.MIDNODE_STRING:
                            choices.Add(choice.ChoiceStringValue);
                            break;
                    }
                       
              
                }

                return choices; 
            }
        }

        public Type DataType
        {
            get
            {
                Type dataType = null;

                switch (m_typeID)
                {
                    case TypeID.BOOLEAN:
                        dataType = typeof(bool);
                        break;
                    case TypeID.DATE:
                        dataType = typeof(DateTime);
                        break;
                    case TypeID.DOUBLE:
                        dataType = typeof(double);
                        break;
                    case TypeID.GUID:
                        dataType = typeof(Guid);
                        break;
                    case TypeID.LONG:
                        dataType = typeof(long);
                        break;
                    case TypeID.STRING:
                        dataType = typeof(string);
                        break;
                    default:
                        dataType = null;
                        break;
                }

                return dataType;
            }
        }

        public bool Hidden
        {
	        get 
            {
                return (m_hidden.HasValue && m_hidden.Value); 
            }
        }

        public bool  Required
        {
	        get 
            {
                return (m_required.HasValue && m_required.Value); 
            }
        }
    }
}
