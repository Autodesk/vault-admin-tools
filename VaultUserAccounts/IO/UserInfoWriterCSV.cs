using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VaultUserAccounts.IO
{
    class UserInfoWriterCSV : IUserInfoWriter
    {
        private static char _separator;
        private static Encoding _encoding;

        public UserInfoWriterCSV(char separator, Encoding encoding)
        {
            _separator = separator;
            _encoding = encoding;
        }

        public void WriteFile(string fileName, List<UserInfo> userInfos)
        {
            using (StreamWriter writer = new StreamWriter(fileName, false, _encoding))
            {
                writer.WriteLine("FirstName" + _separator + "LastName" + _separator + "Username" + _separator +
                                 "Email" + _separator + "Password" + _separator + "Role" + _separator +
                                 "Vault" + _separator + "Group" + _separator + "Enable");
                foreach (UserInfo userInfo in userInfos)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendFormat("{0}{1}", FormatString(userInfo.FirstName),_separator);
                    sb.AppendFormat("{0}{1}", FormatString(userInfo.LastName), _separator);
                    sb.AppendFormat("{0}{1}", FormatString(userInfo.LoginName), _separator);
                    sb.AppendFormat("{0}{1}", FormatString(userInfo.EMail), _separator);
                    sb.AppendFormat("{0}{1}", FormatString(userInfo.Password), _separator);
                    sb.AppendFormat("{0}{1}", ListToString(userInfo.Roles), _separator);
                    sb.AppendFormat("{0}{1}", ListToString(userInfo.Vaults), _separator);
                    sb.AppendFormat("{0}{1}", ListToString(userInfo.Groups), _separator);
                    sb.AppendFormat("{0}", userInfo.Active == false ? "0" : "1");
                    writer.WriteLine(sb.ToString());
                }
                writer.Close();
            }
        }

        private string ListToString(IEnumerable<string> names)
        {
            string result = string.Empty;

            foreach (string tmp in names)
            {
                if (0 < result.Length)
                {
                    result += ";";
                }
                result += tmp;
            }
            return result;
        }

        private static string FormatString(string text)
        {
            string result = string.Empty;

            if (text.Contains(","))
            {
                result = string.Format("\"{0}\"", text);
            }
            else
            {
                result = text;
            }
            return result;
        }

    }
}
