using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Reflection;
using System.Linq;
using System.IO;
using XCase.Controller.Dialogs;

namespace XCase.Gui
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    { 
        public AboutWindow()
        {
            InitializeComponent();

            this.Icon = (System.Windows.Media.ImageSource)FindResource("X");
        }

		private void label1_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (System.IO.File.Exists(path + "\\Documentation\\XCaseUsersGuide.pdf"))
			{
                System.Diagnostics.ProcessStartInfo info =
                    new System.Diagnostics.ProcessStartInfo(path + "\\Documentation\\XCaseUsersGuide.pdf");
				info.UseShellExecute = true;
				info.Verb = "open";
				this.Close();
				System.Diagnostics.Process.Start(info);
				
			}
		}

		private void label2_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(@"http://xcase.codeplex.com");
			info.UseShellExecute = true;
			info.Verb = "open";
			this.Close();
			System.Diagnostics.Process.Start(info);
		}

        private void lCheckForUpdates_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Updater.Updater updater = new Updater.Updater();
            IEnumerable<string> files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll").Concat(Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.exe"));

            Dictionary<string, Version> clientVersions = new Dictionary<string, Version>();
            foreach (string file in files)
            {
                Version clientVersion = AssemblyName.GetAssemblyName(file).Version;
                clientVersions[Path.GetFileName(file)] = clientVersion;
            }

            Dictionary<string, Version> newAvailableVersions;
            if (updater.AreNewVersionsAvailable(clientVersions, out newAvailableVersions) && XCaseYesNoBox.ShowYesNoCancel("New version available", "New version is available. \r\nDo you wish to update?") == MessageBoxResult.Yes )
            {
                System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo("Updater.exe");
                info.UseShellExecute = true;
                this.Close();
                (Application.Current.MainWindow as MainWindow).Close();
                System.Diagnostics.Process.Start(info);
            }
            else
            {
                OkCancelDialog d = new OkCancelDialog();
                d.CancelButtonVisibility = Visibility.Collapsed;
                d.Title = "XCase Update";
                d.PrimaryContent = "Check for updates: ";
                d.SecondaryContent = "This is the latest version.";
                d.ShowDialog();
            }

        }
    }
}
