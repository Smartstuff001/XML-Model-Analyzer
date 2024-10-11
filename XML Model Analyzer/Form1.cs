using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using FastColoredTextBoxNS;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Configuration;
using System.Collections.Specialized;
using System.Net;

namespace XML_Model_Analyzer
{
    public partial class Form1 : Form
    {
        Settings settings;
        
        public int ownUpdate = 0;
        public string logText = "";

        public Form1(string[] args)
        {
            InitializeComponent();
            this.splitContainer2.SplitterDistance = splitContainer2.Height / 2;
            splitContainer2.Panel2Collapsed = true;

            sl = new SyntaxHighlighter(fastColoredTextBox1);
            sl.InitStyleSchema(Language.Custom);
            fastColoredTextBox1.Language = Language.Custom;

            this.AllowDrop = true;
            this.fastColoredTextBox1.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
            this.fastColoredTextBox1.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.fastColoredTextBox1.DragDrop += new DragEventHandler(Form1_DragDrop);


            if (args != null && args.Length > 0)
            {
                dm = new DataModel(this, args[0]);
                toolStripProgressBar1.Value = 50;
                backgroundWorker1.RunWorkerAsync();
            }

            //sortByElementToolStripMenuItem.Visible = false;

            /*
            fastColoredTextBox1.AllowSeveralTextStyleDrawing = true;
            fastColoredTextBox1.AddStyle(originLink);
            fastColoredTextBox1.AddStyle(originLinkWoConnection);
            fastColoredTextBox1.AddStyle(originLinkWredundandConnection);
            fastColoredTextBox1.AddStyle(targetLink);
            fastColoredTextBox1.AddStyle(targetLinkWoConnection);
            */

        }

        public void setKey(bool value)
        {
            //Create the object
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            //make changes
            if (value)
            {
                config.AppSettings.Settings["Key0"].Value = "1";
            }
            else
            {
                config.AppSettings.Settings["Key0"].Value = "0";
            }

            //save to apply changes
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if ((files!=null) && (saveIfUnsaved() == false))
            {
                button1.Visible = false;
                label1.Visible = false;
                lockFile = false;
                lsready = false;
                backgroundWorker1.CancelAsync();
                System.Threading.Thread.Sleep(500);
                if (!backgroundWorker1.IsBusy)
                {
                    dm = new DataModel(this, files[0]);
                    toolStripProgressBar1.Value = 50;
                    backgroundWorker1.RunWorkerAsync();
                }
                else
                {
                    string caption = "File/folder could not be opened";
                    string message = "The former operation is still running and could not be canceled.";
                    MessageBoxButtons buttons = MessageBoxButtons.OK;
                    MessageBox.Show(message, caption, buttons);
                }
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settings = new Settings(this);
            settings.Show();
        }

        public bool internalXmlRepresentation = true;
        public bool onTheFlyLinkage = false;
        public void settingsFormAccepted()
        {
            //internalXmlRepresentation = settings.checkBox2.Checked;
            //onTheFlyLinkage = settings.checkBox1.Checked;
        }

        // **** FCTB 
        SyntaxHighlighter sl;
        MarkerStyle SameWordsStyle = new MarkerStyle(new SolidBrush(Color.FromArgb(40, Color.Gray)));
        private void fastColoredTextBox1_SelectionChangedDelayed(object sender, EventArgs e)
        {
            fastColoredTextBox1.VisibleRange.ClearStyle(SameWordsStyle);
            if (!fastColoredTextBox1.Selection.IsEmpty)
                return;//user selected diapason

            //get fragment around caret
            var fragment = fastColoredTextBox1.Selection.GetFragment(@"\w");
            string text = fragment.Text;
            if (text.Length == 0)
                return;
            //highlight same words
            var ranges = fastColoredTextBox1.VisibleRange.GetRanges("\\b" + text + "\\b").ToArray();
            if (ranges.Length > 1)
                foreach (var r in ranges)
                    r.SetStyle(SameWordsStyle);
        }


        private void fastColoredTextBox1_TextChangedDelayed(object sender, TextChangedEventArgs e)
        {
            if (sl != null)
            {
                /*
                if (dm.xmlType != XMLTypes.AUTOSAR)
                {
                    dm.updateFile();
                    updateListBox();
                }*/

/*
                if (dm.idTag)
                {
                    Range sel = fastColoredTextBox1.Selection;
                    int filenr = (int)dm.linesToFiles[sel.Start.iLine];
                    if (filenr >= 0)
                    {
                        dm.fileModified[filenr] = true;
                    }
                }
*/

                //expand visible range (+- margin)
                var startLine = Math.Max(0, fastColoredTextBox1.VisibleRange.Start.iLine - margin);
                var endLine = Math.Min(fastColoredTextBox1.LinesCount - 1, fastColoredTextBox1.VisibleRange.End.iLine + margin);
                var range = new Range(fastColoredTextBox1, 0, startLine, 0, endLine);

                Range fullRange = fastColoredTextBox1.Range;
                fullRange.ClearStyle(originLink, targetLink, originLinkWoConnection, targetLinkWoConnection, originLinkWredundandConnection);
                //range.ClearStyle(originLink, targetLink, originLinkWoConnection, targetLinkWoConnection, originLinkWredundandConnection);
                sl.HighlightSyntax(fastColoredTextBox1.Language, range);

                //if (dm.xmlType == XMLTypes.AUTOSAR)
                {
                    if ((dm.idTag) && (ownUpdate == 0))
                    {
                        button1.Visible = true;
                    }
                    if (button1.Visible == false)
                    {
                        drawTokens(fastColoredTextBox1, range);
                    }
                }/*
                else
                {
                    drawTokens(fastColoredTextBox1, range);
                }*/
            }

        }

        const int margin = 2000;
        private void fastColoredTextBox1_VisibleRangeChangedDelayed(object sender, EventArgs e)
        {
            if (sl != null)
            {
                //expand visible range (+- margin)
                var startLine = Math.Max(0, fastColoredTextBox1.VisibleRange.Start.iLine - margin);
                var endLine = Math.Min(fastColoredTextBox1.LinesCount - 1, fastColoredTextBox1.VisibleRange.End.iLine + margin);
                var range = new Range(fastColoredTextBox1, 0, startLine, 0, endLine);

                sl.HighlightSyntax(fastColoredTextBox1.Language, range);
                if (button1.Visible == false)
                {
                    drawTokens(fastColoredTextBox1, range);
                }
            }

        }


        private void fastColoredTextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sl != null)
            {
                //sl.HighlightSyntax(fastColoredTextBox1.Language, e.ChangedRange);
            }
        }


        // **end** FCTB 

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        DataModel dm = new DataModel();
        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveIfUnsaved() == false)
            {
                if (dc != null) { dc.Close(); }
                if (fastColoredTextBox1.findForm != null) { fastColoredTextBox1.findForm.Close(); }
                if (fastColoredTextBox1.replaceForm != null) { fastColoredTextBox1.replaceForm.Close(); }
                if (settings != null) { settings.Close(); }
                using (FolderBrowserDialog openFileDialog = new FolderBrowserDialog())
                {
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        button1.Visible = false;
                        label1.Visible = false;
                        lockFile = false;
                        lsready = false;
                        backgroundWorker1.CancelAsync();
                        System.Threading.Thread.Sleep(500);
                        if (!backgroundWorker1.IsBusy)
                        {
                            dm = new DataModel(this, openFileDialog.SelectedPath);
                            toolStripProgressBar1.Value = 50;
                            backgroundWorker1.RunWorkerAsync();
                        }
                        else
                        {
                            string caption = "File/folder could not be opened";
                            string message = "The former operation is still running and could not be canceled.";
                            MessageBoxButtons buttons = MessageBoxButtons.OK;
                            MessageBox.Show(message, caption, buttons);
                        }
                    }
                }
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripProgressBar1.Value = 100;
            if (dm.xmlFolderRead)
            {
                this.Text = @"XML Model Analyzer - " + dm.filePath;
                updateListBox();
                init = false;
                lsready = true;
                drawConnections();
            }
            else
            {
                pictureBox1.Image = null;
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (dm.readXmlFolder() == false)
            {
                
                init = false;
                lsready = false;
                dm = new DataModel();
                //fastColoredTextBox1.Range.ClearStyle(ros);
                fastColoredTextBox1.Text = "";
                lockFile = false;
            }
        }

        int counter = 0;
        //public ReadOnlyStyle ros = new ReadOnlyStyle();
        private void timer1_Tick(object sender, EventArgs e)
        {
            if ((dm.xmlFolderRead == true) && (dm.xmlFolderReadDone == false))
            {
                ownUpdate = 10;
                //fastColoredTextBox1.Range.ClearStyle(ros);
                fastColoredTextBox1.Text = dm.fileContent;
                if (fastColoredTextBox1.Text != dm.fileContent)
                {
                    logText += "Assignment of processed text to textbox failed.\r\n";
                }
                fastColoredTextBox1.ClearUndo();
                //fastColoredTextBox1.Range.SetStyle(ros, @".*?<file name="".*?"">|.*?<\/file>|.*?<open>|.*?<\/open>");

                /*
                fastColoredTextBox1.OpenBindingFile(@"C:\temp\test.xml", Encoding.UTF8);
                fastColoredTextBox1.IsChanged = false;
                fastColoredTextBox1.ClearUndo();
                GC.Collect();
                GC.GetTotalMemory(true);*/

                toolStripStatusLabel3.Text = dm.xmlType.ToString();
                /*
                fastColoredTextBox1.Hints.Clear();
                Range r = new Range(fastColoredTextBox1, 3);
                Hint hint = new Hint(r, "This is hint ", true, true);
                fastColoredTextBox1.Hints.Add(hint);
                fastColoredTextBox2.Hints.Clear();
                Range r2 = new Range(fastColoredTextBox2, 3);
                Hint hint2 = new Hint(r2, "This is hint ", true, true);
                fastColoredTextBox2.Hints.Add(hint2);
                */
                dm.xmlFolderReadDone = true;
            }

            if (textchfromtextassignment && dm.xmlFolderReadDone)
            {
                dm.fileContent = fastColoredTextBox1.Text;
                dm.createCharToLineArray();
                dm.getFileNamesAndLines();

                Range sel = fastColoredTextBox1.Selection;
                if (sel.Start.iLine < dm.linesToFiles.Count)
                {
                    int filenr = (int)dm.linesToFiles[sel.Start.iLine];
                    if (filenr >= 0)
                    {
                        dm.fileModified[filenr] = true;
                    }
                    textchfromtextassignment = false;
                }
            }

            if (button1clicked)
            {
                button1clicked = false;
                int line = fastColoredTextBox1.Selection.Start.iLine;
                button1.Visible = false;
                label1.Visible = false;
                dm.updateFile();
                updateListBox();
                Range r = new Range(fastColoredTextBox1, 0, line, 0, line);
                fastColoredTextBox1.Selection = r;
                fastColoredTextBox1.DoSelectionVisible();
                fastColoredTextBox1.Invalidate();
                this.ActiveControl = fastColoredTextBox1;
            }

            if (setPictureScrollMarker)
            {
                setPictureScroll();
                setPictureScrollMarker = false;
            }

            if (counter++ % 200 == 0)
            {
                updateLog();
            }
            if (counter % 10 == 0)
            {
                drawConnections();
                GC.Collect();
                GC.GetTotalMemory(true);
            }
            if (ownUpdate > 0)
            {
                ownUpdate--;
            }

            if (updateFiles)
            {
                updateFiles = false;
                dm.xmlFolderRead = false;
                dm.idTag = false;
                dm.getFileNamesAndLines();
                dm.createLinkStructureArray();
                dm.xmlFolderRead = true;
                dm.idTag = true;
                updateListBox();
                init = false;
                lsready = true;
                drawConnections();
                fastColoredTextBox1.ClearUndo();
                button1clicked = true;
            }
        }
        bool updateFiles = false;
        Style originLink = new TextStyle(Brushes.Black, Brushes.LightSalmon, FontStyle.Bold);
        Style targetLink = new TextStyle(Brushes.Black, Brushes.CornflowerBlue, FontStyle.Bold);
        Style originLinkWoConnection = new TextStyle(Brushes.Black, Brushes.LightGray, FontStyle.Bold);
        Style targetLinkWoConnection = new TextStyle(Brushes.Black, Brushes.LightSlateGray, FontStyle.Bold);
        Style originLinkWredundandConnection = new TextStyle(Brushes.Black, Brushes.LightGoldenrodYellow, FontStyle.Bold);

        private void drawTokens(FastColoredTextBox fastColoredTextBox, Range range)
        {
            if (dm.xmlFolderRead == false)
            {
                return;
            }

            if (dm.idTag == true)
            {
                Range fullRange = fastColoredTextBox.Range;
                //fullRange.ClearStyle(originLink, targetLink);

                //Range range = new Range(fastColoredTextBox, new Place(0, ro.Start.iLine - 10 < 0 ? 0 : ro.Start.iLine - 10), ro.End);

                range.ClearStyle(originLink, targetLink, originLinkWoConnection, targetLinkWoConnection, originLinkWredundandConnection);
                int c1 = (int)dm.lineToChar[range.Start.iLine];
                int endLine = Math.Min(dm.lineToChar.Count - 1, range.End.iLine);
                int c2 = (int)dm.lineToChar[endLine];
                fastColoredTextBox.Hints.Clear();

                foreach (DictionaryEntry g in dm.hrefToPosition)
                {
                    ArrayList al = (ArrayList)g.Value;
                    foreach (Point p in al)
                    {
                        if ((p.X >= c1) && (p.X <= c1 + c2))
                        {
                            int startCol = p.X - (int)dm.lineToChar[(int)dm.charToLine[p.X]];
                            Range setCol = new Range(fastColoredTextBox, startCol, (int)dm.charToLine[p.X], p.Y + startCol, (int)dm.charToLine[p.X]);
                            setCol.ClearStyle(sl.CommentStyle, sl.XmlTagBracketStyle, sl.XmlTagNameStyle, sl.XmlAttributeStyle, sl.XmlAttributeValueStyle, sl.XmlEntityStyle, sl.XmlCDataStyle);
                            if (al.Count > 1)
                            {
                                setCol.SetStyle(originLinkWredundandConnection);
                            }
                            // todo: to and from could contain the same places
                            if ((dm.hrefFromPosition.ContainsKey(g.Key)) && (((ArrayList)dm.hrefFromPosition[(string)g.Key]).Count > 0))
                            {
                                setCol.SetStyle(originLink);
                            }
                            else
                            {
                                setCol.SetStyle(originLinkWoConnection);
                            }
                            /*
                            Range r = new Range(fastColoredTextBox, (int)dm.charToLine[p.X]);
                            //Hint hint = new Hint(r, checkFileForHref((string)g.Key, true), true, true);
                            Hint hint = new Hint(r, "da", true, true);
                            fastColoredTextBox.Hints.Add(hint);
                            */
                        }
                    }
                }
                foreach (DictionaryEntry g in dm.hrefFromPosition)
                {
                    ArrayList al = (ArrayList)g.Value;
                    foreach (Point p in al)
                    {
                        if ((p.X >= c1) && (p.X + p.Y <= c2))
                        {
                            int startCol = p.X - (int)dm.lineToChar[(int)dm.charToLine[p.X]];
                            Range setCol = new Range(fastColoredTextBox, startCol, (int)dm.charToLine[p.X], p.Y + startCol, (int)dm.charToLine[p.X]);
                            setCol.ClearStyle(sl.CommentStyle, sl.XmlTagBracketStyle, sl.XmlTagNameStyle, sl.XmlAttributeStyle, sl.XmlAttributeValueStyle, sl.XmlEntityStyle, sl.XmlCDataStyle);
                            if (dm.hrefToPosition.ContainsKey(g.Key))
                            {
                                setCol.SetStyle(targetLink);
                            }
                            else
                            {
                                setCol.SetStyle(targetLinkWoConnection);
                            }
                            /*
                            Range r = new Range(fastColoredTextBox, (int)dm.charToLine[p.X]);
                            //Hint hint = new Hint(r, checkFileForHref((string)g.Key, false), true, true);
                            Hint hint = new Hint(r, "da", true, true);
                            fastColoredTextBox.Hints.Add(hint);
                            */
                        }
                    }
                }
            }

        }

        // *** Navigation
        int oldX = 0;
        int oldY = 0;
        int newX = 0;
        int newY = 0;
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!lockFile)
            {
                newX = e.X;
                newY = e.Y;
                if ((Math.Abs(newX - oldX) > 5) || (Math.Abs(newY - oldY) > 5))
                {
                    //textBox1.Text = newY + " - " + newY;
                    oldX = newX;
                    oldY = newY;
                    drawConnections();
                }
            }
        }
        bool lockFile = false;
        private void pictureBox1_Click(object sender, MouseEventArgs e)
        {
            if (dm.idTag == true)
            {

                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    lockFile = !lockFile;
                    drawConnections();
                }
                else
                {
                    int line = (int)dm.charToLine[((LinkStructure)dm.linkStructureArray[e.Y / 12]).startChar];
                    Range rng = new Range(fastColoredTextBox1, 0, line + 1, 0, line + 1);
                    fastColoredTextBox1.Selection = rng;
                    fastColoredTextBox1.DoSelectionVisible();
                    fastColoredTextBox1.Invalidate();
                    this.ActiveControl = fastColoredTextBox1;
                }
            }
        }

        Bitmap bmp;
        Font drawFont = new Font("Calibri", 11);
        SolidBrush drawBrush = new SolidBrush(Color.Black);
        SolidBrush redBrush = new SolidBrush(Color.Red);
        Graphics g;
        bool init = false;
        bool lsready = false;
        private void drawConnections()
        {
            int yPos = 0;
            int xPos = 8;

            if (lsready == false)
            {
                return;
            }

            int height = pictureBox1.Height;
            if (init == false)
            {
                splitContainer1.Panel1.AutoScrollPosition = new Point(0, 0);
                pictureBox1.Top = 0;
                pictureBox1.Left = 0;
                height = dm.linkStructureArray.Count * 12 + 20;
                pictureBox1.Height = height;
                pictureBox1.Width = 1500;
                bmp = new Bitmap(pictureBox1.Width, height);
                g = Graphics.FromImage(bmp);
                // Draw all file names and remember position
                for (int i = 0; i < dm.linkStructureArray.Count; i++)
                {
                    Point myp = new Point(xPos, yPos);
                    //                    myhashfullpath.Add(a, myp);
                    //myhash.Add(b, myp);
                    string str = ((LinkStructure)dm.linkStructureArray[i]).text;
                    //int startpos = Math.Max(0, str.Length - 40);
                    //str = str.Substring(startpos, str.Length-startpos);
                    g.DrawString(str, drawFont, drawBrush, xPos, i * 12);
                }
                pictureBox1.Image = bmp;
                init = true;
            }

            System.Drawing.Imaging.PixelFormat format = bmp.PixelFormat;
            RectangleF cloneRect = new RectangleF(0, 0, pictureBox1.Width, height);
            Bitmap bmp2 = bmp.Clone(cloneRect, format);
            Graphics g2 = Graphics.FromImage(bmp2);

            for (int i = 0; i < dm.fileModified.Length; i++)
            {
                if ((bool)dm.fileModified[i] == true)
                {
                    g2.DrawString("*", drawFont, drawBrush, 0, i * 12);
                }
            }

            // draw in which file the editor currently is
            SolidBrush blueBrush = new SolidBrush(Color.Blue);
            Pen bluePen = new Pen(blueBrush);
            Range sel = fastColoredTextBox1.Selection;
            if (sel.Start.iLine < dm.linesToFiles.Count)
            {
                int blueLine = (int)dm.linesToFiles[sel.Start.iLine];
                if (blueLine < 0)
                {
                    blueLine = 0;
                }
                g2.DrawRectangle(bluePen, new Rectangle(1, blueLine * 12 + 4, pictureBox1.Width - 3, 11));
            }

            int k = newY / 12;
            if (lockFile)
            {
                Pen pen2 = new Pen(redBrush);
                g2.DrawRectangle(pen2, new Rectangle(0, k * 12 + 3, pictureBox1.Width - 2, 12));
            }



            //Pen pto = new Pen(Color.LightSalmon,2);
            //Pen pfrom = new Pen(Color.CornflowerBlue,2);
            Pen pto = new Pen(Color.DarkSalmon, 2);
            Pen pfrom = new Pen(Color.DarkBlue, 2);
            Pen pen = new Pen(drawBrush);
            pto.CustomEndCap = new AdjustableArrowCap(5, 5);
            pfrom.CustomEndCap = new AdjustableArrowCap(5, 5);
            int xstart = 20 - splitContainer1.Panel1.AutoScrollPosition.X;

            //for (int k = 0; k < dm.linkStructureArray.Count; k++)
            if (k < dm.linkStructureArray.Count)
            {
                LinkStructure ls = (LinkStructure)dm.linkStructureArray[k];
                for (int j = 0; j < ls.allLSReferencesTo.Count; j++)
                {
                    int lsnr = (int)ls.allLSReferencesTo[j];
                    //g2.DrawBezier(pto, new Point(250, k * 12 + 10), new Point(150, k * 12 + 10), new Point(100, k * 12 + 10), new Point(50, lsnr * 12 + 10));
                    g2.DrawLine(pto, xstart + j * 15, lsnr * 12 + 10, xstart + j * 15, k * 12 + 10);
                    //g2.FillEllipse(redBrush, new Rectangle(45, lsnr * 12 + 6, 7, 7));
                }
                int l = ls.allLSReferencesTo.Count;
                for (int j = 0; j < ls.allLSReferencesFrom.Count; j++)
                {
                    int lsnr = (int)ls.allLSReferencesFrom[j];
                    //g2.DrawBezier(pfrom, new Point(50, k * 12 + 10), new Point(100, k * 12 + 10), new Point(150, k * 12 + 10), new Point(250, lsnr * 12 + 10));
                    g2.DrawLine(pfrom, xstart + (l + j) * 15, k * 12 + 10, xstart + (l + j) * 15, lsnr * 12 + 10);
                    //g2.FillEllipse(redBrush, new Rectangle(245, lsnr * 12 + 6, 7, 7));
                }
            }

            pictureBox1.Image = bmp2;
        }

        void setPictureScroll()
        {
            Range sel = fastColoredTextBox1.Selection;
            if (sel.Start.iLine < dm.linesToFiles.Count)
            {
                int blueLine = (int)dm.linesToFiles[sel.Start.iLine];
                if (blueLine < 0)
                {
                    blueLine = 0;
                }
                if (!((blueLine * 12 >= -splitContainer1.Panel1.AutoScrollPosition.Y) && (blueLine * 12 < -splitContainer1.Panel1.AutoScrollPosition.Y + splitContainer1.Panel1.Height)))
                {
                    splitContainer1.Panel1.AutoScrollPosition = new Point(0, blueLine * 12);
                }
            }
        }

        public void gotoIdentifier(string key)
        {
            ArrayList al = ((ArrayList)dm.hrefToPosition[key]);
            if (al != null)
            {
                Point p = (Point)al[0];
                int line = ((int)dm.charToLine[p.X]);
                Range r = new Range(fastColoredTextBox1, 0, line, 0, line);
                fastColoredTextBox1.Selection = r;
                fastColoredTextBox1.DoSelectionVisible();
                fastColoredTextBox1.Invalidate();
                setPictureScroll();
                this.ActiveControl = fastColoredTextBox1;
            }
        }

        string checkFileForHref(string key, bool isToHref)
        {
            string rvalue = "";

            if (isToHref)
            {
                if (dm.hrefFromPosition.ContainsKey(key))
                {
                    ArrayList al2 = ((ArrayList)dm.hrefFromPosition[(string)key]);
                    foreach (Point p2 in al2)
                    {
                        int line = ((int)dm.charToLine[p2.X] + 1);
                        rvalue += dm.fileNames[(int)dm.linesToFiles[line]] + ", ";
                    }
                }
            }
            else
            {
                if (dm.hrefToPosition.ContainsKey(key))
                {
                    ArrayList al2 = ((ArrayList)dm.hrefToPosition[(string)key]);
                    foreach (Point p2 in al2)
                    {
                        int line = ((int)dm.charToLine[p2.X] + 1);
                        rvalue += dm.fileNames[(int)dm.linesToFiles[line]] + ", ";
                    }
                }
            }
            return rvalue;
        }

        private void fastColoredTextBox1_MouseDown(object sender, MouseEventArgs e)
        {
            FastColoredTextBox fc = (FastColoredTextBox)sender;
            if (e.Button == MouseButtons.Right)
            {
                ContextMenuStrip cm = new ContextMenuStrip();//make a context menu instance
                ToolStripMenuItem item;
                //item = new ToolStripMenuItem();
                //item.Click += new System.EventHandler(this.menuItem1_Click);
                //item.Text = idTagText;
                //cm.Items.Add(item);


                Point Pform = new Point(e.X, e.Y);
                int CharIndex = 0;
                Place place = fc.PointToPlace(new Point(e.X, e.Y));
                CharIndex = (int)dm.lineToChar[place.iLine] + place.iChar;
                bool unique = true;
                bool isToEntry = false;
                foreach (DictionaryEntry g in dm.hrefToPosition)
                {
                    ArrayList al = (ArrayList)g.Value;
                    foreach (Point Pentry in al)
                    {
                        if ((Pentry.X <= CharIndex) && (Pentry.Y + Pentry.X >= CharIndex))
                        {
                            isToEntry = true;
                            item = new ToolStripMenuItem();
                            item.Click += new System.EventHandler(this.menuItem1_Click);
                            item.Text = "Go to Dictionary: " + g.Key;
                            cm.Items.Add(item);

                            item = new ToolStripMenuItem();
                            item.Text = "Is connected from these lines:";
                            cm.Items.Add(item);

                            if (dm.hrefFromPosition.ContainsKey(g.Key))
                            {
                                /*Point Pto = (Point)((ArrayList)Form1.hrefFromPosition[(string)g.Key])[0];
                                this.richTextBox2.Select(Pto.X, Pto.Y);
                                this.richTextBox2.ScrollToCaret();*/

                                ArrayList al2 = ((ArrayList)dm.hrefFromPosition[(string)g.Key]);
                                foreach (Point p2 in al2)
                                {
                                    item = new ToolStripMenuItem();
                                    item.Click += new System.EventHandler(this.menuItem1_Click);
                                    // it's a different point than the origin
                                    if (p2.X != Pentry.X)
                                    {
                                        //item.Text = "From: " + p2.X;
                                        int line = ((int)dm.charToLine[p2.X] + 1);
                                        item.Text = "Line: " + line + " File: " + dm.fileNames[(int)dm.linesToFiles[line]];
                                        cm.Items.Add(item);
                                        if (cm.Items.Count > 50)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            item = new ToolStripMenuItem();
                            if (al.Count > 1)
                            {
                                unique = false;
                                item.Text = "This ID is not unique. Redundant definitions are:";
                                cm.Items.Add(item);
                                foreach (Point redundant in al)
                                {
                                    if (redundant != Pentry)
                                    {
                                        item = new ToolStripMenuItem();
                                        item.Click += new System.EventHandler(this.menuItem1_Click);
                                        int line = ((int)dm.charToLine[redundant.X] + 1);
                                        item.Text = "Line: " + line + " File: " + dm.fileNames[(int)dm.linesToFiles[line]];
                                        cm.Items.Add(item);
                                        if (cm.Items.Count > 60)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                item.Text = "This ID is unique.";
                                cm.Items.Add(item);
                            }
                            break;
                        }
                    }
                }

                if (isToEntry == false)
                {
                    foreach (DictionaryEntry g in dm.hrefFromPosition)
                    {
                        ArrayList al = (ArrayList)g.Value;
                        foreach (Point Pentry in al)
                        {
                            if ((Pentry.X <= CharIndex) && (Pentry.Y + Pentry.X >= CharIndex))
                            {
                                item = new ToolStripMenuItem();
                                item.Click += new System.EventHandler(this.menuItem1_Click);
                                item.Text = "Go to Dictionary: " + g.Key;
                                cm.Items.Add(item);

                                item = new ToolStripMenuItem();
                                item.Text = "Connects to these lines:";
                                cm.Items.Add(item);

                                if (dm.hrefToPosition.ContainsKey(g.Key))
                                {
                                    /*Point Pto = (Point)((ArrayList)Form1.hrefToPosition[(string)g.Key])[0];
                                    this.richTextBox2.Select(Pto.X, Pto.Y);
                                    this.richTextBox2.ScrollToCaret();*/

                                    ArrayList al2 = ((ArrayList)dm.hrefToPosition[(string)g.Key]);
                                    foreach (Point p2 in al2)
                                    {
                                        if (p2.X != Pentry.X)
                                        {
                                            item = new ToolStripMenuItem();
                                            item.Click += new System.EventHandler(this.menuItem1_Click);
                                            //item.Text = "To: " + p2.X;
                                            //item.Text = p2.X.ToString();
                                            int line = ((int)dm.charToLine[p2.X] + 1);
                                            item.Text = "Line: " + line + " File: " + dm.fileNames[(int)dm.linesToFiles[line]];
                                            cm.Items.Add(item);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                if (cm.Items.Count == 0)
                {
                    item = new ToolStripMenuItem();
                    item.Text = "This element is not connected.";
                    cm.Items.Add(item);
                }

                ((FastColoredTextBox)sender).ContextMenuStrip = cm;//add the context menu to the sender
                cm.Show(fastColoredTextBox1, e.Location);//show the context menu
            }
            else
            {
                setPictureScrollMarker = true;
            }
        }
        bool setPictureScrollMarker = false;

        private void menuItem1_Click(object sender, System.EventArgs e)
        {
            string temp = ((ToolStripMenuItem)sender).Text;
            if (((ToolStripMenuItem)sender).Text.StartsWith("Go to Dictionary"))
            {
                if (dc == null)
                {
                    dc = new Dictionary(this);
                    updateListBox();
                    dc.Show();
                }
                string pattern = @"Go to Dictionary: (.*)";
                string itemtext = "";
                MatchCollection mc = Regex.Matches(temp, pattern);
                if (mc.Count > 0)
                {
                    itemtext = mc[0].Groups[1].Value;
                }

                int index = dc.listBox1.FindStringExact(itemtext);
                dc.listBox1.SelectedIndex = index;
            }
            else
            {
                string pattern = @"Line: (\d+) ";
                int x = 0;
                MatchCollection mc = Regex.Matches(temp, pattern);
                if (mc.Count > 0)
                {
                    x = Int32.Parse(mc[0].Groups[1].Value);
                }

                Range r = new Range(fastColoredTextBox1, 0, x - 1, 0, x - 1);
                fastColoredTextBox1.Selection = r;
                fastColoredTextBox1.DoSelectionVisible();
                fastColoredTextBox1.Invalidate();
                setPictureScroll();
                this.ActiveControl = fastColoredTextBox1;


                //int x = Int32.Parse(((ToolStripMenuItem)sender).Text);
                //this.fastColoredTextBox1.Select((int)dm.lineToChar[x], 0);
                //this.fastColoredTextBox1.ScrollToCaret();
            }
        }

        private void splitContainer2_Resize(object sender, EventArgs e)
        {
            //splitContainer2.SplitterDistance = splitContainer2.Height / 2;
        }

        private void fastColoredTextBox1_AutoIndentNeeded(object sender, AutoIndentEventArgs e)
        {
            //fastColoredTextBox1.DoAutoIndent();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            fastColoredTextBox1.NavigateBackward();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            fastColoredTextBox1.NavigateForward();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fastColoredTextBox1.ShowFindDialog();
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fastColoredTextBox1.ShowReplaceDialog();
            for (int i = 0; i < dm.fileModified.Length; i++)
            {
                dm.fileModified[i] = true;
            }
        }

        private void goToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fastColoredTextBox1.ShowGoToDialog();
        }

        bool button1clicked=false;
        private void button1_Click(object sender, EventArgs e)
        {
            button1clicked = true;
            /*
            int line = fastColoredTextBox1.Selection.Start.iLine;
            button1.Visible = false;
            label1.Visible = false;
            dm.updateFile();
            updateListBox();
            Range r = new Range(fastColoredTextBox1, 0, line, 0, line);
            fastColoredTextBox1.Selection = r;
            fastColoredTextBox1.DoSelectionVisible();
            fastColoredTextBox1.Invalidate();
            this.ActiveControl = fastColoredTextBox1;
            */
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Visible = false;
        }

        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (button1.Visible == true)
            {
                button1clicked = true;
            }
            else
            {
                dm.exportXmlFolder();
            }
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            button1.Left = splitContainer1.SplitterDistance;
            label1.Left = splitContainer1.SplitterDistance + button1.Width + 6;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (saveIfUnsaved() == true)
            {
                e.Cancel = true;
            }
        }

        private bool saveIfUnsaved()
        {
            if (button1.Visible == true)
            {
                button1clicked = true;
                return true;
            }

            bool cancel = false;
            if (dm.isUnsaved())
            {
                if (dc != null) { dc.Close(); }
                if (fastColoredTextBox1.findForm != null) { fastColoredTextBox1.findForm.Close(); }
                if (fastColoredTextBox1.replaceForm != null) { fastColoredTextBox1.replaceForm.Close(); }
                if (settings != null) { settings.Close(); }
                SaveDialogue saveDialogue = new SaveDialogue();

                // Show testDialog as a modal dialog and determine if DialogResult = OK.
                switch (saveDialogue.ShowDialog(this))
                {
                    case DialogResult.Yes:
                        dm.exportXmlFolder();
                        break;
                    case DialogResult.Ignore:
                        break;
                    default:
                    case DialogResult.Cancel:
                        cancel = true;
                        //e.Cancel = true;
                        break;
                }
            }
            return cancel;
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveIfUnsaved() == false)
            {
                if (dc != null) { dc.Close(); }
                if (fastColoredTextBox1.findForm != null) { fastColoredTextBox1.findForm.Close(); }
                if (fastColoredTextBox1.replaceForm != null) { fastColoredTextBox1.replaceForm.Close(); }
                if (settings != null) { settings.Close(); }
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        button1.Visible = false;
                        label1.Visible = false;
                        lockFile = false;
                        lsready = false;
                        backgroundWorker1.CancelAsync();
                        System.Threading.Thread.Sleep(500);
                        if (!backgroundWorker1.IsBusy)
                        {
                            dm = new DataModel(this, openFileDialog.FileName);
                            toolStripProgressBar1.Value = 50;
                            backgroundWorker1.RunWorkerAsync();
                        }
                        else
                        {
                            string caption = "File/folder could not be opened";
                            string message = "The former operation is still running and could not be canceled.";
                            MessageBoxButtons buttons = MessageBoxButtons.OK;
                            MessageBox.Show(message, caption, buttons);
                        }
                    }
                }
            }
        }

        Dictionary dc;
        private void dictionaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dc = new Dictionary(this);
            updateListBox();
            dc.Show();
        }

        private void updateListBox()
        {
            if (dc != null)
            {
                int temp = dc.listBox1.SelectedIndex;
                dc.listBox1.Items.Clear();
                foreach (DictionaryEntry g in dm.hrefToPosition)
                {
                    dc.listBox1.Items.Add(g.Key);
                }
                dc.listBox1.SelectedIndex = temp;
            }
        }

        private void addNewFileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            //reload
            if (saveIfUnsaved() == false)
            {
                if (dm.filePath != "")
                {
                    button1.Visible = false;
                    label1.Visible = false;
                    lockFile = false;
                    lsready = false;
                    backgroundWorker1.CancelAsync();
                    System.Threading.Thread.Sleep(500);
                    if (!backgroundWorker1.IsBusy)
                    {
                        dm = new DataModel(this, dm.filePath);
                        toolStripProgressBar1.Value = 50;
                        backgroundWorker1.RunWorkerAsync();
                    }
                    else
                    {
                        string caption = "File/folder could not be reloaded";
                        string message = "The former operation is still running and could not be canceled.";
                        MessageBoxButtons buttons = MessageBoxButtons.OK;
                        MessageBox.Show(message, caption, buttons);
                    }
                }
            }
        }

        private void fastColoredTextBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if ((dm.xmlFolderRead == false) || (dm.idTag == false))
            {
                return;
            }

            if (button1.Visible == false)
            {
                try
                {
                    FastColoredTextBox fc = (FastColoredTextBox)sender;
                    Point Pform = new Point(e.X, e.Y);
                    Point Pfabsolute = new Point(e.X, e.Y - 24);
                    int CharIndex = 0;
                    Place place = fc.PointToPlace(new Point(e.X, e.Y));
                    CharIndex = (int)dm.lineToChar[place.iLine] + place.iChar;
                    label2.Text = "";
                    label2.BringToFront();
                    label2.BackColor = Color.Transparent;

                    var hash = new HashSet<string>();

                    foreach (DictionaryEntry g in dm.hrefToPosition)
                    {
                        ArrayList al = (ArrayList)g.Value;
                        foreach (Point Pentry in al)
                        {
                            if ((Pentry.X <= CharIndex) && (Pentry.Y + Pentry.X >= CharIndex))
                            {
                                if (dm.hrefFromPosition.ContainsKey(g.Key))
                                {
                                    ArrayList al2 = ((ArrayList)dm.hrefFromPosition[(string)g.Key]);
                                    foreach (Point p2 in al2)
                                    {
                                        // it's a different point than the origin
                                        if (p2.X != Pentry.X)
                                        {
                                            int line = ((int)dm.charToLine[p2.X] + 1);
                                            hash.Add(Path.GetFileName((string)dm.fileNames[(int)dm.linesToFiles[line]]));
                                            //label2.Text += Path.GetFileName((string)dm.fileNames[(int)dm.linesToFiles[line]]) + ", ";
                                            label2.Left = Pfabsolute.X;
                                            label2.Top = Pfabsolute.Y;
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }

                    foreach (DictionaryEntry g in dm.hrefFromPosition)
                    {
                        ArrayList al = (ArrayList)g.Value;
                        foreach (Point Pentry in al)
                        {
                            if ((Pentry.X <= CharIndex) && (Pentry.Y + Pentry.X >= CharIndex))
                            {
                                if (dm.hrefToPosition.ContainsKey(g.Key))
                                {
                                    ArrayList al2 = ((ArrayList)dm.hrefToPosition[(string)g.Key]);
                                    foreach (Point p2 in al2)
                                    {
                                        if (p2.X != Pentry.X)
                                        {
                                            int line = ((int)dm.charToLine[p2.X] + 1);
                                            hash.Add(Path.GetFileName((string)dm.fileNames[(int)dm.linesToFiles[line]]));
                                            //label2.Text += Path.GetFileName((string)dm.fileNames[(int)dm.linesToFiles[line]]) + ", ";
                                            label2.Left = Pfabsolute.X;
                                            label2.Top = Pfabsolute.Y;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    //if (label2.Text != "")
                    if (hash.Count > 0)
                    {
                        string temp = string.Join(", ", hash);
                        //temp = temp.Substring(0, temp.Length - 2);
                        if (dm.fileNames.Count > 1)
                        {
                            label2.Text = "Connected to files: " + temp;
                        }
                        
                    }
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                label2.Text = "";
            }
        }

        public bool textchfromtextassignment = false;
        private void fastColoredTextBox1_TextChanging(object sender, TextChangingEventArgs e)
        {
            if (dm.idTag && dm.xmlFolderReadDone)
            {
                textchfromtextassignment = true;
            }
        }

        public Log log;
        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            log = new Log(logText);
            log.Show();
        }
        public void updateLog()
        {
            if ((log != null) && (log.textBox1.Text != logText))
            {
                log.textBox1.Text = logText;
                log.textBox1.SelectionStart = logText.Length;
                log.textBox1.ScrollToCaret();
                log.textBox1.Refresh();
            }
        }

        bool once = false;
        public bool accepted = false;
        private void Form1_Activated(object sender, EventArgs e)
        {
            if (once == false)
            {
                once = true;

                // is the license agreement accepted?
                string sAttr = ConfigurationManager.AppSettings.Get("Key0");
                if (sAttr == "0")
                {
                    settings = new Settings(this);
                    settings.ShowDialog();
                }
                else
                {
                    accepted = true;
                }
                if (accepted==false)
                {
                    this.Close();
                }

                // is it the first start after installation?
                string fi = ConfigurationManager.AppSettings.Get("Key1");
                if (fi != "1")
                {
                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["Key1"].Value = "1";
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");

                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.xmlmodelanalyzer.com/counti.php");
                        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        response.Close();
                        request.Abort();
                    }
                    catch (Exception ex) { }
                }

                // is it started on a month?
                string dt = DateTime.Now.ToString("M/yyyy");
                string nd = ConfigurationManager.AppSettings.Get("Key2");
                if (nd != dt)
                {
                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["Key2"].Value = dt;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");

                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.xmlmodelanalyzer.com/counts.php");
                        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        response.Close();
                        request.Abort();
                    }
                    catch (Exception ex) { }
                }

                // is it started on a day and is there something to display?
                string dd = DateTime.Now.ToString("d/M/yyyy");
                string nn = ConfigurationManager.AppSettings.Get("Key3");
                if (nn != dd)
                {
                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings["Key3"].Value = dd;
                    config.Save(ConfigurationSaveMode.Modified);
                    ConfigurationManager.RefreshSection("appSettings");

                    string text = "";
                    int nr = 0;
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.xmlmodelanalyzer.com/countd.php");
                        request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                string temp = reader.ReadToEnd();
                                Regex _rx = new Regex(@"\n", RegexOptions.Compiled);
                                temp = _rx.Replace(temp, string.Empty);
                                string pattern = @"<body>\s*(\d+) (.*?)\s*</body>";
                                foreach (Match match in Regex.Matches(temp, pattern))
                                {
                                    if (match.Success && match.Groups.Count > 0)
                                    {
                                        nr = Int32.Parse(match.Groups[1].Value);
                                        text = match.Groups[2].Value;
                                    }
                                }
                            }
                            response.Close();
                        }
                        request.Abort();
                    }
                    catch (Exception ex) { }
                    int savednr = Int32.Parse(ConfigurationManager.AppSettings.Get("Key4"));
                    if ((text != "") && (nr != savednr))
                    {
                        Regex _rx = new Regex(@"\\n", RegexOptions.Compiled);
                        text = _rx.Replace(text, "\n");

                        ShowInfo si = new ShowInfo(this);
                        si.richTextBox1.Text = text;
                        si.ShowDialog();
                        if (intNoShow)
                        {
                            //Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                            config.AppSettings.Settings["Key4"].Value = nr.ToString();
                            config.Save(ConfigurationSaveMode.Modified);
                            ConfigurationManager.RefreshSection("appSettings");

                        }
                    }
                }
            }
        }

        bool intNoShow = false;
        public void setHintNoShow()
        {
            intNoShow = true;
        }

        private void sortByElementToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (button1.Visible)
            {
                return;
            }
            Sort sort = new Sort(this);
            int line = fastColoredTextBox1.Selection.Start.iLine;
            if (dm.xdoc != null)
            {
                foreach (XElement l1 in dm.xdoc.Descendants())
                {
                    int lineNumber = ((IXmlLineInfo)l1).HasLineInfo() ? ((IXmlLineInfo)l1).LineNumber - 2 : -1;
                    if (lineNumber == line)
                    {
                        // the element is found
                        XElement parent = l1.Parent;
                        if (parent == null)
                        {
                            return;
                        }

                        var hash = new HashSet<string>();
                        foreach (var attribute in parent.Descendants().Attributes())
                        {
                            hash.Add(attribute.Name.ToString());
                        }
                        sort.comboBox1.Items.Add("N/A");
                        sort.comboBox1.Text = "N/A";
                        if (hash.Count > 0)
                        {
                            foreach (var entry in hash)
                            {
                                sort.comboBox1.Items.Add(entry);
                            }
                        }

                        var hash2 = new HashSet<string>();
                        //foreach (var element in parent.Elements())
                        foreach (var element in parent.Descendants().Elements())
                        {
                            hash2.Add(element.Name.ToString());
                        }
                        sort.comboBox2.Items.Add("N/A");
                        sort.comboBox2.Text = "N/A";
                        if (hash2.Count > 0)
                        {
                            foreach (var entry in hash2)
                            {
                                sort.comboBox2.Items.Add(entry);
                            }
                        }

                        sort.textBox1.Text = parent.Name.ToString();

                        // mark the text to be modified
                        lineNumber = ((IXmlLineInfo)parent).HasLineInfo() ? ((IXmlLineInfo)parent).LineNumber - 1 : -1;
                        List<XElement> sortedChildren;
                        sortedChildren = parent.Elements().ToList();
                        string sc = "";
                        sortedChildren.ForEach(c => sc += c.ToString() + "\r\n");
                        int numLines = sc.Count(c => c.Equals('\n'));
                        Range r = new Range(fastColoredTextBox1, 0, lineNumber, 0, lineNumber + numLines);
                        fastColoredTextBox1.Selection = r;
                        fastColoredTextBox1.DoSelectionVisible();
                        fastColoredTextBox1.Invalidate();


                        break;
                    }
                }
                sort.ShowDialog();
            }
        }


        public void sortElementsByAttribute(string element, string attribute, int nrchecked)
        {
            int line = fastColoredTextBox1.Selection.Start.iLine;
            foreach (XElement l1 in dm.xdoc.Descendants())
            {
                int lineNumber = ((IXmlLineInfo)l1).HasLineInfo() ? ((IXmlLineInfo)l1).LineNumber - 2 : -1;
                if (lineNumber == line)
                {
                    // the element is found
                    // now sort
                    //IEnumerable<XElement> sorted = l1.AncestorsAndSelf().OrderBy(el => el.Attribute("description").Value.ToString());
                    //List<XElement> sortedChildren = l1.Elements().OrderBy(el => el.Attribute("name").Value.ToString()).ToList();
                    XElement parent = l1.Parent;
                    List<XElement> sortedChildren;
                    if ((attribute != "N/A") && (nrchecked == 0))
                    {
                        //sortedChildren = parent.Elements().OrderBy(el => el.Attribute(attribute) is null ? "" : el.Attribute(attribute).Value.ToString()).ToList();
                        sortedChildren = parent.Elements().OrderBy(el => el.DescendantsAndSelf().Attributes(attribute).FirstOrDefault() is null ? "" : el.DescendantsAndSelf().Attributes(attribute).FirstOrDefault().Value.ToString()).ToList();
                    }

                    // sort by element name
                    else if (nrchecked == 2)
                    {
                        sortedChildren = parent.Elements().OrderBy(el => el.Name.ToString()).ToList();
                    }
                    // sort by element value of element name
                    else if ((element != "N/A") && (nrchecked == 3))
                    {
                        //sortedChildren = parent.Elements().OrderBy(el => el.Value.ToString()).ToList();
                        sortedChildren = parent.Elements().OrderBy(el => el.DescendantsAndSelf().Elements(element).FirstOrDefault() is null ? "" : el.DescendantsAndSelf().Elements(element).FirstOrDefault().Value.ToString()).ToList();
                    }
                    else
                    {
                        return;
                    }

                    if (parent.HasElements)
                    {
                        XNode xe = null;
                        if (parent.FirstNode.NodeType == XmlNodeType.Comment)
                        {
                            xe = parent.FirstNode;
                        }
                        parent.RemoveNodes();
                        if (xe != null)
                        {
                            parent.Add(xe);
                        }
                        sortedChildren.ForEach(c => parent.Add(c));
                    }
                    lineNumber = ((IXmlLineInfo)parent).HasLineInfo() ? ((IXmlLineInfo)parent).LineNumber - 1 : -1;
                    string sc = "";
                    sortedChildren.ForEach(c => sc += c.ToString() + "\r\n");
                    int numLines = sc.Count(c => c.Equals('\n'));
                    Range r = new Range(fastColoredTextBox1, 0, lineNumber, 0, lineNumber + numLines);
                    fastColoredTextBox1.Text = dm.xdoc.ToString();
                    dm.fileContent = dm.xdoc.ToString();
                    fastColoredTextBox1.Selection = r;
                    fastColoredTextBox1.DoSelectionVisible();
                    fastColoredTextBox1.Invalidate();

                    if (parent.Name == "open")
                    {
                        for (int i = 0; i < dm.fileModified.Length; i++)
                        {
                            dm.fileModified[i] = true;
                        }
                    }

                    updateFiles = true;
                    break;
                }
            }
        }

        public void sortAttributesWithinElements()
        {
            int line = fastColoredTextBox1.Selection.Start.iLine;
            foreach (XElement l1 in dm.xdoc.Descendants())
            {
                int lineNumber = ((IXmlLineInfo)l1).HasLineInfo() ? ((IXmlLineInfo)l1).LineNumber - 2 : -1;
                if (lineNumber == line)
                {
                    // the element is found
                    // now sort
                    //IEnumerable<XElement> sorted = l1.AncestorsAndSelf().OrderBy(el => el.Attribute("description").Value.ToString());
                    //List<XElement> sortedChildren = l1.Elements().OrderBy(el => el.Attribute("name").Value.ToString()).ToList();
                    XElement parent = l1.Parent;

                    IEnumerable<XElement> items = from item in parent.Descendants()
                                                  select item;

                    foreach (XElement a in items)
                    {
                        List<XAttribute> sortedAttributes;
                        sortedAttributes = a.Attributes().OrderBy(el => el.Name.ToString()).ToList();
                        a.RemoveAttributes();
                        sortedAttributes.ForEach(c => a.Add(c));
                    }


                    string sc = "";
                    List<XElement> sortedChildren = parent.Elements().ToList();
                    sortedChildren.ForEach(c => sc += c.ToString() + "\r\n");
                    int numLines = sc.Count(c => c.Equals('\n'));

                    lineNumber = ((IXmlLineInfo)parent).HasLineInfo() ? ((IXmlLineInfo)parent).LineNumber - 1 : -1;
                    Range r = new Range(fastColoredTextBox1, 0, lineNumber, 0, lineNumber + numLines);
                    fastColoredTextBox1.Text = dm.xdoc.ToString();
                    dm.fileContent = dm.xdoc.ToString();
                    fastColoredTextBox1.Selection = r;
                    fastColoredTextBox1.DoSelectionVisible();
                    fastColoredTextBox1.Invalidate();

                    if (parent.Name == "open")
                    {
                        for (int i = 0; i < dm.fileModified.Length; i++)
                        {
                            dm.fileModified[i] = true;
                        }
                    }

                    updateFiles = true;
                    break;
                }
            }
        }

        private void fastColoredTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.X:
                        fastColoredTextBox1_TextChangedDelayed(sender, new TextChangedEventArgs(new Range(fastColoredTextBox1)));
                        break;
                }
            }
        }
    }
}
