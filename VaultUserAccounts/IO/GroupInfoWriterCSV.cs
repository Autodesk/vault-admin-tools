using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VaultUserAccounts.IO
{
    class GroupInfoWriterCSV : IGroupInfoWriter
    {
        private static char _separator;
        private static Encoding _encoding;

        public GroupInfoWriterCSV(char separator, Encoding encoding)
        {
            _separator = separator;
            _encoding = encoding;
        }

        public void WriteFile(string fileName, List<GroupInfo> groupInfos)
        {
            using (StreamWriter writer = new StreamWriter(fileName,false, _encoding))
            {
                writer.WriteLine("Name" + _separator + "Email" + _separator + "Role" + _separator + 
                                 "Vault" + _separator + "Group" + _separator + "Enable");

                foreach (GroupInfo groupInfo in groupInfos)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendFormat("{0}{1}", StringUtilities.FormatString(groupInfo.Name), _separator);
                    sb.AppendFormat("{0}{1}", StringUtilities.FormatString(groupInfo.EMail), _separator);
                    sb.AppendFormat("{0}{1}", StringUtilities.ListToString(groupInfo.Roles), _separator);
                    sb.AppendFormat("{0}{1}", StringUtilities.ListToString(groupInfo.Vaults), _separator);
                    sb.AppendFormat("{0}{1}", StringUtilities.ListToString(groupInfo.Groups), _separator);
                    sb.AppendFormat("{0}", groupInfo.Active == false ? "0" : "1");
                    writer.WriteLine(sb.ToString());
                }
                writer.Close();
            }
        }

    }
}
