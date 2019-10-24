using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace liveitbe.ImageCat
{
    public partial class MainWindow : Window
    {
        private const int TAG_ROW_HEIGHT = 28;
        static char[] filterSep = new char[] { ';' };
        bool tagVisible;

        private void InitTagEdit()
        {
            tagVisible = true;
            xamlButtonClearFilter.Click += (s,e) => ClearFilter();
            xamlToggleTags.Click += ToggleTagsVisibility;
            xamlGridTags.SizeChanged += (s, e) => { if (tagVisible) SetupTagGrid(previewLink); };
        }

        private void ToggleTagsVisibility(object sender, RoutedEventArgs e)
        {
            tagVisible = !tagVisible;
            xamlGridTags.Visibility = tagVisible ? Visibility.Visible : Visibility.Hidden;
        }

        private void SetupTagGrid(ImageLink link)
        {
            xamlGridTags.RowDefinitions.Clear();
            xamlGridTags.RowDefinitions.Add(new RowDefinition());
            xamlGridTags.Children.Clear();
            if (link == null)
                return;
            double border = 5;
            double x = 0, lx = 0;
            double xm = xamlGridTags.ActualWidth;
            int row = 0;
            Console.WriteLine("tags:" + link.tags.Count);
            foreach(StringTag tag in link.tags)
            {
                var tagE = TagElement(tag);
                xamlGridTags.Children.Add(tagE);
                tagE.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                tagE.Arrange(new Rect(tagE.DesiredSize));
                x += tagE.ActualWidth + border;
                if (lx > 0 && x > xm)
                {
                    xamlGridTags.RowDefinitions.Add(new RowDefinition());
                    ++row;
                    x = tagE.ActualWidth + border;
                    lx = 0;
                    Console.WriteLine();
                }
                Grid.SetRow(tagE, row);
                tagE.Margin = new Thickness(lx, 1, 0, 1);
                Console.Write(tag + "(" + row + "," + lx + ")" + ", ");
                lx = x;
            }
            Console.WriteLine();
        }

        private Label TagElement(StringTag tag)
        {
            return new Label() {
                Content = tag.ToString(),
                FontSize = 12,
                Foreground = Brushes.DarkGreen,
                Background = Brushes.PaleGreen,
                Width = double.NaN,
                Height = double.NaN,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private void FilterTagsChanged(object sender, TextChangedEventArgs e)
        {
            StringTag.FilterTags(xamlTextTagsFilter.Text.Split(filterSep, StringSplitOptions.RemoveEmptyEntries));
            FilterImages();
            RearrangeImageList();
        }

        private void ClearFilter()
        {
            xamlTextTagsFilter.Text = null;
            StringTag.FilterTags(null);
        }
    }
}
