using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace liveitbe.ImageCat
{
    public partial class MainWindow : Window
    {
        private const int IMAGE_LIST_COL = 5;

        List<ImageLink> imageLinks;
        List<ImageLink> filteredLinks;

        private void InitImageList()
        {
            ImageListColumnSetup(IMAGE_LIST_COL);
            imageLinks = new List<ImageLink>(64);
            filteredLinks = new List<ImageLink>(64);
        }

        private void ImageListColumnSetup(int col)
        {
            for (int r = 0; r < col; ++r)
                xamlGridListImage.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        }

        private void ImageListRowSetup(int row)
        {
            xamlGridListImage.RowDefinitions.Clear();
            for (int r = 0; r < row; ++r)
                xamlGridListImage.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
        }

        private void FillImageList()
        {
            imageLinks.Clear();
            xamlGridListImage.Dispatcher.Invoke(() => xamlGridListImage.Children.Clear());
            for (int r = 0; r < files.Count; ++r)
            {
                xamlGridListImage.Dispatcher.Invoke(() => {
                    ImageLink link = new ImageLink(files[r]);
                    Image img = new Image() { Margin = new Thickness(10), Visibility = Visibility.Visible };
                    img.Unloaded += (object sender, RoutedEventArgs e) =>
                    {
                        if (link.stream == null)
                            return;
                        link.stream.Close();
                        link.stream.Dispose();
                    };
                    link.preview = img;
                    Grid imgGrid = new Grid() { Margin = new Thickness(0), Visibility = Visibility.Collapsed };
                    imgGrid.Children.Add(img);
                    imgGrid.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) =>
                    {
                        e.Handled = true;
                        Preview(link);
                    };
                    xamlGridListImage.Children.Add(imgGrid);
                    link.grid = imgGrid;
                    imageLinks.Add(link);
                });
            }
        }

        private void FilterImages()
        {
            xamlGridListImage.Dispatcher.Invoke(() =>
            {
                filteredLinks.ForEach(l => {
                    l.grid.Visibility = Visibility.Collapsed;
                    l.preview.Source = null;
                });
            });
            GC.Collect(0);
            filteredLinks.Clear();
            filteredLinks.AddRange(imageLinks.Where(l => !StringTag.ShouldFilter(l)));
        }

        private void RearrangeImageList()
        {
            xamlGridListImage.Dispatcher.Invoke(() =>
            {
                Stopwatch sw = Stopwatch.StartNew();
                Grid imgGrid;
                ImageLink link;
                int tRow = Math.Max(filteredLinks.Count / IMAGE_LIST_COL, 1);
                ImageListRowSetup(tRow);
                xamlGridListImage.Height = tRow * 120;
                for (int n = 0; n < filteredLinks.Count; ++n)
                {
                    link = filteredLinks[n];
                    imgGrid = link.grid;
                    imgGrid.Visibility = Visibility.Visible;
                    link.preview.Source = link.Sample(100);
                    int row = n / IMAGE_LIST_COL;
                    int col = n % IMAGE_LIST_COL;
                    Grid.SetColumn(imgGrid, col);
                    Grid.SetRow(imgGrid, row);
                }
                Console.WriteLine("rearrange done: " + sw.ElapsedMilliseconds);
            });
        }

        private void ClearImageList()
        {
            filteredLinks.Clear();
            imageLinks.Clear();
            xamlGridListImage.Dispatcher.Invoke(() => xamlGridListImage.Children.Clear());
        }
    }
}
