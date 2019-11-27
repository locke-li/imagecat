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

namespace liveitbe.ImageCat {
    public class FolderList {
        const string LAST_ACCESS_PATH = "LastAccessPath0";
        const string LAST_SELECTED_PATH = "LastSelectedPath0";

        MainWindow _window;
        string _topPath;

        internal FolderList Init(MainWindow window_) {
            _window = window_;
            _window.xamlButtonSelectFolder.Click += OpenFolder;
            return this;
        }

        internal void TryResumeLastSession() {
            //last acess path from conf
            _topPath = Conf.Value(LAST_ACCESS_PATH);
            Console.WriteLine(_topPath);
            FillDirList();
            //last selected path from conf
            var lastSelected = Conf.Value(LAST_SELECTED_PATH);
            if (lastSelected is null) return;
            var lastDir = new DirectoryInfo(lastSelected);
            if (!lastDir.Exists) return;
            _window.ImageList.RefreshContent(lastDir);
        }

        private void OpenFolder(object sender, RoutedEventArgs e) {
            using var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK) {
                _topPath = dialog.SelectedPath;
                Conf.SetValue(LAST_ACCESS_PATH, _topPath);
                Conf.Save();
                FillDirList();
            }
        }

        private void FillDirList() {
            if (_topPath == null)
                return;
            var topDir = new DirectoryInfo(_topPath);
            if (!topDir.Exists)
                return;
            _window.Clear();
            var itemStack = new Stack<ItemCollection>(64);
            var xamlListDir = _window.xamlListDir;
            xamlListDir.Dispatcher.Invoke(() => {
                var topItem = new TreeViewItem() { Header = topDir.Name, Tag = topDir, IsExpanded = true };
                itemStack.Push(topItem.Items);
                xamlListDir.Items.Clear();
                xamlListDir.Items.Add(topItem);
                xamlListDir.SelectedItemChanged += (_, e) => {
                    var sw = Stopwatch.StartNew();
                    var dir = (DirectoryInfo)((TreeViewItem)e.NewValue).Tag;
                    Conf.SetValue(LAST_SELECTED_PATH, dir.FullName);
                    //TODO save on exit
                    Conf.Save();
                    _window.ImageList.RefreshContent(dir);
                    Console.WriteLine("switch fill done: " + sw.ElapsedMilliseconds);
                };
            });
            //collect folder hierarchy
            Task.Run(() => {
                var dirStack = new Stack<DirectoryInfo>(64);
                dirStack.Push(topDir);
                IEnumerable<DirectoryInfo> dirs;
                ItemCollection items;
                var sw = Stopwatch.StartNew();
                while (dirStack.Count > 0) {
                    items = itemStack.Pop();
                    try {
                        dirs = dirStack.Pop().EnumerateDirectories().Where(d => !ShouldIgnore(d)).OrderBy(d => d.Name);
                    }
                    catch (Exception) {
                        continue;
                    }
                    foreach (var dir in dirs) {
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

        private bool ShouldIgnore(DirectoryInfo dir)
            => (dir.Attributes & (FileAttributes.System | FileAttributes.Hidden)) != 0 || dir.Name.StartsWith(".", StringComparison.Ordinal);
    }
}
