/* ====================================================================
   Copyright (C) 2004-2008  fyiReporting Software, LLC
   Copyright (C) 2011  Peter Gill <peter@majorsilence.com>

   This file is part of the fyiReporting RDL project.
	
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.


   For additional information, email info@fyireporting.com or visit
   the website www.fyiReporting.com.
*/

using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Linq;

namespace fyiReporting.RDL
{

    ///<summary>
    /// Renders a report to DMP.   This is a page oriented formatting renderer.
    ///</summary>
    internal class RenderDotMatrix : IPresent
    {
        Report r;                  // report
        TextWriter tw;				// where the output is going

        public void Dispose() { }

        public RenderDotMatrix(Report report, IStreamGen sg)
        {
            this.r = report;
            tw = sg.GetTextWriter();
        }

        public Report Report()
        {
            return r;
        }

        public bool IsPagingNeeded()
        {
            return true;
        }

        public void Start()
        {
        }

        public void End()
        {
        }


        public void RunPages(Pages pgs)	// this does all the work
        {
            foreach (Page p in pgs)
            {
                ProcessPage(pgs, p);
            }
        }

        // render all the objects in a page in PDF
        private void ProcessPage(Pages pgs, IEnumerable items)
        {
            #region DMP Page Setting
            //Set Page Settings
            StringBuilder startStr = new StringBuilder();
            //Send an ESC @ command to initialize the printer
            startStr.Append(EscCodes.ResetPrinter);

            //Select 10-cpi printing (character width of 1/10 inch)
            startStr.Append(EscCodes._10CPI);
            int CPI = 10;
            int LPI = 6;
            //Set Left and right margin from report
            RSize rptWidth = r.ReportDefinition.PageWidth;
            RSize lM = r.ReportDefinition.LeftMargin;
            RSize rM = r.ReportDefinition.RightMargin;

            int noOfCharPerLine = (int)((rptWidth.Size / RSize.PARTS_PER_INCH) * CPI);
            int noOfCharLeftMargin = (int)((lM.Size / RSize.PARTS_PER_INCH) * CPI);
            int noOfCharRightMargin = (int)((rM.Size / RSize.PARTS_PER_INCH) * CPI);

            //startStr.Append(EscCodes.LeftMargin(noOfCharLeftMargin));
            //startStr.Append(EscCodes.RightMargin(noOfCharPerLine - noOfCharRightMargin));


            //Set Page Length
            RSize rptHeight = r.ReportDefinition.PageHeight;
            int rptHeightinInches = (int)(rptHeight.Size / RSize.PARTS_PER_INCH);
            startStr.Append(EscCodes.PageLengthInInches((rptHeightinInches)));

            
            //Set Print Quality
            startStr.Append(EscCodes.SelectLQOrDraft(true));
            

            tw.Write(startStr.ToString());
            #endregion

            List<DMPItem> lstDmpItems = new List<DMPItem>();

            foreach (PageItem pi in items)
            {
                if (pi is PageText)
                {
                    PageText pt = pi as PageText;
                    lstDmpItems.Add(new DMPItem(pt.X, pt.Y, pt.W, pt.H,LPI,pt.Text,pt.SI));
                    continue;
                }
                #region OtherItems
                /*
                if (pi.SI.BackgroundImage != null)
                {	// put out any background image
                    PageImage bgImg = pi.SI.BackgroundImage;
                    //					elements.AddImage(images, i.Name, content.objectNum, i.SI, i.ImgFormat, 
                    //						pi.X, pi.Y, pi.W, pi.H, i.ImageData,i.SamplesW, i.SamplesH, null);				   
                    //Duc Phan modified 10 Dec, 2007 to support on background image 
                    float imW = Measurement.PointsFromPixels(bgImg.SamplesW, pgs.G.DpiX);
                    float imH = Measurement.PointsFromPixels(bgImg.SamplesH, pgs.G.DpiY);
                    int repeatX = 0;
                    int repeatY = 0;
                    float itemW = pi.W - (pi.SI.PaddingLeft + pi.SI.PaddingRight);
                    float itemH = pi.H - (pi.SI.PaddingTop + pi.SI.PaddingBottom);
                    switch (bgImg.Repeat)
                    {
                        case ImageRepeat.Repeat:
                            repeatX = (int)Math.Floor(itemW / imW);
                            repeatY = (int)Math.Floor(itemH / imH);
                            break;
                        case ImageRepeat.RepeatX:
                            repeatX = (int)Math.Floor(itemW / imW);
                            repeatY = 1;
                            break;
                        case ImageRepeat.RepeatY:
                            repeatY = (int)Math.Floor(itemH / imH);
                            repeatX = 1;
                            break;
                        case ImageRepeat.NoRepeat:
                        default:
                            repeatX = repeatY = 1;
                            break;
                    }

                    //make sure the image is drawn at least 1 times 
                    repeatX = Math.Max(repeatX, 1);
                    repeatY = Math.Max(repeatY, 1);

                    float currX = pi.X + pi.SI.PaddingLeft;
                    float currY = pi.Y + pi.SI.PaddingTop;
                    float startX = currX;
                    float startY = currY;
                    for (int i = 0; i < repeatX; i++)
                    {
                        for (int j = 0; j < repeatY; j++)
                        {
                            currX = startX + i * imW;
                            currY = startY + j * imH;

                            if (r.ItextPDF)
                            {

                                iAddImage(images, bgImg.Name,
                                                content.objectNum, bgImg.SI, bgImg.ImgFormat,
                                                currX, currY, imW, imH, RectangleF.Empty, bgImg.ImageData, bgImg.SamplesW, bgImg.SamplesH, null, pi.Tooltip);
                            }
                            else
                            {
                                elements.AddImage(images, bgImg.Name,
                                           content.objectNum, bgImg.SI, bgImg.ImgFormat,
                                           currX, currY, imW, imH, RectangleF.Empty, bgImg.ImageData, bgImg.SamplesW, bgImg.SamplesH, null, pi.Tooltip);

                            }

                        }
                    }
                }

                if (pi is PageTextHtml)
                {
                    PageTextHtml pth = pi as PageTextHtml;
                    pth.Build(pgs.G);
                    ProcessPage(pgs, pth);
                    continue;
                }

                if (pi is PageLine)
                {
                    PageLine pl = pi as PageLine;


                    if (r.ItextPDF)
                    {
                        iAddLine(pl.X, pl.Y, pl.X2, pl.Y2, pl.SI);
                    }
                    else
                    {
                        elements.AddLine(pl.X, pl.Y, pl.X2, pl.Y2, pl.SI);
                    }

                    continue;
                }

                if (pi is PageEllipse)
                {
                    PageEllipse pe = pi as PageEllipse;


                    if (r.ItextPDF)
                    {
                        iAddEllipse(pe.X, pe.Y, pe.H, pe.W, pe.SI, pe.HyperLink);
                    }
                    else
                    {
                        elements.AddEllipse(pe.X, pe.Y, pe.H, pe.W, pe.SI, pe.HyperLink);
                    }


                    continue;
                }

                if (pi is PageImage)
                {
                    //PageImage i = pi as PageImage;
                    //float x = i.X + i.SI.PaddingLeft;
                    //float y = i.Y + i.SI.PaddingTop;
                    //float w = i.W - i.SI.PaddingLeft - i.SI.PaddingRight;
                    //float h = i.H - i.SI.PaddingTop - i.SI.PaddingBottom;
                    //elements.AddImage(images, i.Name, content.objectNum, i.SI, i.ImgFormat, 
                    //	x, y, w, h, i.ImageData,i.SamplesW, i.SamplesH, i.HyperLink);
                    //continue;
                    PageImage i = pi as PageImage;

                    //Duc Phan added 20 Dec, 2007 to support sized image 
                    RectangleF r2 = new RectangleF(i.X + i.SI.PaddingLeft, i.Y + i.SI.PaddingTop, i.W - i.SI.PaddingLeft - i.SI.PaddingRight, i.H - i.SI.PaddingTop - i.SI.PaddingBottom);

                    RectangleF adjustedRect;   // work rectangle 
                    RectangleF clipRect = RectangleF.Empty;
                    switch (i.Sizing)
                    {
                        case ImageSizingEnum.AutoSize:
                            adjustedRect = new RectangleF(r2.Left, r2.Top,
                                            r2.Width, r2.Height);
                            break;
                        case ImageSizingEnum.Clip:
                            adjustedRect = new RectangleF(r2.Left, r2.Top,
                                            Measurement.PointsFromPixels(i.SamplesW, pgs.G.DpiX), Measurement.PointsFromPixels(i.SamplesH, pgs.G.DpiY));
                            clipRect = new RectangleF(r2.Left, r2.Top,
                                            r2.Width, r2.Height);
                            break;
                        case ImageSizingEnum.FitProportional:
                            float height;
                            float width;
                            float ratioIm = (float)i.SamplesH / i.SamplesW;
                            float ratioR = r2.Height / r2.Width;
                            height = r2.Height;
                            width = r2.Width;
                            if (ratioIm > ratioR)
                            {   // this means the rectangle width must be corrected 
                                width = height * (1 / ratioIm);
                            }
                            else if (ratioIm < ratioR)
                            {   // this means the rectangle height must be corrected 
                                height = width * ratioIm;
                            }
                            adjustedRect = new RectangleF(r2.X, r2.Y, width, height);
                            break;
                        case ImageSizingEnum.Fit:
                        default:
                            adjustedRect = r2;
                            break;
                    }
                    if (i.ImgFormat == System.Drawing.Imaging.ImageFormat.Wmf || i.ImgFormat == System.Drawing.Imaging.ImageFormat.Emf)
                    {
                        //We dont want to add it - its already been broken down into page items;
                    }
                    else
                    {

                        if (r.ItextPDF)
                        {
                            iAddImage(images, i.Name, content.objectNum, i.SI, i.ImgFormat,
                            adjustedRect.X, adjustedRect.Y, adjustedRect.Width, adjustedRect.Height, clipRect, i.ImageData, i.SamplesW, i.SamplesH, i.HyperLink, i.Tooltip);
                        }
                        else
                        {
                            elements.AddImage(images, i.Name, content.objectNum, i.SI, i.ImgFormat,
                           adjustedRect.X, adjustedRect.Y, adjustedRect.Width, adjustedRect.Height, clipRect, i.ImageData, i.SamplesW, i.SamplesH, i.HyperLink, i.Tooltip);

                        }

                    }
                    continue;
                }

                if (pi is PageRectangle)
                {
                    PageRectangle pr = pi as PageRectangle;


                    if (r.ItextPDF)
                    {
                        iAddRectangle(pr.X, pr.Y, pr.H, pr.W, pi.SI, pi.HyperLink, patterns, pi.Tooltip);
                    }
                    else
                    {
                        elements.AddRectangle(pr.X, pr.Y, pr.H, pr.W, pi.SI, pi.HyperLink, patterns, pi.Tooltip);
                    }

                    continue;
                }

                if (pi is PagePie)
                {   // TODO
                    PagePie pp = pi as PagePie;
                    // 

                    if (r.ItextPDF)
                    {
                        iAddPie(pp.X, pp.Y, pp.H, pp.W, pi.SI, pi.HyperLink, patterns, pi.Tooltip);
                    }
                    else
                    {
                        elements.AddPie(pp.X, pp.Y, pp.H, pp.W, pi.SI, pi.HyperLink, patterns, pi.Tooltip);
                    }

                    continue;
                }

                if (pi is PagePolygon)
                {
                    PagePolygon ppo = pi as PagePolygon;


                    if (r.ItextPDF)
                    {
                        iAddPolygon(ppo.Points, pi.SI, pi.HyperLink, patterns);
                    }
                    else
                    {
                        elements.AddPolygon(ppo.Points, pi.SI, pi.HyperLink, patterns);
                    }

                    continue;
                }

                if (pi is PageCurve)
                {
                    PageCurve pc = pi as PageCurve;


                    if (r.ItextPDF)
                    {
                        iAddCurve(pc.Points, pi.SI);
                    }
                    else
                    {
                        elements.AddCurve(pc.Points, pi.SI);
                    }

                    continue;
                }
                */
                #endregion
            }

            //Sort Dmp Items
            lstDmpItems = lstDmpItems.OrderBy(q => q.R).ThenBy(q => q.C).ToList();

            //If page begins with nth line... Fill n-1 empty lines.
            DMPItem fstItm = lstDmpItems.FirstOrDefault();
            if (fstItm != null && fstItm.Y > 0)
            {
                for (int i = (fstItm.R - 1); i > 0; i--) tw.Write(EscCodes.CRLF);
            }


            var grpLines = from dmpItm in lstDmpItems
                           group dmpItm by dmpItm.R into newgrpDmpItm
                           select newgrpDmpItm;

            foreach(var dmpLine in grpLines)
            {
                StringBuilder strLine = new StringBuilder();
                DMPItem firstItm = dmpLine.FirstOrDefault();
                if(firstItm != null && firstItm.X > lM.Points)
                {
                   int wcin10CPI = (int)Math.Ceiling((firstItm.X-lM.Points) / (72.27f / 10));
                    String str = "";
                    str = str.PadLeft(wcin10CPI);
                    strLine.Append(EscCodes._10CPI + str);
                }

                foreach (DMPItem dmpTxt in dmpLine)
                {
                    strLine.Append(dmpTxt);
                }
                strLine.Append(EscCodes.CRLF);
                tw.Write(strLine.ToString());
            }

            tw.Write(EscCodes.FF + EscCodes.ResetPrinter);
        }

        public void BodyStart(Body b)
        {
        }

        public void BodyEnd(Body b)
        {
        }

        public void PageHeaderStart(PageHeader ph)
        {
        }

        public void PageHeaderEnd(PageHeader ph)
        {
        }

        public void PageFooterStart(PageFooter pf)
        {
        }

        public void PageFooterEnd(PageFooter pf)
        {
        }

        public void Textbox(Textbox tb, string t, Row row)
        {
        }

        public void DataRegionNoRows(DataRegion d, string noRowsMsg)
        {
        }

        // Lists
        public bool ListStart(List l, Row r)
        {
            return true;
        }

        public void ListEnd(List l, Row r)
        {
        }

        public void ListEntryBegin(List l, Row r)
        {
        }

        public void ListEntryEnd(List l, Row r)
        {
        }

        // Tables					// Report item table
        public bool TableStart(Table t, Row row)
        {
            return true;
        }

        public void TableEnd(Table t, Row row)
        {
        }

        public void TableBodyStart(Table t, Row row)
        {
        }

        public void TableBodyEnd(Table t, Row row)
        {
        }

        public void TableFooterStart(Footer f, Row row)
        {
        }

        public void TableFooterEnd(Footer f, Row row)
        {
        }

        public void TableHeaderStart(Header h, Row row)
        {
        }

        public void TableHeaderEnd(Header h, Row row)
        {
        }

        public void TableRowStart(TableRow tr, Row row)
        {
        }

        public void TableRowEnd(TableRow tr, Row row)
        {
        }

        public void TableCellStart(TableCell t, Row row)
        {
            return;
        }

        public void TableCellEnd(TableCell t, Row row)
        {
            return;
        }

        public bool MatrixStart(Matrix m, MatrixCellEntry[,] matrix, Row r, int headerRows, int maxRows, int maxCols)				// called first
        {
            return true;
        }

        public void MatrixColumns(Matrix m, MatrixColumns mc)	// called just after MatrixStart
        {
        }

        public void MatrixCellStart(Matrix m, ReportItem ri, int row, int column, Row r, float h, float w, int colSpan)
        {
        }

        public void MatrixCellEnd(Matrix m, ReportItem ri, int row, int column, Row r)
        {
        }

        public void MatrixRowStart(Matrix m, int row, Row r)
        {
        }

        public void MatrixRowEnd(Matrix m, int row, Row r)
        {
        }

        public void MatrixEnd(Matrix m, Row r)				// called last
        {
        }

        public void Chart(Chart c, Row r, ChartBase cb)
        {
        }

        public void Image(fyiReporting.RDL.Image i, Row r, string mimeType, Stream ior)
        {
        }

        public void Line(Line l, Row r)
        {
            return;
        }

        public bool RectangleStart(fyiReporting.RDL.Rectangle rect, Row r)
        {
            return true;
        }

        public void RectangleEnd(fyiReporting.RDL.Rectangle rect, Row r)
        {
        }

        public void Subreport(Subreport s, Row r)
        {
        }

        public void GroupingStart(Grouping g)			// called at start of grouping
        {
        }
        public void GroupingInstanceStart(Grouping g)	// called at start for each grouping instance
        {
        }
        public void GroupingInstanceEnd(Grouping g)	// called at start for each grouping instance
        {
        }
        public void GroupingEnd(Grouping g)			// called at end of grouping
        {
        }

    }



    static class EscCodes
    {
        public const string ESC = "\x1B";
        public const string NUL = "\x00";
        public const string ResetPrinter = ESC + "@";

        public static string PageFormat(int nL, int nH, int mL, int mH)
        {
            return ESC + "(C" + nL.ToString() + nH.ToString() + mL.ToString() + mH.ToString();
        }

        public static string PageFormat(int nL, int nH, int tL, int tH, int bl,int bH)
        {
            return ESC + "(c" + nL.ToString() + nH.ToString() + tL.ToString() + tH.ToString() + bl.ToString() + bH.ToString();
        }

        public static string PageLengthInLines(int noLines)
        {
            return ESC + "C" + noLines.ToString();
        }

        public static string PageLengthInInches(int inch)
        {

            int intValue = 182;
            // Convert integer 182 as a hex in a string variable
            byte[] hexValue = BitConverter.GetBytes(inch);

            string str = ESC + "C" + NUL + ConvertUnicodeString("U+0006");
           return str;
        }

        public static String ConvertUnicodeString(String str)
        {
            String result = "";
            String[] uCodes = str.Split("U+".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (String uStr in uCodes)
            {
                int c = int.Parse(uStr, System.Globalization.NumberStyles.HexNumber);
                result += (char)c;
            }
            return result;
        }

        public static string BottomMargin(byte n)
        {
            return ESC + "N" + n.ToString();
        }

        public const string CancelMargin = ESC + "O";

        public static string RightMargin(int n)
        {
            return ESC + "Q" + n.ToString();
        }

        public static string LeftMargin(int n)
        {
            return ESC + "l" + n.ToString();
        }

        public const string CR = "\x0D";
        public const string LF = "\x0A";
        public const string CRLF = "\x0D\x0A";
        public const string FF = "\x0C";

        public static string AbsHPossition(int nL, int nH)
        {
            return ESC + "$" + nL.ToString() + nH.ToString();
        }

        public static string RelHPossition(int nL, int nH)
        {
            return ESC + @"\" + nL.ToString() + nH.ToString();
        }

        public static string AbsVPossition(int nL, int nH, int mL, int mH)
        {
            return ESC + "(V" + nL.ToString() + nH.ToString() + mL.ToString() + mH.ToString();
        }

        public static string AdvancePrintPossitionV(byte n)
        {
            return ESC + "J" + n.ToString();
        }

        public const string HT = "\x09";
        public const string VT = "\x0B";
        public const string BS = "\x08";

        public static string Skip(bool isVertical ,byte n)
        {
            int isV = isVertical ? 1 : 0;
            return ESC + "f" + isV.ToString() + n.ToString();
        }

        public static string Unit(int nL, int nH, int m)
        {
           return ESC + "(U" + nL.ToString() + nH.ToString() + m.ToString();
        }

        public const string OneByEightInchLineSpacing = ESC + "0";
        public const string OneBySixInchLineSpacing = ESC + "2";
        public const string SevenBy72InchLineSpacing = ESC + "1";
        public static string NBy216InchLineSpacing(byte n)
        {
            return ESC + "3" + n.ToString();
        }
        public static string NBy380InchLineSpacing(byte n)
        {
            return ESC + "+" + n.ToString();
        }
        public static string NBy60InchLineSpacing(byte n)
        {
            return ESC + "A" + n.ToString();
        }

        public static string HTabs(byte[] n)
        {
            String res = ESC + "D";
            foreach(byte b in n) res += n;
            res = res + NUL;
            return res;
        }

        public static string VTabs(byte[] n)
        {
            String res = ESC + "B";
            foreach (byte b in n) res += n;
            res = res + NUL;
            return res;
        }
        public static string VTabsVFU(byte[] n)
        {
            String res = ESC + "b";
            foreach (byte b in n) res += n;
            res = res + NUL;
            return res;
        }

        public static string VTabVFU(byte m)
        {
            return ESC + "/" + m.ToString();
        }
        
        /*
        public enum Justify
        {
            FlushLeft = 0,
            Centered = 1,
            FlushRight = 2,
            Full = 3
        }*/

        public static string Justify(byte n)
        {
            return ESC + "a" + n.ToString();
        }

        public static string AssignCharTable(int nL,int nH, int d1,int d2,int d3)
        {
            return ESC + "(t" + nL.ToString() + nH.ToString() + d1.ToString() + d2.ToString() + d3.ToString();
        }

        public static string SelectCharTable(int n)
        {
            return ESC + "t" + n.ToString();
        }

        public static string SelectInterNationalCharTable(int n)
        {
            return ESC + "R" + n.ToString();
        }

        public static string SetUserDefinedChars(int n, int m, int a0, int a1, int a2, char[] chars)
        {
            String res = ESC + "&" + NUL + n.ToString() + m.ToString() + "[" + a0.ToString() + a1.ToString() + a2.ToString();
            foreach (char c in chars) res += c;
            res = res + "]";
            return res;
        }

        public static string CopyROMtoRAM(bool isRoman, int m)
        {
            int isV = isRoman ? 0 : 1;
            return ESC + ":" + NUL + isV.ToString() + m.ToString();
        }

        public static string SelectUserDefinedChars(int n)
        {
            return ESC + "%" + n.ToString();
        }

        public static string SelectLQOrDraft(bool isDraft)
        {
            int isV = isDraft ? 0 : 1;
            return ESC + "x" + isV.ToString() ;
        }

        public static string SelectTypeface(int n)
        {
            return ESC + "k" + n.ToString();
        }

        public static string SelectFont(int m, int nL, int nH)
        {
            return ESC + "X" + nL.ToString() + nH.ToString();
        }

        public static string HMI(int nL, int nH)
        {
            return ESC + "c" + nL.ToString() + nH.ToString();
        }

        public const string _10CPI = ESC + "P";
        public const string _12CPI = ESC + "M";
        public const string _15CPI = ESC + "g";

        public static string ProportionalMode(int n)
        {
            return ESC + "p" + n.ToString();
        }

        public static string InterCharSpace(int n)
        {
            return ESC + " " + n.ToString();
        }

        public static string MasterSelect(int n)
        {
            return ESC + "!" + n.ToString();
        }

        public const string BELL = ESC + "\x07";
        public const string BOLD = ESC + "E";
        public const string CancelBOLD = ESC + "F";
        public const string Italic = ESC + "4";
        public const string CancelItalic = ESC + "5";
        public const string DoubleStrike = ESC + "G";
        public const string CancelDoubleStrike = ESC + "H";
        public const string Condensed = ESC + "\x0F";
        public const string CancelCondensed = "\x12";
        public const string DoubleWidth = ESC + "\x0E";
        public const string CancelDoubleWidth = "\x14";
        public const string DisablePaperOutDetector = ESC + "8";
        public const string EnablePaperOutDetector = ESC + "9";



        public static string Underline(bool isUnderLine)
        {
            int isV = isUnderLine ? 1 : 0;
            return ESC + "-" + isV.ToString();
        }

        public static string SelectLineScore( int nL, int nH, int m, int d1, int d2)
        {
           
            return ESC + "(-"+ nL.ToString() + nH.ToString() + m.ToString() + d1.ToString() + d2.ToString();
        }

        public static string SuperSubscipt(bool isSupescript)
        {
            int isV = isSupescript ? 1 : 0;
            return ESC + "S" + isV.ToString();
        }
        public const string CancelSuperSubscipt = ESC + "T";

        public static string SelectCharStyle(int n)
        {
            return ESC + "q" + n.ToString();
        }

        public static string DoubleWidthOnOff(bool flag)
        {
            int isV = flag ? 1 : 0;
            return ESC + "W" + isV.ToString();
        }

        public static string DoubleHeightOnOff(bool flag)
        {
            int isV = flag ? 1 : 0;
            return ESC + "w" + isV.ToString();
        }

        public static string PrintData(int nL, int nH, char[] chars)
        {
            string res = ESC + "(^" ;
            foreach (char c in chars) res += c;
            return res;
        }

        public const string PrintUpperControl9Pin = ESC + "6";
        public const string PrintUpperControl = ESC + "7";
        public const string SelectMSB0 = ESC + "=";
        public const string SelectMSB1 = ESC + ">";
        public const string CancelMSB = ESC + "#";
        public static string EnablePrintControlCodes(int n)
        {
            return ESC + "I" + n.ToString();
        }
        public static string SelectPrintUpperControl(int n)
        {
            return ESC + "m" + n.ToString();
        }

        public static string PaperLoading(char n)
        {
            return ESC + "\x19" + n.ToString();
        }

        public static string SelectUniDirectional(bool flag)
        {
            int isV = flag ? 1 : 0;
            return ESC + "U" + isV.ToString();
        }

        public const string UniDirectionOneLine = ESC + "<";

        public static string LowSpeed(bool isLowSpeed)
        {
            int isV = isLowSpeed ? 1 : 0;
            return ESC + "s" + isV.ToString();
        }

        public static string SelectGraphicsMode(int nL, int nH, int m )
        {
            return ESC + "(G" + nL.ToString() + nH.ToString() + m.ToString();
        }

        public static string SelectMicroWaveMode(bool flag)
        {
            int isV = flag ? 1 : 0;
            return ESC + "(i0100" + isV.ToString();
        }

        public static string RevesePaperFeed(int n)
        {
            return ESC + "j" + n.ToString();
        }
    }
}
