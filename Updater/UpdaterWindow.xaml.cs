using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using XCase.Updater.Properties;

namespace XCase.Updater
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class UpdaterWindow
    {
        public UpdaterWindow()
        {
            InitializeComponent();

            Updater = new Updater();
            CheckForNewVersions(false);
        }

        Dictionary<string, Version> newAvailableVersions;
        private Dictionary<string, Version> clientVersions;

        private void CheckForNewVersions(bool canReinstall)
        {
            clientVersions = GetClientVersions();

            if (Updater.AreNewVersionsAvailable(clientVersions, out newAvailableVersions))
            {
                if (Updater.MustReinstal(clientVersions))
                {
                    label1.Content = "New version is available. ";
                    if (canReinstall && MessageBox.Show("Automatic update can not continue, new version must be installed. \r\nProceed with instalation now?", "Reainstallation needed", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Reinstall();
                    }
                }
                else
                {
                    label1.Content = "New version is available. ";
                    bDownload.IsEnabled = true;
                }
            }
            else
            {
                label1.Content = "You are using the latest version. ";
                bDownload.IsEnabled = false;
            }


        }

        private void Reinstall()
        {
            try
            {
                downloadSuccessfull =
                    delegate(Dictionary<string, string> files)
                    {
                        if (File.Exists(files.First().Key))
                            File.Delete(files.First().Key);
                        File.Move(files.First().Value, files.First().Key);
                        System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo("XCase.msi");
                        info.UseShellExecute = true;
                        this.Dispatcher.Invoke(new Action(this.Close));
                        System.Diagnostics.Process.Start(info);
                    };
                StartDownloading(new[] { "XCase.msi" });
            }
            catch
            {
                downloadSuccessfull = null;
                throw;
            }
        }

        private Dictionary<string, Version> GetClientVersions()
        {
            IEnumerable<string> files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.dll").Concat(Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.exe"));

            Dictionary<string, Version> clientVersions = new Dictionary<string, Version>();
            foreach (string file in files)
            {
                Version clientVersion = AssemblyName.GetAssemblyName(file).Version;
                clientVersions[Path.GetFileName(file)] = clientVersion;
            }
            return clientVersions;
        }

        public Updater Updater { get; set; }

        private void bCheck_Click(object sender, RoutedEventArgs e)
        {
            CheckForNewVersions(true);
        }

        private void bDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                downloadSuccessfull =
                    delegate(Dictionary<string, string> files)
                    {
                        foreach (KeyValuePair<string, string> downloadedFile in files)
                        {
                            if (File.Exists(downloadedFile.Key))
                                File.Delete(downloadedFile.Key);
                            File.Move(downloadedFile.Value, downloadedFile.Key);
                        }
                        label1.Dispatcher.Invoke(new Action(() => label1.Content = "Update successful."));
                    };
                StartDownloading(newAvailableVersions.Keys);
            }
            catch
            {
                downloadSuccessfull = null;
                throw;
            }
        }

        private void StartDownloading(IEnumerable<string> files)
        {
            bDownload.IsEnabled = false;
            bCheck.IsEnabled = false;
            bCancel.Visibility = Visibility.Visible; 
            if (files != null && files.Count() > 0)
            {
                // Let the user know we are connecting to the server
                lblProgress.Content = "Download Starting";
                // Create a new thread that calls the Download() method
                thrDownload = new Thread(_files => Download((IEnumerable<string>)_files));
                // Start the thread, and thus vcall Download()
                thrDownload.Start(files);
            }
        }

        #region fields
        // The thread inside which the download happens
        private Thread thrDownload;
        // The stream of data retrieved from the web server
        private Stream strResponse;
        // The stream of data that we write to the harddrive
        private FileStream strLocal;
        // The request to the web server for file information
        private HttpWebRequest webRequest;
        // The response from the web server containing information about the file
        private HttpWebResponse webResponse;
        // The progress of the download in percentage
        private static int PercentProgress;
        #endregion

        private void UpdateProgress(Int64 BytesRead, Int64 TotalBytes)
        {
            // Calculate the download progress in percentages
            PercentProgress = Convert.ToInt32((BytesRead * 100) / TotalBytes);

            // Make progress on the progress bar
            prgDownload.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => { prgDownload.Value = PercentProgress; }));

            // Display the current progress on the form
            lblProgress.Dispatcher.Invoke(new Action(() => lblProgress.Content = string.Format("Downloaded {0} kB out of {1} kB ({2}%).", BytesRead / 1000, TotalBytes / 1000, PercentProgress)));
        }

        private Dictionary<string, string> downloadedFiles;

        private void Download(IEnumerable<string> files)
        {
            downloadedFiles = new Dictionary<string, string>();
            using (WebClient wcDownload = new WebClient())
            {
                try
                {
                    int i = 0;
                    foreach (string file in files)
                    {
                        i++;
                        string downloadUrl = Updater.GetDownloadUrl(file);
                        lFile.Dispatcher.Invoke(new Action(() => lFile.Content = String.Format("Downloading file {0} (file {1} of {2})", file, i, newAvailableVersions.Count)));
                        prgDownload.Dispatcher.Invoke(new Action(() => { prgDownload.Visibility = Visibility.Visible; prgDownload.Value = 0; }));
                        // Create a request to the file we are downloading
                        webRequest = (HttpWebRequest)WebRequest.Create(downloadUrl);
                        // Set default authentication for retrieving the file
                        webRequest.Credentials = CredentialCache.DefaultCredentials;
                        // Retrieve the response from the server
                        webResponse = (HttpWebResponse)webRequest.GetResponse();
                        // Ask the server for the file size and store it
                        Int64 fileSize = webResponse.ContentLength;

                        // Open the URL for download 
                        strResponse = wcDownload.OpenRead(downloadUrl);
                        // Create a new file stream where we will be saving the data (local drive)
                        string tmpFile = file + ".tmp";
                        downloadedFiles[file] = tmpFile;
                        strLocal = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.None);

                        // It will store the current number of bytes we retrieved from the server
                        int bytesSize = 0;
                        // A buffer for storing and writing the data retrieved from the server
                        byte[] downBuffer = new byte[2048];

                        // Loop through the buffer until the buffer is empty
                        while ((bytesSize = strResponse.Read(downBuffer, 0, downBuffer.Length)) > 0)
                        {
                            // Write the data from the buffer to the local hard drive
                            strLocal.Write(downBuffer, 0, bytesSize);
                            // Invoke the method that updates the form's label and progress bar
                            UpdateProgress(strLocal.Length, fileSize);
                        }
                    }
                }
                finally
                {
                    // When the above code has ended, close the streams
                    strResponse.Close();
                    strLocal.Close();

                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        prgDownload.Visibility = Visibility.Collapsed;
                        lFile.Content = null;
                        bCancel.Visibility = Visibility.Hidden;
                        lblProgress.Content = null;
                        bCheck.IsEnabled = true;
                    }));
                }

                if (downloadSuccessfull != null)
                    downloadSuccessfull(downloadedFiles);
            }
        }

        private delegate void DownloadSuccessfullHandler(Dictionary<string, string> downloadedFiles);

        private DownloadSuccessfullHandler downloadSuccessfull;

        private void bClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            // Close the web response and the streams
            webResponse.Close();
            strResponse.Close();
            strLocal.Close();
            // Abort the thread that's downloading
            thrDownload.Abort();
            // Set the progress bar back to 0 and the label
            prgDownload.Value = 0;
            lblProgress.Content = "Download cancelled.";

            foreach (string tmpFile in downloadedFiles.Values)
            {
                try
                {
                    if (File.Exists(tmpFile))
                        File.Delete(tmpFile);
                }
                catch (Exception)
                {
                    
                }
            }

            prgDownload.Visibility = Visibility.Collapsed;
            lFile.Content = null;
            lblProgress.Content = null;
            bCheck.IsEnabled = true;
            bDownload.IsEnabled = true;
            bCancel.Visibility = Visibility.Hidden; 
        }
    }
}
