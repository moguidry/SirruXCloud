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

namespace SpatiaX.sxDocs.API
{
    public class sxMarkup : IsxMarkup
    {
        IAnnotation m_ann;

        public sxMarkup(IAnnotation ann)
        {
            m_ann = ann;
        }

        public string ID
        {
            get { return m_ann.Id.ToString(); }
        }

        public string Name
        {
            get 
            { 
                return Path.GetFileNameWithoutExtension(((IContentTransfer)m_ann.ContentElements[0]).RetrievalName); 
            }
        }

        public DateTime? LastModified
        {
            get { return m_ann.DateLastModified; }
        }

        public double? FileSize
        {
            get { return m_ann.ContentSize; }
        }

        public string Content
        {
            get
            {
                try
                {
                    Stream stream = m_ann.AccessContentStream(0);
                    StreamReader reader = new StreamReader(stream);
                    string text = reader.ReadToEnd();

                    return text;
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK);
                    return "";
                }
            }
        }

        public string Creator
        {
            get
            {
                return m_ann.Creator;
            }
        }
    }
}
