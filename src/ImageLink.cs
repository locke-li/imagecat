using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace liveitbe.ImageCat
{
    class ImageLink
    {
        public FileInfo file;
        public FileStream stream;
        public List<StringTag> tags;
        public Grid grid;
        public Image preview;
        public static char[] tagSep = new char[] { tagSepUI, tagSepInFile };
        private const char tagSepInFile = '_';
        private const char tagSepUI = ';';
        private const char tagStartInFile = '@';
        private const char tagEndInFile = '.';
        public string name { get; private set; }
        public string alltags;

        public ImageLink(FileInfo file_)
        {
            tags = new List<StringTag>(8);
            file = file_;
            stream = file.OpenRead();
            int s0 = file.Name.LastIndexOf(tagStartInFile);
            int s1 = file.Name.LastIndexOf(tagEndInFile);
            if (s0 < 0)
                goto notag;
            name = file.Name.Substring(0, s0);
            string tagSegment = file.Name.Substring(s0 + 1, s1 - s0 - 1).Replace(tagSepInFile, tagSepUI);
            RefreshTag(tagSegment, false);
            return;
        notag:
            name = file.Name.Substring(0, s1);
            tags = new List<StringTag>(4);
        }

        public ImageSource Sample(int width)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            stream.Seek(0, SeekOrigin.Begin);
            bi.StreamSource = stream;
            bi.DecodePixelWidth = width;
            bi.EndInit();
            bi.Freeze();
            return bi;
        }

        public void RefreshTag(string alltags_, bool rename = true)
        {
            alltags = alltags_;
            tags.Clear();
            string[] tagSegment = alltags.Split(tagSep, StringSplitOptions.RemoveEmptyEntries);
            tags.AddRange(tagSegment.Select(s => StringTag.Ref(s)));
            if (rename)
                RenameFile(name);
        }

        public void AddTag(StringTag st)
        {
            tags.Add(st);
            RenameFile(name);
        }

        public void ReplaceTag(string o, string n)
        {
            if (o == n)
                return;
            int t = tags.FindIndex(g => g.name == o);
            tags[t].Unref();
            tags[t] = StringTag.Ref(n);
            RenameFile(name);
        }

        public void RemoveTag(string o)
        {
            int t = tags.FindIndex(g => g.name == o);
            tags[t].Unref();
            tags.RemoveAt(t);
            RenameFile(name);
        }

        private void RenameFile(string n)
        {
            StringBuilder sb = new StringBuilder(file.DirectoryName);
            sb.Append(Path.DirectorySeparatorChar).Append(n).Append(tagStartInFile);
            tags.ForEach(g => sb.Append(g).Append(tagSepInFile));
            --sb.Length;
            sb.Append(file.Extension);
            stream.Close();
            stream.Dispose();
            file.MoveTo(sb.ToString());
            stream = file.OpenRead();
        }

        public void Rename(string n)
        {
            RenameFile(n);
            name = n;
        }
    }
}
