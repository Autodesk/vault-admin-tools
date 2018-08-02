using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VaultUserAccounts.IO
{
    class UserInfoReaderCSV : IUserInfoReader
    {

        private static char _separator;
        private static Encoding _encoding;

        public UserInfoReaderCSV(char separator, Encoding encoding)
        {
            _separator = separator;
            _encoding = encoding;
        }

        #region IUserInfoReader Members

        public List<UserInfo> ReadFile(string fileName)
        {
            List<UserInfo> result = new List<UserInfo>();
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
                    UserInfo newItem = new UserInfo();

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
                                newItem.FirstName = field;
                                break;
                            case 1:
                                newItem.LastName = field;
                                break;
                            case 2:
                                newItem.LoginName = field;
                                break;
                            case 3:
                                newItem.EMail = field;
                                break;
                            case 4:
                                newItem.Password = field;
                                break;
                            case 5:
                                string[] roles = field.Split(';');

                                if (null != roles)
                                {
                                    foreach (string role in roles)
                                    {
                                        newItem.Roles.Add(role);
                                    }
                                }
                                break;
                            case 6:
                                string[] vaults = field.Split(';');

                                if (null != vaults)
                                {
                                    foreach (string vault in vaults)
                                    {
                                        newItem.Vaults.Add(vault);
                                    }
                                }
                                break;
                            case 7:
                                string[] groups = field.Split(';');

                                if (null != groups)
                                {
                                    foreach (string group in groups)
                                    {
                                        newItem.Groups.Add(group);
                                    }
                                }
                                break;
                            case 8:
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
