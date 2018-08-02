using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VaultUserAccounts.IO
{
    class GroupInfoReaderCSV : IGroupInfoReader
    {

        private static char _separator;
        private static Encoding _encoding;

        public GroupInfoReaderCSV(char separator, Encoding encoding)
        {
            _separator = separator;
            _encoding = encoding;
        }

        #region IGroupInfoReader Members

        public List<GroupInfo> ReadFile(string fileName)
        {
            List<GroupInfo> result = new List<GroupInfo>();

            using (StreamReader reader = new StreamReader(fileName, _encoding))
            {
                string line;
                bool firstLine = true;

                while (null != (line = reader.ReadLine()))
                {
                    if (true == firstLine)
                    {
                        firstLine = false;
                        continue;
                    }
                    List<string> fields = StringUtilities.ParseLine(line, _separator);

                    if (fields.Count == 0)
                    {
                        continue;
                    }
                    GroupInfo newItem = new GroupInfo();

                    for (int i = 0; i < fields.Count; i++)
                    {
                        string field = fields[i];

                        if (true == string.IsNullOrEmpty(field))
                        {
                            continue;
                        }
                        switch (i)
                        {
                            case 0:
                                newItem.Name = field;
                                break;
                            case 1:
                                newItem.EMail = field;
                                break;
                            case 2:
                                string[] roles = field.Split(';');

                                if (null != roles)
                                {
                                    foreach (string role in roles)
                                    {
                                        newItem.Roles.Add(role);
                                    }
                                }
                                break;
                            case 3:
                                string[] vaults = field.Split(';');

                                if (null != vaults)
                                {
                                    foreach (string vault in vaults)
                                    {
                                        newItem.Vaults.Add(vault);
                                    }
                                }
                                break;
                            case 4:
                                string[] groups = field.Split(';');

                                if (null != groups)
                                {
                                    foreach (string group in groups)
                                    {
                                        newItem.Groups.Add(group);
                                    }
                                }
                                break;
                            case 5:
                                newItem.Active = (0 == string.Compare(field, "1") ? true : false);
                                break;
                        }
                    }
                    result.Add(newItem);
                }
            }
            return result;
        }

        #endregion

    }
}
