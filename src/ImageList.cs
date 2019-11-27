using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace liveitbe.ImageCat {
    public class ImageList {
        private const int IMAGE_LIST_COL = 5;
        private const int PREVIEW_GRID_IMAGE_SIZE = 100;
        private const int PREVIEW_GRID_IMAGE_MARGIN = 10;
        private const int PREVIEW_GRID_HEIGHT = PREVIEW_GRID_IMAGE_SIZE + PREVIEW_GRID_IMAGE_MARGIN * 2;

        MainWindow _window;
        ScrollViewer _xamlListImage;
        Grid _xamlGridListImage;
        string[] _fileFilter;
        List<ImageLink> imageLinks;
        List<ImageLink> filteredLinks;
        CancellationTokenSource _cancelPreview;

        internal ImageList Init(MainWindow window_) {
            _window = window_;
            _xamlListImage = _window.xamlListImage;
            _xamlGridListImage = _window.xamlGridListImage;
            _fileFilter = new string[] {
               "*.jpg",
                "*.jpeg",
                "*.png",
                "*.bmp",
                "*.tiff",
                "*.gif",
            };
            ImageListColumnSetup(IMAGE_LIST_COL);
            imageLinks = new List<ImageLink>(64);
            filteredLinks = new List<ImageLink>(64);
            return this;
        }

        private void ImageListColumnSetup(int col) {
            for (int r = 0; r < col; ++r)
                _xamlGridListImage.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        }

        private void ImageListRowSetup(int row) {
            _xamlGridListImage.RowDefinitions.Clear();
            for (int r = 0; r < row; ++r)
                _xamlGridListImage.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
        }

        private void FillImageList(IEnumerable<FileInfo> files) {
            //TODO image link cache
            ImageLink.Recycle(imageLinks);
            imageLinks.Clear();
            foreach (var file in files) {
                var link = ImageLink.Create(file);
                var img = new Image() { Margin = new Thickness(PREVIEW_GRID_IMAGE_MARGIN), Visibility = Visibility.Visible };
                var imgGrid = new Grid() { Margin = new Thickness(0), Visibility = Visibility.Collapsed };
                imgGrid.Children.Add(img);
                imgGrid.MouseLeftButtonDown += (_, e) => {
                    e.Handled = true;
                    _window.ImageViewer.Preview(link);
                };
                _xamlGridListImage.Children.Add(imgGrid);
                link.grid = imgGrid;
                link.preview = img;
                imageLinks.Add(link);
            }
            _xamlGridListImage.Dispatcher.Invoke(() => {
                _xamlGridListImage.Children.Clear();
                foreach (var link in imageLinks) {
                    _xamlGridListImage.Children.Add(link.grid);
                }
            });
        }

        private void FilterImages() {
            _xamlGridListImage.Dispatcher.Invoke(() => {
                filteredLinks.ForEach(l => {
                    l.grid.Visibility = Visibility.Collapsed;
                });
            });
            filteredLinks.Clear();
            filteredLinks.AddRange(imageLinks.Where(l => !StringTag.ShouldFilter(l)));
            GC.Collect(0);
        }

        private void RearrangeImageList() {
            _xamlGridListImage.Dispatcher.Invoke(() => {
                var sw = Stopwatch.StartNew();
                Grid imgGrid;
                ImageLink link;
                int tRow = Math.Max((int)Math.Ceiling((float)filteredLinks.Count / IMAGE_LIST_COL), 1);
                Console.WriteLine(filteredLinks.Count + ", " + tRow);
                ImageListRowSetup(tRow);
                _xamlListImage.ScrollToTop();
                _xamlGridListImage.Height = tRow * PREVIEW_GRID_HEIGHT;
                for (int n = 0; n < filteredLinks.Count; ++n) {
                    link = filteredLinks[n];
                    imgGrid = link.grid;
                    imgGrid.Visibility = Visibility.Visible;
                    int row = n / IMAGE_LIST_COL;
                    int col = n % IMAGE_LIST_COL;
                    Grid.SetColumn(imgGrid, col);
                    Grid.SetRow(imgGrid, row);
                    //Console.WriteLine(link.name + "(" + col + "," + row + ")");
                }
                Console.WriteLine("rearrange done: " + sw.ElapsedMilliseconds);
            });
            var cancel = new CancellationTokenSource();
            _cancelPreview = cancel;
            Task.Run(() => {
                var sw = Stopwatch.StartNew();
                foreach (var link in filteredLinks) {
                    link.PreviewAsync(PREVIEW_GRID_IMAGE_SIZE);
                    if (cancel.IsCancellationRequested) {
                        Console.WriteLine("cancelled");
                        break;
                    }
                }
                if (_cancelPreview == cancel) {
                    _cancelPreview = null;
                }
                cancel.Dispose();
                Console.WriteLine("async preview complete: " + sw.ElapsedMilliseconds);
            });
        }

        internal void TryCancelPreview() {
            _cancelPreview?.Cancel();
            Console.WriteLine(nameof(_cancelPreview));
        }

        internal void RefreshContentByFilter() {
            FilterImages();
            RearrangeImageList();
        }

#nullable enable

        internal void RefreshContent(DirectoryInfo targetDir) {
            TryCancelPreview();
            _window.ImageViewer.Clear();
            //ClearFilter();
            var files = _fileFilter
                .SelectMany(f => targetDir.EnumerateFiles(f))
                .OrderBy(f => f.Name);
            FillImageList(files);
            FilterImages();
            RearrangeImageList();
            //TODO watch for changes of the selected dir
        }

#nullable disable

        internal void Clear() {
            filteredLinks.Clear();
            imageLinks.Clear();
            _xamlGridListImage.Dispatcher.Invoke(() => _xamlGridListImage.Children.Clear());
        }
    }
}
