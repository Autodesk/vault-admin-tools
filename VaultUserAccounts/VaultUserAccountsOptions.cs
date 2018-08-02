using System;

using Autodesk.Connectivity.WebServices;

namespace VaultUserAccounts
{
    class VaultUserAccountsOptions
    {
        public enum CommandType
        {
            ExportCommand,
            ImportCommand,
        }

        public enum OperationMode
        {
            UsersMode,
            GroupsMode,
        }

        private string _fileName;
        private string _userName = "Administrator";
        private string _password = string.Empty;
        private string _server = "localhost";
        private CommandType _commandType;
        private AuthTyp _authType = AuthTyp.Vault;
        private OperationMode _mode = OperationMode.UsersMode;
        private string _encoding = "UTF-8";

        private VaultUserAccountsOptions()
        {
        }

        public static VaultUserAccountsOptions Parse(string[] args)
        {
            VaultUserAccountsOptions result = new VaultUserAccountsOptions();

            if (null == args)
            {
                throw new ArgumentException();
            }
            byte flags = 0;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (true == arg.StartsWith("-U", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (0 != (flags & 0x01))
                    {
                        throw new ArgumentException();
                    }
                    result.UserName = args[++i];
                    flags |= 0x01;
                }
                else if (true == arg.StartsWith("-P", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (0 != (flags & 0x02))
                    {
                        throw new ArgumentException();
                    }
                    result.Password = args[++i];
                    flags |= 0x02;
                }
                else if (true == arg.StartsWith("-S", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (0 != (flags & 0x04))
                    {
                        throw new ArgumentException();
                    }
                    result.Server = args[++i];
                    flags |= 0x04;
                }
                else if (true == arg.StartsWith("-A", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (0 != (flags & 0x08))
                    {
                        throw new ArgumentException();
                    }
                    string authType = args[++i];

                    if ((0 == string.Compare(authType, "Vault", true)) || (0 == string.Compare(authType, "Windows", true)))
                    {
                        if (0 == string.Compare(authType, "Vault", true))
                        {
                            result.AuthenticationType = AuthTyp.Vault;
                        }
                        else if (0 == string.Compare(authType, "Windows", true))
                        {
                            result.AuthenticationType = AuthTyp.ActiveDir;
                        }
                        flags |= 0x08;
                    }
                }
                else if (true == arg.StartsWith("-G", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (0 != (flags & 0x10))
                    {
                        throw new ArgumentException();
                    }
                    result.Mode = OperationMode.GroupsMode;
                    flags |= 0x10;
                }
                else if (0 == string.Compare("-e", arg, true))
                {
                    if (0 != (flags & 0x20))
                    {
                        throw new ArgumentException();
                    }
                    result.Encoding = args[++i];
                    flags |= 0x20;
                }
                else
                {
                    if ((0 == string.Compare(arg, "Export", true)) || (0 == string.Compare(arg, "Import", true)))
                    {
                        if (0 != (flags & 0x40))
                        {
                            throw new ArgumentException();
                        }
                        if (0 == string.Compare(arg, "Export", true))
                        {
                            result.Command = CommandType.ExportCommand;
                        }
                        else if (0 == string.Compare(arg, "Import", true))
                        {
                            result.Command = CommandType.ImportCommand;
                        }
                        flags |= 0x40;
                    }
                    else
                    {
                        if (0 != (flags & 0x80))
                        {
                            throw new ArgumentException();
                        }
                        result.FileName = arg;
                        flags |= 0x80;
                    }
                }
            }
            if (flags < 0x80)
            {
                throw new ArgumentException();
            }

            return result;
        }

        public AuthTyp AuthenticationType
        {
            get { return _authType; }
            private set { _authType = value; }
        }

        public CommandType Command
        {
            get { return _commandType; }
            private set { _commandType = value; }
        }

        public OperationMode Mode 
        {
            get { return _mode; }
            private set { _mode = value; }
        }

        public string FileName
        {
            get { return _fileName; }
            private set { _fileName = value; }
        }

        public string UserName
        {
            get { return _userName; }
            private set { _userName = value; }
        }

        public string Password
        {
            get { return _password; }
            private set { _password = value; }
        }

        public string Server
        {
            get { return _server; }
            private set { _server = value; }
        }

        public string Encoding 
        {
            get { return _encoding; }
            private set { _encoding = value; }

        }
    }
}
