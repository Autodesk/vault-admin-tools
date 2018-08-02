using System.Collections.Generic;

namespace VaultUserAccounts.IO
{
    interface IUserInfoReader
    {
        List<UserInfo> ReadFile(string fileName);
    }
}
