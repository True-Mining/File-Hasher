using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace FileHasher
{
    public partial class FileHasher : Window
    {
        public static configs Config { get; set; } = new();

        public FileHasher()
        {
            InitializeComponent();
            DataContext = Config;
        }

        public void GuiUpdater()
        {
            TextBoxResult.Text = Config.StringResult;
            TextStartPath.Text = Config.StartPath;
            TextStartDlLink.Text = Config.StartDlLink;
        }

        private CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog() { Multiselect = true, AllowNonFileSystemItems = true, EnsureFileExists = true, EnsurePathExists = true, EnsureReadOnly = true, EnsureValidNames = true, IsFolderPicker = true, NavigateToShortcut = true, ShowHiddenItems = true, ShowPlacesList = true, Title = "Select what you want to checksum", InitialDirectory = Config.StartPathCkecked && Directory.Exists(Config.StartPath) ? Config.StartPath : null };

        private void BtnGetDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                MessageBox.Show("Nothing selected");
                return;
            }

            ListOfFiles.Clear();

            Config.StringResult = "Hashing all files. The process may take some time and the screen may freeze in the meantime. hold...";
            GuiUpdater();

            foreach (string filePath in openFileDialog.FileNames)
            {
                if (File.Exists(filePath))
                {
                    ListOfFiles.Add(new FileParameters() { DlLink = Config.StartDlLinkCkecked && Uri.IsWellFormedUriString(Config.StartDlLink, UriKind.Absolute) && File.Exists(filePath) && Config.StartPathCkecked && Directory.Exists(Config.StartPath) ? new Uri(new Uri(Config.StartDlLink, UriKind.Absolute), Path.GetRelativePath(Config.StartPath + '/', filePath), false).AbsoluteUri : null, FileName = Path.GetFileName(filePath), Path = Config.StartPathCkecked ? Directory.Exists(Config.StartPath) ? Path.GetDirectoryName(Path.GetRelativePath(Config.StartPath, filePath)).Replace(@"//", @"/").Replace(@"\\", @"\").Replace(@"\", @"/") : null : Directory.Exists(Config.StartPath) ? Path.GetDirectoryName(Path.GetFullPath(filePath)).Replace(@"//", @"/").Replace(@"\\", @"\").Replace(@"\", @"/") : null, Sha256 = File.Exists(filePath) ? FileSHA256(filePath) : null }); ;
                }
                else if (Directory.Exists(filePath))
                {
                    foreach (string filePathInSubdir in Directory.GetFiles(filePath, "*", SearchOption.AllDirectories))
                    {
                        ListOfFiles.Add(new FileParameters() { DlLink = Config.StartDlLinkCkecked && Uri.IsWellFormedUriString(Config.StartDlLink, UriKind.Absolute) && File.Exists(filePathInSubdir) && Config.StartPathCkecked && Directory.Exists(Config.StartPath) ? new Uri(new Uri(Config.StartDlLink, UriKind.Absolute), Path.GetRelativePath(Config.StartPath + '/', filePathInSubdir), false).AbsoluteUri : null, FileName = Path.GetFileName(filePathInSubdir), Path = Config.StartPathCkecked ? Directory.Exists(Config.StartPath) ? Path.GetDirectoryName(Path.GetRelativePath(Config.StartPath, filePathInSubdir)).Replace(@"//", @"/").Replace(@"\\", @"\").Replace(@"\", @"/") : null : Directory.Exists(Config.StartPath) ? Path.GetDirectoryName(Path.GetFullPath(filePathInSubdir)).Replace(@"//", @"/").Replace(@"\\", @"\").Replace(@"\", @"/") : null, Sha256 = File.Exists(filePathInSubdir) ? FileSHA256(filePathInSubdir) : null }); ;
                    }
                }
            }

            if (ListOfFiles.Count == 0) { Config.StringResult = "No files in selected directories"; }
            else
            {
                Config.StringResult = JsonConvert.SerializeObject(ListOfFiles, Formatting.Indented);
            }

            GuiUpdater();
        }

        public static string FileSHA256(string filePath)
        {
            using System.Security.Cryptography.SHA256 hashAlgorithm = System.Security.Cryptography.SHA256.Create();
            using (FileStream stream = System.IO.File.OpenRead(filePath))
            {
                return BitConverter.ToString(hashAlgorithm.ComputeHash(stream)).Replace("-", "").ToUpper();
            }
        }

        public static List<FileParameters> ListOfFiles { get; set; } = new();

        public partial class FileParameters
        {
            [JsonProperty("dlLink", NullValueHandling = NullValueHandling.Ignore)]
            public string DlLink { get; set; }

            [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
            public string Path { get; set; }

            [JsonProperty("fileName", NullValueHandling = NullValueHandling.Ignore)]
            public string FileName { get; set; }

            [JsonProperty("sha256", NullValueHandling = NullValueHandling.Ignore)]
            public string Sha256 { get; set; }
        }

        public class configs
        {
            public bool StartPathCkecked { get; set; } = false;
            public bool StartDlLinkCkecked { get; set; } = false;
            public string StartPath { get; set; } = @"C:\";
            public string StartDlLink { get; set; } = @"https://";
            public string StringResult { get; set; } = "Select files to show result";
        }

        private void BtnGetStartDirectory_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog cfd = new CommonOpenFileDialog
            {
                EnsurePathExists = true,
                EnsureFileExists = false,
                AllowNonFileSystemItems = true,
                Multiselect = false,
                IsFolderPicker = true,
                Title = "Select a Directory"
            };

            if (cfd.ShowDialog() != CommonFileDialogResult.Ok)
            {
                MessageBox.Show("No directory selected");
                return;
            }

            Config.StartPath = Directory.Exists(cfd.FileName) ? cfd.FileName : Path.GetDirectoryName(cfd.FileName);

            GuiUpdater();
        }
    }
}