using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace liveitbe.ImageCat {
    public class TagEdit {
        private const int TAG_ROW_HEIGHT = 28;
        private static readonly char[] _filterSep = new char[] { ';' };

        private MainWindow _window;
        private Grid _xamlGridTags;
        private TextBox _xamlTextTagsFilter;
        bool _visible;

        internal TagEdit Init(MainWindow window_) {
            _window = window_;
            _xamlGridTags = window_.xamlGridTags;
            _xamlTextTagsFilter = window_.xamlTextTagsFilter;
            _visible = true;
            window_.xamlButtonClearFilter.Click += (s, e) => Clear();
            window_.xamlToggleTags.Click += ToggleTagsVisibility;
            _xamlTextTagsFilter.TextChanged += FilterTagsChanged;
            _xamlGridTags.SizeChanged += (s, e) => { if (_visible) SetupTagGrid(window_.ImageViewer.PreviewLink); };
            return this;
        }

        private void ToggleTagsVisibility(object sender, RoutedEventArgs e) {
            _visible = !_visible;
            _xamlGridTags.Visibility = _visible ? Visibility.Visible : Visibility.Hidden;
        }

        internal void SetupTagGrid(ImageLink link) {
            _xamlGridTags.RowDefinitions.Clear();
            _xamlGridTags.RowDefinitions.Add(new RowDefinition());
            _xamlGridTags.Children.Clear();
            if (link == null)
                return;
            double border = 5;
            double x = 0, lx = 0;
            double xm = _xamlGridTags.ActualWidth;
            int row = 0;
            Console.WriteLine("tags:" + link.tag.Count);
            foreach (StringTag tag in link.tag) {
                var tagE = TagElement(tag);
                _xamlGridTags.Children.Add(tagE);
                tagE.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                tagE.Arrange(new Rect(tagE.DesiredSize));
                x += tagE.ActualWidth + border;
                if (lx > 0 && x > xm) {
                    _xamlGridTags.RowDefinitions.Add(new RowDefinition());
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

        private Label TagElement(StringTag tag) {
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

        private void FilterTagsChanged(object sender, TextChangedEventArgs e) {
            StringTag.FilterTags(_xamlTextTagsFilter.Text.Split(_filterSep, StringSplitOptions.RemoveEmptyEntries));
            _window.ImageList.RefreshContentByFilter();
        }

        internal void Clear() {
            _xamlTextTagsFilter.Text = null;
            StringTag.FilterTags(null);
        }
    }
}
