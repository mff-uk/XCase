using System.Collections.Generic;
using System.IO;
using XCase.Model;

namespace XCase.Translation.XmlSchema
{
    public class ProjectTranslator
    {
        public Project Project { get; set; }

        private readonly Dictionary<PSMDiagram, string> schemaFiles = new Dictionary<PSMDiagram, string>();

        public Dictionary<PSMDiagram, string> SchemaFiles
        {
            get { return schemaFiles; }
        }

        /// <summary>
        /// 
        /// </summary> 
        /// <param name="targetDirectory">Directory where schemas are saved. When left to null, project's direcotry is used</param>
        public void SaveSchemas(string targetDirectory)
	    {
	        if (string.IsNullOrEmpty(targetDirectory))
	        {
	            targetDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(Project.FilePath));
	        }

            SchemaFiles.Clear();
            foreach (PSMDiagram psmDiagram in Project.PSMDiagrams)
            {
                XmlSchemaTranslator t = new XmlSchemaTranslator();
                string schema = t.Translate(psmDiagram);
                schema = schema.Replace("utf-16", "utf-8");
                string targetPath = string.Format("{0}\\{1}.xsd", targetDirectory, psmDiagram.Caption);
                File.WriteAllText(targetPath, schema, System.Text.Encoding.UTF8);
                SchemaFiles[psmDiagram] = targetPath;
            }
	    }
    }
}