using System;
using System.Web.Services.Protocols;
using VDF = Autodesk.DataManagement.Client.Framework;

namespace ImportPropertyList
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ApplicationOptions options = ApplicationOptions.Parse(args);
                Application app = new Application();

                Application.PrintHeader();
                app.Run(options);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                {
                    Application.PrintUsage();
                }
                else
                {
                    Console.WriteLine("ERROR: {0}", VDF.Library.ExceptionParser.GetMessage(ex));
                }
            }
        }
    }
}
