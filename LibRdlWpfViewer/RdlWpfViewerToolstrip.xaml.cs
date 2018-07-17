using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LibRdlWpfViewer
{
    /// <summary>
    /// Interaction logic for RdlWpfViewer.xaml
    /// </summary>
    public partial class RdlWpfViewerToolstrip : UserControl
    {
        public RdlWpfViewerToolstrip()
        {
            InitializeComponent();
        }

        public RdlWpfViewerToolstrip(RdlWpfViewer viewer)
        {
            InitializeComponent();
            this.Viewer = viewer;
        }

        public RdlWpfViewer Viewer
        {
            get { return _viewer; }
            set
            {
                _viewer = value;
                this.viewerToolstrip.Viewer = _viewer.reportViewer;
            }
        }

        private RdlWpfViewer _viewer = null;
    }

}

