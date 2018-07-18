using System.Windows.Controls;

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

        public void HideOpen()
        {
            ((System.Windows.Forms.ToolStripButton)this.viewerToolstrip.Items[0]).Visible = false;
        }
    }

}

