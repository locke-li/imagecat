using System.Windows;

namespace liveitbe.ImageCat
{
    public partial class MainWindow : Window
    {
        private void Init()
        {
            Conf.Init();
            InitFolderList();
            InitImageList();
            InitTagEdit();
            InitPreview();
            TryLoadPreviousPath();
        }

        private void FullClear()
        {
            ClearFilter();
            ClearPreview();
            ClearImageList();
            ClearFolderList();
        }
    }
}
