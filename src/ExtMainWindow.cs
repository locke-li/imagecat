using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
