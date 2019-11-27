using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace liveitbe.ImageCat
{
    public class ImageViewer
    {
        MainWindow _window;
        private TextBox _xamlTextFileName;
        private TextBox _xamlTextFileTags;
        private ScrollViewer _xamlViewImagePreview;
        private ScaleTransform _xamlImagePreviewScale;
        private Image _xamlImagePreview;
        internal ImageLink PreviewLink { get; set; }
        string _previewName;
        Point _mousePos;
        bool _previewDrag = false;
        int _scale;
        double _scalef;
        const double scaleb = 10000;
        const int scalel = 2500;
        const int scaleu = 40000;
        const int scaleSen = 10;

        internal ImageViewer Init(MainWindow window_)
        {
            _window = window_;
            _scale = (int)scaleb;
            _xamlTextFileName = _window.xamlTextFileName;
            _xamlTextFileTags = _window.xamlTextFileTags;
            _xamlViewImagePreview = _window.xamlViewImagePreview;
            _xamlImagePreviewScale = _window.xamlImagePreviewScale;
            _xamlImagePreview = _window.xamlImagePreview;
            _xamlTextFileName.TextChanged += FileRename;
            _xamlTextFileName.IsEnabled = false;
            _xamlTextFileTags.TextChanged += FileTagEdit;
            _xamlTextFileTags.IsEnabled = false;
            _xamlViewImagePreview.PreviewMouseLeftButtonDown += ImagePreviewMouseDown;
            _xamlViewImagePreview.PreviewMouseLeftButtonUp += ImagePreviewMouseEnd;
            _xamlViewImagePreview.MouseMove += ImagePreviewMouseMove;
            _xamlViewImagePreview.PreviewMouseDoubleClick += (s,e) => ResetPreviewScale();
            _xamlViewImagePreview.PreviewMouseWheel += ImagePreviewWheel;
            return this;
        }

        private void ImagePreviewWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            _scale += e.Delta * scaleSen;
            if (_scale < scalel)
                _scale = scalel;
            else if (_scale > scaleu)
                _scale = scaleu;
            _scalef = _scale / scaleb;
            _xamlImagePreviewScale.ScaleX = _xamlImagePreviewScale.ScaleY = _scalef;
        }

        private void ImagePreviewMouseEnd(object sender, MouseButtonEventArgs e)
        {
            _previewDrag = false;
            _xamlViewImagePreview.ReleaseMouseCapture();
        }

        private void ImagePreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _mousePos = e.GetPosition(_xamlViewImagePreview);
            _xamlViewImagePreview.CaptureMouse();
            _previewDrag = true;
        }

        private void ImagePreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_previewDrag)
                return;
            var mousePosC = e.GetPosition(_xamlViewImagePreview);
            var diff = mousePosC - _mousePos;
            _mousePos = mousePosC;
            _xamlViewImagePreview.ScrollToHorizontalOffset(_xamlViewImagePreview.HorizontalOffset - diff.X);
            _xamlViewImagePreview.ScrollToVerticalOffset(_xamlViewImagePreview.VerticalOffset - diff.Y);
        }

        private void ResetPreviewScale()
        {
            _scale = (int)scaleb;
            _xamlImagePreviewScale.ScaleX = _xamlImagePreviewScale.ScaleY = 1;
        }

        internal void Preview(ImageLink link)
        {
            if (PreviewLink != null)
                PreviewLink.grid.Background = Brushes.Transparent;
            PreviewLink = link;
            _previewName = link.Name;
            _xamlTextFileName.Text = _previewName;
            _xamlTextFileName.IsEnabled = true;
            _xamlTextFileTags.Text = link.AllTag;
            _xamlTextFileTags.IsEnabled = true;
            _xamlImagePreview.Source = PreviewLink.Sample(0);
            PreviewLink.grid.Background = Brushes.LightSkyBlue;
            ResetPreviewScale();
        }

        private void FileRename(object sender, TextChangedEventArgs e)
        {
            try
            {
                PreviewLink.Rename(_xamlTextFileName.Text);
                _previewName = PreviewLink.Name;
            }
            catch (Exception)
            {
                Console.WriteLine("file rename failed");
            }
            finally
            {
                _xamlTextFileName.Text = _previewName;
            }
        }

        private void FileTagEdit(object sender, TextChangedEventArgs e)
        {
            if (PreviewLink == null)
                return;
            PreviewLink.RefreshTag(_xamlTextFileTags.Text);
            _window.TagEdit.SetupTagGrid(PreviewLink);
        }

        internal void Clear()
        {
            PreviewLink = null;
            _previewName = null;
            _xamlTextFileName.Text = null;
            _xamlTextFileName.IsEnabled = false;
            _xamlTextFileTags.Text = null;
            _xamlTextFileTags.IsEnabled = false;
        }
    }
}
