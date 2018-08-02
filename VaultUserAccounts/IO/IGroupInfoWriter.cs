using System.Collections.Generic;

namespace VaultUserAccounts.IO
{
    interface IGroupInfoWriter
    {
        void WriteFile(string fileName, List<GroupInfo> groupInfos);
    }
}
