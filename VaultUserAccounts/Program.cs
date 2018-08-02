using System;
using System.Text;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Services.Protocols;
using Autodesk.Connectivity.WebServices;
using VDF = Autodesk.DataManagement.Client.Framework;
using VaultUserAccounts.IO;

namespace VaultUserAccounts
{
    class Program
    {
        private static char _separator;
        private static Encoding _encoding;

        static void Main(string[] args)
        {
            try
            {
                PrintHeader();
                VaultUserAccountsOptions options = VaultUserAccountsOptions.Parse(args);

                Run(options);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                {
                    Console.WriteLine("Usage: VaultUserAccounts [-u username] [-p password] [-s server] [-a authenticationType] [-g] IMPORT|EXPORT filename");
                    Console.WriteLine("  -u                UserName for access to database (default = Administrator).");
                    Console.WriteLine("  -p                Password for access to database (default = empty password).");
                    Console.WriteLine("  -s                Name of server (default = localhost).");
                    Console.WriteLine("  -a                Authentication Type - Windows or Vault (default = Vault).");
                    Console.WriteLine("  -g                Exports group information instead of user information.");
                    Console.WriteLine("  -e                Encoding. Provide either codepage or name. (default = UTF-8).");
                    Console.WriteLine("  IMPORT|EXPORT     Action type");
                    Console.WriteLine("  filename          CSV File which is used to write or read user information.");
                    Console.WriteLine("");
                    Console.WriteLine("Examples:");
                    Console.WriteLine("  VaultUserAccounts -U Joe -P 123Password -S VaultServer -A Vault -e UTF-8 EXPORT users.txt");
                    Console.WriteLine("  VaultUserAccounts -S VaultServer -A Windows EXPORT users.txt");
                }
                else
                {
                    Console.WriteLine("ERROR: {0}", VDF.Library.ExceptionParser.GetMessage(ex));
#if DEBUG
                    Console.WriteLine("StackTrace: {0}", ex.StackTrace);
#endif
                }
            }
            finally
            {
                VDF.Vault.Library.ConnectionManager.CloseAllConnections();
            }
        }

        private static void Run(VaultUserAccountsOptions options)
        {
            SetFileProcessingOptions(options);
            if (VaultUserAccountsOptions.CommandType.ImportCommand == options.Command)
            {
                if (options.Mode == VaultUserAccountsOptions.OperationMode.UsersMode)
                {
                    ImportUsers(options);
                }
                else if (options.Mode == VaultUserAccountsOptions.OperationMode.GroupsMode)
                {
                    ImportGroups(options);
                }
            }
            else if (VaultUserAccountsOptions.CommandType.ExportCommand == options.Command)
            {
                if (options.Mode == VaultUserAccountsOptions.OperationMode.UsersMode)
                {
                    ExportUsers(options);
                }
                else if (options.Mode == VaultUserAccountsOptions.OperationMode.GroupsMode)
                {
                    ExportGroups(options);
                }
            }
        }

        private static void SetFileProcessingOptions(VaultUserAccountsOptions Options)
        {
            string sep = ConfigurationManager.AppSettings["CSVSeparatorInAscii"];

            if (string.IsNullOrEmpty(sep) == false)
            {
                int code = Convert.ToInt32(sep);

                _separator = Convert.ToChar(code);
            }
            else
                //default separator is comma
                _separator = Convert.ToChar(44);

            EncodingInfo[] encodingInfos = Encoding.GetEncodings();
            EncodingInfo encodingInfo = null;

            if (encodingInfos != null)
            {
                encodingInfo = encodingInfos.FirstOrDefault(e => e.Name.Equals(Options.Encoding, StringComparison.CurrentCultureIgnoreCase));
                if (encodingInfo == null)
                {
                    int codePage;

                    if (Int32.TryParse(Options.Encoding, out codePage))
                    {
                        encodingInfo = encodingInfos.FirstOrDefault(e => e.CodePage == codePage);
                    }
                }
            }
            if (encodingInfo == null)
            {
                throw new ApplicationException("Invalid value for encoding. Either valid code page or encoding name must be provided.");
            }
            _encoding = encodingInfo.GetEncoding();
        }

        private static void ExportUsers(VaultUserAccountsOptions options)
        {
            using (KnowledgeMaster master = new KnowledgeMaster(options.Server, options.UserName, options.Password, options.AuthenticationType))
            {
                List<UserInfo> userInfos = new List<UserInfo>();

                foreach (User user in master.GetAllUsers())
                {
                    if (true == user.IsSys)
                    {
                        continue;
                    }
                    Console.WriteLine("Processing user '{0}'", user.Name);
                    UserInfo userInfo = new UserInfo(user);

                    userInfo.Groups.AddRange(master.GetGroupNamesForUser(user.Id));
                    userInfo.Roles.AddRange(master.GetRoleNamesForUser(user.Id));
                    userInfo.Vaults.AddRange(master.GetVaultNamesForUser(user.Id));
                    userInfos.Add(userInfo);
                }
                WriteFile(options.FileName, userInfos);
            }
        }

        private static void ExportGroups(VaultUserAccountsOptions options)
        {
            using (KnowledgeMaster master = new KnowledgeMaster(options.Server, options.UserName, options.Password, options.AuthenticationType))
            {
                List<GroupInfo> groupInfos = new List<GroupInfo>();

                foreach (Group group in master.GetAllGroups())
                {
                    if (true == group.IsSys)
                    {
                        continue;
                    }
                    Console.WriteLine("Processing group '{0}'", group.Name);
                    GroupInfo groupInfo = new GroupInfo(group);

                    groupInfo.Groups.AddRange(master.GetGroupNamesForGroup(group.Id));
                    groupInfo.Roles.AddRange(master.GetRoleNamesForGroup(group.Id));
                    groupInfo.Vaults.AddRange(master.GetVaultNamesForGroup(group.Id));
                    groupInfos.Add(groupInfo);
                }
                WriteGroupsToFile(options.FileName, groupInfos);
            }
        }

        private static void ImportGroups(VaultUserAccountsOptions options)
        {
            using (KnowledgeMaster master = new KnowledgeMaster(options.Server, options.UserName, options.Password, options.AuthenticationType))
            {
                List<GroupInfo> groupInfos = ReadGroupsFromFile(options.FileName);

                foreach (GroupInfo groupInfo in groupInfos)
                {
                    try
                    {
                        Console.WriteLine("Processing group '{0}'", groupInfo.Name);

                        master.UpdateGroup(groupInfo);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR: {0}", VDF.Library.ExceptionParser.GetMessage(ex));
#if DEBUG
                        Console.WriteLine("StackTrace: {0}", ex.StackTrace);
#endif
                    }
                }
            }
        }

        private static void ImportUsers(VaultUserAccountsOptions options)
        {
            using (KnowledgeMaster master = new KnowledgeMaster(options.Server, options.UserName, options.Password, options.AuthenticationType))
            {
                List<UserInfo> userInfos = ReadFile(options.FileName);

                foreach (UserInfo userInfo in userInfos)
                {
                    try
                    {
                        Console.WriteLine("Processing user '{0}'", userInfo.LoginName);

                        master.UpdateUser(userInfo);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("ERROR: {0}", VDF.Library.ExceptionParser.GetMessage(ex));
#if DEBUG
                        Console.WriteLine("StackTrace: {0}", ex.StackTrace);
#endif
                    }
                }
            }
        }

        private static List<GroupInfo> ReadGroupsFromFile(string fileName)
        {
            IGroupInfoReader reader = new GroupInfoReaderCSV(_separator, _encoding);

            return reader.ReadFile(fileName);
        }

        private static List<UserInfo> ReadFile(string fileName)
        {
            IUserInfoReader reader = new UserInfoReaderCSV(_separator, _encoding);

            return reader.ReadFile(fileName);
        }

        private static void WriteGroupsToFile(string fileName, List<GroupInfo> groupInfos)
        {
            IGroupInfoWriter writer = new GroupInfoWriterCSV(_separator, _encoding);

            writer.WriteFile(fileName, groupInfos);
        }

        private static void WriteFile(string fileName, List<UserInfo> userInfos)
        {
            IUserInfoWriter writer = new UserInfoWriterCSV(_separator, _encoding);

            writer.WriteFile(fileName, userInfos);
        }

        private static void PrintHeader()
        {
            Console.WriteLine("VaultUserAccounts v{0} - imports/exports ADMS user accounts",
              Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.WriteLine("Copyright (c) 2012 Autodesk, Inc. All rights reserved.");
            Console.WriteLine("");
        }

    }
}
