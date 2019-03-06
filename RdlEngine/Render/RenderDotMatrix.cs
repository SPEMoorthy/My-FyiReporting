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
            bool isRollPaper = r.ReportDefinition.RollPaper;
            if (isRollPaper)
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

                startStr.Append(EscCodes.LeftMargin(noOfCharLeftMargin));
                startStr.Append(EscCodes.RightMargin(noOfCharPerLine - noOfCharRightMargin));


                //Set Page Length
                //RSize rptHeight = r.ReportDefinition.PageHeight;
                //int rptHeightinInches = (int)(rptHeight.Size / RSize.PARTS_PER_INCH);
                //startStr.Append(EscCodes.PageLengthInInches((rptHeightinInches)));


                //Set Print Quality
                startStr.Append(EscCodes.SelectLQOrDraft(true));


                tw.Write(startStr.ToString());
                #endregion
                foreach (Page p in pgs)
                {
                    ProcessPageForRollPaper(pgs, p);
                }
                tw.Write(EscCodes.FF + EscCodes.CR);
            }
            else
            {
                foreach (Page p in pgs)
                {
                    ProcessPage(pgs, p);
                }
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

            bool isRollPaper = r.ReportDefinition.RollPaper;

            int noOfCharPerLine = (int)((rptWidth.Size / RSize.PARTS_PER_INCH) * CPI);
            int noOfCharLeftMargin = (int)((lM.Size / RSize.PARTS_PER_INCH) * CPI);
            int noOfCharRightMargin = (int)((rM.Size / RSize.PARTS_PER_INCH) * CPI);

            startStr.Append(EscCodes.LeftMargin(noOfCharLeftMargin));
            startStr.Append(EscCodes.RightMargin(noOfCharPerLine - noOfCharRightMargin));

            
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


            /*
            //If page begins with nth line... Fill n-1 empty lines.
            DMPItem fstItm = lstDmpItems.FirstOrDefault();
            if (fstItm != null && fstItm.Y > 0)
            {
                for (int i = (fstItm.R - 1); i > 0; i--) tw.Write(EscCodes.CRLF);
            }
            */

            var grpLines = from dmpItm in lstDmpItems
                           group dmpItm by dmpItm.R into newgrpDmpItm
                           select newgrpDmpItm;
            int curLine = 1;
            foreach(var dmpLine in grpLines)
            {
                StringBuilder strLine = new StringBuilder();
                DMPItem firstItm = dmpLine.FirstOrDefault();

                //If Line not matching curr line add empty lines.
               while(curLine < firstItm.R)
                {
                    tw.Write(EscCodes.CRLF);
                    curLine++;
                }

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
                curLine++;
            }

            tw.Write(EscCodes.FF + EscCodes.CR);
        }


        // render all the objects in a page in PDF
        private void ProcessPageForRollPaper(Pages pgs, IEnumerable items)
        {
            int CPI = 10;
            int LPI = 6;
            //Set Left and right margin from report
            RSize rptWidth = r.ReportDefinition.PageWidth;
            RSize lM = r.ReportDefinition.LeftMargin;
            RSize rM = r.ReportDefinition.RightMargin;

            List<DMPItem> lstDmpItems = new List<DMPItem>();

            foreach (PageItem pi in items)
            {
                if (pi is PageText)
                {
                    PageText pt = pi as PageText;
                    lstDmpItems.Add(new DMPItem(pt.X, pt.Y, pt.W, pt.H, LPI, pt.Text, pt.SI));
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


            /*
            //If page begins with nth line... Fill n-1 empty lines.
            DMPItem fstItm = lstDmpItems.FirstOrDefault();
            if (fstItm != null && fstItm.Y > 0)
            {
                for (int i = (fstItm.R - 1); i > 0; i--) tw.Write(EscCodes.CRLF);
            }
            */

            var grpLines = from dmpItm in lstDmpItems
                           group dmpItm by dmpItm.R into newgrpDmpItm
                           select newgrpDmpItm;
            int curLine = 1;
            foreach (var dmpLine in grpLines)
            {
                StringBuilder strLine = new StringBuilder();
                DMPItem firstItm = dmpLine.FirstOrDefault();

                //If Line not matching curr line add empty lines.
                while (curLine < firstItm.R)
                {
                    tw.Write(EscCodes.CRLF);
                    curLine++;
                }

                if (firstItm != null && firstItm.X > lM.Points)
                {
                    int wcin10CPI = (int)Math.Ceiling((firstItm.X - lM.Points) / (72.27f / 10));
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
                curLine++;
            }           
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
            return ESC + "(C" + ((char)nL) + ((char)nH) + ((char)mL) + ((char)mH);
        }

        public static string PageFormat(int nL, int nH, int tL, int tH, int bL,int bH)
        {
            return ESC + "(c" + ((char)nL) + ((char)nH) + ((char)tL) + ((char)tH) + ((char)bL) + ((char)bH);
        }

        public static string PageLengthInLines(int noLines)
        {
            return ESC + "C" + (char)noLines;
        }

        public static string PageLengthInInches(int inch)
        {
           string str = ESC + "C" + NUL + (char)inch;
           return str;
        }

        public static string BottomMargin(byte n)
        {
            return ESC + "N" + (char)n;
        }

        public const string CancelMargin = ESC + "O";

        public static string RightMargin(int n)
        {
            return ESC + "Q" + (char)n;
        }

        public static string LeftMargin(int n)
        {
            return ESC + "l" + (char)n;
        }

        public const string CR = "\x0D";
        public const string LF = "\x0A";
        public const string CRLF = "\x0D\x0A";
        public const string FF = "\x0C";

        public static string AbsHPossition(int nL, int nH)
        {
            return ESC + "$" + ((char)nL) + ((char)nH);
        }

        public static string RelHPossition(int nL, int nH)
        {
            return ESC + @"\" + ((char)nL) + ((char)nH);
        }

        public static string AbsVPossition(int nL, int nH, int mL, int mH)
        {
            return ESC + "(V" + ((char)nL) + ((char)nH) + ((char)mL) + ((char)mH);
        }

        public static string AdvancePrintPossitionV(byte n)
        {
            return ESC + "J" + (char)n;
        }

        public const string HT = "\x09";
        public const string VT = "\x0B";
        public const string BS = "\x08";

        public static string Skip(bool isVertical ,byte n)
        {
            int isV = isVertical ? 1 : 0;
            return ESC + "f" + ((char)isV )+ ((char)n);
        }

        public static string Unit(int nL, int nH, int m)
        {
           return ESC + "(U" + ((char)nL) + ((char)nH) + ((char)m);
        }

        public const string OneByEightInchLineSpacing = ESC + "0";
        public const string OneBySixInchLineSpacing = ESC + "2";
        public const string SevenBy72InchLineSpacing = ESC + "1";
        public static string NBy216InchLineSpacing(byte n)
        {
            return ESC + "3" + (char)n;
        }
        public static string NBy380InchLineSpacing(byte n)
        {
            return ESC + "+" + (char)n;
        }
        public static string NBy60InchLineSpacing(byte n)
        {
            return ESC + "A" + (char)n;
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
            return ESC + "/" + (char)m;
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
            return ESC + "a" + (char)n;
        }

        public static string AssignCharTable(int nL,int nH, int d1,int d2,int d3)
        {
            return ESC + "(t" + (char)nL + (char)nH + (char)d1 + (char)d2 + (char)d3;
        }

        public static string SelectCharTable(int n)
        {
            return ESC + "t" + (char)n;
        }

        public static string SelectInterNationalCharTable(int n)
        {
            return ESC + "R" + (char)n;
        }

        public static string SetUserDefinedChars(int n, int m, int a0, int a1, int a2, char[] chars)
        {
            String res = ESC + "&" + NUL + (char)n + (char)m + "[" + (char)a0 + (char)a1 + (char)a2;
            foreach (char c in chars) res += c;
            res = res + "]";
            return res;
        }

        public static string CopyROMtoRAM(bool isRoman, int m)
        {
            int isV = isRoman ? 0 : 1;
            return ESC + ":" + NUL + (char)isV + (char)m;
        }

        public static string SelectUserDefinedChars(int n)
        {
            return ESC + "%" + (char)n;
        }

        public static string SelectLQOrDraft(bool isDraft)
        {
            int isV = isDraft ? 0 : 1;
            return ESC + "x" + (char)isV;
        }

        public static string SelectTypeface(int n)
        {
            return ESC + "k" + (char)n;
        }

        public static string SelectFont(int m, int nL, int nH)
        {
            return ESC + "X" + (char)nL + (char)nH;
        }

        public static string HMI(int nL, int nH)
        {
            return ESC + "c" + (char)nL + (char)nH;
        }

        public const string _10CPI = ESC + "P";
        public const string _12CPI = ESC + "M";
        public const string _15CPI = ESC + "g";

        public static string ProportionalMode(int n)
        {
            return ESC + "p" + (char)n;
        }

        public static string InterCharSpace(int n)
        {
            return ESC + " " + (char)n;
        }

        public static string MasterSelect(int n)
        {
            return ESC + "!" + (char)n;
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
            return ESC + "-" + (char)isV;
        }

        public static string SelectLineScore( int nL, int nH, int m, int d1, int d2)
        {
           
            return ESC + "(-"+ (char)nL + (char)nH + (char)m + (char)d1 + (char)d2;
        }

        public static string SuperSubscipt(bool isSupescript)
        {
            int isV = isSupescript ? 1 : 0;
            return ESC + "S" + (char)isV;
        }
        public const string CancelSuperSubscipt = ESC + "T";

        public static string SelectCharStyle(int n)
        {
            return ESC + "q" + (char)n;
        }

        public static string DoubleWidthOnOff(bool flag)
        {
            int isV = flag ? 1 : 0;
            return ESC + "W" + (char)isV;
        }

        public static string DoubleHeightOnOff(bool flag)
        {
            int isV = flag ? 1 : 0;
            return ESC + "w" + (char)isV;
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
            return ESC + "I" + (char)n;
        }
        public static string SelectPrintUpperControl(int n)
        {
            return ESC + "m" + (char)n;
        }

        public static string PaperLoading(char n)
        {
            return ESC + "\x19" + (char)n;
        }

        public static string SelectUniDirectional(bool flag)
        {
            int isV = flag ? 1 : 0;
            return ESC + "U" + (char)isV;
        }

        public const string UniDirectionOneLine = ESC + "<";

        public static string LowSpeed(bool isLowSpeed)
        {
            int isV = isLowSpeed ? 1 : 0;
            return ESC + "s" + (char)isV;
        }

        public static string SelectGraphicsMode(int nL, int nH, int m )
        {
            return ESC + "(G" + (char)nL + (char)nH + (char)m;
        }

        public static string SelectMicroWaveMode(bool flag)
        {
            int isV = flag ? 1 : 0;
            return ESC + "(i0100" + (char)isV;
        }

        public static string RevesePaperFeed(int n)
        {
            return ESC + "j" + (char)n;
        }
    }
}
