using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace liveitbe.ImageCat
{
    public partial class MainWindow : Window
    {
        ImageLink previewLink;
        string previewName;

        private void InitPreview()
        {
            xamlTextFileName.TextChanged += FileRename;
            xamlTextFileName.IsEnabled = false;
            xamlTextFileTags.TextChanged += FileTagEdit;
            xamlTextFileTags.IsEnabled = false;
        }

        private void Preview(ImageLink link)
        {
            if (previewLink != null)
                previewLink.grid.Background = Brushes.Transparent;
            previewLink = link;
            previewName = link.name;
            xamlTextFileName.Text = previewName;
            xamlTextFileName.IsEnabled = true;
            xamlTextFileTags.Text = link.alltags;
            xamlTextFileTags.IsEnabled = true;
            xamlImagePreview.Source = previewLink.Sample(0);
            previewLink.grid.Background = Brushes.LightSkyBlue;
        }

        private void FileRename(object sender, TextChangedEventArgs e)
        {
            try
            {
                previewLink.Rename(xamlTextFileName.Text);
                previewName = previewLink.name;
            }
            catch (Exception)
            {
                Console.WriteLine("file rename failed");
            }
            finally
            {
                xamlTextFileName.Text = previewName;
            }
        }

        private void FileTagEdit(object sender, TextChangedEventArgs e)
        {
            previewLink.RefreshTag(xamlTextFileTags.Text);
            SetupTagGrid(previewLink);
        }

        private void ClearPreview()
        {
            previewLink = null;
            previewName = null;
            xamlTextFileName.Text = null;
            xamlTextFileName.IsEnabled = false;
            xamlTextFileTags.Text = null;
            xamlTextFileTags.IsEnabled = false;
        }
    }
}
