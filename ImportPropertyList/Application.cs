using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using Autodesk.Connectivity.WebServices;
using Autodesk.Connectivity.WebServicesTools;
using VDF = Autodesk.DataManagement.Client.Framework;

namespace ImportPropertyList
{
    class Application
    {

        private static Encoding _encoding = Encoding.UTF8;
        private WebServiceManager ServiceManager { get; set; }

        public void Run(ApplicationOptions options)
        {
            VDF.Vault.Results.LogInResult result =
                VDF.Vault.Library.ConnectionManager.LogIn(options.Server, options.KnowledgeVault, options.UserName, options.Password, 
                    VDF.Vault.Currency.Connections.AuthenticationFlags.Standard, null);

            if (!result.Success)
            {
                if (result.Exception == null)
                {
                    if (result.ErrorMessages.Count > 0)
                    {
                        string message = result.ErrorMessages.ElementAt(0).Key.ToString() + ", " + result.ErrorMessages.ElementAt(0).Value;
                        throw new ApplicationException(message);
                    }
                    else
                    {
                        throw new ApplicationException("Error connecting to Vault.");
                    }
                }
                else
                {
                    throw result.Exception;
                }
            }

            try
            {
                ServiceManager = result.Connection.WebServiceManager;

                //VSK-223 - Use Encoding
                SetFileEncodingOption(options);

                List<string> values = ReadValues(options.FileName);

                if (0 == values.Count)
                {
                    throw new ApplicationException();
                }
                if (PropertyType.File == options.PropertyType)
                {
                    ImportProperty("FILE", options.PropertyName, values.ToArray());
                }
                else if (PropertyType.Item == options.PropertyType)
                {
                    ImportProperty("ITEM", options.PropertyName, values.ToArray());
                }
                else
                {
                    ImportProperty("CUSTENT", options.PropertyName, values.ToArray());
                }
            }
            finally
            {
                VDF.Vault.Library.ConnectionManager.CloseAllConnections();
            }
        }

        public static void PrintHeader()
        {
            Console.WriteLine("ImportPropertyList v{0} - imports property list",
              Assembly.GetExecutingAssembly().GetName().Version.ToString());            
            Console.WriteLine("");
        }

        public static void PrintUsage()
        {
            Console.WriteLine("Usage: ImportPropertyList [-s srv] [-db dbase] [-u user] [-p pwd] [-t File|Item|Object] propertyName filename");
            Console.WriteLine("  -s                Name of server (default = localhost).");
            Console.WriteLine("  -db               Name of database (default = Vault).");
            Console.WriteLine("  -u                UserName for access to database (default = Administrator).");
            Console.WriteLine("  -p                Password for access to database (default = empty password).");
            Console.WriteLine("  -e                Encoding. Provide either codepage or name. (default = UTF-8).");
            Console.WriteLine("  -t                Property Type (default = File).");
            Console.WriteLine("  propertyName      Property Name.");
            Console.WriteLine("  filename          File which contains property values.");
        }

        private static void SetFileEncodingOption(ApplicationOptions Options)
        {
            EncodingInfo[] encodingInfos = Encoding.GetEncodings();
            EncodingInfo encodingInfo = null;

            if (encodingInfos != null)
            {
                encodingInfo = encodingInfos.FirstOrDefault(e => e.Name.Equals(Options.Encoding, StringComparison.CurrentCultureIgnoreCase));
                if (encodingInfo == null)
                {
                    int codePage;

                    if (Int32.TryParse(Options.Encoding, out codePage))
                    {
                        encodingInfo = encodingInfos.FirstOrDefault(e => e.CodePage == codePage);
                    }
                }
            }
            if (encodingInfo == null)
            {
                throw new ApplicationException("Invalid value for encoding. Either valid code page or encoding name must be provided.");
            }
            _encoding = encodingInfo.GetEncoding();
        }

        private List<string> ReadValues(string fileName)
        {
            List<string> result = new List<string>();

            using (StreamReader reader = new StreamReader(fileName, _encoding))
            {
                string line;

                while (null != (line = reader.ReadLine()))
                {
                    result.Add(line);
                }
            }
            return result;
        }

        private void ImportProperty(string entityClassId, string propertyName, string[] values)
        {
            List<PropDef> propDefs = new List<PropDef>();
            PropDef[] tmp = ServiceManager.PropertyService.GetPropertyDefinitionsByEntityClassId(entityClassId);

            if (null != tmp)
            {
                propDefs.AddRange(tmp);
            }
            IEnumerable<PropDef> props = propDefs.Where(p => p.DispName == propertyName);

            if (props.Count() != 0)
            {
                PropDef propDef = props.ElementAt(0);
                EntClassAssoc entAssoc = propDef.EntClassAssocArray.SingleOrDefault(i => i.EntClassId == entityClassId);

                if (entAssoc == null)
                {
                    entAssoc = new EntClassAssoc();
                    entAssoc.EntClassId = entityClassId;
                    entAssoc.MapDirection = AllowedMappingDirection.ReadAndWrite;
                    List<EntClassAssoc> assocs = new List<EntClassAssoc>();

                    assocs.AddRange(propDef.EntClassAssocArray);
                    assocs.Add(entAssoc);
                    propDef.EntClassAssocArray = assocs.ToArray();
                }
                EntClassCtntSrcPropCfg[] contentMappings = null;
                PropConstr[] constraints = null;
                PropDefInfo[] propInfos = ServiceManager.PropertyService.GetPropertyDefinitionInfosByEntityClassId(entityClassId, new long[] { propDef.Id });

                if (propInfos != null)
                {
                    contentMappings = propInfos[0].EntClassCtntSrcPropCfgArray;
                    constraints = propInfos[0].PropConstrArray;
                }
                ServiceManager.PropertyService.UpdatePropertyDefinitionInfo(propDef, contentMappings, constraints, values);
                return;
            }
            // doesn't exist, create new one
            string systemName = Guid.NewGuid().ToString("D");

            ServiceManager.PropertyService.AddPropertyDefinition(systemName, propertyName, DataType.String, true, true, values[0], new string[] { entityClassId }, null, null, values);
        }
    }
}
