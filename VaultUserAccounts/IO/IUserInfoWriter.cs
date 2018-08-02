using System.Collections.Generic;

namespace VaultUserAccounts.IO
{
    interface IUserInfoWriter
    {
        void WriteFile(string fileName, List<UserInfo> userInfos);
    }
}
