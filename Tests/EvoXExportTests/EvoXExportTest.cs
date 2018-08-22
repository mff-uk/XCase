using System.Configuration;
using System.IO;
using EvoX.Model.Serialization;
using NUnit.Framework;
using System.Collections.Generic;
using XCase.Model;
using XCase.Model.EvoXExport;

namespace Tests.EvoXExportTests
{
    [TestFixture]
    public class EvoXExportTest
    {
        private static string TEST_BASE_DIR
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["EvoXExportTestFilesDirectory"]))
                    throw new ConfigurationErrorsException("Key EvolutionTestFilesDirectory not found in the configuration file");
                return ConfigurationManager.AppSettings["EvoXExportTestFilesDirectory"];
            }

        }//= @"D:\Programování\XCase\Test\Evolution\";
        private const int RANDOM_FILE_COUNT = 10;

        [TestFixtureSetUp]
        public void LookupTestFiles()
        {
            DirectoryInfo d = new DirectoryInfo(TEST_BASE_DIR);
            Assert.IsTrue(Directory.Exists(d.FullName), "Test base dir not found");

            List<object> ls = new List<object>();

            foreach (FileInfo fileInfo in d.GetFiles("*.XCase"))
            {
                ls.Add(fileInfo.FullName);
            }
            
            testFiles = ls.ToArray();
        }

        public object[] testFiles;

        public object[] TestFiles
        {
            get
            {
                if (testFiles == null)
                {
                    LookupTestFiles();
                }
                return testFiles;
            }
        }

        EvoX.Model.Serialization.ProjectSerializationManager projectSerializationManager = new ProjectSerializationManager();

        [Test, TestCaseSource("TestFiles")]
        public void Test(string filename)
        {
            XmlDeserializatorBase deserializator;
            if (XmlDeserializatorVersions.UsesVersions(filename))
            {
                deserializator = new XmlDeserializatorVersions();
            }
            else
            {
                deserializator = new XmlDeserializator();
            }
            Project xcaseProject = deserializator.RestoreProject(filename);
            EvoXExport evoXExport = new EvoXExport();
            EvoX.Model.Project evoxProject = evoXExport.ConvertToEvoXProject(xcaseProject);

            string resultPath = TEST_BASE_DIR + Path.GetFileNameWithoutExtension(filename) + ".EvoX";
            projectSerializationManager.SaveProject(evoxProject, resultPath);
        }
    }
}