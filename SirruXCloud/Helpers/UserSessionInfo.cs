using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace SirruXCloud.Helpers
{
    public class UserSessionInfo
    {
        private readonly EncryptionHelper _encryption;
        private readonly string _userName;
        private readonly string _userP8P;
        private readonly string _desktop;


        public UserSessionInfo(string userName, string userPass, string desktop)
        {
            _encryption = new EncryptionHelper();

            _userName = _encryption.EncryptString(userName);
            _userP8P = _encryption.EncryptString(userPass);
            _desktop = _encryption.EncryptString(desktop);

        }

        public string RetrieveUserName()
        {
            return _encryption.DecryptString(_userName);
        }

        public string RetrievePassword()
        {
            return _encryption.DecryptString(_userP8P);
        }

        public string RetrieveDesktop()
        {
            return _encryption.DecryptString(_desktop);
        }

        public string RetrieveRawUserName()
        {
            return _userName;
        }

        public string RetrieveRawUserP8P()
        {
            return _userP8P;
        }


    }
}