using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace liveitbe.ImageCat {
    class ImageLink {
        public static char[] tagSep = new char[] { tagSepUI, tagSepInFile };
        private const char tagSepInFile = '_';
        private const char tagSepUI = ';';
        private const char tagStartInFile = '@';
        private const char tagEndInFile = '.';

        private static Stack<ImageLink> _pool;

        private static void InitPool() {
            _pool ??= new Stack<ImageLink>(64);
        }

        public static ImageLink Create(FileInfo file) {
            InitPool();
            return (_pool.Count > 0 ? _pool.Pop() : new ImageLink()).Reset(file);
        }

        public static void Recycle(IEnumerable<ImageLink> recycle) {
            InitPool();
            foreach (var r in recycle) {
                r.Clear();
                _pool.Push(r);
            }
        }

        private FileInfo file;
        private FileStream stream;
        public List<StringTag> tag;
        public Grid grid;
        public Image preview;
        public string Name { get; private set; }
        public string AllTag { get; private set; }
        public bool _previewLoaded;

        private ImageLink() { }

        private ImageLink Reset(FileInfo file_) {
            tag ??= new List<StringTag>(8);
            file = file_;
            stream = file.OpenRead();
            int s0 = file.Name.LastIndexOf(tagStartInFile);
            int s1 = file.Name.LastIndexOf(tagEndInFile);
            if (s0 < 0) {
                Name = file.Name.Substring(0, s1);
            }
            else {
                Name = file.Name.Substring(0, s0);
                var tagSegment = file.Name.Substring(s0 + 1, s1 - s0 - 1).Replace(tagSepInFile, tagSepUI);
                RefreshTag(tagSegment, false);
            }
            return this;
        }

        private void Clear() {
            tag.Clear();
            file = null;
            if (stream != null) {
                stream.Close();
                stream.Dispose();
            }
            stream = null;
            Name = null;
            _previewLoaded = false;
        }

        public BitmapImage Sample(int width) {
            var bi = new BitmapImage();
            bi.BeginInit();
            stream.Seek(0, SeekOrigin.Begin);
            bi.StreamSource = stream;
            //force synchronous load, so we dont freeze the UI thread afterwards, denoted by RenderTime in profiler
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.DecodePixelWidth = width;
            bi.EndInit();
            bi.Freeze();
            //Console.WriteLine($"{name} {width} {sw.ElapsedMilliseconds}");
            return bi;
        }

        public void PreviewAsync(int width) {
            if (_previewLoaded) return;
            var bitmap = Sample(width);
            _previewLoaded = true;
            grid.Dispatcher.Invoke(() => {
                preview.Source = bitmap;
            });
        }

        public void RefreshTag(string alltags_, bool rename = true) {
            AllTag = alltags_;
            tag.Clear();
            var tagSegment = AllTag.Split(tagSep, StringSplitOptions.RemoveEmptyEntries);
            tag.AddRange(tagSegment.Select(s => StringTag.Ref(s)));
            if (rename)
                RenameFile(Name);
        }

        public void AddTag(StringTag st) {
            tag.Add(st);
            RenameFile(Name);
        }

        public void ReplaceTag(string o, string n) {
            if (o == n)
                return;
            int t = tag.FindIndex(g => g.name == o);
            tag[t].Unref();
            tag[t] = StringTag.Ref(n);
            RenameFile(Name);
        }

        public void RemoveTag(string o) {
            int t = tag.FindIndex(g => g.name == o);
            tag[t].Unref();
            tag.RemoveAt(t);
            RenameFile(Name);
        }

        private void RenameFile(string n) {
            var sb = new StringBuilder(file.DirectoryName);
            sb.Append(Path.DirectorySeparatorChar).Append(n).Append(tagStartInFile);
            tag.ForEach(g => sb.Append(g).Append(tagSepInFile));
            --sb.Length;
            sb.Append(file.Extension);
            stream.Close();
            stream.Dispose();
            file.MoveTo(sb.ToString());
            stream = file.OpenRead();
        }

        public void Rename(string n) {
            RenameFile(n);
            Name = n;
        }
    }
}
