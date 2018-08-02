using System;
using System.Configuration;
using System.Linq;
using System.Text;
using AWS = Autodesk.Connectivity.WebServices;

namespace ImportFolderStructure
{
    class ApplicationOptions
    {
        private ApplicationOptions()
        {
            AuthenticationType = AWS.AuthTyp.Vault;
            Server = "localhost";
            KnowledgeVault = "Vault";
            UserName = "Administrator";
            PathColumn = "Path";
            UserGroupColumn = "User Group Name";
            ACLReadColumn = "ACL Read";
            ACLWriteColumn = "ACL Write";
            ACLDeleteColumn = "ACL Delete";
            CategoryColumn = "Category";
            StateColumn = "State";
            LibraryColumn = "Library";
            CSVSeparator = char.MinValue;
            Encoding = null;
        }

        public AWS.AuthTyp AuthenticationType { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }
        public string KnowledgeVault { get; private set; }
        public string Server { get; private set; }
        public string InputFile { get; private set; }
        public char CSVSeparator { get; private set; }
        public Encoding Encoding { get; private set; }

        // column names - this has to be read from config file
        public string PathColumn { get; private set; }
        public string UserGroupColumn { get; private set; }
        public string ACLReadColumn { get; private set; }
        public string ACLWriteColumn { get; private set; }
        public string ACLDeleteColumn { get; private set; }
        public string CategoryColumn { get; private set; }
        public string StateColumn { get; private set; }
        public string LibraryColumn { get; private set; }

        public static ApplicationOptions Parse(string[] args)
        {
            ApplicationOptions result = new ApplicationOptions();
            byte flags = 0;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (arg.Equals("-s", StringComparison.CurrentCultureIgnoreCase))
                {
                    result.Server = args[++i];
                    flags |= 0x01;
                }
                else if (arg.Equals("-db", StringComparison.CurrentCultureIgnoreCase))
                {
                    result.KnowledgeVault = args[++i];
                    flags |= 0x02;
                }
                else if (arg.Equals("-u", StringComparison.CurrentCultureIgnoreCase))
                {
                    result.UserName = args[++i];
                    flags |= 0x04;
                }
                else if (arg.Equals("-p", StringComparison.CurrentCultureIgnoreCase))
                {
                    result.Password = args[++i];
                    flags |= 0x08;
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
                    flags |= 0x10;
                }
                else if (arg.Equals("-e", StringComparison.CurrentCultureIgnoreCase))
                {
                    string encoding = args[++i];

                    result.Encoding = GetEncoding(encoding);
                    flags |= 0x20;
                }
                else
                {
                    if (0 == (flags & 0x40))
                    {
                        result.InputFile = arg;
                        flags |= 0x40;
                    }
                }
            }
            if (flags < 0x40)
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
        }

        private static Encoding GetEncoding(string name)
        {
            EncodingInfo[] encodingInfos = Encoding.GetEncodings();
            EncodingInfo encodingInfo = null;

            if (encodingInfos != null)
            {
                encodingInfo = encodingInfos.FirstOrDefault(e => e.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                if (encodingInfo == null)
                {
                    int codePage;

                    if (Int32.TryParse(name, out codePage))
                    {
                        encodingInfo = encodingInfos.FirstOrDefault(e => e.CodePage == codePage);
                    }
                }
            }
            if (encodingInfo == null)
            {
                throw new ApplicationException("Invalid value for encoding. Either valid code page or encoding name must be provided.");
            }
            return encodingInfo.GetEncoding();
        }
    }
}
