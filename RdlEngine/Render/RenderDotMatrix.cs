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
using fyiReporting.RDL;
using System.IO;
using System.Collections;
using System.Text;
using System.Collections.Generic;

namespace fyiReporting.RDL
{

    ///<summary>
    ///The primary class to "run" a report to XML
    ///</summary>
    internal class RenderDotMatrix : IPresent
    {
        Report report;					// report
        TextWriter tw;				// where the output is going
        List<String> dmpOut;
        public RenderDotMatrix(Report report, IStreamGen sg)
        {
            this.report = report;
            tw = sg.GetTextWriter();
            dmpOut = new List<string>();
        }

        public void Dispose() { } 

        public Report Report()
        {
            return report;
        }

        public bool IsPagingNeeded()
        {
            return false;
        }

        public void Start()
        {
            StringBuilder startStr = new StringBuilder();
            //Send an ESC @ command to initialize the printer
            startStr.Append(EscCodes.ResetPrinter);

            //Select 10-cpi printing (character width of 1/10 inch)
            startStr.Append(EscCodes._10CPI);
            int CPI = 10;

            //Set Left and right margin from report
            RSize rptWidth = report.ReportDefinition.PageWidth;
            RSize lM= report.ReportDefinition.LeftMargin;
            RSize rM = report.ReportDefinition.RightMargin;

            int noOfCharPerLine = (int)((rptWidth.Size / RSize.PARTS_PER_INCH) * CPI);
            int noOfCharLeftMargin = (int)((lM.Size / RSize.PARTS_PER_INCH) * CPI);
            int noOfCharRightMargin = (int)((rM.Size / RSize.PARTS_PER_INCH) * CPI);

            startStr.Append(EscCodes.LeftMargin(noOfCharLeftMargin));            
            startStr.Append(EscCodes.RightMargin(noOfCharPerLine - noOfCharRightMargin));


            //Set Page Length
            RSize rptHeight = report.ReportDefinition.PageHeight;
            int rptHeightinInches = (int)(rptHeight.Size / RSize.PARTS_PER_INCH);
            startStr.Append(EscCodes.PageLengthInInches(rptHeightinInches));

           tw.Write(startStr.ToString());
        }

        public void End()
        {
            tw.Write(EscCodes.ResetPrinter);
        }

        public void RunPages(Pages pgs)
        {
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
            if (ph.PrintOnFirstPage||ph.PrintOnLastPage) {tw.WriteLine();}           
        }

        public void PageFooterStart(PageFooter pf)
        {
        }

        public void PageFooterEnd(PageFooter pf)
        {
            if (pf.PrintOnLastPage || pf.PrintOnFirstPage) {tw.WriteLine();}
        }

        public void Textbox(Textbox tb, string t, Row r)
        {
            tw.Write(t);
        }	
        
        public void DataRegionNoRows(DataRegion d, string noRowsMsg)
        {
        }

        public bool ListStart(List l, Row r)
        {
            return true;
        }
        
        public void ListEnd(List l, Row r)
        {
            tw.WriteLine();
        }
        
        public void ListEntryBegin(List l, Row r)
        {
        }
        public void ListEntryEnd(List l, Row r)
        {
            tw.WriteLine();
        }

        public bool TableStart(Table t, Row r)
        {
            return true;
        }
        
        public void TableEnd(Table t, Row r)
        {
        }
        
        public void TableBodyStart(Table t, Row r)
        {
        }
        
        public void TableBodyEnd(Table t, Row r)
        {
        }
        
        public void TableFooterStart(Footer f, Row r)
        {
        }
        
        public void TableFooterEnd(Footer f, Row r)
        {
        }
        
        public void TableHeaderStart(Header h, Row r)
        {
        }
        
        public void TableHeaderEnd(Header h, Row r)
        {
        }
        
        public void TableRowStart(TableRow tr, Row r)
        {
        }
        
        public void TableRowEnd(TableRow tr, Row r)
        {
            tw.WriteLine();
        }
        
        public void TableCellStart(TableCell t, Row r)
        {
        }
        
        public void TableCellEnd(TableCell t, Row r)
        {
        }

        public bool MatrixStart(Matrix m, MatrixCellEntry[,] matrix, Row r, int headerRows, int maxRows, int maxCols)
        {
            return true;
        }
        
        public void MatrixColumns(Matrix m, MatrixColumns mc)
        {
        }

        public void MatrixRowStart(Matrix m, int row, Row r)
        {
        }
        
        public void MatrixRowEnd(Matrix m, int row, Row r)
        {
            tw.WriteLine();
        }
        
        public void MatrixCellStart(Matrix m, ReportItem ri, int row, int column, Row r, float h, float w, int colSpan)
        {
        }

        public void MatrixCellEnd(Matrix m, ReportItem ri, int row, int column, Row r)
        {
        }

        public void MatrixEnd(Matrix m, Row r)
        {
        }
        
        public void Chart(Chart c, Row r, ChartBase cb)
        {
        }

        public void Image(Image i, Row r, string mimeType, Stream io)
        {
        }

        public void Line(Line l, Row r)
        {
        }

        public bool RectangleStart(Rectangle rect, Row r)
        {
            return true;
        }
        
        public void RectangleEnd(Rectangle rect, Row r)
        {
        }	
        
        public void Subreport(Subreport s, Row r)
        {
        }

        public void GroupingStart(Grouping g)
        {
        }
        
        public void GroupingInstanceStart(Grouping g)
        {
        }
        
        public void GroupingInstanceEnd(Grouping g)
        {
        }
        
        public void GroupingEnd(Grouping g)
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

        public static string PageLengthInLines(byte noLines)
        {
            return ESC + "C" + noLines.ToString();
        }

        public static string PageLengthInInches(int inch)
        {
            return ESC + "C" + NUL + inch.ToString();
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
