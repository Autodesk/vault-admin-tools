using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AWS = Autodesk.Connectivity.WebServices;

namespace ImportNamingSchemeList
{
    enum SchemeType
    {
        File,
        Item,
    };

    class ApplicationOptions
    {
        private ApplicationOptions()
        {
            AuthenticationType = AWS.AuthTyp.Vault;
            Server = "localhost";
            UserName = "Administrator";
            Vault = "Vault";
            Password = "";
            SchemeType = SchemeType.File;
        }

        public AWS.AuthTyp AuthenticationType { get; private set; }
        public string Server { get; private set; }
        public string Vault { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public string InputFile { get; private set; }
        public string SchemeName { get; private set; }
        public SchemeType SchemeType { get; private set; }

        public static ApplicationOptions Parse(string[] args)
        {
            ApplicationOptions options = new ApplicationOptions();

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (string.Equals("-s", arg, StringComparison.CurrentCultureIgnoreCase))
                {
                    options.Server = args[++i];
                }
                else if (string.Equals("-db", arg, StringComparison.CurrentCultureIgnoreCase))
                {
                    options.Vault = args[++i];
                }
                else if (string.Equals("-u", arg, StringComparison.CurrentCultureIgnoreCase))
                {
                    options.UserName = args[++i];
                }
                else if (string.Equals("-p", arg, StringComparison.CurrentCultureIgnoreCase))
                {
                    options.Password = args[++i];
                }
                else if (string.Equals("-a", arg, StringComparison.CurrentCultureIgnoreCase))
                {
                    string authType = args[++i];

                    if (authType.Equals("Vault", StringComparison.CurrentCultureIgnoreCase) || authType.Equals("V", StringComparison.CurrentCultureIgnoreCase))
                    {
                        options.AuthenticationType = AWS.AuthTyp.Vault;
                    }
                    if (authType.Equals("Windows", StringComparison.CurrentCultureIgnoreCase) || authType.Equals("W", StringComparison.CurrentCultureIgnoreCase))
                    {
                        options.AuthenticationType = AWS.AuthTyp.ActiveDir;
                    }
                }
                else if (string.Equals("-t", arg, StringComparison.CurrentCultureIgnoreCase))
                {
                    string schemeType = args[++i];

                    if (schemeType.Equals("File", StringComparison.CurrentCultureIgnoreCase))
                    {
                        options.SchemeType = SchemeType.File;
                    }
                    if (schemeType.Equals("Item", StringComparison.CurrentCultureIgnoreCase))
                    {
                        options.SchemeType = SchemeType.Item;
                    }
                }
                else if (string.Equals("-n", arg, StringComparison.CurrentCultureIgnoreCase))
                {
                    options.SchemeName = args[++i];
                }
                else
                {
                    options.InputFile = arg;
                }
            }
            if (string.IsNullOrEmpty(options.SchemeName))
            {
                throw new ArgumentException("Empty scheme name");
            }
            if (string.IsNullOrEmpty(options.InputFile))
            {
                throw new ArgumentException("Empty input file name");
            }
            return options;
        }
    }
}
