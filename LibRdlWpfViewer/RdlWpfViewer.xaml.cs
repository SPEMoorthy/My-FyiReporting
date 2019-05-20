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
using static fyiReporting.RdlViewer.RdlViewer;

namespace LibRdlWpfViewer
{
    /// <summary>
    /// Interaction logic for RdlWpfViewer.xaml
    /// </summary>
    public partial class RdlWpfViewer : UserControl
    {
        public RdlWpfViewer()
        {
            InitializeComponent();
        }

        public void Rebuild()
        {
            this.reportViewer.Rebuild();
         }

        

        public void SaveAs(string FileName, fyiReporting.RDL.OutputPresentationType type)
        {
            this.reportViewer.SaveAs(FileName, type);
        }

        public Uri SourceFile
        {
            get
            {
                return this.reportViewer.SourceFile;
            }
            set
            {
                this.reportViewer.SourceFile = value;
            }
        }
        public event HyperlinkEventHandler Hyperlink
        {
            remove
            {
                this.reportViewer.Hyperlink -= value;
            }
            add
            {
                this.reportViewer.Hyperlink += value;
            }
        }

        public string SourceRdl
        {
            get
            {
                return this.reportViewer.SourceRdl;
            }
            set
            {
                this.reportViewer.SourceRdl = value;
            }
        }

        public string Parameters
        {
            get
            {
                return this.reportViewer.Parameters;
            }
            set
            {
                this.reportViewer.Parameters = value;
            }
        }

        public fyiReporting.RDL.Report Report
        {
            get
            {
                return this.reportViewer.Report;
            }
        }


        public bool ShowFindPanel
        {
            get
            {
                return this.reportViewer.ShowFindPanel;
            }
            set
            {
                this.reportViewer.ShowFindPanel = value;
            }
        }


        public bool ShowParameterPanel
        {
            get
            {
                return this.reportViewer.ShowParameterPanel;
            }
            set
            {
                this.reportViewer.ShowParameterPanel = value;
            }
        }

        public void ShowRunButton()
        {
            this.reportViewer.ShowRunButton();
        }

        public void HideRunButton()
        {
            this.reportViewer.HideRunButton();
        }

        public void Show()
        {
            this.reportViewer.Show();
        }

        public void Hide()
        {
            this.reportViewer.Hide();
        }

        /// <summary>
        /// Enables/Disables the selection tool.  The selection tool allows the user
        /// to select text and images on the display and copy it to the clipboard.
        /// </summary>
        public bool SelectTool
        {
            get { return this.reportViewer.SelectTool; }
            set { this.reportViewer.SelectTool = value; }
        }
    }

}

