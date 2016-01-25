using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Microsoft.Web.Services3.Security.Tokens;
using SpatiaX.sxDocs.Interface;
using FileNet.Api.Core;
using FileNet.Api.Collection;
using FileNet.Api.Util;
using FileNet.Api.Meta;
using FileNet.Api.Property;
using FileNet.Api.Admin;
using FileNet.Api.Query;
using SirruXCloud.Helpers;
using SpatiaX.sxDocs.API.Forms;

namespace SpatiaX.sxDocs.API
{
    public static class Util
    {

        public static bool GetNullBool(bool? nullBool)
        {
            return nullBool.HasValue && nullBool.Value;
        }

        public static IClassDefinition GetDocClass(string name)
        {
            return Factory.ClassDefinition.FetchInstance(sxAPI.ObjectStore, name, null);
        }

        public static string CombineWebServiceURL(string server, string port, string folder)
        {
            string url = server + ":" + port + "/" + folder + "/";
            url = url.Replace("//", "/");

            return "http://" + url;
        }
    }
}
