using System.Collections.Generic;

namespace VaultUserAccounts.IO
{
    interface IGroupInfoReader
    {
        List<GroupInfo> ReadFile(string fileName);
    }
}
