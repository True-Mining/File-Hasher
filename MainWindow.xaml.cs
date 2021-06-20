using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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


namespace get_files_hash_checksum
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public bool startPathCkecked { get; set; } = false;
        public bool startDlLinkCkecked { get; set; } = false;
        public string startPath { get; set; } = @"C:\";
        public string startDlLink { get; set; } = @"https://";
        public OpenFileDialog selectedFiles { get; set; } = null;
        public string stringResult { get; set; } = "Select files to show result";


        private void btnGetDirectory_Click(object sender, RoutedEventArgs e)
        {
            ListOfFiles.Clear();

            OpenFileDialog openFileDialog = new OpenFileDialog() { Multiselect = true, CheckFileExists = true, CheckPathExists = true, Title = "Select what you want to checksum", InitialDirectory = startPathCkecked && Directory.Exists(startPath) ? startPath : null};
            if (openFileDialog.ShowDialog() == true)
            {
                selectedFiles = openFileDialog;

                foreach(string filePath in openFileDialog.FileNames)
                {
                    ListOfFiles.Add(new FileParameters() { DlLink = startDlLinkCkecked && Uri.IsWellFormedUriString(startDlLink, UriKind.Absolute) && File.Exists(filePath) && startPathCkecked && Directory.Exists(startPath) ? new Uri(new Uri(startDlLink, UriKind.Absolute), Path.GetRelativePath(startPath + '/', filePath), false).AbsoluteUri : null, FileName = Path.GetFileName(filePath), Path = startPathCkecked ? Directory.Exists(startPath) ? Path.GetRelativePath(startPath, filePath).Replace(@"//", @"/").Replace(@"\\", @"\").Replace(@"\", @"/") : null : Directory.Exists(startPath) ? Path.GetFullPath(filePath) : null, Sha256 = File.Exists(filePath) ? FileSHA256(filePath) : null }); ;
                }

                textBoxResult.Text = JsonConvert.SerializeObject(ListOfFiles, Formatting.Indented);
                MessageBox.Show(JsonConvert.SerializeObject(ListOfFiles, Formatting.Indented));
            }
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
    }
}
