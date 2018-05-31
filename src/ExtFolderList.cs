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
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    topPath = dialog.SelectedPath;
                    Conf.SetValue(LastAccessPath, topPath);
                    Conf.Save();
                    FillDirList();
                }
            }
        }

        private void FillDirList()
        {
            if (topPath == null)
                return;
            DirectoryInfo topDir = new DirectoryInfo(topPath);
            if (!topDir.Exists)
                return;
            FullClear();
            Stack<ItemCollection> itemStack = new Stack<ItemCollection>(64);
            xamlListDir.Dispatcher.Invoke(() => {
                xamlListDir.Items.Clear();
                TreeViewItem topItem = new TreeViewItem() { Header = topDir.Name, Tag = topDir, IsExpanded = true };
                xamlListDir.Items.Add(topItem);
                itemStack.Push(topItem.Items);
                xamlListDir.SelectedItemChanged += (object sender, RoutedPropertyChangedEventArgs<Object> e) =>
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    ClearPreview();
                    ClearFilter();
                    files.Clear();
                    if (e.NewValue != null)
                    {
                        DirectoryInfo sdir = ((TreeViewItem)e.NewValue).Tag as DirectoryInfo;
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
                Stack<DirectoryInfo> dirStack = new Stack<DirectoryInfo>(64);
                dirStack.Push(topDir);
                IEnumerable<DirectoryInfo> dirs;
                ItemCollection items;
                Stopwatch sw = Stopwatch.StartNew();
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
                    foreach (DirectoryInfo dir in dirs)
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
        {
            string n = dir.Name;
            FileAttributes fa = dir.Attributes;
            return (int)(fa & (FileAttributes.System | FileAttributes.Hidden)) != 0 || n.StartsWith(".");
        }

        private void ClearFolderList()
        {
            files.Clear();
        }
    }
}
