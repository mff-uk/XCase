using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using NUnit.Framework;
using XCase.Model;
using XCase.Evolution;
using XCase.Translation.DataGenerator;
using Version = XCase.Model.Version;

namespace Tests.Evolution
{
    [TestFixture]
    public class EvolutionOutputTest
    {
        private static string TEST_BASE_DIR
        {
            get
            {
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["EvolutionTestFilesDirectory"]))
                    throw new ConfigurationErrorsException("Key EvolutionTestFilesDirectory not found in the configuration file");
                return ConfigurationManager.AppSettings["EvolutionTestFilesDirectory"];
            }

        }//= @"D:\Programování\XCase\Test\Evolution\";
        private const int RANDOM_FILE_COUNT = 10;

        [TestFixtureSetUp]
        public void LookupTestFiles()
        {
            DirectoryInfo d = new DirectoryInfo(TEST_BASE_DIR);
            Assert.IsTrue(Directory.Exists(d.FullName), "Test base dir not found");
            
            List<object> ls = new List<object>();

            foreach (DirectoryInfo subdirectory in d.GetDirectories())
            {
                if (subdirectory.Name.StartsWith("#"))
                    continue;
                foreach (FileInfo fileInfo in subdirectory.GetFiles("*.XCase"))
                {
                    ls.Add(new object[] { subdirectory.FullName.Substring(TEST_BASE_DIR.Length) + "\\" + fileInfo} );
                }
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

        [Test, TestCaseSource("TestFiles")]
        public void TestByCompare(string filename)
        {
            //string filename = "Attributes\\Attributes5.XCase";
            XmlDeserializatorVersions deserializator = new XmlDeserializatorVersions();
            VersionManager versionManager = deserializator.RestoreVersionedProject(TEST_BASE_DIR + filename);
            Assert.IsTrue(versionManager != null && versionManager.VersionedProjects.Count == 2 &&
                          versionManager.Versions.Count == 2);

            Version oldVersion = versionManager.Versions[0];
            Version newVersion = versionManager.Versions[1];
            PSMDiagram diagramNewVersion = versionManager.VersionedProjects[newVersion].PSMDiagrams[0];
            PSMDiagram diagramOldVersion = versionManager.VersionedProjects[oldVersion].PSMDiagrams[0];

            ChangesDetector detector = new ChangesDetector();
            List<EvolutionChange> evolutionChanges = detector.Translate(diagramOldVersion, diagramNewVersion);

            XsltTemplateGenerator xsltGenerator = new XsltTemplateGenerator(diagramNewVersion);
            string xsltTransformation = xsltGenerator.Generate(evolutionChanges, oldVersion, newVersion);

            string inputFileDir = TEST_BASE_DIR + Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + "-in";
            string outputFileDir = TEST_BASE_DIR + Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + "-out";

            if (!Directory.Exists(inputFileDir))
            {
                Console.WriteLine("No input files. ");
                Assert.Inconclusive(string.Format("Input file directory does not exist : {0}", inputFileDir));
            }
            if (!Directory.Exists(outputFileDir))
            {
                Console.WriteLine("No reference files. ");
                Assert.Inconclusive(string.Format("Output file directory does not exist: {0}", outputFileDir));
            }

            DirectoryInfo dir = new DirectoryInfo(inputFileDir);

            FileInfo[] files = dir.GetFiles("*.xml");
            bool fail = false;
            int failedFiles = 0;
            foreach (FileInfo fileInfo in files)
            {
                Console.WriteLine(string.Format("File: {0}", fileInfo.Name));
                string input = File.ReadAllText(fileInfo.FullName);

                string result = XsltProcessing.Transform(input, xsltTransformation, TEST_BASE_DIR);
                File.WriteAllText(outputFileDir + "//" + fileInfo, result);

                DocumentValidator validator = new DocumentValidator {SilentMode = true};

                bool inputInvalid = false;
                bool resultInvalid = false;
                bool compareFail = false; 
                if (!validator.ValidateDocument(diagramOldVersion, input))
                {
                    inputInvalid = true;
                }

                if (!validator.ValidateDocument(diagramNewVersion, result))
                {
                    Console.WriteLine("Result document INVALID: " + validator.ErrorMessage);
                    resultInvalid = true; 
                }

                string referenceFile = outputFileDir + "//" + Path.GetFileNameWithoutExtension(fileInfo.Name) + "-reference.xml";
                Assert.IsTrue(File.Exists(referenceFile), "Reference output file does not exist for: " + fileInfo.Name);
                string referenceOutput = File.ReadAllText(referenceFile);

                File.WriteAllText(outputFileDir + "//" + "last-stylesheet.xslt", xsltTransformation);
                
                if ((!inputInvalid && !resultInvalid) && result != referenceOutput)
                {
                    Console.WriteLine("Output and REFERENCE output DIFFERS for: " + fileInfo.Name);
                    compareFail = true; 
                    Console.WriteLine("Input file: ");
                    Console.WriteLine(input);
                    Console.WriteLine("Result file: ");
                    Console.WriteLine(result);
                    Console.WriteLine("Reference file: ");
                    Console.WriteLine(referenceOutput);
                    
                }
                else if (!inputInvalid && !resultInvalid && result == referenceOutput)
                {
                    Console.WriteLine("Comparison succeeded.");
                    File.WriteAllText(outputFileDir + "//" + "last-working-stylesheet.xslt", xsltTransformation);
                }

                if (inputInvalid)
                {
                    Console.WriteLine("Input document INVALID: " + validator.ErrorMessage);
                    Console.WriteLine("Input file: ");
                    Console.WriteLine(input);
                    fail = true;
                    failedFiles++;
                }

                if (resultInvalid)
                {
                    Console.WriteLine("Result document INVALID: " + validator.ErrorMessage);
                    Console.WriteLine("Result file: ");
                    Console.WriteLine(result);
                    fail = true; 
                    failedFiles++;
                }

                if (compareFail)
                {
                    Console.WriteLine("Result of {0} DIFFERS from expected result.", fileInfo.Name);
                    fail = true; 
                    failedFiles++;
                }
            }
            if (files.Length == 0)
            {
                Console.WriteLine("No input files. ");
                Assert.Inconclusive("No input files. ");
            }
            if (fail)
            {
                Console.WriteLine("{0} out of {1} files FAILED. ", failedFiles, files.Length);
                Assert.Fail("{0} out of {1} files FAILED. ", failedFiles, files.Length);
            }
            Console.WriteLine("Altogether tested on {0} files.", files.Length);
        }

        [Test, TestCaseSource("TestFiles")]
        public void TestValidationOnRandomFiles(string filename)
        {
            Console.WriteLine("Testing file {0}.", filename);
            //string filename = "Attributes\\Attributes5.XCase";
            XmlDeserializatorVersions deserializator = new XmlDeserializatorVersions();
            VersionManager versionManager = deserializator.RestoreVersionedProject(TEST_BASE_DIR + filename);
            Assert.IsTrue(versionManager != null && versionManager.VersionedProjects.Count == 2 &&
                          versionManager.Versions.Count == 2);

            Version oldVersion = versionManager.Versions[0];
            Version newVersion = versionManager.Versions[1];
            PSMDiagram diagramNewVersion = versionManager.VersionedProjects[newVersion].PSMDiagrams[0];
            PSMDiagram diagramOldVersion = versionManager.VersionedProjects[oldVersion].PSMDiagrams[0];

            ChangesDetector detector = new ChangesDetector();
            List<EvolutionChange> evolutionChanges = detector.Translate(diagramOldVersion, diagramNewVersion);

            XsltTemplateGenerator xsltGenerator = new XsltTemplateGenerator(diagramNewVersion);
            string xsltTransformation = xsltGenerator.Generate(evolutionChanges, oldVersion, newVersion);

            SampleDataGenerator g = new SampleDataGenerator();
            DocumentValidator validator = new DocumentValidator { SilentMode = true };

            string outputFileDir = TEST_BASE_DIR + Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + "-error";

            int fails = 0; 
            for (int i = 0; i < RANDOM_FILE_COUNT; i++)
            {
                string input = g.Translate(diagramOldVersion);
                if (!validator.ValidateDocument(diagramOldVersion, input))
                {
                    if (!Directory.Exists(outputFileDir))
                    {
                        Directory.CreateDirectory(outputFileDir);
                    }
                    File.WriteAllText(string.Format("{0}\\i{1}.xml", outputFileDir, i), input);
                    Console.WriteLine("Input document INVALID: " + validator.ErrorMessage);
                    fails++;
                    continue;
                }
                string result = XsltProcessing.Transform(input, xsltTransformation, TEST_BASE_DIR);
                if (!validator.ValidateDocument(diagramNewVersion, result))
                {
                    if (!Directory.Exists(outputFileDir))
                    {
                        Directory.CreateDirectory(outputFileDir);
                    }
                    File.WriteAllText(string.Format("{0}\\i{1}.xml", outputFileDir, i), input);
                    File.WriteAllText(string.Format("{0}\\o{1}.xml", outputFileDir, i), result);
                    Console.WriteLine("Result document INVALID: " + validator.ErrorMessage);
                    fails++;
                    continue;
                }
                Console.WriteLine("Test passed on file {0}/{1}", i, RANDOM_FILE_COUNT);
            }
            Console.WriteLine("Altogether tested on {0} random files. ", RANDOM_FILE_COUNT);
            if (fails != 0)
            {
                Console.WriteLine("{0} out of {1} files failed to pass. ", fails,RANDOM_FILE_COUNT);
                Assert.Fail("{0} out of {1} files failed to pass. ", fails, RANDOM_FILE_COUNT);
            }
            else
            {
                Console.WriteLine("All {0} files passed. ", RANDOM_FILE_COUNT);
            }

            
        }
    }
}