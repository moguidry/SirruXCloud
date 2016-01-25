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
    public class sxDocClass : IsxDocClass
    {
        private IClassDefinition m_cd;

        public sxDocClass(IClassDefinition cd)
        {
            m_cd = cd;
        }

        public string ID
        {
            get { return m_cd.Id.ToString(); }
        }

        public string Name
        {
            get { return m_cd.SymbolicName; }
        }

        public string DisplayName
        {
            get { return m_cd.Name; }
        }

        public Collection<IsxDocClass> SubClasses
        {
            get
            {
                Collection<IsxDocClass> docClasses = new Collection<IsxDocClass>();

                if (m_cd != null)
                {
                    foreach (IClassDefinition cd in m_cd.ImmediateSubclassDefinitions)
                    {
                        if (cd.IsHidden.HasValue && ! cd.IsHidden.Value)
                        {
                            IsxDocClass docClass = new sxDocClass(cd);
                            docClasses.Add(docClass);
                        }
                    }
                }

                return docClasses;
            }
        }

        public Dictionary<string, IsxPropertyDefinition> PropertyDefinitions
        {
            get
            {
                Dictionary<string, IsxPropertyDefinition> propDefs = new Dictionary<string, IsxPropertyDefinition>();

                if (m_cd != null)
                {
                    foreach (IPropertyDefinition pd in m_cd.PropertyDefinitions)
                    {
                        if (pd.IsHidden.HasValue && !pd.IsHidden.Value)
                        {
                            IsxPropertyDefinition propDesc = new sxPropertyDefinition(pd);
                            propDefs.Add(pd.SymbolicName, propDesc);
                        }
                    }
                }

                return propDefs;
            }
        }
    }
}
