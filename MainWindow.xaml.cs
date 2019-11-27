using System.IO;
using System.Windows;

namespace liveitbe.ImageCat {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public FolderList FolderList { get; private set; }
        public ImageList ImageList { get; private set; }
        public TagEdit TagEdit { get; private set; }
        public ImageViewer ImageViewer { get; private set; }

        public MainWindow() {
            InitializeComponent();
            Init();
        }

        private void Init() {
            Conf.Init();
            FolderList = new FolderList().Init(this);
            ImageList = new ImageList().Init(this);
            TagEdit = new TagEdit().Init(this);
            ImageViewer = new ImageViewer().Init(this);
            FolderList.TryResumeLastSession();
        }

        internal void Clear() {
            TagEdit.Clear();
            ImageViewer.Clear();
            ImageList.Clear();
        }
    }
}
