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
        Point mousePos;
        bool previewDrag = false;
        int scale;
        double scalef;
        const double scaleb = 10000;
        const int scalel = 2500;
        const int scaleu = 40000;
        const int scaleSen = 10;

        private void InitPreview()
        {
            scale = (int)scaleb;
            xamlTextFileName.TextChanged += FileRename;
            xamlTextFileName.IsEnabled = false;
            xamlTextFileTags.TextChanged += FileTagEdit;
            xamlTextFileTags.IsEnabled = false;
            xamlViewImagePreview.PreviewMouseLeftButtonDown += ImagePreviewMouseDown;
            xamlViewImagePreview.PreviewMouseLeftButtonUp += ImagePreviewMouseEnd;
            xamlViewImagePreview.MouseMove += ImagePreviewMouseMove;
            xamlViewImagePreview.PreviewMouseDoubleClick += (s,e) => ResetPreviewScale();
            xamlViewImagePreview.PreviewMouseWheel += ImagePreviewWheel;
        }

        private void ImagePreviewWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            scale += e.Delta * scaleSen;
            if (scale < scalel)
                scale = scalel;
            else if (scale > scaleu)
                scale = scaleu;
            scalef = scale / scaleb;
            xamlImagePreviewScale.ScaleX = xamlImagePreviewScale.ScaleY = scalef;
        }

        private void ImagePreviewMouseEnd(object sender, MouseButtonEventArgs e)
        {
            previewDrag = false;
            xamlViewImagePreview.ReleaseMouseCapture();
        }

        private void ImagePreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            mousePos = e.GetPosition(xamlViewImagePreview);
            xamlViewImagePreview.CaptureMouse();
            previewDrag = true;
        }

        private void ImagePreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!previewDrag)
                return;
            Point mousePosC = e.GetPosition(xamlViewImagePreview);
            Vector diff = mousePosC - mousePos;
            mousePos = mousePosC;
            xamlViewImagePreview.ScrollToHorizontalOffset(xamlViewImagePreview.HorizontalOffset - diff.X);
            xamlViewImagePreview.ScrollToVerticalOffset(xamlViewImagePreview.VerticalOffset - diff.Y);
        }

        private void ResetPreviewScale()
        {
            scale = (int)scaleb;
            xamlImagePreviewScale.ScaleX = xamlImagePreviewScale.ScaleY = 1;
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
            ResetPreviewScale();
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
            if (previewLink == null)
                return;
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
