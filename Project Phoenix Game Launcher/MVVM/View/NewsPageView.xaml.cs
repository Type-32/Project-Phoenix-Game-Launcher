using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text.Json.Nodes;
using Path = System.IO.Path;
using Project_Phoenix_Game_Launcher;

namespace Project_Phoenix_Game_Launcher.MVVM.View
{
    public partial class NewsPageView : UserControl
    {
        public List<string> CachedVersions = new();
        public List<string> CachedVersionDates = new();
        public List<string> CachedVersionContents = new();
        public NewsPageView()
        {
            InitializeComponent();
            RefreshUpdateLogs();
            //MainWindow.WindowContentRendered += CheckForUpdates;
            //CheckForUpdates();
        }
        public async void RefreshUpdateLogs()
        {
            var versions = await GetAllCloudVersions();
            if (versions != null) CachedVersions = versions;
            var contents = await GetAllCloudVersionContents();
            if (contents != null) CachedVersionContents = contents;
            var dates = await GetAllCloudVersionDates();
            if (dates != null) CachedVersionDates = dates;
        }
        public async Task<List<string>?> GetAllCloudVersions()
        {
            HttpClient client = new HttpClient();

            var res = await client.GetStringAsync(LauncherConfig.RELEASE_REPO);
            var releases = JsonArray.Parse(res);

            if (releases == null || releases.AsArray().Count == 0)
            {
                MessageBox.Show("An Error Ocurred when trying to fetch Cloud Version: Cannot find Latest Version");
                return null;
            }
            List<string> result = new List<string>();
            for (int i = 0; i < releases.AsArray().Count; i++)
            {
                var release = (string)releases.AsArray()[i][LauncherConfig.VERSION_FETCH_KEY];
                if (release != null)
                {
                    result.Add(release);
                }
            }
            return result;
        }
        public async Task<List<string>?> GetAllCloudVersionContents()
        {
            HttpClient client = new HttpClient();

            var res = await client.GetStringAsync(LauncherConfig.RELEASE_REPO);
            var contents = JsonArray.Parse(res);

            if (contents == null || contents.AsArray().Count == 0)
            {
                MessageBox.Show("An Error Ocurred when trying to fetch Cloud Version: Cannot find Latest Version");
                return null;
            }
            List<string> result = new List<string>();
            for (int i = 0; i < contents.AsArray().Count; i++)
            {
                var content = (string)contents.AsArray()[i][LauncherConfig.VERSION_CONTENT_FETCH_KEY];
                if (content != null)
                {
                    result.Add(content);
                }
            }
            return result;
        }
        public async Task<List<string>?> GetAllCloudVersionDates()
        {
            HttpClient client = new HttpClient();

            var res = await client.GetStringAsync(LauncherConfig.RELEASE_REPO);
            var dates = JsonArray.Parse(res);

            if (dates == null || dates.AsArray().Count == 0)
            {
                MessageBox.Show("An Error Ocurred when trying to fetch Cloud Data: Cannot find Latest Version");
                return null;
            }
            List<string> result = new List<string>();
            for (int i = 0; i < dates.AsArray().Count; i++)
            {
                var date = (string)dates.AsArray()[i][LauncherConfig.VERSION_DATE_FETCH_KEY];
                if (date != null)
                {
                    result.Add(date);
                }
            }
            return result;
        }
    }
}