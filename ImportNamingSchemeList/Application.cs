using Autodesk.Connectivity.WebServicesTools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using AWS = Autodesk.Connectivity.WebServices;
using VDF = Autodesk.DataManagement.Client.Framework;

namespace ImportNamingSchemeList
{
    class Application
    {
        public Application()
        {
            Logger = new ConsoleLogger();
        }

        public ILogger Logger { get; set; }

        private ApplicationOptions Options { get; set; }
        private DataTable Data { get; set; }
        private WebServiceManager ServiceManager { get; set; }

        public static void PrintHeader()
        {
            Console.WriteLine("ImportNamingSchemeList v{0} - imports naming scheme list values from CSV file",
                Assembly.GetExecutingAssembly().GetName().Version.ToString());
            Console.WriteLine("");
        }

        public static void PrintHelp()
        {
            Console.WriteLine("Usage: ImportNamingSchemeList [-t File|Item] [-a Vault|Windows] [-s srv] [-db dbase] [-u user] [-p pwd] [-n name] filename");
            Console.WriteLine("  -a                Authentication type (default = Vault).");
            Console.WriteLine("  -s                Name of server (default = localhost).");
            Console.WriteLine("  -db               Name of database (default = Vault).");
            Console.WriteLine("  -u                UserName for access to database (default = Administrator).");
            Console.WriteLine("  -p                Password for access to database (default = empty password).");
            Console.WriteLine("  -t                Type of naming scheme - either File or Item (default = File).");
            Console.WriteLine("  -n                Scheme name.");
            Console.WriteLine("  filename          CSV File with scheme values.");
        }

        public void Run(ApplicationOptions options)
        {
            Options = options;

            VDF.Vault.Currency.Connections.AuthenticationFlags authFlags = VDF.Vault.Currency.Connections.AuthenticationFlags.Standard;

            if (Options.AuthenticationType == AWS.AuthTyp.ActiveDir)
            {
                authFlags = VDF.Vault.Currency.Connections.AuthenticationFlags.WindowsAuthentication;
            }
            VDF.Vault.Results.LogInResult result =
                VDF.Vault.Library.ConnectionManager.LogIn(Options.Server, Options.Vault, Options.UserName, Options.Password, authFlags, null);

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

                Data = ReadData();
                if (Options.SchemeType == SchemeType.File)
                {
                    ProcessFileScheme();
                }
                else if (Options.SchemeType == SchemeType.Item)
                {
                    ProcessItemScheme();
                }
            }
            finally
            {
                VDF.Vault.Library.ConnectionManager.CloseAllConnections();
            }
        }

        protected void Log(MessageCategory category, string message)
        {
            if (Logger != null)
            {
                Logger.Log(category, message);
            }
        }

        protected void Log(MessageCategory category, string format, params object[] args)
        {
            if (Logger != null)
            {
                Logger.Log(category, format, args);
            }
        }

        private DataTable ReadData()
        {
            return CSVDataReader.ReadFile(Options.InputFile);
        }

        private void ProcessFileScheme()
        {
            AWS.NumSchm[] schemes = ServiceManager.DocumentService.GetNumberingSchemesByType(AWS.NumSchmType.All);
            AWS.NumSchm scheme = schemes.FirstOrDefault(s => s.Name.Equals(Options.SchemeName, StringComparison.InvariantCultureIgnoreCase));

            if (scheme == null)
            {
                string message = string.Format("Unable to find scheme: {0}", Options.SchemeName);

                throw new InvalidOperationException(message);
            }
            Log(MessageCategory.Info, "Processing scheme '{0}'", scheme.Name);
            int fieldCount = 0;

            foreach (AWS.NumSchmField field in scheme.FieldArray)
            {
                if (field.FieldTyp != AWS.FieldType.PredefinedList)
                {
                    continue;
                }
                if (ProcessField((AWS.PredefListField)field))
                {
                    fieldCount++;
                }
            }
            if (fieldCount > 0)
            {
                Log(MessageCategory.Info, "Saving scheme changes");
                ServiceManager.DocumentService.UpdateNumberingScheme(scheme.SchmID, scheme.Name, scheme.FieldArray, scheme.ToUpper);
            }
            else
            {
                Log(MessageCategory.Warning, "No scheme changes detected");
            }
        }

        private void ProcessItemScheme()
        {
            AWS.NumSchm[] schemes = ServiceManager.ItemService.GetNumberingSchemesByType(AWS.NumSchmType.All);
            AWS.NumSchm scheme = schemes.FirstOrDefault(s => s.Name.Equals(Options.SchemeName, StringComparison.InvariantCultureIgnoreCase));

            if (scheme == null)
            {
                string message = string.Format("Unable to find scheme: {0}", Options.SchemeName);

                throw new InvalidOperationException(message);
            }
            Log(MessageCategory.Info, "Processing scheme '{0}'", scheme.Name);
            int fieldCount = 0;

            foreach (AWS.NumSchmField field in scheme.FieldArray)
            {
                if (field.FieldTyp != AWS.FieldType.PredefinedList)
                {
                    continue;
                }
                if (ProcessField((AWS.PredefListField)field))
                {
                    fieldCount++;
                }
            }
            if (fieldCount > 0)
            {
                Log(MessageCategory.Info, "Saving scheme changes");
                ServiceManager.ItemService.UpdateNumberingScheme(scheme);
            }
            else
            {
                Log(MessageCategory.Warning, "No scheme changes detected");
            }
        }

        private bool ProcessField(AWS.PredefListField field)
        {
            Log(MessageCategory.Info, "Processing field '{0}'", field.Name);
            DataColumn colCode = Data.Columns[field.Name];
            string name;

            if (colCode == null)
            {
                name = string.Format("{0}(Code)", field.Name);
                colCode = Data.Columns[name];
            }
            if (colCode == null)
            {
                Log(MessageCategory.Warning, "No column found for given field");
                return false;
            }
            name = string.Format("{0}(Description)", field.Name);
            DataColumn colDescription = Data.Columns[name];

            if (colDescription != null)
            {
                Log(MessageCategory.Info, "Description column is available - description value will be imported");
            }
            List<AWS.CodeWord> listValues = new List<AWS.CodeWord>();

            foreach (DataRow row in Data.Rows)
            {
                AWS.CodeWord value = new AWS.CodeWord();

                value.Code = row.Field<string>(colCode);
                if (string.IsNullOrEmpty(value.Code))
                {
                    continue;
                }
                if (colDescription != null)
                {
                    value.Descr = row.Field<string>(colDescription);
                }
                listValues.Add(value);
            }
            if (listValues.Count == 0)
            {
                return false;
            }
            field.DfltVal = listValues[0].Code;
            field.CodeArray = listValues.ToArray();
            Log(MessageCategory.Info, "Number of field values: {0}", field.CodeArray.Length);
            return true;
        }
    }
}
