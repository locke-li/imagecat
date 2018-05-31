using System;
using System.Collections.Generic;
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
        private const int TAG_ROW_HEIGHT = 28;
        static char[] filterSep = new char[] { ';' };

        private void InitTagEdit()
        {
            xamlButtonClearFilter.Click += (s,e) => ClearFilter();
        }

        private void SetupTagGrid(ImageLink link)
        {
            xamlGridTags.RowDefinitions.Clear();
            xamlGridTags.RowDefinitions.Add(new RowDefinition());
            xamlGridTags.Children.Clear();
            double border = 5;
            double x = 0, lx = 0;
            double xm = xamlGridTags.ActualWidth;
            int row = 0;
            Console.Write("tags:");
            foreach(StringTag tag in link.tags)
            {
                Label tagE = TagElement(tag);
                xamlGridTags.Children.Add(tagE);
                tagE.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                tagE.Arrange(new Rect(tagE.DesiredSize));
                x += tagE.ActualWidth + border;
                if (x > xm)
                {
                    xamlGridTags.RowDefinitions.Add(new RowDefinition());
                    ++row;
                    x = 0;
                    lx = 0;
                }
                Grid.SetRow(tagE, row);
                tagE.Margin = new Thickness(lx, 0, 0, 0);
                Console.Write(tag + "(" + row + "," + lx + ")" + ", ");
                lx = x;
            }
            Console.WriteLine();
        }

        private Label TagElement(StringTag tag)
        {
            Label ret = new Label() {
                Content = tag.ToString(),
                FontSize = 12,
                Background = Brushes.LightGreen,
                Width = double.NaN,
                Height = double.NaN,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            return ret;
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
