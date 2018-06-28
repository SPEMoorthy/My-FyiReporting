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

namespace fyiReporting.RDL
{

    ///<summary>
    ///The primary class to "run" a report to XML
    ///</summary>
    internal class RenderDotMatrix : IPresent
    {
        Report report;					// report
        TextWriter tw;				// where the output is going

        public RenderDotMatrix(Report report, IStreamGen sg)
        {
            this.report = report;
            tw = sg.GetTextWriter();
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
            tw.WriteLine("Dot Printing Start");
        }

        public void End()
        {
            tw.WriteLine("Dot Printing End");
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
            object value = tb.Evaluate(report, r);
            tw.Write(value);
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

        public static string PageLengthInInches(byte inch)
        {
            return ESC + "C" + NUL + inch.ToString();
        }

        public static string BottomMargin(byte n)
        {
            return ESC + "N" + n.ToString();
        }

        public const string CancelMargin = ESC + "O";

        public static string RightMargin(byte n)
        {
            return ESC + "Q" + n.ToString();
        }

        public static string LeftMargin(byte n)
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
    }
}
