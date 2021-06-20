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
        public static configs config { get; set; } = new();

        public FileHasher()
        {
            InitializeComponent();
            DataContext = config;
        }

        public void GuiUpdater()
        {
            textBoxResult.Text = config.stringResult;
            textStartPath.Text = config.startPath;
            textStartDlLink.Text = config.startDlLink;
        }

        private CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog() { Multiselect = true, AllowNonFileSystemItems = true, EnsureFileExists = true, EnsurePathExists = true, EnsureReadOnly = true, EnsureValidNames = true, IsFolderPicker = true, NavigateToShortcut = true, ShowHiddenItems = true, ShowPlacesList = true, Title = "Select what you want to checksum", InitialDirectory = config.startPathCkecked && Directory.Exists(config.startPath) ? config.startPath : null };

        private void btnGetDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                MessageBox.Show("Nothing selected");
                return;
            }

            ListOfFiles.Clear();

            config.stringResult = "Hashing all files. The process may take some time and the screen may freeze in the meantime. hold...";
            GuiUpdater();

            foreach (string filePath in openFileDialog.FileNames)
            {
                MessageBox.Show(JsonConvert.SerializeObject(openFileDialog.FileNames));
                if (File.Exists(filePath))
                {
                    ListOfFiles.Add(new FileParameters() { DlLink = config.startDlLinkCkecked && Uri.IsWellFormedUriString(config.startDlLink, UriKind.Absolute) && File.Exists(filePath) && config.startPathCkecked && Directory.Exists(config.startPath) ? new Uri(new Uri(config.startDlLink, UriKind.Absolute), Path.GetRelativePath(config.startPath + '/', filePath), false).AbsoluteUri : null, FileName = Path.GetFileName(filePath), Path = config.startPathCkecked ? Directory.Exists(config.startPath) ? Path.GetRelativePath(config.startPath, filePath).Replace(@"//", @"/").Replace(@"\\", @"\").Replace(@"\", @"/") : null : Directory.Exists(config.startPath) ? Path.GetFullPath(filePath).Replace(@"//", @"/").Replace(@"\\", @"\").Replace(@"\", @"/") : null, Sha256 = File.Exists(filePath) ? FileSHA256(filePath) : null }); ;
                }
                else if (Directory.Exists(filePath))
                {
                    foreach (string filePathInSubdir in Directory.GetFiles(filePath, "*", SearchOption.AllDirectories))
                    {
                        ListOfFiles.Add(new FileParameters() { DlLink = config.startDlLinkCkecked && Uri.IsWellFormedUriString(config.startDlLink, UriKind.Absolute) && File.Exists(filePathInSubdir) && config.startPathCkecked && Directory.Exists(config.startPath) ? new Uri(new Uri(config.startDlLink, UriKind.Absolute), Path.GetRelativePath(config.startPath + '/', filePathInSubdir), false).AbsoluteUri : null, FileName = Path.GetFileName(filePathInSubdir), Path = config.startPathCkecked ? Directory.Exists(config.startPath) ? Path.GetRelativePath(config.startPath, filePathInSubdir).Replace(@"//", @"/").Replace(@"\\", @"\").Replace(@"\", @"/") : null : Directory.Exists(config.startPath) ? Path.GetFullPath(filePathInSubdir).Replace(@"//", @"/").Replace(@"\\", @"\").Replace(@"\", @"/") : null, Sha256 = File.Exists(filePathInSubdir) ? FileSHA256(filePathInSubdir) : null }); ;
                    }
                }
            }

            if (ListOfFiles.Count == 0) { config.stringResult = "No files in selected directories"; }
            else
            {
                config.stringResult = JsonConvert.SerializeObject(ListOfFiles, Formatting.Indented);
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
            public bool startPathCkecked { get; set; } = false;
            public bool startDlLinkCkecked { get; set; } = false;
            public string startPath { get; set; } = @"C:\";
            public string startDlLink { get; set; } = @"https://";
            public string stringResult { get; set; } = "Select files to show result";
        }

        private void btnGetStartDirectory_Click(object sender, RoutedEventArgs e)
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

            config.startPath = Directory.Exists(cfd.FileName) ? cfd.FileName : Path.GetDirectoryName(cfd.FileName);

            GuiUpdater();
        }
    }
}