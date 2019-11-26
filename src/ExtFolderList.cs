using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace liveitbe.ImageCat
{
    public partial class MainWindow : Window
    {
        const string LAST_ACCESS_PATH = "LastAccessPath0";
        const string LAST_SELECTED_PATH = "LastSelectedPath0";
        string _topPath;
        string[] _fileFilter;

        private void InitFolderList()
        {
            _fileFilter = new string[] {
               "*.jpg",
                "*.jpeg",
                "*.png",
                "*.bmp",
                "*.tiff",
                "*.gif",
            };
            xamlButtonSelectFolder.Click += OpenFolder;
        }

        private void TryLoadPreviousPath()
        {
            _topPath = Conf.Value(LAST_ACCESS_PATH);
            Console.WriteLine(_topPath);
            FillDirList();
        }

        private void TryRestoreLastSelectedPath() {
            var lastSelected = Conf.Value(LAST_SELECTED_PATH);
            if (lastSelected is null) return;
            var lastDir = new DirectoryInfo(lastSelected);
            if (!lastDir.Exists) return;
            RefreshContent(lastDir);
        }

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK) {
                _topPath = dialog.SelectedPath;
                Conf.SetValue(LAST_ACCESS_PATH, _topPath);
                Conf.Save();
                FillDirList();
            }
        }

        private void FillDirList()
        {
            if (_topPath == null)
                return;
            var topDir = new DirectoryInfo(_topPath);
            if (!topDir.Exists)
                return;
            FullClear();
            var itemStack = new Stack<ItemCollection>(64);
            xamlListDir.Dispatcher.Invoke(() =>
            {
                var topItem = new TreeViewItem() { Header = topDir.Name, Tag = topDir, IsExpanded = true };
                itemStack.Push(topItem.Items);
                xamlListDir.Items.Clear();
                xamlListDir.Items.Add(topItem);
                xamlListDir.SelectedItemChanged += (_, e) => {
                    var sw = Stopwatch.StartNew();
                    var dir = (DirectoryInfo)((TreeViewItem)e.NewValue).Tag;
                    RefreshContent(dir);
                    Console.WriteLine("switch fill done: " + sw.ElapsedMilliseconds);
                };
            });
            //collect folder hierarchy
            Task.Run(() =>
            {
                var dirStack = new Stack<DirectoryInfo>(64);
                dirStack.Push(topDir);
                IEnumerable<DirectoryInfo> dirs;
                ItemCollection items;
                var sw = Stopwatch.StartNew();
                while (dirStack.Count > 0)
                {
                    items = itemStack.Pop();
                    try
                    {
                        dirs = dirStack.Pop().EnumerateDirectories().Where(d => !ShouldIgnore(d)).OrderBy(d => d.Name);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    foreach (var dir in dirs)
                    {
                        //TODO only probe dir to a certain depth, from the nearest expanded list
                        //now we only probe first level folder
                        //dirStack.Push(dir);
                        xamlListDir.Dispatcher.Invoke(() => {
                            var item = new TreeViewItem() { Header = dir.Name, Tag = dir };
                            items.Add(item);
                            itemStack.Push(item.Items);
                        });
                    }
                }
                Console.WriteLine("folder listing done: " + sw.ElapsedMilliseconds);
            });
        }

#nullable enable

        private void RefreshContent(DirectoryInfo sdir)
        {
            _cancelPreview?.Cancel();
            Console.WriteLine(nameof(_cancelPreview));
            Conf.SetValue(LAST_SELECTED_PATH, sdir.FullName);
            //TODO save on exit
            Conf.Save();
            ClearPreview();
            //ClearFilter();
            var files = _fileFilter
                .SelectMany(f => sdir.EnumerateFiles(f))
                .OrderBy(f => f.Name);
            FillImageList(files);
            FilterImages();
            RearrangeImageList();
            //TODO watch for changes of the selected dir
        }

#nullable disable

        private bool ShouldIgnore(DirectoryInfo dir) 
            => (dir.Attributes & (FileAttributes.System | FileAttributes.Hidden)) != 0 || dir.Name.StartsWith(".", StringComparison.Ordinal);
    }
}
