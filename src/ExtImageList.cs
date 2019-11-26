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
    public partial class MainWindow : Window {
        private const int IMAGE_LIST_COL = 5;
        private const int PREVIEW_GRID_IMAGE_SIZE= 100;
        private const int PREVIEW_GRID_IMAGE_MARGIN = 10;
        private const int PREVIEW_GRID_HEIGHT = PREVIEW_GRID_IMAGE_SIZE + PREVIEW_GRID_IMAGE_MARGIN * 2;

        List<ImageLink> imageLinks;
        List<ImageLink> filteredLinks;
        CancellationTokenSource _cancelPreview;

        private void InitImageList() {
            ImageListColumnSetup(IMAGE_LIST_COL);
            imageLinks = new List<ImageLink>(64);
            filteredLinks = new List<ImageLink>(64);
        }

        private void ImageListColumnSetup(int col) {
            for (int r = 0; r < col; ++r)
                xamlGridListImage.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        }

        private void ImageListRowSetup(int row) {
            xamlGridListImage.RowDefinitions.Clear();
            for (int r = 0; r < row; ++r)
                xamlGridListImage.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
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
                    Preview(link);
                };
                xamlGridListImage.Children.Add(imgGrid);
                link.grid = imgGrid;
                link.preview = img;
                imageLinks.Add(link);
            }
            xamlGridListImage.Dispatcher.Invoke(() => {
                xamlGridListImage.Children.Clear();
                foreach (var link in imageLinks) {
                    xamlGridListImage.Children.Add(link.grid);
                }
            });
        }

        private void FilterImages() {
            xamlGridListImage.Dispatcher.Invoke(() => {
                filteredLinks.ForEach(l => {
                    l.grid.Visibility = Visibility.Collapsed;
                });
            });
            filteredLinks.Clear();
            filteredLinks.AddRange(imageLinks.Where(l => !StringTag.ShouldFilter(l)));
            GC.Collect(0);
        }

        private void RearrangeImageList() {
            xamlGridListImage.Dispatcher.Invoke(() => {
                var sw = Stopwatch.StartNew();
                Grid imgGrid;
                ImageLink link;
                int tRow = Math.Max((int)Math.Ceiling((float)filteredLinks.Count / IMAGE_LIST_COL), 1);
                Console.WriteLine(filteredLinks.Count + ", " + tRow);
                ImageListRowSetup(tRow);
                xamlListImage.ScrollToTop();
                xamlGridListImage.Height = tRow * PREVIEW_GRID_HEIGHT;
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
            Task.Run(() => {
                var sw = Stopwatch.StartNew();
                foreach (var link in filteredLinks) {
                    link.PreviewAsync(PREVIEW_GRID_IMAGE_SIZE);
                    if (cancel.IsCancellationRequested) {
                        Console.WriteLine("cancelled");
                        return;
                    }
                }
                Console.WriteLine("async preview complete: " + sw.ElapsedMilliseconds);
            });
            _cancelPreview = cancel;
        }

        private void ClearImageList() {
            filteredLinks.Clear();
            imageLinks.Clear();
            xamlGridListImage.Dispatcher.Invoke(() => xamlGridListImage.Children.Clear());
        }
    }
}
