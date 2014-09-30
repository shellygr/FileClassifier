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

namespace FileClassifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[] files;
        int numOfRelevantFiles;
        string currentFile;
        int currentFileNumberWorkingOn = 0;
        string outputFilename;
        string lastDescriptionText;

        string[] mediaExtensions = { "mp3", "mp4", "wav", "3ga", "avi", "flv", "wmv", "wma", "flac" };
        string[] imageExtensions = { "jpg", "jpeg", "gif", "bmp", "png" };

        AxAXVLC.AxVLCPlugin vlcPlayer = new AxAXVLC.AxVLCPlugin();
        public MainWindow()
        {
            InitializeComponent();
            mediaPreview.Child = vlcPlayer;
            path.Text = "C:\\Users\\";
        }

        private void browse(object sender, RoutedEventArgs e)
        {
            //Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.RootFolder = System.Environment.SpecialFolder.MyDocuments;
            dlg.ShowNewFolderButton = false;
            
            // Set filter for file extension and default file extension 
            //dlg.DefaultExt = ".jpg";
            //dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";

            // Display OpenFileDialog by calling ShowDialog method 
            System.Windows.Forms.DialogResult result = dlg.ShowDialog();
            
            // Get the selected file name and display in a TextBox 
            if (result.Equals(System.Windows.Forms.DialogResult.OK))
            {
                string pathName = dlg.SelectedPath;
                path.Text = pathName;
            }
        }


        private void calcNumOfRelevantFiles()
        {
            string[] relevantExtensions = {};
            relevantExtensions = relevantExtensions.Concat(imageExtensions).ToArray();
            relevantExtensions = relevantExtensions.Concat(mediaExtensions).ToArray();

            //MessageBox.Show(String.Join(",", imageExtensions), "relevant extensions", MessageBoxButton.OK);
            //MessageBox.Show(String.Join(",", mediaExtensions), "relevant extensions", MessageBoxButton.OK);
            //MessageBox.Show(String.Join(",", relevantExtensions), "relevant extensions", MessageBoxButton.OK);

            int counter = 0;

            foreach (string file in files)
            {
                string ext = file.Split('.').Last();
                if (relevantExtensions.Contains(ext))
                {
                    //MessageBox.Show("Relevant extension " + ext + " found", "Found relevant extension", MessageBoxButton.OK);
                    ++counter;
                }
                else
                {
                    MessageBox.Show("Extension" + ext + " is not relevant", "Extension not relevant", MessageBoxButton.OK);
                }
            }

            numOfRelevantFiles = counter;
        }

        // TODO: numOfRelevantFiles will contain only the new files not yet reviewed
        private void fetchFiles(object sender, RoutedEventArgs e)
        {
            files = System.IO.Directory.GetFiles(path.Text, "*", System.IO.SearchOption.TopDirectoryOnly);
            calcNumOfRelevantFiles();
            outputFilename = path.Text + "\\folderDescription_" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm");

            counter.Content = currentFileNumberWorkingOn+1 + "/" + numOfRelevantFiles;

            if (numOfRelevantFiles == 0)
            {
                MessageBox.Show("Nothing to work on - no relevant files", "Error");
                return;
            }

            currentFileNumberWorkingOn = 0;          

            workOnCurrentFile();
            //MessageBox.Show(outputFilename, "path", MessageBoxButton.OK);
        }
        
        private void workOnCurrentFile()
        {
            if (currentFileNumberWorkingOn > numOfRelevantFiles)
            {
                return;
            }

            currentFile = files[currentFileNumberWorkingOn];
            currentFileGUIBox.Content = currentFile;
            description.Focus();

            string ext = currentFile.Split('.').Last();
            if (imageExtensions.Contains(ext))
            {
                preview.Source = new BitmapImage(new Uri(currentFile));
            }
            else if (mediaExtensions.Contains(ext))
            {
                vlcPlayer.stop();
                vlcPlayer.playlistClear();
                vlcPlayer.addTarget("file:///" + currentFile, null, AXVLC.VLCPlaylistMode.VLCPlayListReplaceAndGo, 0);
                vlcPlayer.play();
            }
            else
            {
                //MessageBox.Show("Got irrelevant file " + currentFile + ", Skipping");
                ++currentFileNumberWorkingOn;
                workOnCurrentFile();
            }
        }

        // TODO: Add support for update mechanism. That is, take the last description file, and the new file will be the same but updated - only new files will be added.
        private void submit(object sender, RoutedEventArgs e)
        {
            using (System.IO.StreamWriter outputFile = new System.IO.StreamWriter(outputFilename, true))
            {
                outputFile.WriteLine(currentFile + "," + description.Text);
                ++currentFileNumberWorkingOn;
            }

            lastDescriptionText = description.Text;
            description.Text = "";

            if (currentFileNumberWorkingOn > numOfRelevantFiles)
            {
                MessageBox.Show("Done!\nChoose another folder.", "FileClassifier");
                return;
            }

            counter.Content = currentFileNumberWorkingOn + "/" + numOfRelevantFiles;
            workOnCurrentFile();
        }

        // TODO: full history
        private void description_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Up))
            {
                description.Text = lastDescriptionText;
            }
        }
    }
}
