#define SAVE_DOC_FOR_TEST
//#define AUTOCREATE_SAMPLES
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Schema;
using ICSharpCode.AvalonEdit.Folding;
using Microsoft.Win32;
using XCase.Evolution;
using XCase.Evolution.Xslt;
using XCase.Model;
using XCase.Translation.DataGenerator;
using System.Xml;
using System.Xml.Xsl;
using XCase.Translation.XmlSchema;
using System.Configuration;

namespace XCase.Gui.Windows
{
    /// <summary>
    /// Interaction logic for XsltTestWindow.xaml
    /// </summary>
    public partial class XsltTestWindow
    {
        private static string BASE_DIR = @"D:\Programování\XCase\Test\Evolution\";

        readonly IEnumerable<string> fileNames;
        
        public XsltTestWindow()
        {
            InitializeComponent();

            if (Directory.Exists(BASE_DIR))
            {
                FileInfo[] fileInfos = (new DirectoryInfo(BASE_DIR)).GetFiles("*.xslt");

                fileNames = fileInfos.Select(a => a.Name);
                cbXsltList.ItemsSource = fileNames;

                if (fileNames.Contains("tmp.xslt"))
                    OpenXslt("tmp.xslt");
            }

#if DEBUG
#else 
            bTestOutputCreation.Visibility = Visibility.Collapsed;
            bSaveRef.Visibility = Visibility.Collapsed;
            pXSLT.Visibility = Visibility.Collapsed;
#endif
        }

        private XsltTestWindow(List<EvolutionChange> changes)
            : this()
        {
            Changes = changes; 

            foldingManager = FoldingManager.Install(tbOldDoc.TextArea);
            foldingManager = FoldingManager.Install(tbNewDoc.TextArea);
            foldingManager = FoldingManager.Install(tbXslt.TextArea);

            tbOldDoc.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("XML");
            tbOldDoc.ShowLineNumbers = true;

            tbNewDoc.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("XML");
            tbNewDoc.ShowLineNumbers = true;

            tbXslt.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("XML");
            tbXslt.ShowLineNumbers = true;

            foldingStrategy = new XmlFoldingStrategy();
        }

        protected EvolutionChange SelectedChange { get; set; }

        private PSMDiagram diagramOldVersion;
        protected PSMDiagram DiagramOldVersion
        {
            get { return diagramOldVersion; }
            set
            {
                diagramOldVersion = value;
#if AUTOCREATE_SAMPLES
                CreateSampleDocument();
#endif
                if (fileNames != null)
                {
                    if (fileNames.Contains(diagramOldVersion.Caption + ".xslt"))
                    {
                        cbXsltList.SelectedItem = diagramOldVersion.Caption + ".xslt";
                        OpenXslt(cbXsltList.SelectedItem.ToString());
                    }
                    if (fileNames.Contains(diagramOldVersion.Project.Caption + ".xslt"))
                    {
                        cbXsltList.SelectedItem = diagramOldVersion.Project.Caption + ".xslt";
                        OpenXslt(cbXsltList.SelectedItem.ToString());
                    }
                    if (fileNames.Contains(Path.GetFileNameWithoutExtension(diagramOldVersion.Project.FilePath) + ".xslt"))
                    {
                        cbXsltList.SelectedItem = Path.GetFileNameWithoutExtension(diagramOldVersion.Project.FilePath) + ".xslt";
                        OpenXslt(cbXsltList.SelectedItem.ToString());
                    }
                }
            }
        }

        private PSMDiagram diagramNewVersion;

        protected PSMDiagram DiagramNewVersion
        {
            get { return diagramNewVersion; }
            set 
            { 
                diagramNewVersion = value;
#if AUTOCREATE_SAMPLES
                CreateSampleNewDocument();
#endif 
            }
        }

        private void CreateSampleNewDocument()
        {
            SampleDataGenerator sampleDataGenerator = new SampleDataGenerator();
            sampleDataGenerator.GenerateComments = false;
            string sampleDoc = sampleDataGenerator.Translate(diagramNewVersion);
            tbNewDoc.Text = sampleDoc;
            UpdateFolding();
        }

#if DEBUG
#if SAVE_DOC_FOR_TEST
        const string SAVE_DIR = @"D:\Programování\XCase\StylusStudio\";
        const string SAVE_DOCUMENT = @"D:\Programování\XCase\StylusStudio\LastInput.xml";
        const string SAVE_STYLESHEET = @"D:\Programování\XCase\StylusStudio\LastStylesheet.xslt";
#endif 
#endif 

        private void CreateSampleDocument()
        {
            SampleDataGenerator sampleDataGenerator = new SampleDataGenerator();
            sampleDataGenerator.GenerateComments = false;
            string sampleDoc = sampleDataGenerator.Translate(diagramOldVersion);
            tbOldDoc.Text = sampleDoc;

            SaveOutput(sampleDoc);
            UpdateFolding();
        }

        private static void SaveOutput(string sampleDoc)
        {
#if DEBUG
#if SAVE_DOC_FOR_TEST
            int si = sampleDoc.IndexOf("xmlns:xsi=\"");
            int ei = sampleDoc.IndexOf("\"", si + "xmlns:xsi=\"".Length) + 1;
            string text = sampleDoc.Remove(si, ei - si);
            si = text.IndexOf("xmlns=\"");
            ei = text.IndexOf("\"", si + "xmlns=\"".Length) + 1;
            string xmlns = text.Substring(si, ei - si);
            text = text.Remove(si, ei - si);
            File.WriteAllText(SAVE_DOCUMENT, text.Replace("utf-16", "utf-8"), Encoding.UTF8);
#endif
#endif
        }

        public static bool? ShowDialog(List<EvolutionChange> changes, PSMDiagram activeDiagramOldVersion, PSMDiagram activeDiagramNewVersion)
        {
            XsltTestWindow xsltTestWindow = new XsltTestWindow(changes);
            xsltTestWindow.DiagramOldVersion = activeDiagramOldVersion;
            xsltTestWindow.DiagramNewVersion = activeDiagramNewVersion;
            
            //return xsltTestWindow.ShowDialog();
            xsltTestWindow.Show();
            return true;
        }

        protected List<EvolutionChange> Changes { get; private set; }

        private readonly XmlFoldingStrategy foldingStrategy;
        private readonly FoldingManager foldingManager;

        private void UpdateFolding()
        {
            try
            {
                if (foldingStrategy != null)
                {
                    if (!string.IsNullOrEmpty(tbOldDoc.Text))
                        foldingStrategy.UpdateFoldings(foldingManager, tbOldDoc.Document);
                    if (!string.IsNullOrEmpty(tbNewDoc.Text))
                        foldingStrategy.UpdateFoldings(foldingManager, tbNewDoc.Document);
                    if (!string.IsNullOrEmpty(tbXslt.Text))
                        foldingStrategy.UpdateFoldings(foldingManager, tbXslt.Document);
                }
            }
            catch (Exception)
            {

            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CreateSampleDocument();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Transform();
        }

        private void Transform()
        {
            if (string.IsNullOrEmpty(tbOldDoc.Text) || string.IsNullOrEmpty(tbXslt.Text))
            {
                return;
            }

            string outDoc = XsltProcessing.Transform(tbOldDoc.Text, tbXslt.Text, BASE_DIR);
            tbNewDoc.Text = outDoc;

#if DEBUG
#if SAVE_DOC_FOR_TEST
            int si = outDoc.IndexOf("xmlns:xsi=\"");
            int ei = outDoc.IndexOf("\"", si + "xmlns:xsi=\"".Length) + 1;
            string text = outDoc.Remove(si, ei - si);
            si = text.IndexOf("xmlns=\"");
            ei = text.IndexOf("\"", si + "xmlns=\"".Length) + 1;
            string xmlns = text.Substring(si, ei - si);
            text = text.Remove(si, ei - si);
            File.WriteAllText(SAVE_DOCUMENT.Replace("LastInput", "LastOutput"), text.Replace("utf-16", "utf-8"), Encoding.UTF8);
#endif
#endif
        }

        

        private void xmlEdit_TextChanged(object sender, EventArgs e)
        {
            //UpdateFolding();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
                                                {
                CheckFileExists = false,
                DefaultExt = "xslt",
                Filter = "XSLT Transformations|*.xslt"
            };

            string path = System.IO.Path.GetDirectoryName(diagramNewVersion.Project.FilePath) + "\\" +
                System.IO.Path.GetFileNameWithoutExtension(diagramNewVersion.Project.FilePath) + "-out" + "\\last-template.xslt";

            if (path.Contains("Test\\Evolution"))
            {
                if (!Directory.Exists(System.IO.Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                }
                File.WriteAllText(path, tbXslt.Text);
            }
            else if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, tbXslt.Text);
            }

            
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
                                                {
                                                    CheckFileExists = true,
                                                    DefaultExt = "xslt",
                                                    Filter = "XSLT Transformations|*.xslt"
                                                };

            if (openFileDialog.ShowDialog() == true)
            {
                OpenXslt(openFileDialog.FileName);
            }
        }

        private void OpenXslt(string filename)
        {
            if (File.Exists(filename))
                tbXslt.Text = File.ReadAllText(filename);
            else 
                tbXslt.Text = File.ReadAllText(BASE_DIR + filename);
        }

        private void bXsltBasic_Click(object sender, RoutedEventArgs e)
        {
            tbXslt.Text = String.Empty; // Evolution.Stylesheets.Stylesheets.XSLT_EMPTY;
        }

        private void cbXsltList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string file = cbXsltList.SelectedValue.ToString();

            string xsltString = File.ReadAllText(BASE_DIR + file);
            tbXslt.Text = xsltString;
            UpdateFolding();
        }
        
        private void bXsltFromChanges_Click(object sender, RoutedEventArgs e)
        {
            XsltTemplateGenerator xsltTemplateGenerator = new XsltTemplateGenerator(diagramNewVersion);
            string xslt = xsltTemplateGenerator.Generate(Changes, diagramOldVersion.Version, diagramNewVersion.Version);
            tbXslt.Text = xslt;
            #if DEBUG
            #if SAVE_DOC_FOR_TEST
            File.WriteAllText(SAVE_STYLESHEET, xslt);
            string dir = Path.GetDirectoryName(diagramNewVersion.Project.FilePath) + "\\" + Path.GetFileNameWithoutExtension(diagramNewVersion.Project.FilePath) + "-out";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(dir + "\\last-stylesheet.xslt", xslt);
            #endif
            #endif
            UpdateFolding();
        }

        

        private void bValidateOld_Click(object sender, RoutedEventArgs e)
        {
            DocumentValidator validator = new DocumentValidator();
            validator.ValidateDocument(diagramOldVersion, tbOldDoc.Text);
        }

        private void bValidateNew_Click(object sender, RoutedEventArgs e)
        {
            DocumentValidator validator = new DocumentValidator();
            validator.ValidateDocument(diagramNewVersion, tbNewDoc.Text);
        }

        private void SaveRef_Click(object sender, RoutedEventArgs e)
        {
            SaveRef(null);
        }

        private void SaveRef(string desiredName)
        {
            string name = Path.GetFileNameWithoutExtension(diagramNewVersion.Project.FilePath);
            string dir = Path.GetDirectoryName(diagramNewVersion.Project.FilePath);

            string inputDir = dir + "\\" + name + "-in";
            string outputDir = dir + "\\" + name + "-out";

            if (!Directory.Exists(inputDir))
                Directory.CreateDirectory(inputDir);

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
            string s;
            if (string.IsNullOrEmpty(desiredName))
            {
                string ts = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString().Replace(":", "-");
                s = string.Format("{0} test {1}", name, ts);
            }
            else
            {
                s = desiredName;
            }
            string filenameIn = string.Format("{0}.xml", s);
            string filenameRef = string.Format("{0}-reference.xml", s);

            File.WriteAllText(inputDir + "\\" + filenameIn, tbOldDoc.Text);
            File.WriteAllText(outputDir + "\\" + filenameRef, tbNewDoc.Text);
        }

        private void SaveRefCust_Click(object sender, RoutedEventArgs e)
        {
            string desiredName;
            if (InputBox.Show("FileName: ", out desiredName) == true)
            {
                SaveRef(desiredName);
            }
        }

        private void TestOutputCreation_Click(object sender, RoutedEventArgs e)
        {
            CreateSampleDocument();

            DocumentValidator validator = new DocumentValidator { SilentMode = true };
            
            if (!validator.ValidateDocument(diagramOldVersion, tbOldDoc.Text))
                throw new Exception("Old document invalid");

            Transform();

            validator = new DocumentValidator { SilentMode = true };
            if (!validator.ValidateDocument(diagramNewVersion, tbNewDoc.Text))
                throw new Exception("New document invalid");

            SaveRef(null);

            MessageBox.Show("Created OK", "Created OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GenerateOutput_Click(object sender, RoutedEventArgs e)
        {
            CreateSampleNewDocument();
            SaveOutput(tbNewDoc.Text);
        }
    }
}
