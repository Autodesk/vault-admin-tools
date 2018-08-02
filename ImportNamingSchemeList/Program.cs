using System;
using VDF = Autodesk.DataManagement.Client.Framework;

namespace ImportNamingSchemeList
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ApplicationOptions options = ApplicationOptions.Parse(args);
                Application app = new Application();

                System.Net.ServicePointManager.Expect100Continue = true;
                Application.PrintHeader();
                app.Run(options);
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                {
                    Application.PrintHelp();
                }
                else
                {
                    Console.WriteLine("ERROR: {0}", VDF.Library.ExceptionParser.GetMessage(ex));
                }
            }
        }
    }
}
