using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyPS
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string commandName = args[0];

                IHistory history = new HistorySetting();

                using (PSCommandContainer commandParameters = new PSCommandContainer(commandName))
                {
                    if (commandParameters.Setup(history))
                    {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new EasyPSForm(commandName, commandParameters));
                    }
                }
            }
            else
            {
                Console.WriteLine("ERROR: No command given.");
            }
        }
    }
}
