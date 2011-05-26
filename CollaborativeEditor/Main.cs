using System;
using System.IO;
using Gtk;

namespace SteticTutorial
{
	class MainClass
	{
		public static void Main(string[] args)
		{
            try
            {
                Application.Init();
                MainWindow win = new MainWindow();
                win.Show();
                Application.Run();
            }

            // Make sure all exceptions get logged, no matter what.
            catch (Exception e)
            {
                using (TextWriter tw = new StreamWriter("Log.txt", false))
                    tw.WriteLine(e.ToString());

                throw;
            }
		}
	}
}
