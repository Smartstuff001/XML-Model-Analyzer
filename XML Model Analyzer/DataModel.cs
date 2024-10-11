using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace XML_Model_Analyzer
{
    public enum XMLTypes
    {
        Unknown,
        XMI,
        EnterpriseArchitect,
        OpenXML,
        AUTOSAR,
        ReqIf
    };
    class LinkStructure
    {
        public string text;
        public int startChar;
        public int endChar;
        public ArrayList allReferences;
        public ArrayList allLSReferencesTo;
        public ArrayList allLSReferencesFrom;
        public bool processed = false;
        public int number = 0;
        public LinkStructure()
        {
            allReferences = new ArrayList();
            allLSReferencesTo = new ArrayList();
            allLSReferencesFrom = new ArrayList();
        }
    }
    class DataModel
    {
        public Hashtable hrefFromPosition = new Hashtable();
        public Hashtable hrefToPosition = new Hashtable();
        public XDocument xdoc;
        public bool xmlFolderRead;
        public bool xmlFolderReadDone;
        public bool idTag;
        public bool idTagDone;
        public string filePath;
        ModelXMI mxmi;
        ModelAUTOSAR masr;
        ModelEA mea;
        ModelOpenXML mopenxml;
        ModelReqIf mreqif;
        Form1 form1;
        public bool modified;

        public DataModel(Form1 f1, string path)
        {
            form1 = f1;
            filePath = path;
            xmlFolderRead = false;
            xmlFolderReadDone = false;
            mxmi = new ModelXMI(this);
            masr = new ModelAUTOSAR(this);
            mea = new ModelEA(this);
            mopenxml = new ModelOpenXML(this);
            mreqif = new ModelReqIf(this);
        }
        public DataModel()
        {
            filePath = "";
            xmlFolderRead = false;
            xmlFolderReadDone = false;
            mxmi = new ModelXMI(this);
            masr = new ModelAUTOSAR(this);
            mea = new ModelEA(this);
            mopenxml = new ModelOpenXML(this);
            mreqif = new ModelReqIf(this);
        }

        public bool isUnsaved()
        {
            bool result = false;
            for (int i = 0; i < fileNames.Count; i++)
            {
                if (fileModified[i] == true)
                {
                    result = true;
                }
            }
            return result;
        }

        public void exportXmlFolder()
        {
            string sentence = "";

            Regex _rx = new Regex(@"\n", RegexOptions.Compiled);
            sentence = _rx.Replace(fileContent, string.Empty);

            for (int i = 0; i < fileNames.Count; i++)
            {
                if (fileModified[i] == true)
                {
                    string fileName = fileNames[i].ToString();
                    if (fileName != filePath)
                    {
                        fileName = filePath + fileName;
                    }
                    form1.logText += "Write file " + fileName + "\r\n";

                    FileAttributes attributes = File.GetAttributes(fileName);
                    if ((attributes.HasFlag(FileAttributes.ReadOnly)) ||
                        (attributes.HasFlag(FileAttributes.Hidden)))
                    {
                        File.SetAttributes(fileName, FileAttributes.Normal);
                        form1.logText += " Overwrite ro or hidden attribute\r\n";
                    }

                    int line = (int)fileLine[i] + 1;
                    if (line == 1)
                    {
                        line++;
                    }
                    using (StreamWriter writer = new StreamWriter(fileName))
                    {
                        int startChar = (int)lineToChar[line];
                        int endChar = 0;
                        if (i + 1 < fileNames.Count)
                        {
                            endChar = (int)lineToChar[(int)fileLine[i + 1] - 1] - 1;
                        }
                        else
                        {
                            endChar = (int)lineToChar[lineToChar.Count - 2] - 1;
                        }
                        string fileText = sentence.Substring(startChar, endChar - startChar);
                        Regex _rx2;
                        _rx2 = new Regex(@"\<\!--(.*?) This will be no comment in saved files, this is only for processing! --\>", RegexOptions.Compiled);
                        fileText = _rx2.Replace(fileText, m => string.Format(@"<{0}>", m.Groups[1].Value));
                        fileText = Regex.Replace(fileText, "\r", "\r\n");
                        fileText = Regex.Replace(fileText, "^    ", "");
                        fileText = Regex.Replace(fileText, "\n    ", "\n");
                        writer.Write(fileText);
                    }
                }
            }
            fileModified = new bool[fileNames.Count];


            /*
            var result = string.Empty;
            IEnumerable<XElement> files = from item in xdoc.Descendants("file")
                                          select item;

            foreach (XElement a in files)
            {
                string name = a.Attribute("name").Value;
                //Regex _rx = new Regex(@".*\\|\""", RegexOptions.Compiled);
                //string fileName = _rx.Replace(name, string.Empty);
                Regex _rx = new Regex(@"\""", RegexOptions.Compiled);
                String fileName = _rx.Replace(name, string.Empty);

                XElement b = (XElement)a.DescendantNodes().FirstOrDefault();
                //b.Save(@"C:\temp\export\"+ fileName);
                b.Save(fileName);
            }
            */
        }


        public string fileContent = string.Empty;
        string tempFileName = "";
        public bool readXmlFolder()
        {
            fileContent = string.Empty;
            xdoc = new XDocument();
            //string filePath = string.Empty;
            string filesWithXml = string.Empty;
            string filesWithoutXml = string.Empty;
            Hashtable myhash = new Hashtable();
            Hashtable myhashfullpath = new Hashtable();
            string fileContentCleaned = string.Empty;
            fileContent = "<open>\n";

            form1.logText += "Open " + filePath + "\r\n";
            if (File.Exists(filePath))
            {
                //form1.splitContainer1.Panel1.Width = 0;
                //form1.splitContainer1.Panel1Collapsed = true;

                form1.logText += " read file - ";
                try
                {
                using (StreamReader strreader = new StreamReader(filePath))
                {
                    var line = strreader.ReadLine();
                    Regex _rx2;
                    _rx2 = new Regex(@"\<\?xml(.*?)\>", RegexOptions.Compiled);
                    var line2 = _rx2.Replace(line, m => string.Format(@"<!--?xml{0} This will be no comment in saved files, this is only for processing! -->", m.Groups[1].Value));


                    //if ((line.Contains("<")) && (line.Contains(">")))
                    //if (line.Contains("xml"))
                    {
                        string d;
                        d = filePath;

                        fileContent += @"<file name=""" + d + @""">" + "\n";
                        fileContent += line2 + "\n";
                        fileContent += strreader.ReadToEnd();
                        fileContent += "\n</file>\n";

                        if ((form1.internalXmlRepresentation == false) && (masr.Detect(fileContent)))
                        {
                                form1.internalXmlRepresentation = true;
                        }


                    }
                }
                form1.logText += " success." + "\r\n";
                }
                catch (Exception ex)
                {
                    form1.logText += " error." + "\r\n";
                }
            }
            else
            {
               // form1.splitContainer1.Panel1Collapsed = false;
               // form1.splitContainer1.Panel1.Width = 150;

                form1.logText += " read folder" + "\r\n";
                string[] filesArray = Directory.GetFiles(filePath, "*", SearchOption.AllDirectories);
                foreach (string element in filesArray)
                {
                    if (!(element.Contains(".xlsx")))
                    {
                        bool enableChecked = false;
                        try {
                        using (StreamReader strreader = new StreamReader(element))
                        {
                            var line = strreader.ReadLine();
                            Regex _rx2;
                            _rx2 = new Regex(@"\<\?xml(.*?)\>", RegexOptions.Compiled);
                            var line2 = _rx2.Replace(line, m => string.Format(@"<!--?xml{0} This will be no comment in saved files, this is only for processing! -->", m.Groups[1].Value));

                            string tempContent = line2 + "\n" + strreader.ReadToEnd();

                            if ((form1.internalXmlRepresentation == false) && (masr.Detect(tempContent)))
                            {
                                enableChecked = true;
                                form1.internalXmlRepresentation = true;
                                string caption = "Auto-format needed to be activated.";
                                string message = "The loaded files need an internal XML-representation so auto-format was activated. Please load now the folder again.";
                                MessageBoxButtons buttons = MessageBoxButtons.OK;
                                MessageBox.Show(message, caption, buttons);
                                enableChecked = false;
                                return false;
                            }

                            Regex _rx;
                            _rx = new Regex(@"&#x([0-8BCEFbcef]|1[0-9A-Fa-f]);|[\x01-\x08\x0B\x0C\x0E\x0F\u0000-\u0008\u000B\u000C\u000E-\u001F]", RegexOptions.Compiled);
                            tempContent = _rx.Replace(tempContent, string.Empty);
                            //_rx = new Regex(@"http:///de/ikv", RegexOptions.Compiled);
                            //tempContent = _rx.Replace(tempContent, @"http://www.ikv.de/medini");

                            bool success = false;
                            if (form1.internalXmlRepresentation)
                            {
                                try
                                {
                                    XDocument.Parse(tempContent);
                                    success = true;
                                }
                                catch (Exception ex)
                                {
                                    form1.logText += " read file " + element + " - " + " error wile parsing file as XML." + "\r\n";
                                }
                            }
                            else
                            {
                                if ((line.Contains("<")) && (line.Contains(">")))
                                //if (line.Contains("xml"))
                                {
                                    success = true;
                                }
                            }

                            //if (line.Contains("xml"))
                            if(success)
                            {
                                string d;
                                d = element;

                                fileContent += @"<file name=""" + d + @""">" + "\n";
                                fileContent += tempContent;
                                fileContent += "</file>\n";
                            }
                        }
                        }
                        catch (Exception ex)
                        {
                            if (enableChecked)
                            {
                                form1.internalXmlRepresentation = true;
                                string caption = "Auto-format needed to be activated.";
                                string message = "The loaded files need an internal XML-representation so auto-format was activated. Please load now the folder again.";
                                MessageBoxButtons buttons = MessageBoxButtons.OK;
                                MessageBox.Show(message, caption, buttons);
                                enableChecked = false;
                                return false;
                            }
                            form1.logText += " read file " + element + " - " + " error." + "\r\n";
                        }
                    }

                }
            }
            fileContent += "</open>";
            //if (masr.Detect())
            if (form1.internalXmlRepresentation)
            {
                Regex _rx;
                _rx = new Regex(@"&#x([0-8BCEFbcef]|1[0-9A-Fa-f]);|[\x01-\x08\x0B\x0C\x0E\x0F\u0000-\u0008\u000B\u000C\u000E-\u001F]", RegexOptions.Compiled);
                fileContent = _rx.Replace(fileContent, string.Empty);
                //fileContentCleaned = _rx.Replace(fileContent, string.Empty);
                //_rx = new Regex(@"http:///de/ikv", RegexOptions.Compiled);
                //fileContent = _rx.Replace(fileContentCleaned, @"http://www.ikv.de/medini");
                fileContentCleaned = "";

                try
                {
                    xdoc = XDocument.Parse(fileContent, LoadOptions.SetLineInfo);
                    tempFileName = Path.GetTempFileName();
                    xdoc.Save(tempFileName);
                    xdoc = XDocument.Load(tempFileName, LoadOptions.SetLineInfo);
                    File.Delete(tempFileName);

                    fileContent = xdoc.ToString();
                    //string temp = xdoc.ToString();
                    //_rx = new Regex(@"\n", RegexOptions.Compiled);
                    //fileContent = _rx.Replace(temp, string.Empty);
                    if (!File.Exists(filePath))
                    {
                        form1.logText += " (all other) files in the folder folder could be read successfully." + "\r\n";
                    }
                }
                catch (Exception ex)
                {
                    form1.logText += " error wile parsing complete folder/file." + "\r\n";
                }
            }
            form1.logText += "-----------------------\r\n";
            //tokens = XmlTokenizer.Tokenize(sentence);
            createCharToLineArray();
            recognizeType();
            getFileNamesAndLines();
            fileModified = new bool[fileNames.Count];
            createLinkStructureArray();

            xmlFolderRead = true;
            idTag = true;
            return true;
        }
        
        public XMLTypes xmlType = XMLTypes.Unknown;
        void recognizeType()
        {
            xmlType = XMLTypes.Unknown;
            hrefToPosition.Clear();
            hrefFromPosition.Clear();
            if (mxmi.Detect())
            {
                xmlType = XMLTypes.XMI;
                mxmi.Process();
            }
            if (mea.Detect())
            {
                xmlType = XMLTypes.EnterpriseArchitect;
                mea.Process();
            }
            if (mopenxml.Detect())
            {
                xmlType = XMLTypes.OpenXML;
                mopenxml.Process();
            }
            if (masr.Detect())
            {
                xmlType = XMLTypes.AUTOSAR;
                masr.Process();
            }
            if (mreqif.Detect())
            {
                xmlType = XMLTypes.ReqIf;
                mreqif.Process();
            }
        }

        public string xsdContent = "";

        //public ArrayList charToLine = new ArrayList();
        public int[] charToLine = new int[0];
        public ArrayList lineToChar = new ArrayList();
        public void createCharToLineArray()
        {
            lineToChar.Clear();
            //charToLine.Clear();
            string sentence = "";
            lineToChar.Add(0);
            Regex _rx;
            _rx = new Regex(@"\n", RegexOptions.Compiled);
            sentence = _rx.Replace(fileContent, string.Empty);
            //sentence = temp;
            var lineNumber = 0;
            charToLine = new int[sentence.Length+1];
            charToLine[0] = 0;
            for (int i = 0; i < sentence.Length; i++)
            {
                //charToLine.Add(lineNumber);
                charToLine[i] = lineNumber;
                if (sentence[i] == '\r')
                {
                    lineNumber++;
                    lineToChar.Add(i+1);
                }
            }
        }

        public void updateFile()
        {
            if (false)
            {
                try
                {
                    xdoc = XDocument.Parse(form1.fastColoredTextBox1.Text, LoadOptions.SetLineInfo);
                    xdoc.Save(tempFileName);
                    xdoc = XDocument.Load(tempFileName, LoadOptions.SetLineInfo);
                    File.Delete(tempFileName);
                }
                catch (Exception e)
                { }
                string temp = xdoc.ToString();
                Regex _rx = new Regex(@"\n", RegexOptions.Compiled);
                string sentence = _rx.Replace(temp, string.Empty);
                //tokens = XmlTokenizer.Tokenize(sentence);
                createCharToLineArray();
            }

            xmlFolderRead = false;
            idTag = false;

            if (form1 != null)
            {
                fileContent = form1.fastColoredTextBox1.Text;

                if (true)
                {
                    //if (xmlType == XMLTypes.AUTOSAR)
                    {
                        try
                        {
                            xdoc = XDocument.Parse(fileContent, LoadOptions.SetLineInfo);
                            xdoc.Save(tempFileName);
                            xdoc = XDocument.Load(tempFileName, LoadOptions.SetLineInfo);
                            File.Delete(tempFileName);
                            fileContent = xdoc.ToString();
                        }
                        catch (Exception e)
                        {
                            //data model incorrect
                            form1.button1.Visible = true;
                            form1.label1.Visible = true;
                        }
                        if (!form1.label1.Visible)
                        {
                            //form1.fastColoredTextBox1.Range.ClearStyle(form1.ros);
                            form1.fastColoredTextBox1.Text = fileContent;
                            if (form1.fastColoredTextBox1.Text != fileContent)
                            {
                                form1.logText += "Assignment of processed text to textbox failed.\r\n";
                            }
                            //form1.fastColoredTextBox1.Range.SetStyle(form1.ros, @".*?<file name="".*?"">|.*?<\/file>|.*?<open>|.*?<\/open>");
                            form1.ownUpdate = 10;
                        }
                    }
                }

                createCharToLineArray();
                recognizeType();
                getFileNamesAndLines();
                createLinkStructureArray();

                xmlFolderRead = true;
                idTag = true;
            }
        }

        public ArrayList fileLine = new ArrayList();
        public ArrayList fileNames = new ArrayList();
        public bool[] fileModified;
        public ArrayList linesToFiles = new ArrayList();
        public void getFileNamesAndLines()
        {
            fileLine.Clear();
            fileNames.Clear();
            linesToFiles.Clear();

            Regex _rx;
            _rx = new Regex(@"\n", RegexOptions.Compiled);
            string sentence = _rx.Replace(fileContent, string.Empty);

            string pattern = @"<file name=""(.*?)""";
            int lastI = 0;
            foreach (Match match in Regex.Matches(sentence, pattern))
            {
                if (match.Success && match.Groups.Count > 0)
                {
                    int lineNumber = (int)charToLine[match.Groups[1].Index];
                    fileLine.Add(lineNumber);

                    string fileName = match.Groups[1].Value;
                    string cleanPath = fileName;
                    if (fileName != null)
                    {
                        int index = fileName.IndexOf(filePath);
                        cleanPath = (index < 0)
                            ? fileName
                            : fileName.Remove(index, filePath.Length);
                    }

                    for (int i=lastI; i < lineNumber; i++)
                    {
                        linesToFiles.Add(fileNames.Count-1);
                    }
                    lastI = lineNumber;

                    if (cleanPath != "")
                    {
                        fileNames.Add(cleanPath);
                    }
                    else
                    {
                        fileNames.Add(filePath);
                    }
                }
            }
            for (int i = lastI; i < lineToChar.Count; i++)
            {
                linesToFiles.Add(fileNames.Count - 1);
            }
        }


        public ArrayList linkStructureArray = new ArrayList();

        public void createLinkStructureArray()
        {
            linkStructureArray.Clear();
            //if (idTag == true)
            {
                // extract start and end number
                for (int i = 0; i < fileNames.Count; i++)
                {
                    LinkStructure ls = new LinkStructure();
                    ls.text = fileNames[i].ToString();
                    ls.startChar = (int)lineToChar[(int)fileLine[i]];
                    if (i + 1 < fileNames.Count)
                    {
                        ls.endChar = (int)lineToChar[(int)fileLine[i + 1]] - 1;
                    }
                    else
                    {
                        ls.endChar = charToLine.Length - 1;
                    }
                    ls.number = i;

                    linkStructureArray.Add(ls);
                }


                for (int i = 0; i < linkStructureArray.Count; i++)
                {
                    LinkStructure ls = (LinkStructure)linkStructureArray[i];
                    foreach (DictionaryEntry g in hrefToPosition)
                    {
                        ArrayList al = (ArrayList)g.Value;
                        foreach (Point Pentry in al)
                        {
                            if ((ls.startChar <= Pentry.X) && (ls.endChar >= Pentry.Y + Pentry.X))
                            {
                                if (hrefFromPosition.ContainsKey(g.Key))
                                {
                                    ArrayList al2 = ((ArrayList)hrefFromPosition[(string)g.Key]);
                                    foreach (Point p2 in al2)
                                    {
                                        // it's a point outside the current ls
                                        if (!((ls.startChar <= p2.X) && (ls.endChar >= p2.X + p2.Y)))
                                        {
                                            for (int j = 0; j < linkStructureArray.Count; j++)
                                            {
                                                if (i != j)
                                                {
                                                    if ((((LinkStructure)linkStructureArray[j]).startChar <= p2.X) && (((LinkStructure)linkStructureArray[j]).endChar >= p2.X + p2.Y))
                                                    {
                                                        if (!ls.allLSReferencesTo.Contains(j))
                                                        {
                                                            ls.allLSReferencesTo.Add(j);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    foreach (DictionaryEntry g in hrefFromPosition)
                    {
                        ArrayList al = (ArrayList)g.Value;
                        foreach (Point Pentry in al)
                        {
                            if ((ls.startChar <= Pentry.X) && (ls.endChar >= Pentry.Y + Pentry.X))
                            {
                                if (hrefToPosition.ContainsKey(g.Key))
                                {
                                    ArrayList al2 = ((ArrayList)hrefToPosition[(string)g.Key]);
                                    foreach (Point p2 in al2)
                                    {
                                        // it's a point outside the current ls
                                        if (!((ls.startChar <= p2.X) && (ls.endChar >= p2.X + p2.Y)))
                                        {
                                            for (int j = 0; j < linkStructureArray.Count; j++)
                                            {
                                                if (i != j)
                                                {
                                                    if ((((LinkStructure)linkStructureArray[j]).startChar <= p2.X) && (((LinkStructure)linkStructureArray[j]).endChar >= p2.X + p2.Y))
                                                    {
                                                        if (!ls.allLSReferencesFrom.Contains(j))
                                                        {
                                                            ls.allLSReferencesFrom.Add(j);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }

            }
        }

    }
}