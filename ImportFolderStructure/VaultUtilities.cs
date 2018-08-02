using System;
using System.Linq;
using AWS = Autodesk.Connectivity.WebServices;

namespace ImportFolderStructure
{
    static class VaultUtilities
    {
        public static AWS.Folder GetOrCreateFolder(AWS.DocumentService docSvc, string path, bool libraryFolder)
        {
            AWS.Folder result = null;

            if (String.IsNullOrEmpty(path))
                return result;

            if (path.EndsWith("/"))
            {
                path = path.TrimEnd('/');
            }
            try
            {
                result = docSvc.GetFolderByPath(path);
            }
            catch
            {
            }
            if (result != null)
            {
                return result;
            }
            // doesn't exist, create new one
            AWS.Folder parent = null;
            string[] subPaths = path.Split('/');
            string path2 = string.Empty;

            foreach (string subPath in subPaths)
            {
                AWS.Folder folder = null;

                if (path2.Length > 0)
                {
                    path2 += "/";
                }
                path2 += subPath;
                if (path2.Equals("$"))
                {
                    folder = docSvc.GetFolderRoot();
                }
                else
                {
                    AWS.Folder[] subFolders = docSvc.GetFoldersByParentId(parent.Id, false);

                    if (subFolders != null)
                    {
                        folder = subFolders.FirstOrDefault(f => f.FullName.Equals(path2, StringComparison.OrdinalIgnoreCase));
                    }
                }
                if (folder == null)
                {
                    if (parent.ParId != -1)
                    {
                        libraryFolder = parent.IsLib;
                    }
                    folder = docSvc.AddFolder(subPath, parent.Id, libraryFolder);
                }
                parent = folder;
            }
            return parent;
        }
    }
}
