﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections;
using System.Drawing;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;

namespace XML_Model_Analyzer
{
    class ModelAUTOSAR
    {
        DataModel dm;
        public ModelAUTOSAR(DataModel dm1)
        {
            dm = dm1;
        }

        public bool Detect()
        {
            string pattern = @"autosar\.org";
            if (Regex.IsMatch(dm.fileContent, pattern))
            {
                return true;
            }
            return false;
        }

        /*
        ArrayList xmlElementList = new ArrayList();
        ArrayList referencePaths = new ArrayList();
        private void recursiveAnalyzeElementsSub(IEnumerable<XElement> elementslx, string nameChain, int level)
        {
            foreach (XElement l1 in elementslx)
            {
                if (l1.Name.LocalName == @"SHORT-NAME")
                {
                    // SHORT-NAME found
                    nameChain += "/" + l1.Value;
                    xmlElementList.Add(nameChain);

                    int lineNumber = ((IXmlLineInfo)l1).HasLineInfo() ? ((IXmlLineInfo)l1).LineNumber - 2 : -1;
                    Point p = new Point();
                    p.X = (int)dm.lineToChar[lineNumber];
                    p.Y = (int)dm.lineToChar[lineNumber + 1] - (int)dm.lineToChar[lineNumber];
                    if (!dm.hrefToPosition.ContainsKey(nameChain))
                    {
                        ArrayList al = new ArrayList();
                        al.Add(p);
                        dm.hrefToPosition.Add(nameChain, al);
                    }
                    else
                    {
                        ArrayList al = (ArrayList)dm.hrefToPosition[nameChain];
                        if (!al.Contains(p))
                        {
                            al.Add(p);
                        }
                    }
                }
                else
                {
                    var firstTextNode = l1.Nodes().OfType<XText>().FirstOrDefault();
                    string path = "";
                    if (firstTextNode != null)
                    {
                        path = firstTextNode.Value;
                    }

                    // look for path
                    //string path = res.Value.ToString();
                    if ((path.Length > 0) && (path[0] == '/'))
                    {
                        referencePaths.Add(path);

                        int lineNumber = ((IXmlLineInfo)l1).HasLineInfo() ? ((IXmlLineInfo)l1).LineNumber - 2 : -1;
                        Point p = new Point();
                        p.X = (int)dm.lineToChar[lineNumber];
                        p.Y = (int)dm.lineToChar[lineNumber + 1] - (int)dm.lineToChar[lineNumber];
                        if (!dm.hrefFromPosition.ContainsKey(path))
                        {
                            ArrayList al = new ArrayList();
                            al.Add(p);
                            dm.hrefFromPosition.Add(path, al);
                        }
                        else
                        {
                            ArrayList al = (ArrayList)dm.hrefFromPosition[path];
                            if (!al.Contains(p))
                            {
                                al.Add(p);
                            }
                        }
                    }
                }
                recursiveAnalyzeElementsSub(l1.Elements(), nameChain, level + 1);
            }
        }
        private void analyzeElements()
        {
            IEnumerable<XElement> elementsl1 = null;
            xmlElementList.Clear();
            referencePaths.Clear();
            elementsl1 = dm.xdoc.Root.Elements();

            recursiveAnalyzeElementsSub(elementsl1, "", 1);
        }


        public void Process()
        {
            dm.hrefToPosition.Clear();
            dm.hrefFromPosition.Clear();
            analyzeElements();
        }
        */

        public void Process()
        {
            dm.hrefToPosition.Clear();
            dm.hrefFromPosition.Clear();
            string sentence = "";
            Regex _rx;
            _rx = new Regex(@"\n", RegexOptions.Compiled);
            sentence = _rx.Replace(dm.fileContent, string.Empty);
            string pattern;
            int indent = 0;
            string withLines = "";
            // go line by line
            foreach (var myString in dm.fileContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                pattern = @"\<(.*?)\>";
                foreach (Match match in Regex.Matches(myString, pattern))
                {
                    if (match.Success && match.Groups.Count > 0)
                    {
                        string text = match.Groups[1].Value;
                        if (text[0] == '/')
                        {
                            indent -= 1;
                        }
                        else if ((text[text.Length-1] == '/') || (text[0] == '!'))
                        {
                            //nothing to do
                            pattern = "";
                        }
                        else
                        {
                            indent += 1;
                        }
                    }
                }
                withLines += indent + " " + myString + "\n";

                pattern = @"\<(.*?)\>(.*?)\<\/.*?\>";
                foreach (Match match in Regex.Matches(myString, pattern))
                {
                    if (match.Success && match.Groups.Count > 0)
                    {
                        if (match.Groups[1].Value.Equals("SHORT-NAME"))
                        {
                            // SHORT-NAME found
                            Point p = new Point();
                            var text = "/" + match.Groups[2].Value;
                            p.X = match.Groups[2].Index;
                            p.Y = match.Groups[2].Length;
                            if (!dm.hrefToPosition.ContainsKey(text))
                            {
                                ArrayList al = new ArrayList();
                                al.Add(p);
                                dm.hrefToPosition.Add(text, al);
                            }
                            else
                            {
                                ArrayList al = (ArrayList)dm.hrefToPosition[text];
                                if (!al.Contains(p))
                                {
                                    al.Add(p);
                                }
                            }
                        }
                        else
                        {
                            Point p = new Point();
                            var text = match.Groups[2].Value;
                            if (text[0] == '/')
                            {
                                p.X = match.Groups[2].Index;
                                p.Y = match.Groups[2].Length;

                                if (!dm.hrefFromPosition.ContainsKey(text))
                                {
                                    ArrayList al = new ArrayList();
                                    al.Add(p);
                                    dm.hrefFromPosition.Add(text, al);
                                }
                                else
                                {
                                    ArrayList al = (ArrayList)dm.hrefFromPosition[text];
                                    if (!al.Contains(p))
                                    {
                                        al.Add(p);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Update()
        {

        }
    }
}
