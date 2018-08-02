using Autodesk.Connectivity.WebServicesTools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using AWS = Autodesk.Connectivity.WebServices;
using VDF = Autodesk.DataManagement.Client.Framework;

namespace ImportFolderStructure
{
    enum MessageCategory
    {
        Info,
        Debug,
        Warning,
        Error,
    }

    class Application
    {
        public Application()
        {
        }

        private DataTable Data { get; set; }
        private ApplicationOptions Options { get; set; }
        private WebServiceManager ServiceManager { get; set; }

        private List<AWS.Cat> Categories { get; set; }
        private List<AWS.Group> Groups { get; set; }
        private List<AWS.PropDefInfo> Properties { get; set; }
        private List<AWS.User> Users { get; set; }

        private Dictionary<long, string> FolderStates { get; set; }

        public static void PrintHeader()
        {
            Console.WriteLine("ImportFolderStructure v{0} - imports folder structure from CSV file",
                Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.WriteLine("");
        }

        public static void PrintHelp()
        {
            Console.WriteLine("Usage: ImportFolderStructure [-a Vault|Windows] [-s srv] [-db dbase] [-u user] [-p pwd] [-e codepage|name] filename");
            Console.WriteLine("  -a                Authentication type (default = Vault).");
            Console.WriteLine("  -s                Name of server (default = localhost).");
            Console.WriteLine("  -db               Name of database (default = Vault).");
            Console.WriteLine("  -u                UserName for access to database (default = Administrator).");
            Console.WriteLine("  -p                Password for access to database (default = empty password).");
            Console.WriteLine("  -e                Encoding. Provide either codepage or name. (default = UTF-8).");
            Console.WriteLine("  filename          CSV File which contains folder information.");
        }

        public void Run(ApplicationOptions options)
        {
            Options = options;
            VDF.Vault.Currency.Connections.AuthenticationFlags authFlags = VDF.Vault.Currency.Connections.AuthenticationFlags.Standard;

            if (Options.AuthenticationType == AWS.AuthTyp.ActiveDir)
            {
                authFlags = VDF.Vault.Currency.Connections.AuthenticationFlags.WindowsAuthentication;
            }
            VDF.Vault.Results.LogInResult result =
                VDF.Vault.Library.ConnectionManager.LogIn(Options.Server, Options.KnowledgeVault, Options.UserName, Options.Password, authFlags, null);

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
                Log(MessageCategory.Error, "Error connecting to Vault: {0}", message);
                return;
            }

            try
            {
                ServiceManager = result.Connection.WebServiceManager;
                Data = ReadData(Options.InputFile);

                Initialize();
                ImportFoldersAndProperties();
                UpdatePermissionsAndLifecycles();
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
            finally
            {
                VDF.Vault.Library.ConnectionManager.CloseAllConnections();
            }
        }

        private DataTable ReadData(string fileName)
        {
            if (Options.CSVSeparator != char.MinValue)
            {
                CSVDataReader.Separator = Options.CSVSeparator;
            }
            if (Options.Encoding != null)
            {
                CSVDataReader.Encoding = Options.Encoding;
            }
            DataTable result = CSVDataReader.ReadFile(fileName);

            // go through folder names and remove spaces and training slash
            DataColumn column = result.Columns[Options.PathColumn];

            foreach (DataRow row in result.Rows)
            {
                string path = row.Field<string>(column);

                path = path.Trim();
                if (path.EndsWith("/"))
                {
                    path = path.TrimEnd(new char[] { '/' });
                }
                row.SetField<string>(column, path);
            }
            return result;
        }

        private void Initialize()
        {
            Log(MessageCategory.Info, "Input file: '{0}'", Options.InputFile);
            Log(MessageCategory.Info, "Number of rows: '{0}'", Data.Rows.Count);
            Log(MessageCategory.Info, "");
            LoadCategories();
            LoadGroups();
            LoadUsers();
            LoadProperties();
            FolderStates = new Dictionary<long, string>();
        }

        private void ImportFoldersAndProperties()
        {
            foreach (DataRow row in Data.Rows)
            {
                try
                {
                    ImportFolderAndProperties(row);
                }
                catch (Exception ex)
                {
                    LogError(ex);
                }
            }
            
        }

        private void LoadCategories()
        {
            AWS.Cat[] categories = ServiceManager.CategoryService.GetCategoriesByEntityClassId("FLDR", true);

            Categories = new List<AWS.Cat>();
            if (categories != null)
            {
                Categories.AddRange(categories);
            }
        }

        private void LoadGroups()
        {
            AWS.Group[] groups = ServiceManager.AdminService.GetAllGroups();

            Groups = new List<AWS.Group>();
            if (groups != null)
            {
                Groups.AddRange(groups);
            }
        }

        private void LoadUsers()
        {
            AWS.User[] users = ServiceManager.AdminService.GetAllUsers();

            Users = new List<AWS.User>();
            if (users != null)
            {
                Users.AddRange(users);
            }
        }

        private void LoadProperties()
        {
            AWS.PropDefInfo[] properties = ServiceManager.PropertyService.GetPropertyDefinitionInfosByEntityClassId("FLDR", null);

            Properties = new List<AWS.PropDefInfo>();
            if (Properties != null)
            {
                Properties.AddRange(properties);
            }
        }

        private void ImportFolderAndProperties(DataRow row)
        {
            string path = row.Field<string>(Options.PathColumn);

            Log(MessageCategory.Info, "[{0}/{1}] Processing path '{2}'.", Data.Rows.IndexOf(row) + 1, Data.Rows.Count, path);

            if (String.IsNullOrEmpty(path))
                Log(MessageCategory.Warning, "Path name is null or empty.");
            bool library = false;

            if (Data.Columns.IndexOf(Options.LibraryColumn) > -1)
            {
                string value = row.Field<string>(Options.LibraryColumn);

                if (string.Equals(value, "1"))
                {
                    library = true;
                }
            }
            AWS.Folder folder = VaultUtilities.GetOrCreateFolder(ServiceManager.DocumentService, path, library);

            if (Data.Columns.IndexOf(Options.CategoryColumn) > -1)
            {
                folder = UpdateCategory(folder, row.Field<string>(Options.CategoryColumn));
            }
            UpdateProperties(folder, row);
            if (Data.Columns.IndexOf(Options.StateColumn) > -1)
            {
                string state = row.Field<string>(Options.StateColumn);

                if (string.IsNullOrEmpty(state) == false)
                {
                    FolderStates[folder.Id] = state;
                }
            }
        }

        private AWS.Folder UpdateCategory(AWS.Folder folder, string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                return folder;
            }
            AWS.Cat category = Categories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.CurrentCultureIgnoreCase));

            if (category == null)
            {
                Log(MessageCategory.Warning, "  Category '{0}' doesn't exist", categoryName);
                return folder;
            }
            Log(MessageCategory.Info, "  Applying category '{0}'", categoryName);
            ServiceManager.DocumentServiceExtensions.UpdateFolderCategories(new long[] { folder.Id }, new long[] { category.Id });
            return ServiceManager.DocumentService.GetFolderById(folder.Id);
        }

        private void ResetPermission(AWS.Folder folder)
        {
            // TODO: for whatever reason resetting permission on root is causing an error 308
            // we skip it for now
            if (folder.ParId == -1)
            {
                return;
            }
            ServiceManager.SecurityService.UpdateSystemACL(folder.Id, null, AWS.PrpgType.None, AWS.SysAclBeh.Override);
        }

        private void UpdatePermission(AWS.Folder folder, string groupName, string readPermis, string writePermis, string deletePermis)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                return;
            }
            AWS.Group group = Groups.FirstOrDefault(g => g.Name.Equals(groupName, StringComparison.CurrentCultureIgnoreCase));
            long usrGrpId = -1;

            if (group != null)
            {
                usrGrpId = group.Id;
            }
            else
            {
                AWS.User user = Users.FirstOrDefault(u => u.Name.Equals(groupName, StringComparison.CurrentCultureIgnoreCase));

                if (user != null)
                {
                    usrGrpId = user.Id;
                }
            }
            if (usrGrpId == -1)
            {
                Log(MessageCategory.Warning, "  Unable to find group or user with given name '{0}'", groupName);
                return;
            }
            AWS.EntsAndACLs entsAndACLs = ServiceManager.SecurityService.GetEntACLsByEntityIds(new long[] { folder.Id });
            List<AWS.ACE> entries = new List<AWS.ACE>();
            bool handled = false;

            if ((entsAndACLs != null) && (entsAndACLs.EntACLArray != null) && (entsAndACLs.ACLArray != null))
            {
                AWS.EntACL entACL = entsAndACLs.EntACLArray.FirstOrDefault(e => e.EntId == folder.Id);
                if (entACL != null)
                {
                    AWS.ACL acl = entsAndACLs.ACLArray.FirstOrDefault(a => a.Id == entACL.ACLId);

                    if (acl != null && acl.ACEArray != null)
                    {
                        foreach (AWS.ACE ace in acl.ACEArray)
                        {
                            if (ace.UserGrpId == usrGrpId)
                            {
                                List<AWS.AccessPermis> permissions = GetAccessPermissions(readPermis, writePermis, deletePermis);

                                AWS.ACE newAce = new AWS.ACE
                                {
                                    UserGrpId = usrGrpId,
                                    PermisArray = permissions.ToArray(),
                                };

                                entries.Add(newAce);
                                handled = true;
                            }
                            else
                            {
                                entries.Add(ace);
                            }
                        }
                    }
                }
            }

            if (handled == false)
            {
                List<AWS.AccessPermis> permissions = GetAccessPermissions(readPermis, writePermis, deletePermis);
                AWS.ACE newAce = new AWS.ACE
                {
                    UserGrpId = usrGrpId,
                    PermisArray = permissions.ToArray(),
                };

                entries.Add(newAce);
            }

            Log(MessageCategory.Info, "  Updating permission for user group '{0}'", groupName);
            ServiceManager.SecurityService.UpdateACL(folder.Id, entries.ToArray(), AWS.PrpgType.None);
        }

        private List<AWS.AccessPermis> GetAccessPermissions(string readPermis, string writePermis, string deletePermis)
        {
            List<AWS.AccessPermis> result = new List<AWS.AccessPermis>();
            AWS.AccessPermis readAccess = GetAccessPermission(1, readPermis);

            if (readAccess != null)
            {
                result.Add(readAccess);
            }
            AWS.AccessPermis writeAccess = GetAccessPermission(2, writePermis);

            if (writeAccess != null)
            {
                result.Add(writeAccess);
            }
            AWS.AccessPermis deleteAccess = GetAccessPermission(3, deletePermis);

            if (deleteAccess != null)
            {
                result.Add(deleteAccess);
            }
            return result;
        }

        private AWS.AccessPermis GetAccessPermission(long id, string permis)
        {
            if (string.IsNullOrEmpty(permis))
            {
                return null;
            }
            AWS.AccessPermis result = new AWS.AccessPermis { Id = id };

            if (permis.Equals("1", StringComparison.CurrentCultureIgnoreCase))
            {
                result.Val = true;
            }
            else if (permis.Equals("0", StringComparison.CurrentCultureIgnoreCase))
            {
                result.Val = false;
            }
            return result;
        }

        private void UpdateProperties(AWS.Folder folder, DataRow row)
        {
            List<AWS.PropInstParam> propertyValues = new List<AWS.PropInstParam>();

            foreach (DataColumn column in Data.Columns)
            {
                if (IsPropertyColumn(column.ColumnName) == false)
                {
                    continue;
                }
                AWS.PropDefInfo propertyInfo = Properties.FirstOrDefault(p => p.PropDef.DispName.Equals(column.ColumnName, StringComparison.CurrentCultureIgnoreCase));

                if (propertyInfo == null)
                {
                    Log(MessageCategory.Warning, "  Property '{0}' doesn't exist", column.ColumnName);
                    continue;
                }
                object value = GetPropertyValue(propertyInfo.PropDef, row.Field<string>(column));

                if (value == null)
                {
                    continue;
                }
                AWS.PropInstParam propParam = new AWS.PropInstParam();
                propParam.PropDefId = propertyInfo.PropDef.Id;
                propParam.Val = value;
                propertyValues.Add(propParam);
            }
            if (propertyValues.Count > 0)
            {
                Log(MessageCategory.Info, "  Updating properties");
                ServiceManager.DocumentServiceExtensions.UpdateFolderProperties(new long[] { folder.Id },
                            new AWS.PropInstParamArray[] {
                            new AWS.PropInstParamArray() {
                                Items= propertyValues.ToArray()
                                }   
                        });
            }
        }

        private bool IsPropertyColumn(string name)
        {
            string[] names = new string[] { Options.PathColumn, Options.UserGroupColumn, Options.ACLReadColumn,
                Options.ACLWriteColumn, Options.ACLDeleteColumn, Options.CategoryColumn,
                Options.StateColumn, Options.LibraryColumn };

            if (names.Any(n => n.Equals(name, StringComparison.CurrentCultureIgnoreCase)))
            {
                return false;
            }
            return true;
        }

        private object GetPropertyValue(AWS.PropDef definition, string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue) == true)
            {
                return null;
            }
            object propertyValue = null;

            if (definition.Typ == AWS.DataType.Bool)
            {
                if (rawValue.Equals("1"))
                {
                    propertyValue = true;
                }
                else if (rawValue.Equals("0"))
                {
                    propertyValue = false;
                }
                else
                {
                    propertyValue = Convert.ToBoolean(rawValue);
                }
            }
            else if (definition.Typ == AWS.DataType.String)
            {
                propertyValue = rawValue;
            }
            else if (definition.Typ == AWS.DataType.Numeric)
            {
                propertyValue = Convert.ToDouble(rawValue);
            }
            else if (definition.Typ == AWS.DataType.DateTime)
            {
                propertyValue = Convert.ToDateTime(rawValue);
            }
            return propertyValue;
        }

        private void UpdatePermissionsAndLifecycles()
        {
            AWS.Folder folder = ServiceManager.DocumentService.GetFolderRoot();

            UpdatePermissionsAndLifecycle(folder);
        }

        private void UpdatePermissionsAndLifecycle(AWS.Folder folder)
        {
            AWS.Folder[] subFolders = ServiceManager.DocumentService.GetFoldersByParentId(folder.Id, false);

            if (subFolders != null)
            {
                foreach (AWS.Folder subFolder in subFolders)
                {
                    UpdatePermissionsAndLifecycle(subFolder);
                }
            }
            try
            {
                Log(MessageCategory.Info, "Processing folder '{0}'", folder.FullName);
                UpdatePermission(folder);
                UpdateLifecycleState(folder);
            }
            catch (Exception ex)
            {
                LogError(ex);
            }
        }

        private void UpdatePermission(AWS.Folder folder)
        {
            string qry = string.Format("{0} LIKE '{1}'", Options.PathColumn, Escape(folder.FullName));
            DataRow[] rows = Data.Select(qry);

            if (rows == null)
            {
                return;
            }
            bool updatePermission = false;

            if ((Data.Columns.IndexOf(Options.UserGroupColumn) > -1) && (Data.Columns.IndexOf(Options.ACLReadColumn) > -1) &&
                (Data.Columns.IndexOf(Options.ACLWriteColumn) > -1) && (Data.Columns.IndexOf(Options.ACLDeleteColumn) > -1))
            {
                updatePermission = true;
            }
            if (updatePermission == false)
            {
                return;
            }
            ResetPermission(folder);
            foreach (DataRow row in rows)
            {
                if ((Data.Columns.IndexOf(Options.UserGroupColumn) > -1) && (Data.Columns.IndexOf(Options.ACLReadColumn) > -1) &&
                    (Data.Columns.IndexOf(Options.ACLWriteColumn) > -1) && (Data.Columns.IndexOf(Options.ACLDeleteColumn) > -1))
                {
                    UpdatePermission(folder, row.Field<string>(Options.UserGroupColumn),
                        row.Field<string>(Options.ACLReadColumn), row.Field<string>(Options.ACLWriteColumn), row.Field<string>(Options.ACLDeleteColumn));
                }
            }
        }

        private void UpdateLifecycleState(AWS.Folder folder)
        {
            string stateName = string.Empty;

            if (FolderStates.TryGetValue(folder.Id, out stateName))
            {
                UpdateLifecycleState(folder, stateName);
            }
        }

        private void UpdateLifecycleState(AWS.Folder folder, string stateName)
        {
            Log(MessageCategory.Info, "  Setting lifecycle state to '{0}'", stateName);
            AWS.CatCfg[] configurations = ServiceManager.CategoryService.GetCategoryConfigurationsByBehaviorNames("FLDR", true, new string[] { "LifeCycle" });

            if (configurations == null)
            {
                Log(MessageCategory.Warning, "    Unable to obtain lifecycle info");
                return;
            }
            AWS.CatCfg configuration = configurations.FirstOrDefault(c => c.Cat.Id == folder.Cat.CatId);

            if (configuration == null)
            {
                Log(MessageCategory.Warning, "    Unable to obtain lifecycle info");
                return;
            }
            if (configuration.BhvCfgArray == null)
            {
                Log(MessageCategory.Warning, "    Unable to obtain lifecycle info");
                return;
            }
            AWS.BhvCfg behaviorConfiguration = configuration.BhvCfgArray.FirstOrDefault(b => b.Name.Equals("LifeCycle", StringComparison.CurrentCultureIgnoreCase));

            if ((behaviorConfiguration == null) || (behaviorConfiguration.BhvArray == null))
            {
                Log(MessageCategory.Warning, "    Unable to obtain lifecycle info");
                return;
            }
            AWS.Bhv behavior = behaviorConfiguration.BhvArray.FirstOrDefault(b => b.AssignTyp == AWS.BehaviorAssignmentType.Default);
            AWS.LfCycDef[] lifecycleDefinitions = ServiceManager.LifeCycleService.GetLifeCycleDefinitionsByIds(new long[] { behavior.Id });

            if (lifecycleDefinitions == null)
            {
                Log(MessageCategory.Warning, "    Unable to obtain lifecycle info");
                return;
            }
            AWS.LfCycState lifecycleState = lifecycleDefinitions[0].StateArray.FirstOrDefault(s => s.DispName.Equals(stateName, StringComparison.CurrentCultureIgnoreCase));

            if (lifecycleState == null)
            {
                Log(MessageCategory.Warning, "    Unable to obtain lifecycle info");
                return;
            }
            ServiceManager.DocumentServiceExtensions.UpdateFolderLifeCycleStates(new long[] { folder.Id }, new long[] { lifecycleState.Id }, string.Empty);
        }

        /// <summary>
        /// Escapes special characters for LIKE query.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string Escape(string text)
        {
            StringBuilder sb = new StringBuilder(text.Length);

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                switch (c)
                {
                    case ']':
                    case '[':
                    case '%':
                    case '*':
                        sb.AppendFormat("[{0}]", c);
                        break;
                    case '\'':
                        sb.Append("''");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }

        private static void Log(MessageCategory category, string message)
        {
            if (category == MessageCategory.Debug)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }
            if (category == MessageCategory.Warning)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else if (category == MessageCategory.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void Log(MessageCategory category, string format, params object[] args)
        {
            string message = string.Format(format, args);

            Log(category, message);
        }

        private static void LogError(Exception ex)
        {
            Log(MessageCategory.Error, "ERROR: {0}", VDF.Library.ExceptionParser.GetMessage(ex));
            Log(MessageCategory.Debug, " Source: " + ex.Source);
            Log(MessageCategory.Debug, " StackTrace: " + ex.StackTrace);
            Log(MessageCategory.Debug, " Target: " + ex.TargetSite);
        }
    }
}
