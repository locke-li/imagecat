using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace liveitbe.ImageCat
{
    public partial class MainWindow : Window
    {
        const string LastAccessPath = "LastAccessPath0";
        string topPath;
        List<FileInfo> files;

        private void InitFolderList()
        {
            files = new List<FileInfo>(128);
            xamlButtonSelectFolder.Click += OpenFolder;
        }

        private void TryLoadPreviousPath()
        {
            topPath = Conf.Value(LastAccessPath);
            Console.WriteLine(topPath);
            FillDirList();
        }

        private void OpenFolder(object sender, RoutedEventArgs e)
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK) {
                topPath = dialog.SelectedPath;
                Conf.SetValue(LastAccessPath, topPath);
                Conf.Save();
                FillDirList();
            }
        }

        private void FillDirList()
        {
            if (topPath == null)
                return;
            var topDir = new DirectoryInfo(topPath);
            if (!topDir.Exists)
                return;
            FullClear();
            var itemStack = new Stack<ItemCollection>(64);
            xamlListDir.Dispatcher.Invoke(() => {
                xamlListDir.Items.Clear();
                var topItem = new TreeViewItem() { Header = topDir.Name, Tag = topDir, IsExpanded = true };
                xamlListDir.Items.Add(topItem);
                itemStack.Push(topItem.Items);
                xamlListDir.SelectedItemChanged += (sender, e) =>
                {
                    var sw = Stopwatch.StartNew();
                    ClearPreview();
                    ClearFilter();
                    files.Clear();
                    if (e.NewValue != null)
                    {
                        var sdir = (DirectoryInfo)((TreeViewItem)e.NewValue).Tag;
                        files.AddRange(sdir.GetFiles("*.jpg"));
                        files.AddRange(sdir.GetFiles("*.jpeg"));
                        files.AddRange(sdir.GetFiles("*.png"));
                        files.AddRange(sdir.GetFiles("*.bmp"));
                        files.AddRange(sdir.GetFiles("*.tiff"));
                        files.AddRange(sdir.GetFiles("*.gif"));
                        files.OrderBy(f => f.Name);
                    }
                    FillImageList();
                    FilterImages();
                    RearrangeImageList();
                    Console.WriteLine("switch fill done: " + sw.ElapsedMilliseconds);
                };
            });
            Task.Run(() =>
            {
                var dirStack = new Stack<DirectoryInfo>(64);
                dirStack.Push(topDir);
                IEnumerable<DirectoryInfo> dirs;
                ItemCollection items;
                var sw = Stopwatch.StartNew();
                while (dirStack.Count > 0)
                {
                    try
                    {
                        items = itemStack.Pop();
                        dirs = dirStack.Pop().EnumerateDirectories().Where(d => !ShouldIgnore(d)).OrderBy(d => d.Name);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    foreach (var dir in dirs)
                    {
                        dirStack.Push(dir);
                        xamlListDir.Dispatcher.Invoke(() => {
                            TreeViewItem item = new TreeViewItem() { Header = dir.Name, Tag = dir };
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

        private void ClearFolderList() => files.Clear();
    }
}
