using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using FileNet.Api.Admin;
using FileNet.Api.Authentication;
using FileNet.Api.Collection;
using FileNet.Api.Core;
using FileNet.Api.Property;
using FileNet.Api.Util;
using Microsoft.Web.Services3.Security.Tokens;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security.DataHandler;
using SirruXCloud.Controllers;
using SirruXCloud.Models;
using SpatiaX.sxDocs.API;
using SpatiaX.sxDocs.Interface;

namespace SirruXCloud.Helpers
{
    public class sxAPIHelper
    {
        private string _webServiceURI = "Http://sxcpe1:9080/wsi/FNCEWS40MTOM";
        private readonly string _objStoreName;
        private IConnection _connection;
        private IDomain _domain;
        private static IObjectStore _objectStore;
        private readonly string _userName;
        private readonly string _userPass;


        public sxAPIHelper(string userName, string userPass, string objStoreName)
        {
            _userName = userName;
            _userPass = userPass;
            _objStoreName = objStoreName;
           
        }

        public IObjectStore GetObjectStore()
        {
            return _objectStore;
        }

        public bool Login()
        {
           
                UserContext.SetProcessSecurityToken(new UsernameToken(_userName, _userPass, PasswordOption.SendPlainText));
                _connection = Factory.Connection.GetConnection(_webServiceURI);
                _domain = Factory.Domain.FetchInstance(_connection, "", null);
                _objectStore = Factory.ObjectStore.FetchInstance(_domain, _objStoreName, null);        
                return true;
            }
          
        }

    }
    public class FolderHelper
    {
        private static IObjectStore os;

        //
        // Provides the getter and setter for currently
        // selected ObjectStore by the user.
        //
        public static IObjectStore OS
        {
            set
            {
                os = value;
            }
            get
            {
                return os;
            }
        }
        
        //
        // API call to CE to fetch Folder instance from supplied ObjectStore
        // and path.
        //
        public static IFolder FetchFolder(IObjectStore os, String fPath)
        {
            IFolder f = Factory.Folder.FetchInstance(os, fPath, null);
            return f;
        }

        //
        // API call to CE to retrieve sub folders of the specified
        // Folder instance.
        //
        public static IFolderSet GetSubFolders(IFolder f)
        {
            
            IFolderSet fs = f.SubFolders;
            return fs;
        }

    //
    // API call to CE to retrieve sub folders of the specified
    // Folder instance.
    //
   

      

    //
    // API call to CE to retrieve Name property of the specified
    // Folder instance.
    //
    public static String GetFolderName(IFolder f)
        {
            String name = f.Name;
            return name;
        }

        //
        // API call to CE to retrieve containees of specified Folder instance. 
        // It returns ReferentialContainmentRelationshipSet. You can iterate the
        // ReferentialContainmentRelationshipSet  to get ReferentialContainmentRelationship objects,
        // from which Documents and CustomObjects can be retrieved. 
        //
        public static IReferentialContainmentRelationshipSet GetFolderContainees(IFolder f)
        {
            IReferentialContainmentRelationshipSet rcrSet = f.Containees;
            return rcrSet;
        }

        //
        // API call to Content Engine to retrieve ContainmentName property of 
        // ReferentialContainmentRelationship object.
        //
        public static String GetContainmentName(IReferentialContainmentRelationship rcr)
        {
            String name = rcr.ContainmentName;
            return name;
        }
    }
