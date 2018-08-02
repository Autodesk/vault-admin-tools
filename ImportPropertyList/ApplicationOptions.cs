using System;

namespace ImportPropertyList
{
    enum PropertyType
    {
        File,
        Item,
        CustomObject
    }

    class ApplicationOptions
    {

        private ApplicationOptions()
        {
            PropertyName = string.Empty;
            FileName = string.Empty;
            Server = "localhost";
            UserName = "Administrator";
            Password = string.Empty;
            KnowledgeVault = "Vault";
            Encoding = "UTF-8";
        }

        public string FileName { get; private set; }

        public string PropertyName { get; private set; }

        public PropertyType PropertyType { get; private set; }

        public string Password { get; private set; }

        public string Server { get; private set; }

        public string KnowledgeVault { get; private set; }

        public string UserName { get; private set; }

        public string Encoding { get; private set; }

        public static ApplicationOptions Parse(string[] args)
        {
            ApplicationOptions result = new ApplicationOptions();
            int flags = 0;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (0 == string.Compare("-s", arg, true))
                {
                    result.Server = args[++i];
                    flags |= 0x01;
                }
                else if (0 == string.Compare("-db", arg, true))
                {
                    result.KnowledgeVault = args[++i];
                    flags |= 0x02;
                }
                else if (0 == string.Compare("-u", arg, true))
                {
                    result.UserName = args[++i];
                    flags |= 0x04;
                }
                else if (0 == string.Compare("-p", arg, true))
                {
                    result.Password = args[++i];
                    flags |= 0x08;
                }
                else if (0 == string.Compare("-t", arg, true))
                {
                    string nextArg = args[++i];

                    switch (nextArg.ToUpper())
                    {
                        case "FILE":
                        default:
                            result.PropertyType = PropertyType.File;
                            break;
                        case "ITEM":
                            result.PropertyType = PropertyType.Item;
                            break;
                        case "OBJECT":
                            result.PropertyType = PropertyType.CustomObject;
                            break;
                    }
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
                else if (0 == string.Compare("-a", arg, true))
                {
                    if (0 != (flags & 0x40))
                    {
                        throw new ArgumentException();
                    }
                    result.KnowledgeVault = args[++i];
                    flags |= 0x40;
                }
                else
                {
                    if (0 == (flags & 0x80))
                    {
                        result.PropertyName = arg;
                        flags |= 0x80;
                    }
                    else if (0 == (flags & 0x100))
                    {
                        result.FileName = arg;
                        flags |= 0x100;
                    }
                }
            }
            if (flags < 0x100)
            {
                throw new ArgumentException();
            }
            return result;
        }

    }
}
