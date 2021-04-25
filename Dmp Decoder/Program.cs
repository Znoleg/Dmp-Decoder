using System;
using System.Threading;
using System.Windows.Forms;

namespace Dmp_Decoder
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Application.ThreadException += new ThreadExceptionEventHandler(Form1.HandleExceptions);
            //Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            var form = new Form1();
            if (args != null && args.Length > 0)
            {
                Console.WriteLine(args[0]);
                form = new Form1(args[0]);
            }

            Application.Run(form);
        }

        
    }
}
