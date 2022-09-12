﻿using System;
using System.Windows;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;

namespace Project_Phoenix_Game_Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
        private string onlineVersionFileLink = "https://sheep-codenas.direct.quickconnect.cn:59374/d/s/qHyjpAyf73Ke20YVuZ95Mio6Kpf9LQBF/webapi/entry.cgi/Version.txt?api=SYNO.SynologyDrive.Files&method=download&version=2&files=%5B%22id%3A707854181972617208%22%5D&force_download=true&json_error=true&c2_offload=%22allow%22&_dc=1662909366310&sharing_token=%22a7V9Ta7cv80tymmV3tevb_Z5O_Gj3xn3SRUZW9kzWTb_N_OIYvbBzZJFXeeArC6YM2u56Lc_NYio4E_Lqhe_UsoYa4ejOaqf.jlpwXUL_kyIk5FT3ib4oeCx54wr_n_630gdAm9fyDio0XX1Vkvc.Nec71G1XZw5dMncnDi_Z4UePgiHq0ogTptih9KxVX6n8mBkLeogJABmHTUlLI6U7SfcinT5ajAqyv5jrrDkUs2P6dfTp.oXPc03%22&SynoToken=0LUEF7HbOzCN6";
        private string onlineBuildZipLink = "https://sheep-codenas.direct.quickconnect.cn:59374/d/s/qHyjtTNCgdTCK4c9uzCxFMGo4WKYNxX3/webapi/entry.cgi/Build.zip?api=SYNO.SynologyDrive.Files&method=download&version=2&files=%5B%22id%3A707854185908971514%22%5D&force_download=true&json_error=true&c2_offload=%22allow%22&_dc=1662909328661&sharing_token=%222nMTzJBiuQgGxvsHlY_h9qU_Be7PpDl8vXy9nKcTjvQP06E4UHg1ZbEM5UpxaHGxTgJxxeyNqWrgXKRlTpvLIzqim5cHz903AUzs1dgDVvnmaau8X_AY7UDumE4UuGf0V_psTlYVJWrnT0CcpftU7aTkq_3pl58nZYVwuxUPP986hit26SK6lQrTHdWs4haysqkhk5Yqn3ch3sL3hvRPd.PrFXaAkDUhYWKtfkKxK3JCAylV4nqKSOAZ%22&SynoToken=0LUEF7HbOzCN6";
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
                    //Version File Link
                    _onlineVersion = new Version(webClient.DownloadString(onlineVersionFileLink));
                }
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
                //Game Build ZIp Link
                webClient.DownloadFileAsync(new Uri(onlineBuildZipLink), gameZip, _onlineVersion);
                //webClient.DownloadFileAsync(new Uri(GetGoogleDriveDownloadLinkFromUrl("https://drive.google.com/file/d/15ykBxR5ChCXpvEp9kzNpNMNmtQrU8ZA9/view?usp=sharing")), gameZip, _onlineVersion);
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.Failed;
                MessageBox.Show($"Error Installing Game Files: {ex}");
            }
        }

        public static string GetGoogleDriveDownloadLinkFromUrl(string url)
        {
            int index = url.IndexOf("id=");
            int closingIndex;
            if (index > 0)
            {
                index += 3;
                closingIndex = url.IndexOf('&', index);
                if (closingIndex < 0)
                    closingIndex = url.Length;
            }
            else
            {
                index = url.IndexOf("file/d/");
                if (index < 0) // url is not in any of the supported forms
                    return string.Empty;

                index += 7;

                closingIndex = url.IndexOf('/', index);
                if (closingIndex < 0)
                {
                    closingIndex = url.IndexOf('?', index);
                    if (closingIndex < 0)
                        closingIndex = url.Length;
                }
            }

            return string.Format("https://drive.google.com/uc?id={0}&export=download", url.Substring(index, closingIndex - index));
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
