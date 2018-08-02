using System.Collections.Generic;
using Autodesk.Connectivity.WebServices;

namespace VaultUserAccounts
{
    class UserInfo
    {
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _loginName = string.Empty;
        private string _password = string.Empty;
        private string _email = string.Empty;
        private List<string> _roles;
        private List<string> _groups;
        private List<string> _vaults;
        private bool _active = false;

        public UserInfo()
        {
        }

        public UserInfo(User user)
        {
            FirstName = user.FirstName;
            LastName = user.LastName;
            LoginName = user.Name;
            EMail = user.Email;
            Active = user.IsActive;
        }

        public bool Active
        {
            get { return _active; }
            set { _active = value; }
        }

        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }

        public string LoginName
        {
            get { return _loginName; }
            set { _loginName = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public string EMail
        {
            get { return _email; }
            set { _email = value; }
        }

        public List<string> Groups
        {
            get
            {
                if (null == _groups)
                {
                    _groups = new List<string>();
                }
                return _groups;
            }
        }

        public List<string> Roles
        {
            get
            {
                if (null == _roles)
                {
                    _roles = new List<string>();
                }
                return _roles;
            }
        }

        public List<string> Vaults
        {
            get
            {
                if (null == _vaults)
                {
                    _vaults = new List<string>();
                }
                return _vaults;
            }
        }

    }
}
