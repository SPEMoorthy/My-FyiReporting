﻿using System;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Drawing;
using System.Resources;
using System.Reflection;
using System.IO;
using fyiReporting.RDL;
using System.Runtime.InteropServices;

namespace fyiReporting.RdlViewer
{
    public class ViewerToolstrip : ToolStrip
    {
        public ViewerToolstrip()
        {
            Init();
        }

        public ViewerToolstrip(RdlViewer viewer)
        {
            Init();
            this.Viewer = viewer;
        }

        private RdlViewer _viewer = null;

        public RdlViewer Viewer
        { 
            get{ return _viewer; }
            set
            { 
                _viewer = value;
                this.Viewer.PageNavigation += HandlePageNavigation;
            } 
        }

        private ToolStripTextBox currentPage = new ToolStripTextBox();
        private ToolStripLabel pageCount = new ToolStripLabel("");

        private void Init()
        {
            InitializeToolBar();
     
        }

        private void OpenClicked(object sender, System.EventArgs e)
        {
            var dlg = new OpenFileDialog();
            var result = dlg.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            Viewer.SourceFile = new Uri(dlg.FileName);
            Viewer.Rebuild();

            currentPage.Text = Viewer.PageCurrent.ToString();
            pageCount.Text = "/" + Viewer.PageCount;
        }

        private void PrintClicked(object sender, System.EventArgs e)
        {
            if (Viewer == null)
            {
                return;
            }

            PrintDocument pd = new PrintDocument();
            //pd.DocumentName = Viewer.SourceFile.LocalPath;
            pd.PrinterSettings.FromPage = 1;
            pd.PrinterSettings.ToPage = Viewer.PageCount;
            pd.PrinterSettings.MaximumPage = Viewer.PageCount;
            pd.PrinterSettings.MinimumPage = 1;
            pd.DefaultPageSettings.Landscape = Viewer.PageWidth > Viewer.PageHeight ? true : false;
            using (PrintDialog dlg = new PrintDialog())
            {
                dlg.Document = pd;
                dlg.AllowSelection = true;
                dlg.AllowSomePages = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Viewer.Print(pd);
                }
            }

        }

        private void DMPPrintClicked(object sender, EventArgs e)
        {
            if (Viewer == null)
            {
                return;
            }

            using (PrintDialog dlg = new PrintDialog())
            {   
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Viewer.SaveAs("C:\\Temp\\Report.txt", OutputPresentationType.DMP);
                    RawPrinterHelper.SendFileToPrinter(dlg.PrinterSettings.PrinterName, "C:\\Temp\\Report.txt");
                    /*
                    MemoryStreamGen ms = new MemoryStreamGen();
                    Viewer.Report.RunRender(ms, RDL.OutputPresentationType.DMP);
                    byte[] bytes = ((MemoryStream)ms.GetStream()).ToArray();
                    IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
                    Marshal.Copy(bytes, 0, ptr, bytes.Length);
                    RawPrinterHelper.SendBytesToPrinter(dlg.PrinterSettings.PrinterName, ptr, bytes.Length);
                    FileStream fs = new FileStream("C:\\Temp\\Report.txt", FileMode.Create);
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Close();
                    */
                }
            }
        }

        private void SaveAsClicked(object sender, System.EventArgs e)
        {
            if (Viewer == null)
            {
                return;
            }

            var dlg = new SaveFileDialog();
            dlg.Filter = "PDF files|*.pdf|XML files|*.xml|HTML files|*.html|CSV files|*.csv|RTF files|*.rtf|TIF files|*.tif|Excel files|*.xlsx|MHT files|*.mht|DMP files|*.dmp";
            dlg.FileName = ".pdf";
            var result = dlg.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }
            // save the report in a rendered format
            string ext = null;
            int i = dlg.FileName.LastIndexOf('.');
            if (i < 1)
            {
                ext = "";
            }
            else
            {
                ext = dlg.FileName.Substring(i + 1).ToLower();
            }
            fyiReporting.RDL.OutputPresentationType type = fyiReporting.RDL.OutputPresentationType.Internal;
            switch (ext)
            {
                case "pdf":
                    type = fyiReporting.RDL.OutputPresentationType.PDF;
                    break;
                case "xml":
                    type = fyiReporting.RDL.OutputPresentationType.XML;
                    break;
                case "html":
                    type = fyiReporting.RDL.OutputPresentationType.HTML;
                    break;
                case "htm":
                    type = fyiReporting.RDL.OutputPresentationType.HTML;
                    break;
                case "dmp":
                    type = fyiReporting.RDL.OutputPresentationType.DMP;
                    break;
                case "csv":
                    type = fyiReporting.RDL.OutputPresentationType.CSV;
                    break;
                case "rtf":
                    type = fyiReporting.RDL.OutputPresentationType.RTF;
                    break;
                case "mht":
                    type = fyiReporting.RDL.OutputPresentationType.MHTML;
                    break;
                case "mhtml":
                    type = fyiReporting.RDL.OutputPresentationType.MHTML;
                    break;
                case "xlsx":
                    type = fyiReporting.RDL.OutputPresentationType.Excel;
                    break;
                case "tif":
                    type = fyiReporting.RDL.OutputPresentationType.TIF;
                    break;
                case "tiff":
                    type = fyiReporting.RDL.OutputPresentationType.TIF;
                    break;
                default:
                    MessageBox.Show(String.Format("{0} is not a valid file type. File extension must be PDF, XML, HTML, CSV, MHT, RTF, TIF, XLSX, DMP.", dlg.FileName),
                        "Invalid File Type", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }

            Viewer.SaveAs(dlg.FileName, type);
        }

        private void FirstPageClicked(object sender, System.EventArgs e)
        {
            if (Viewer == null)
            {
                return;
            }

            Viewer.PageCurrent = 1;
        }

        private void PreviousPageClicked(object sender, System.EventArgs e)
        {
            if (Viewer == null)
            {
                return;
            }

            if (Viewer.PageCurrent == 1)
            {
                return;
            }

            Viewer.PageCurrent -= 1;

        }

        private void NextPageClicked(object sender, System.EventArgs e)
        {
            if (Viewer == null)
            {
                return;
            }

            if (Viewer.PageCurrent == Viewer.PageCount)
            {
                return;
            }
            Viewer.PageCurrent += 1;

        }

        private void LastPageClicked(object sender, System.EventArgs e)
        {
            if (Viewer == null)
            {
                return;
            }

            Viewer.PageCurrent = Viewer.PageCount;
        }

        private void ZoomInClicked(object sender, System.EventArgs e)
        {
            if (Viewer == null)
            {
                return;
            }

            Viewer.Zoom += 0.5f;
        }

        private void ZoomOutClicked(object sender, System.EventArgs e)
        {
            if (Viewer == null)
            {
                return;
            }

            Viewer.Zoom -= 0.5f;
        }


        private void SelectToolClicked(object sender, System.EventArgs e)
        {
            Viewer.SelectTool = !Viewer.SelectTool;
        }

        private void InitializeToolBar()
        {
            this.Items.Add(new ToolStripButton("Open", GetImage("fyiReporting.RdlViewer.Resources.document-open.png"), OpenClicked));
            this.Items.Add(new ToolStripButton("Save As", GetImage("fyiReporting.RdlViewer.Resources.document-save.png"), SaveAsClicked));
            this.Items.Add(new ToolStripButton("Print", GetImage("fyiReporting.RdlViewer.Resources.document-print.png"), PrintClicked));
            this.Items.Add(new ToolStripButton("DMP Print", GetImage("fyiReporting.RdlViewer.Resources.document-print.png"), DMPPrintClicked));
            this.Items.Add(new ToolStripButton("<<", null, FirstPageClicked));
            this.Items.Add(new ToolStripButton("<", null, PreviousPageClicked));
            this.Items.Add(new ToolStripButton(">", null, NextPageClicked));
            this.Items.Add(new ToolStripButton(">>", null, LastPageClicked));
            this.Items.Add(this.currentPage);
            this.Items.Add(this.pageCount);
            this.Items.Add(new ToolStripButton("Zoom In", null, ZoomInClicked));
            this.Items.Add(new ToolStripButton("Zoom Out", null, ZoomOutClicked));
            this.Items.Add(new ToolStripButton("+",null,SelectToolClicked));
        }

        void HandlePageNavigation(object sender, PageNavigationEventArgs e)
        {
            currentPage.Text = e.NewPage.ToString();
        }

        private Bitmap GetImage(string resourceName)
        {

            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                return new Bitmap(stream);
            }

        }

    }
}

