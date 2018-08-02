using System;
using System.Configuration;
using AWS = Autodesk.Connectivity.WebServices;

namespace ImportObjectProperties
{
    public enum ImportType
    {
        File,
        Item,
        CustomEntity,
    }

    class ApplicationOptions
    {
        #region Fields

        #endregion

        private ApplicationOptions()
        {
            ObjectType = ImportType.File;
            AuthenticationType = AWS.AuthTyp.Vault;
            Server = "localhost";
            UserName = "Administrator";
            Password = string.Empty;
            KnowledgeVault = "Vault";
            InputFile = string.Empty;
            EnableDebugMessages = false;
            CSVSeparator = char.MinValue;
            Encoding = "UTF-8";
            UseExplorerUtil = false;
        }

        public ImportType ObjectType { get; private set; }

        public AWS.AuthTyp AuthenticationType { get; private set; }

        public string InputFile { get; private set; }

        public string KnowledgeVault { get; private set; }

        public string Server { get; private set; }

        public string UserName { get; private set; }

        public string Password { get; private set; }

        public bool EnableDebugMessages { get; private set; }

        public string Encoding { get; private set; }

        public char CSVSeparator { get; private set; }

        public string DescriptionProperty { get; private set; }

        public string TitleProperty { get; private set; }

        public string UnitsProperty { get; private set; }

        public bool UseExplorerUtil { get; private set; }

        public static ApplicationOptions Parse(string[] args)
        {
            ApplicationOptions result = new ApplicationOptions();
            byte flags = 0;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (0 == string.Compare("-t", arg, true))
                {
                    string objectType = args[++i];
                    if (objectType.ToLower().Equals("file"))
                        result.ObjectType = ImportType.File;
                    else if (objectType.ToLower().Equals("item"))
                        result.ObjectType = ImportType.Item;
                    else if (objectType.ToLower().Equals("custent"))
                        result.ObjectType = ImportType.CustomEntity;
                    flags |= 0x01;
                }

                else if (arg.Equals("-a", StringComparison.CurrentCultureIgnoreCase))
                {
                    string authType = args[++i];

                    if (authType.Equals("Vault", StringComparison.CurrentCultureIgnoreCase) || authType.Equals("V", StringComparison.CurrentCultureIgnoreCase))
                    {
                        result.AuthenticationType = AWS.AuthTyp.Vault;
                    }
                    if (authType.Equals("Windows", StringComparison.CurrentCultureIgnoreCase) || authType.Equals("W", StringComparison.CurrentCultureIgnoreCase))
                    {
                        result.AuthenticationType = AWS.AuthTyp.Vault;
                    }
                    flags |= 0x02;
                }

                else if (0 == string.Compare("-s", arg, true))
                {
                    result.Server = args[++i];
                    flags |= 0x04;
                }
                else if (0 == string.Compare("-db", arg, true))
                {
                    result.KnowledgeVault = args[++i];
                    flags |= 0x08;
                }
                else if (0 == string.Compare("-u", arg, true))
                {
                    result.UserName = args[++i];
                    flags |= 0x10;
                }
                else if (0 == string.Compare("-p", arg, true))
                {
                    result.Password = args[++i];
                    flags |= 0x20;
                }
                else if (0 == string.Compare("-l", arg, true))
                {
                    string nextArg = args[++i];

                    if (string.Compare(nextArg, "d", true) == 0)
                    {
                        result.EnableDebugMessages = true;
                    }
                    flags |= 0x30;
                }
                else if (0 == string.Compare("-e", arg, true))
                {
                    result.Encoding = args[++i];
                    flags |= 0x40;
                }
                else
                {
                    if (0 == (flags & 0x40))
                    {
                        result.InputFile = arg;
                        flags |= 0x50;
                    }
                }
            }
            if (flags < 0x50)
            {
                throw new ArgumentException();
            }
            LoadConfiguration(result);
            return result;
        }

        private static void LoadConfiguration(ApplicationOptions options)
        {
            string separator = ConfigurationManager.AppSettings["CSVSeparatorInASCII"];

            if (string.IsNullOrEmpty(separator) == false)
            {
                int code = Convert.ToInt32(separator);

                options.CSVSeparator = Convert.ToChar(code);
            }
            string descriptionPropertyName = ConfigurationManager.AppSettings["ItemCoDescriptionPropertyName"];

            if (string.IsNullOrEmpty(descriptionPropertyName))
            {
                descriptionPropertyName = "Description (Item,CO)";
            }
            options.DescriptionProperty = descriptionPropertyName;
            string titlePropertyName = ConfigurationManager.AppSettings["ItemCoTitlePropertyName"];

            if (string.IsNullOrEmpty(titlePropertyName))
            {
                titlePropertyName = "Title (Item,CO)";
            }
            options.TitleProperty = titlePropertyName;

            string unitsPropertyName = ConfigurationManager.AppSettings["ItemUnitsPropertyName"];

            if (string.IsNullOrEmpty(unitsPropertyName))
            {
                unitsPropertyName = "Units";
            }
            options.UnitsProperty = unitsPropertyName;


            string useExplorerUtil = ConfigurationManager.AppSettings["UseExplorerUtil"];

            if (string.IsNullOrEmpty(useExplorerUtil) == false)
            {
                options.UseExplorerUtil = Convert.ToBoolean(useExplorerUtil);
            }
        }
    }
}
