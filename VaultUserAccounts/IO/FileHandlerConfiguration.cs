using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace VaultUserAccounts.IO
{
    class FileHandlerConfiguration : IConfigurationSectionHandler
    {
        #region IConfigurationSectionHandler Members

        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            List<FileHandlerInfo> result = new List<FileHandlerInfo>();

            foreach (XmlNode node in section.ChildNodes)
            {
                try
                {
                    FileHandlerInfo handlerInfo = new FileHandlerInfo(node);

                    result.Add(handlerInfo);
                }
                catch
                {
                }
            }
            return result;
        }

        #endregion
    }
}
