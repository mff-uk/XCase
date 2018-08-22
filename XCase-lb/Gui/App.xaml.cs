//#define consoletest

using System;
using System.Windows;
using System.Data;
using System.Xml;
using System.Configuration;
using XCase.Gui.MainMenuCommands;
using XCase.Model;
using XCase.View.Controls;

namespace XCase.Gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
		/// <summary>
		/// Application Entry Point.
		/// </summary>
		[System.STAThreadAttribute()]
		[System.Diagnostics.DebuggerNonUserCodeAttribute()]
		public static void Main(params string[] args)
		{
			//if (false)
			if (args.Length == 0 || !Array.Exists(args, s => s.StartsWith("-")))
			{
				ConsoleManager.Hide();
				SplashScreen splashScreen = new SplashScreen("resources/xcase%20splashscreen.png");
				splashScreen.Show(true);
			}
			XCase.Gui.App app = new XCase.Gui.App();
			
			app.InitializeComponent();
			app.Run();
		}

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
        	StartupManager startupManager = new StartupManager();
			startupManager.StartApp(e.Args);
        }
    }
}