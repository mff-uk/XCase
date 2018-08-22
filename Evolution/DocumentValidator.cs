using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using XCase.Model;
using XCase.Translation.XmlSchema;
using System.Windows;

namespace XCase.Evolution
{
    public class DocumentValidator
    {
        private bool isValid;
        private bool abort;
        public bool SilentMode { get; set; }
        public string ErrorMessage { get; set; }

        public bool ValidateDocument(PSMDiagram diagram, string xmltext)
        {
            XmlSchemaTranslator t = new XmlSchemaTranslator();
            string xmlSchemaText = t.Translate(diagram);
            XmlReader xmlfile = null;
            XmlReader schemaReader = null;
            MemoryStream _msSchemaText = null;
            isValid = true;
            abort = false;
            try
            {
                byte[] b = Encoding.Unicode.GetBytes(xmlSchemaText);
                _msSchemaText = new MemoryStream(b);
                schemaReader = new XmlTextReader(_msSchemaText);
                XmlSchema schema = XmlSchema.Read(schemaReader, schemaSettings_ValidationEventHandler);
                schema.TargetNamespace = diagram.Project.Schema.XMLNamespaceOrDefaultNamespace;

                XmlReaderSettings schemaSettings = new XmlReaderSettings();
                schemaSettings.Schemas.Add(schema);
                schemaSettings.ValidationType = ValidationType.Schema;
                schemaSettings.ValidationEventHandler += schemaSettings_ValidationEventHandler;
                schemaSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                try
                {
                    xmlfile = XmlReader.Create(new StringReader(xmltext), schemaSettings);
                }
                catch (XmlSchemaValidationException ex)
                {
                    isValid = false;
                    if (!SilentMode)
                    MessageBox.Show(string.Format("Validation can not continue - schema is invalid. \r\n\r\n{0}", ex.Message), "Invalid schema", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    ErrorMessage = string.Format("Validation can not continue - schema is invalid. \r\n\r\n{0}", ex.Message);
                    return false;
                }

                if (isValid)
                {
                    while (xmlfile.Read() && !abort)
                    {
                    }
                }
            }
            catch (XmlSchemaValidationException ex)
            {
                isValid = false;
                if (!SilentMode)
                MessageBox.Show(string.Format("{0} \r\n\r\nValidation can not continue.", ex.Message), "Invalid document", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                ErrorMessage = string.Format("{0} \r\n\r\nValidation can not continue.", ex.Message);
            }
            catch (Exception ex)
            {
                isValid = false;
                if (!SilentMode)
                MessageBox.Show(string.Format("{0} \r\n\r\nValidation can not continue.", ex.Message), "Invalid document", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                ErrorMessage = string.Format("{0} \r\n\r\nValidation can not continue.", ex.Message);
            }
            finally
            {
                if (xmlfile != null) xmlfile.Close();
                if (schemaReader != null) schemaReader.Close();
                if (_msSchemaText != null) _msSchemaText.Dispose();
            }

            if (isValid)
            {
                if (!SilentMode)
                MessageBox.Show("Document is valid", "Document valid", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            return isValid;
        }

        void schemaSettings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            string location = string.Empty;
            if (e.Exception != null)
            {
                location = string.Format("\r\n\rLine number: {0} position {1}", e.Exception.LineNumber, e.Exception.LinePosition);
            }
            if (!SilentMode && !abort)
            {
                MessageBoxResult result =
                    MessageBox.Show(string.Format("{0}{1}\r\n\rContinue validation?", e.Message, location),
                                    "Invalid document", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

                if (result == MessageBoxResult.No)
                    abort = true;
            }
            else
            {
                abort = true;
                ErrorMessage = string.Format("{0}{1}\r\n\r", e.Message, location);
            }
            isValid = false;
        }
    }
}