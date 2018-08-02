using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Connectivity.WebServices;
using Autodesk.Connectivity.WebServicesTools;
using VDF = Autodesk.DataManagement.Client.Framework;

namespace VaultUserAccounts
{
    class KnowledgeMaster : IDisposable
    {

        public KnowledgeMaster(string server, string userName, string userPassword, AuthTyp authType)
        {
            VDF.Vault.Currency.Connections.AuthenticationFlags authFlags = VDF.Vault.Currency.Connections.AuthenticationFlags.Standard;

            if (authType == AuthTyp.ActiveDir)
            {
                authFlags = VDF.Vault.Currency.Connections.AuthenticationFlags.WindowsAuthentication;
            }
            VDF.Vault.Results.LogInResult result =
                VDF.Vault.Library.ConnectionManager.LogIn(server, null, userName, userPassword, VDF.Vault.Currency.Connections.AuthenticationFlags.ServerOnly | authFlags, null);

            if (result.Success == false)
            {
                string message = "Login failed";
                if (result.Exception == null)
                {
                    if (result.ErrorMessages.Count > 0)
                    {
                        message = result.ErrorMessages.ElementAt(0).Key.ToString() + ", " + result.ErrorMessages.ElementAt(0).Value;
                    }
                }
                else
                {
                    message = VDF.Library.ExceptionParser.GetMessage(result.Exception);
                }
                throw new ApplicationException(message);
            }
            AuthenticationType = authType;
            ServiceManager = result.Connection.WebServiceManager;
        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        public WebServiceManager ServiceManager { get; private set; }

        public AuthTyp AuthenticationType { get; private set; }

        public Group[] GetAllGroups()
        {
            return ServiceManager.AdminService.GetAllGroups();
        }

        public User[] GetAllUsers()
        {
            return ServiceManager.AdminService.GetAllUsers();
        }

        public IEnumerable<string> GetGroupNamesForGroup(long groupId)
        {
            List<string> result = new List<string>();
            Autodesk.Connectivity.WebServices.GroupInfo groupInfo = ServiceManager.AdminService.GetGroupInfoByGroupId(groupId);

            if (groupInfo.Groups != null)
            {
                result.AddRange(groupInfo.Groups.Select(g => g.Name));
            }
            return result;
        }

        public string[] GetGroupNamesForUser(long userId)
        {
            List<string> result = new List<string>();
            Group[] groups = ServiceManager.AdminService.GetAllGroups();

            if (null == groups)
            {
                return result.ToArray();
            }
            foreach (Group group in groups)
            {
                Autodesk.Connectivity.WebServices.GroupInfo groupInfo = ServiceManager.AdminService.GetGroupInfoByGroupId(group.Id);

                if (true == groupInfo.Group.IsSys)
                {
                    continue;
                }
                if (null == groupInfo.Users)
                {
                    continue;
                }
                foreach (User user in groupInfo.Users)
                {
                    if (user.Id != userId)
                    {
                        continue;
                    }
                    if (false == result.Contains(groupInfo.Group.Name))
                    {
                        result.Add(groupInfo.Group.Name);
                    }
                }
            }
            return result.ToArray();
        }

        public IEnumerable<string> GetRoleNamesForGroup(long groupId)
        {
            List<string> result = new List<string>();
            Autodesk.Connectivity.WebServices.GroupInfo groupInfo = ServiceManager.AdminService.GetGroupInfoByGroupId(groupId);

            if (groupInfo.Roles != null)
            {
                result.AddRange(groupInfo.Roles.Select(r => r.Name));
            }
            return result;
        }

        public string[] GetRoleNamesForUser(long userId)
        {
            List<string> result = new List<string>();
            Autodesk.Connectivity.WebServices.UserInfo userInfo = ServiceManager.AdminService.GetUserInfoByUserId(userId);

            if (null == userInfo.Roles)
            {
                return result.ToArray();
            }
            foreach (Role role in userInfo.Roles)
            {
                if (false == result.Contains(role.Name))
                {
                    result.Add(role.Name);
                }
            }
            return result.ToArray();
        }

        public IEnumerable<string> GetVaultNamesForGroup(long groupId)
        {
            List<string> result = new List<string>();
            Autodesk.Connectivity.WebServices.GroupInfo groupInfo = ServiceManager.AdminService.GetGroupInfoByGroupId(groupId);

            if (groupInfo.Vaults != null)
            {
                result.AddRange(groupInfo.Vaults.Select(v => v.Name));
            }
            return result.ToArray();
        }

        public string[] GetVaultNamesForUser(long userId)
        {
            List<string> result = new List<string>();
            Autodesk.Connectivity.WebServices.UserInfo userInfo = ServiceManager.AdminService.GetUserInfoByUserId(userId);

            if (null == userInfo.Vaults)
            {
                return result.ToArray();
            }
            foreach (KnowledgeVault vault in userInfo.Vaults)
            {
                if (false == result.Contains(vault.Name))
                {
                    result.Add(vault.Name);
                }
            }
            return result.ToArray();
        }

        public void UpdateUser(UserInfo userInfo)
        {
            List<long> roleIds = GetRoleIdsFromNames(userInfo.Roles),
              vaultIds = GetVaultIdsFromNames(userInfo.Vaults);
            User user = FindUser(userInfo.LoginName);

            if (null == user)
            {
                user = ServiceManager.AdminService.AddUser(userInfo.LoginName, userInfo.Password,
                  AuthenticationType,
                  userInfo.FirstName, userInfo.LastName, userInfo.EMail, userInfo.Active,
                  roleIds.ToArray(), vaultIds.ToArray());
            }
            else
            {
                ServiceManager.AdminService.UpdateUserInfo(user.Id, userInfo.LoginName,
                  AuthenticationType,
                  userInfo.FirstName, userInfo.LastName, userInfo.EMail, userInfo.Active,
                  roleIds.ToArray(), vaultIds.ToArray());
            }
            UpdateGroupInfo(user, userInfo);
        }

        public void UpdateGroup(GroupInfo groupInfo)
        {
            List<long> groupIds = GetGroupIdsFromNames(groupInfo.Groups),
                roleIds = GetRoleIdsFromNames(groupInfo.Roles),
                vaultIds = GetVaultIdsFromNames(groupInfo.Vaults),
                userIds = new List<long>();
            Group group = FindGroup(groupInfo.Name);

            if (null == group)
            {
                group = ServiceManager.AdminService.AddGroup(groupInfo.Name, AuthenticationType,
                  groupInfo.Active, groupInfo.EMail,
                  roleIds.Count == 0 ? null : roleIds.ToArray(),
                  vaultIds.Count == 0 ? null : vaultIds.ToArray());
            }
            else
            {
                Autodesk.Connectivity.WebServices.GroupInfo currentGroupInfo = ServiceManager.AdminService.GetGroupInfoByGroupId(group.Id);

                if (currentGroupInfo.Users != null)
                {
                    userIds.AddRange(currentGroupInfo.Users.Select(u => u.Id));
                }
            }
            ServiceManager.AdminService.UpdateGroupInfo(group.Id, groupInfo.Name, AuthenticationType,
                groupInfo.EMail, groupInfo.Active,
                roleIds.Count == 0 ? null : roleIds.ToArray(),
                vaultIds.Count == 0 ? null : vaultIds.ToArray(),
                userIds.Count == 0 ? null : userIds.ToArray(),
                groupIds.Count == 0 ? null : groupIds.ToArray());
        }

        private Group FindGroup(string name)
        {
            Group[] allGroups = ServiceManager.AdminService.GetAllGroups();

            if (null == allGroups)
            {
                return null;
            }
            return allGroups.FirstOrDefault(g => g.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }

        private User FindUser(string loginName)
        {
            User[] allUsers = ServiceManager.AdminService.GetAllUsers();

            if (null == allUsers)
            {
                return null;
            }
            foreach (User user in allUsers)
            {
                if (0 == string.Compare(user.Name, loginName, true))
                {
                    return user;
                }
            }
            return null;
        }

        private void UpdateGroupInfo(User user, UserInfo userInfo)
        {
            Group[] allGroups = ServiceManager.AdminService.GetAllGroups();

            if (null == allGroups)
            {
                return;
            }
            foreach (Group group in allGroups)
            {
                if (true == group.IsSys)
                {
                    continue;
                }
                bool addToGroup = false;

                if (true == ContainsName(userInfo.Groups, group.Name, true))
                {
                    addToGroup = true;
                }
                if (true == IsMemberOfGroup(group, user))
                {
                    if (false == addToGroup)
                    {
                        ServiceManager.AdminService.DeleteUserFromGroup(user.Id, group.Id);
                    }
                }
                else
                {
                    if (true == addToGroup)
                    {
                        ServiceManager.AdminService.AddUserToGroup(user.Id, group.Id);
                    }
                }
            }
        }

        private List<long> GetGroupIdsFromNames(IEnumerable<string> names)
        {
            List<long> result = new List<long>();
            Group[] groups = ServiceManager.AdminService.GetAllGroups();

            if (groups != null)
            {
                IEnumerable<long> ids = from n in names
                                        from g in groups
                                        where n.Equals(g.Name, StringComparison.CurrentCultureIgnoreCase)
                                        select g.Id;

                result.AddRange(ids);
            }
            return result;
        }

        private List<long> GetRoleIdsFromNames(IEnumerable<string> roleNames)
        {
            List<long> result = new List<long>();
            Role[] roles = ServiceManager.AdminService.GetAllRoles();

            if (null != roles)
            {
                foreach (string roleName in roleNames)
                {
                    bool found = false;

                    foreach (Role role in roles)
                    {
                        if (0 == string.Compare(role.Name, roleName, true))
                        {
                            result.Add(role.Id);
                            found = true;
                            break;
                        }
                    }
                    if (false == found)
                    {
                        string msg = string.Format("Role '{0}' doesn't exist", roleName);

                        throw new ApplicationException(msg);
                    }
                }
            }
            return result;
        }

        private List<long> GetVaultIdsFromNames(IEnumerable<string> vaultNames)
        {
            List<long> result = new List<long>();
            KnowledgeVault[] vaults = ServiceManager.FilestoreVaultService.GetAllKnowledgeVaults();

            if (null != vaults)
            {
                foreach (string vaultName in vaultNames)
                {
                    bool found = false;

                    foreach (KnowledgeVault vault in vaults)
                    {
                        if (0 == string.Compare(vault.Name, vaultName, true))
                        {
                            result.Add(vault.Id);
                            found = true;
                            break;
                        }
                    }
                    if (false == found)
                    {
                        string msg = string.Format("Vault '{0}' doesn't exist", vaultName);

                        throw new ApplicationException(msg);
                    }
                }
            }
            return result;
        }

        private bool IsMemberOfGroup(Group group, User user)
        {
            Autodesk.Connectivity.WebServices.GroupInfo groupInfo = ServiceManager.AdminService.GetGroupInfoByGroupId(group.Id);

            if (null == groupInfo.Users)
            {
                return false;
            }
            foreach (User tmp in groupInfo.Users)
            {
                if (tmp.Id == user.Id)
                {
                    return true;
                }
            }
            return false;
        }

        private bool ContainsName(IEnumerable<string> names, string name, bool ignoreCase)
        {
            foreach (string tmp in names)
            {
                if (0 == string.Compare(tmp, name, ignoreCase))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
