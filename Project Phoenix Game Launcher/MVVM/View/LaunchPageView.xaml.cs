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
using System.Runtime.InteropServices;

namespace Project_Phoenix_Game_Launcher.MVVM.View
{
    /// <summary>
    /// Interaction logic for LaunchPageView.xaml
    /// </summary>

    public class LauncherConfig
    {
        public static string RELEASE_REPO = "https://repo.smartsheep.studio/api/v1/repos/CRTL_Prototype_Studios/Project_Phoenix_Files/releases";
        public static string DIST_DOWNLOAD_KEY = "Build.zip";
        public static string MAC_DIST_DOWNLOAD_KEY = "Silicon.app";
        public static string VERSION_FETCH_KEY = "name";
        public static string VERSION_DATE_FETCH_KEY = "created_at";
        public static string VERSION_CONTENT_FETCH_KEY = "body";
    }
    public static class LocalOperatingSystem
    {
        public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public static bool IsMacOS() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        public static bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }

    enum LauncherStatus
    {
        Ready,
        Failed,
        DownloadingGame,
        DownloadingUpdate
    }
    public partial class LaunchPageView : UserControl
    {
        private string rootPath;
        private string versionFile;
        private string gameZip;
        private string gameExe;
        private string gameSilicon;
        private LauncherStatus _status;
        internal LauncherStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case LauncherStatus.Ready:
                        PlayButton.Content = "Launch Game";
                        break;
                    case LauncherStatus.Failed:
                        PlayButton.Content = "Update Failed - Retry";
                        break;
                    case LauncherStatus.DownloadingGame:
                        PlayButton.Content = "Downloading...";
                        break;
                    case LauncherStatus.DownloadingUpdate:
                        PlayButton.Content = "Updating...";
                        break;
                    default:
                        break;
                }
            }
        }
        public LaunchPageView()
        {
            InitializeComponent();
            rootPath = Directory.GetCurrentDirectory();
            versionFile = Path.Combine(rootPath, "Version.txt");
            gameZip = Path.Combine(rootPath, "Build.zip");
            gameExe = Path.Combine(rootPath, "Build", "Project Phoenix.exe");
            gameSilicon = Path.Combine(rootPath, "Silicon.app");
            //MainWindow.WindowContentRendered += CheckForUpdates;
            CheckForUpdates();
        }

        private async void CheckForUpdates()
        {
            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = "Game Version " + localVersion.ToString();
                try
                {
                    var cloudVerStr = await GetCloudLatestVersion();
                    if (cloudVerStr == null)
                    {
                        throw new Exception("Failed to fetch version from cloud");
                    }

                    var cloudVer = new Version(cloudVerStr);
                    if (cloudVer.IsDifferentThan(localVersion))
                    {
                        InstallGameFiles(true, cloudVer);
                    }
                    else
                    {
                        Status = LauncherStatus.Ready;
                    }
                }
                catch (Exception ex)
                {
                    Status = LauncherStatus.Failed;
                    MessageBox.Show($"Error when checking games update: {ex}");
                }
            }
            else
            {
                InstallGameFiles(false, Version.zero);
            }
        }

        private async void InstallGameFiles(bool _isUpdate, Version _onlineVersion)
        {
            try
            {
                WebClient webClient = new();
                if (_isUpdate)
                {
                    Status = LauncherStatus.DownloadingUpdate;
                }
                else
                {
                    Status = LauncherStatus.DownloadingGame;
                }

                // Fetch cloud version
                var cloudVerStr = await GetCloudLatestVersion();
                if (cloudVerStr == null)
                {
                    throw new Exception("Failed to fetch Version Manifest from the Cloud");
                }
                _onlineVersion = new Version(cloudVerStr);

                // Fetch cloud download url
                var cloudDownloadURL = await GetCloudLatestDistDownloadURL();
                if (cloudDownloadURL == null)
                {
                    throw new Exception("Failed to fetch Download URL from the Cloud");
                }

                // Download
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
                webClient.DownloadFileAsync(new Uri(cloudDownloadURL), gameZip, _onlineVersion);

                // After download update local version file
                // File.WriteAllText(versionFile, _onlineVersion.ToString());
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.Failed;
                MessageBox.Show($"Error Installing Game Files: {ex}");
            }
        }

        public async Task<string?> GetCloudLatestVersion()
        {
            HttpClient client = new HttpClient();

            var res = await client.GetStringAsync(LauncherConfig.RELEASE_REPO);
            var releases = JsonArray.Parse(res);

            if (releases == null || releases.AsArray().Count == 0)
            {
                Status = LauncherStatus.Failed;
                MessageBox.Show("An Error Ocurred when trying to fetch Cloud Version: Cannot find Latest Version");
                return null;
            }

            return (string)releases[0][LauncherConfig.VERSION_FETCH_KEY];
        }
        public async Task<string?> GetCloudKey(string key)
        {
            HttpClient client = new HttpClient();

            var res = await client.GetStringAsync(LauncherConfig.RELEASE_REPO);
            var releases = JsonArray.Parse(res);

            if (releases == null || releases.AsArray().Count == 0)
            {
                Status = LauncherStatus.Failed;
                MessageBox.Show("An Error Ocurred when trying to fetch Cloud Version: Cannot find Latest Version");
                return null;
            }

            return (string)releases[0][key];
        }

        public async Task<string?> GetCloudLatestDistDownloadURL()
        {
            HttpClient client = new();

            var res = await client.GetStringAsync(LauncherConfig.RELEASE_REPO);
            var releases = JsonArray.Parse(res);

            if (releases == null || releases.AsArray().Count == 0)
            {
                Status = LauncherStatus.Failed;
                MessageBox.Show("An Error Ocurred when trying to fetch Download URL: Cannot find Latest Version");
                return null;
            }
            if (LocalOperatingSystem.IsWindows())
            {
                foreach (var asset in releases[0]["assets"].AsArray())
                {
                    if ((string)asset["name"] == LauncherConfig.DIST_DOWNLOAD_KEY)
                    {
                        return (string)asset["browser_download_url"];
                    }
                }
            }else if (LocalOperatingSystem.IsMacOS())
            {
                foreach (var asset in releases[0]["assets"].AsArray())
                {
                    if ((string)asset["name"] == LauncherConfig.MAC_DIST_DOWNLOAD_KEY)
                    {
                        return (string)asset["browser_download_url"];
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            MessageBox.Show($"An Error Ocurred when trying to fetch download URL: Asset {(LocalOperatingSystem.IsWindows() ? LauncherConfig.DIST_DOWNLOAD_KEY : LocalOperatingSystem.IsMacOS() ? LauncherConfig.MAC_DIST_DOWNLOAD_KEY : "NullReferenceException")} Does not exist");
            return null;
        }

        private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                string onlineVersion = ((Version)e.UserState).ToString();
                if (LocalOperatingSystem.IsWindows())
                {
                    ZipFile.ExtractToDirectory(gameZip, rootPath, true);
                    File.Delete(gameZip);
                }else if (LocalOperatingSystem.IsMacOS())
                {
                    File.Move(gameSilicon, rootPath, true);
                    if(File.Exists(gameZip)) File.Delete(gameZip);
                }

                File.WriteAllText(versionFile, onlineVersion);
                VersionText.Text = "Game Version " + onlineVersion;
                Status = LauncherStatus.Ready;
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.Failed;
                MessageBox.Show($"Error Finishing Game Files Download: {ex}");
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (LocalOperatingSystem.IsWindows())
            {
                if (File.Exists(gameExe) && Status == LauncherStatus.Ready)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                    if (CheckForRunningInstances())
                    {
                        MessageBox.Show($"Error Running Game Instance Found; Cannot have two instances exist simultaneously.");
                    }
                    else
                    {
                        startInfo.WorkingDirectory = Path.Combine(rootPath, "Build");
                        Process.Start(startInfo);
                        //InitializeComponent();
                    }
                }
                else if (Status == LauncherStatus.Failed)
                {
                    CheckForUpdates();
                }
            }else if (LocalOperatingSystem.IsMacOS())
            {
                if (File.Exists(gameSilicon) && Status == LauncherStatus.Ready)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo(gameSilicon);
                    if (CheckForRunningInstances())
                    {
                        MessageBox.Show($"Error Running Game Instance Found; Cannot have two instances exist simultaneously.");
                    }
                    else
                    {
                        startInfo.WorkingDirectory = rootPath;
                        Process.Start(startInfo);
                        //InitializeComponent();
                    }
                }
                else if (Status == LauncherStatus.Failed)
                {
                    CheckForUpdates();
                }
            }
        }
        private async void UpdateLogButton_Click(object sender, RoutedEventArgs e)
        {
            var cloudGetKey = await GetCloudKey(LauncherConfig.VERSION_CONTENT_FETCH_KEY);
            if (cloudGetKey != null)
            {
                MessageBox.Show(cloudGetKey);
            }
            else
            {
                throw new Exception("Failed to fetch version update content");
            }
        }
        private bool CheckForRunningInstances()
        {
            bool success = false;
            //if (Process.GetProcesses(gameExe).Length > 0) success = true;
            return success;
        }
    }

    struct Version
    {
        internal static Version zero = new Version(0, 0, 0, "");
        private short major;
        private short minor;
        private short subMinor;
        private string verIndex;

        internal Version(short _major, short _minor, short _subMinor, string _verIndex)
        {
            major = _major;
            minor = _minor;
            subMinor = _subMinor;
            verIndex = _verIndex;
        }
        internal Version(string _version)
        {
            string[] _versionStrings = _version.Split('.');
            if (_versionStrings.Length != 4)
            {
                major = 0;
                minor = 0;
                subMinor = 0;
                verIndex = "rc0";
                return;
            }
            major = short.Parse(_versionStrings[0]);
            minor = short.Parse(_versionStrings[1]);
            subMinor = short.Parse(_versionStrings[2]);
            verIndex = _versionStrings[3];
        }
        internal bool IsDifferentThan(Version _otherVersion)
        {
            if (major != _otherVersion.major)
            {
                return true;
            }
            else
            {
                if (minor != _otherVersion.minor)
                {
                    return true;
                }
                else
                {
                    if (subMinor != _otherVersion.subMinor)
                    {
                        return true;
                    }
                    else
                    {
                        if (verIndex != _otherVersion.verIndex)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public override string ToString()
        {
            return $"{major}.{minor}.{subMinor}.{verIndex}";
        }
    }
}