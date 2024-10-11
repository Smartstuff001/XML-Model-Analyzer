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
    class ModelXMI
    {
        DataModel dm;
        public ModelXMI(DataModel dm1)
        {
            dm = dm1;
        }

        public bool Detect()
        {
            string pattern = @"xmi[\.\:]id\s*\=""";
            if (Regex.IsMatch(dm.fileContent, pattern))
            {
                return true;
            }
            return false;
        }

        string attribute=@"xmi[\.\:]id";
        string aname = @"[_\w\-]+"; 

        public void Process()
        {
            string sentence = "";
            Regex _rx;
            _rx = new Regex(@"\n", RegexOptions.Compiled);
            sentence = _rx.Replace(dm.fileContent, string.Empty);
            string pattern;
            pattern = @"(" + attribute + @")=""(" + aname + @")""";
            foreach (Match match in Regex.Matches(sentence, pattern))
            {
                if (match.Success && match.Groups.Count > 0)
                {
                    //if (match.Groups[1].Value == "xmi:id")
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

            pattern = @"[""\s\/\\#](" + aname + @")";
            foreach (Match match in Regex.Matches(sentence, pattern))
            {
                if (match.Success && match.Groups.Count > 0)
                {
                    if (dm.hrefToPosition.ContainsKey(match.Groups[1].Value))
                    {
                        Point p = new Point();
                        var text = match.Groups[1].Value;
                        p.X = match.Groups[1].Index;
                        p.Y = match.Groups[1].Length;

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
        public void Update()
        {

        }
    }
}
