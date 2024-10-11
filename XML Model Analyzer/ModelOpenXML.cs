using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections;
using System.Drawing;

namespace XML_Model_Analyzer
{
    class ModelOpenXML
    {
        DataModel dm;
        public ModelOpenXML(DataModel dm1)
        {
            dm = dm1;
        }

        public bool Detect()
        {
            string pattern = @"openxmlformats";
            if (Regex.IsMatch(dm.fileContent, pattern))
            {
                return true;
            }
            return false;
        }

        void analyzeOPC()
        {
            String sentence = "";
            Regex _rx;
            _rx = new Regex(@"\n", RegexOptions.Compiled);
            sentence = _rx.Replace(dm.fileContent, string.Empty);
            //string pattern = @"([\S]+?)=""[\S]*?(_[\w\-]{22})""";
            string pattern = @"(\w+)=""([\w\/\.\:\\_]+)""";
            // pattern = @"(" + psd.t1 + @")=""[\S]*?(" + psd.t2 + @")""";
            //string pattern = @"(UUID)=""([\w\-]+)"""; 
            foreach (Match match in Regex.Matches(sentence, pattern))
            {
                if (match.Success && match.Groups.Count > 0)
                {
                    if ((match.Groups[1].Value == "Id") || (match.Groups[1].Value == "Target"))
                    {
                        if (match.Success && match.Groups.Count > 0)
                        {
                            Point p = new Point();
                            var text = match.Groups[2].Value;
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
                    }
                }
            }

            foreach (DictionaryEntry g in dm.hrefToPosition)
            {
                string matchString = Regex.Escape(g.Key.ToString());
                foreach (Match match in Regex.Matches(sentence, matchString))
                {
                    Point p = new Point();
                    var text = match.Value;
                    p.X = match.Index;
                    p.Y = match.Length;

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

        public void Process()
        {
            analyzeOPC();
        }
        public void Update()
        {

        }
    }
}
