using System;
using System.Collections.Generic;
using System.Xml;

namespace VaultUserAccounts.IO
{
    class FileHandlerInfo
    {
        private List<string> _supportedExtensions = new List<string>();
        private Type _readerType = null;
        private Type _writerType = null;

        public FileHandlerInfo(XmlNode node)
        {
            _readerType = Type.GetType(node.Attributes["readerType"].Value);
            _writerType = Type.GetType(node.Attributes["writerType"].Value);
            string[] exts = node.Attributes["extensions"].Value.Split(';');

            foreach (string ext in exts)
            {
                _supportedExtensions.Add(ext);
            }
        }

        public string[] SupportedExtensions
        {
            get { return _supportedExtensions.ToArray(); }
        }

        public Type ReaderType
        {
            get { return _readerType; }
        }

        public Type WriterType
        {
            get { return _writerType; }
        }
    }
}
