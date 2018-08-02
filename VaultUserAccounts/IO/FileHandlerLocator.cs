using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace VaultUserAccounts.IO
{
    class FileHandlerLocator
    {
        private static readonly FileHandlerLocator _instance = new FileHandlerLocator();

        public static FileHandlerLocator Instance
        {
            get { return _instance; }
        }

        public T GetHandler<T>(string filename)
        {
            List<FileHandlerInfo> fileHandlers = (List<FileHandlerInfo>)ConfigurationManager.GetSection("fileHandlers");
            string ext = Path.GetExtension(filename).Substring(1);
            FileHandlerInfo resultHandler = null;

            foreach (FileHandlerInfo handlerInfo in fileHandlers)
            {
                bool supportsThisType = false;

                foreach (string tmp in handlerInfo.SupportedExtensions)
                {
                    if (0 == string.Compare(tmp, ext, true))
                    {
                        supportsThisType = true;
                        break;
                    }
                }
                if (true == supportsThisType)
                {
                    resultHandler = handlerInfo;
                    break;
                }
            }
            if (null == resultHandler)
            {
                resultHandler = fileHandlers[0];
            }
            Type handlerType = null;

            if (true == HasInterface(resultHandler.ReaderType, typeof(T)))
            {
                handlerType = resultHandler.ReaderType;
            }
            if (true == HasInterface(resultHandler.WriterType, typeof(T)))
            {
                handlerType = resultHandler.WriterType;
            }
            if (null == handlerType)
            {
                throw new ApplicationException("Failed to get type from file handler");
            }
            T handler = (T)Activator.CreateInstance(handlerType);

            return handler;
        }

        private bool HasInterface(Type t, Type interfaceType)
        {
            foreach (Type tmp in t.GetInterfaces())
            {
                if (tmp == interfaceType)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
