using System;
using System.Windows;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json.Nodes;

namespace Project_Phoenix_Game_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class LauncherConfig {
        public static string RELEASE_REPO = "https://repo.smartsheep.space/api/v1/repos/CRTL_Prototype_Studios/Project_Phoenix_Files/releases";
    }

    enum LauncherStatus
    {
        Ready,
        Failed,
        DownloadingGame,
        DownloadingUpdate
    }
    public partial class MainWindow : Window
    {
        private string rootPath;
        private string versionFile;
        private string gameZip;
        private string gameExe;
        private LauncherStatus _status;
        private string onlineVersionFileLink = "https://sheep-codenas.direct.quickconnect.cn:59374/d/s/qHyjpAyf73Ke20YVuZ95Mio6Kpf9LQBF/webapi/entry.cgi/Version.txt?api=SYNO.SynologyDrive.Files&method=download&version=2&files=%5B%22id%3A707854181972617208%22%5D&force_download=true&json_error=true&c2_offload=%22allow%22&_dc=1665674248487&sharing_token=%22RLhNCt9HY7ZOsSN8g5evTR3G1C1Rr3oklSC.Mwclm9MZMiSW7NrpDzApXfdLmcfCsglDhXoXPGqtsXmqhDhWa19HPc3RHSe7hfc2miGBoGXzrN7a9YsNW4nju.7ZIh.lMlVwzLBR5IUNU40dtWKll2YnDS2WEQAwU811gA.GHR.A0nLWKg4sqxt77WlmDwGVslYHwG5W9evF36U196FjRcMR7XVCgp.8HeRAseBb2Kaa7ksWU001G4eK%22&SynoToken=U9GWVKqE6vFmM";
        private string onlineBuildZipLink = "https://sheep-codenas.direct.quickconnect.cn:59374/d/s/qHyjtTNCgdTCK4c9uzCxFMGo4WKYNxX3/webapi/entry.cgi/Build.zip?api=SYNO.SynologyDrive.Files&method=download&version=2&files=%5B%22id%3A707854185908971514%22%5D&force_download=true&json_error=true&c2_offload=%22allow%22&_dc=1665674137577&sharing_token=%22V61nNDaN3MoPSk3otKlcDpHGtGHJJxNg.KHi3WsVqIbRvqlsSKm65.b0UuqxSrdlwBgdlPy_GeW_VXPk48E66EvEQ5Yx.17LKSE.M8LLFZT56kZZOY6MFgeF32Ujz3Yp9ojTj9jJ38XhnsH_0kRWPgk2JllQ0RR6MmM9pKFVN7eG5oBYlZbFwF83iNrgnFGsh3t6USYZIEuWxD1wv4e8r2LjgyeQymW9CJVEMnXaybx5hqA5MJ_3KBn2%22&SynoToken=U9GWVKqE6vFmM";
        internal LauncherStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case LauncherStatus.Ready:
                        PlayButton.Content = "Play";
                        break;
                    case LauncherStatus.Failed:
                        PlayButton.Content = "Update Failed - Retry";
                        break;
                    case LauncherStatus.DownloadingGame:
                        PlayButton.Content = "Downloading Game Files...";
                        break;
                    case LauncherStatus.DownloadingUpdate:
                        PlayButton.Content = "Updating Game Files...";
                        break;
                    default:
                        break;
                }
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            rootPath = Directory.GetCurrentDirectory();
            versionFile = Path.Combine(rootPath, "Version.txt");
            gameZip = Path.Combine(rootPath, "Build.zip");
            gameExe = Path.Combine(rootPath, "Build", "Project Phoenix.exe");
        }
        private void CheckForUpdates()
        {
            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = "Game Version " + localVersion.ToString();
                try
                {
                    WebClient webClient = new WebClient();
                    //Version File Link
                    Version onlineVersion = new Version(webClient.DownloadString(onlineVersionFileLink));
                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        InstallGameFiles(true, onlineVersion);
                    }
                    else
                    {
                        Status = LauncherStatus.Ready;
                    }
                }
                catch (Exception ex)
                {
                    Status = LauncherStatus.Failed;
                    MessageBox.Show($"Error Checking for Game Updates: {ex}");
                }
            }
            else
            {
                InstallGameFiles(false, Version.zero);
            }
        }
        private void InstallGameFiles(bool _isUpdate, Version _onlineVersion)
        {
            try
            {
                WebClient webClient = new WebClient();
                if (_isUpdate)
                {
                    Status = LauncherStatus.DownloadingUpdate;
                }
                else
                {
                    Status = LauncherStatus.DownloadingGame;
                    // Version File Link
                    _onlineVersion = new Version(webClient.DownloadString(onlineVersionFileLink));
                }
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
                // Game Build ZIp Link
                webClient.DownloadFileAsync(new Uri(onlineBuildZipLink), gameZip, _onlineVersion);
                
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.Failed;
                MessageBox.Show($"Error Installing Game Files: {ex}");
            }
        }

        public async Task<string?> GetCloudVersionAsync()
        {
            HttpClient client = new HttpClient();

            var res = await client.GetStringAsync(LauncherConfig.RELEASE_REPO);
            var releases = JsonArray.Parse(res);

            if (releases == null || releases.AsArray().Count == 0)
            {
                Status = LauncherStatus.Failed;
                MessageBox.Show("Error when get cloud version: cannot found latest version");
                return null;
            }

            return (string)releases[0]["tag_name"];
        }

        public void GetGameDistDownlaodURL(string repo)
        {

        }

        private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                string onlineVersion = ((Version)e.UserState).ToString();
                ZipFile.ExtractToDirectory(gameZip, rootPath, true);
                File.Delete(gameZip);

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
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CheckForUpdates();
        }
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(gameExe) && Status == LauncherStatus.Ready)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                startInfo.WorkingDirectory = Path.Combine(rootPath, "Build");
                Process.Start(startInfo);
                Close();
            }
            else if (Status == LauncherStatus.Failed)
            {
                CheckForUpdates();
            }
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
                        if(verIndex != _otherVersion.verIndex)
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
