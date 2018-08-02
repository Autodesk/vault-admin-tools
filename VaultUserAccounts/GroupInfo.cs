using System.Collections.Generic;
using Autodesk.Connectivity.WebServices;

namespace VaultUserAccounts
{
    class GroupInfo
    {
        public GroupInfo()
        {
            Groups = new List<string>();
            Roles = new List<string>();
            Vaults = new List<string>();
            EMail = string.Empty;
        }

        public GroupInfo(Group group)
        {
            Name = group.Name;
            EMail = group.EmailDL;
            Active = group.IsActive;
            Groups = new List<string>();
            Roles = new List<string>();
            Vaults = new List<string>();
        }

        public bool Active { get; set; }

        public string Name { get; set; }

        public string EMail { get; set; }

        public List<string> Groups { get; private set; }

        public List<string> Roles { get; private set; }

        public List<string> Vaults { get; private set; }

    }
}
