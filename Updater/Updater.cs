using System;
using System.Collections.Generic;
using System.Linq;
using XCase.Updater.UpdaterService;
using Version = System.Version;

namespace XCase.Updater
{
    public class Updater
    {
        private string updaterServiceUrl;
        public string UpdaterServiceUrl
        {
            get { return updaterServiceUrl; }
            set { 
                if (updaterServiceUrl != value)
                {
                    updaterServiceUrl = value;
                    if (service != null)
                    {
                        UpdaterService.Url = value; 
                    }
                } 
            }
        }

        private UpdaterService.UpdaterService service;

        public UpdaterService.UpdaterService UpdaterService
        {
            get 
            {
                if (service == null)
                {
                    service = new UpdaterService.UpdaterService();
                    if (!string.IsNullOrEmpty(UpdaterServiceUrl))
                    {
                        service.Url = UpdaterServiceUrl;
                    }
                }
                return service; 
            }
        }

        public bool MustReinstal(Dictionary<string, Version> clientVersions)
        {
            return UpdaterService.MustReinstal(
                clientVersions.Keys.ToArray(), 
                clientVersions.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value.ToString()).ToArray());
        }

        public bool AreNewVersionsAvailable(Dictionary<string, Version> clientVersions, out Dictionary<string, Version> newAvailableVersions)
        {
            string[] _versions = UpdaterService.GetVersions(clientVersions.Keys.ToArray());
            List<Version> versions = _versions.Select(_v => string.IsNullOrEmpty(_v) ? null : new Version(_v)).ToList();
            newAvailableVersions = new Dictionary<string, Version>();

            int i = 0;
            foreach (KeyValuePair<string, Version> kvp in clientVersions)
            {
                Version serverVersion = versions[i];
                Version clientVersion = kvp.Value;

                if (serverVersion != null)
                {
                    //if (serverVersion > clientVersion)
                        newAvailableVersions[kvp.Key] = serverVersion;
                }
                i++;
            }

            return newAvailableVersions.Count > 0;
        }

        public string GetDownloadUrl(string file)
        {
            return string.Format("{0}GetFile.ashx?filename={1}", UpdaterService.Url.Remove(UpdaterService.Url.IndexOf("UpdaterService.asmx")), file);
        }
    }
}