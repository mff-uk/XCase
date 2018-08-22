using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using ConfigWizardUI;
using ConfigWizard;

namespace XCase.Translation.XmlSchema
{
    public class Configuration
    {
        enum Level { IGNORE = 0, WARNING, ERROR }
        const int MAX_INDENTATION_SPACES = 8;

        public static string[] validMainParameters = new string[]
            { "design_pattern", "attr_qualified", "use_namespaces", "namespace_prefix",
              "emptyline_on_globals", "projname_in_namespace", "indentation_spaces",
              "group_name_mask", "attrgroup_name_mask", "rename_postfix",
              "elim_redundant_aps", "elim_redundant_attr_decls", "elim_redundancy_in_nestings"
            };

        private Dictionary<string, string> mainParameters;

        // object construction
        public Configuration()
        {
            Initialize();
        }

        // initialization, re-initialization
        public void Initialize()
        {
            mainParameters = new Dictionary<string, string>
            {
                {"design_pattern", "venetian blind"},
                {"use_namespaces", "disabled"},                
                {"attrgroup_name_mask", "%-a"},
                {"group_name_mask", "%-c"},

                {"elim_redundant_aps", "disabled"},
                {"elim_redundant_attr_decls", "disabled"},
                {"elim_redundancy_in_nestings", "disabled"},

                {"indentation_spaces", "2"},
                {"namespace_prefix", "http://kocour.ms.mff.cuni.cz/xcase/"},
                {"emptyline_on_globals", "yes"},
                {"attr_qualified", "qualified"},
                {"projname_in_namespace", "no"}
            };
        }


        // Loads configuration from the given location.
        // If loading fails, it will reinitialize configuration and return false.
        public bool Load(string fileName)
        {
            XmlDocument confdoc = new XmlDocument();
            confdoc.Load(fileName);

            XmlNode root = confdoc.SelectSingleNode("/settings");
            if (root == null)
                return false;
            XmlAttribute attrVersion = root.Attributes["version"];
            if ((attrVersion == null) || (attrVersion.Value != "1.0"))
                return false;

            if (confdoc.SelectNodes("/settings/main-parameters").Count == 0)
                return false;

            // first loading main parameters from configuration file
            XmlNodeList mainParams = confdoc.SelectNodes("/settings/main-parameters/param");
            foreach ( XmlNode param in mainParams )
            {
                string name = param.Attributes["name"].Value;
                string value = param.InnerText;

                if (!isValidMainParameter(name))
                {
                    Initialize();
                    return false;
                }

                if (!isValidValue(name, value))
                {
                    Initialize();
                    return false;
                }

                // lets set loaded value
                mainParameters[name] = value;
            }

            // everything went fine
            return true;
        }


        // It will save configuration to the given location
        public void Save(string fileName)
        {
            // we will construct a XmlDocument, fill it with configuration
            // parameters and then save everything to given path using a FileStream

            XmlDocument confdoc = new XmlDocument();
            FileStream fstr = new FileStream(fileName, FileMode.OpenOrCreate);

            // put <?xml..> declaration and root element to the top of the file
            confdoc.AppendChild(confdoc.CreateXmlDeclaration("1.0", "utf-8", "no"));
            XmlElement elemSettings = confdoc.CreateElement("settings");
            elemSettings.SetAttribute("version", "1.0");
            confdoc.AppendChild(elemSettings);

            // first we will process the main parameters
            XmlElement elemMainParams = confdoc.CreateElement("main-parameters");
            elemSettings.AppendChild(elemMainParams);
            foreach (string paramName in mainParameters.Keys)
            {
                XmlElement elem = confdoc.CreateElement("param");
                elem.SetAttribute("name", paramName);
                elem.InnerText = mainParameters[paramName];
                elemMainParams.AppendChild(elem);
            }

            // XmlDocument of configuration is prepared, lets put it to the file stream
            confdoc.Save(fstr);
            fstr.Close();
        }


        // function constructs and displays a wizard helping to create new configuration
        public static string createWithWizard()
        {
            WizardSheet wizard = new WizardSheet();
            WelcomePage welcomePage = new WelcomePage();

            MidPage_DesignApproach midpage_DA = new MidPage_DesignApproach();
            MidPage_UseNamespaces midpage_UN = new MidPage_UseNamespaces();
            MidPage_Redundancy midpage_R = new MidPage_Redundancy();
            CompletePage completePage = new CompletePage();

            wizard.Pages.Add(welcomePage);
            wizard.Pages.Add(midpage_DA);
            wizard.Pages.Add(midpage_UN);
            wizard.Pages.Add(midpage_R);
            wizard.Pages.Add(completePage);

            if (wizard.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Configuration conf = new Configuration();
                conf.setDesignApproach(midpage_DA.getDesignApproach());
                conf.setUsingNamespaces(midpage_UN.getUseNamespaces());
                conf.setEliminateRedundantAPs(midpage_R.isElimRedAPsEnabled());
                conf.setEliminateRedundantAttrDecls(midpage_R.isElimRedAttrDeclsEnabled());
                conf.setEliminateRedundancyInNestings(midpage_R.isElimRedInNestingsEnabled());

                string saveFileName = completePage.getFileName();
                if ((saveFileName != null) && (saveFileName != ""))
                    conf.Save(saveFileName);

                return saveFileName;
            }
            return null;   
        }


        #region functions to read configuration
        public string getGroupNameMask()
        {
            return mainParameters["group_name_mask"];
        }

        public string getAttrGroupNameMask()
        {
            return mainParameters["attrgroup_name_mask"];
        }

        public bool useAlphabetForRenaming()
        {
            string paramValue = mainParameters["rename_prefix"];
            if (paramValue.Substring(0, 1).Equals("a"))
                return true;
            return false;
        }

        public bool useNumbersForRenaming()
        {
            string paramValue = mainParameters["rename_prefix"];
            if (paramValue.Substring(0, 1).Equals("n"))
                return true;
            return false;
        }

        public bool useSequenceForRenaming()
        {
            string paramValue = mainParameters["rename_prefix"];
            if (paramValue.Substring(0, 1).Equals("s"))
                return true;
            return false;
        }

        public List<string> getSequenceForRenaming()
        {
            string paramValue = mainParameters["rename_prefix"];
            if (!paramValue.Substring(0, 1).Equals("s"))
                return null;
            
            // get sequence from comma-separated string
            string [] items = paramValue.Split(',');
            return new List<string>(items);
        }

        public int cntIndentSpaces()
        {
            return Convert.ToInt32(mainParameters["indentation_spaces"]);            
        }

        public bool isAttrQualified()
        {
            return (mainParameters["attr_qualified"] == "qualified" ? true : false);
        }

        public string getElementFormDefault()
        {
            return mainParameters["attr_qualified"];
        }

        public bool isUsingNamespaces()
        {
            return (mainParameters["use_namespaces"] == "enabled" ? true : false);
        }

        public string getNamespacePrefix()
        {
            return mainParameters["namespace_prefix"];
        }

        public bool useProjectNameInNamespace()
        {
            return (mainParameters["projname_in_namespace"] == "yes" ? true : false);
        }

        public bool emptyLineBeforeGlobal()
        {
            return (mainParameters["emptyline_on_globals"] == "yes" ? true : false);
        }

        public bool isVenetianBlind()
        {
            return ((mainParameters["design_pattern"] == "venetian blind") ? true : false);
        }

        public bool isRussianDoll()
        {
            return ((mainParameters["design_pattern"] == "russian doll") ? true : false);
        }

        public bool isSalamiSlice()
        {
            return ((mainParameters["design_pattern"] == "salami slice") ? true : false);
        }

        public bool isGardenOfEden()
        {
            return ((mainParameters["design_pattern"] == "garden of eden") ? true : false);
        }

        public bool allElementsGlobal()
        {
            return isGardenOfEden() || isSalamiSlice();
        }

        public bool allElementsLocal()
        {
            return isVenetianBlind() || isRussianDoll();
        }

        public bool allComplexTypesGlobal()
        {
            return isVenetianBlind() || isGardenOfEden();
        }

        public bool allComplexTypesLocal()
        {
            return isRussianDoll() || isSalamiSlice();
        }

        public bool isEliminateRedundantAPsEnabled()
        {
            return ((mainParameters["elim_redundant_aps"] == "enabled") ? true : false);
        }

        public bool isEliminateRedundantAttrDeclsEnabled()
        {
            return ((mainParameters["elim_redundant_attr_decls"] == "enabled") ? true : false);
        }

        public bool isEliminateRedundancyInNestingsEnabled()
        {
            return ((mainParameters["elim_redundancy_in_nestings"] == "enabled") ? true : false);
        }
        #endregion  // functions to read configuration

        #region functions to set configuration
        // param postfix: 0 .. numbers, 1 .. alphabet, 2 .. sequence
        public void setRenamePostfix(int postfix, string sequenceText)
        {
            string value = "";
            switch (postfix)
            {
                case 0: value = "n"; break;
                case 1: value = "a"; break;
                case 2: value = "s[" + sequenceText + "]"; break;
            }
            mainParameters["rename_postfix"] = value;
        }

        public void setGroupNameMask(string mask)
        {
            mainParameters["group_name_mask"] = mask;
        }

        public void setAttrGroupNameMask(string mask)
        {
            mainParameters["attrgroup_name_mask"] = mask;
        }

        public void setUseProjectNameInNamespace(bool includeProjName)
        {
            mainParameters["projname_in_namespace"] = (includeProjName ? "yes" : "no");
        }

        public void setIndentSpacesCnt(int cntSpaces)
        {
            mainParameters["indentation_spaces"] = cntSpaces.ToString();
        }

        public void setEmptyLineBeforeGlobal(bool newValue)
        {
            mainParameters["emptyline_on_globals"] = (newValue ? "yes" : "no");
        }

        public void setDesignApproach(string approach)
        {
            mainParameters["design_pattern"] = approach;
        }

        public void setAttrQualified(bool newValue)
        {
            mainParameters["attr_qualified"] = (newValue ? "qualified" : "unqualified");
        }

        public void setUsingNamespaces(bool newValue)
        {
            mainParameters["use_namespaces"] = (newValue ? "enabled" : "disabled");
        }

        public void setNamespacePrefix(string namespacePrefix)
        {
            mainParameters["namespace_prefix"] = namespacePrefix;
        }

        public void setEliminateRedundantAPs(bool enabled)
        {
            mainParameters["elim_redundant_aps"] = (enabled ? "enabled" : "disabled");
        }
        public void setEliminateRedundantAttrDecls(bool enabled)
        {
            mainParameters["elim_redundant_attr_decls"] = (enabled ? "enabled" : "disabled");
        }
        public void setEliminateRedundancyInNestings(bool enabled)
        {
            mainParameters["elim_redundancy_in_nestings"] = (enabled ? "enabled" : "disabled");
        }
        #endregion

        // determine if paramName is name of a valid main parameter of configuration
        private bool isValidMainParameter(string paramName)
        {
            if (validMainParameters.Contains(paramName))
                return true;
            return false;
        }

        private bool isValidValue(string name, string value)
        {
            switch (name)
            {
                // yes X no
                case "emptyline_on_globals":
                case "projname_in_namespace":
                    value = value.ToLower();
                    if ((value != "yes") && (value != "no"))
                        return false;
                    return true;

                case "use_namespaces":
                case "elim_redundant_aps":
                case "elim_redundant_attr_decls":
                case "elim_redundancy_in_nestings":
                    value = value.ToLower();
                    if ((value != "enabled") && (value != "disabled"))
                        return false;
                    return true;

                case "design_pattern":
                    value = value.ToLower();
                    if ((value != "venetian blind") && (value != "garden of eden")
                        && (value != "russian doll") && (value != "salami slice"))
                        return false;
                    return true;

                case "attr_qualified":
                    value = value.ToLower();
                    if ((value != "qualified") && (value != "unqualified"))
                        return false;
                    return true;

                case "indentation_spaces":
                    int cntSpaces;
                    try
                    {
                        cntSpaces = Convert.ToInt32(value);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                    if ((cntSpaces < 0) || (cntSpaces > MAX_INDENTATION_SPACES))
                        return false;
                    return true;

                case "namespace_prefix":
                    // must be valid URI
                    try
                    {
                        Uri prefixURI = new Uri(value);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                    return true;
            }
            return true;
        }

        // converts string representation of given log level to int representation
        // returns false if conversion is not possible
        private bool getLevelFromString(string value, out Level level)
        {
            value = value.ToLower();
            level = Level.IGNORE;

            switch (value)
            {
                case "ignore": return true;
                case "warning": level = Level.WARNING; return true;
                case "error": level = Level.ERROR; return true;
                default: return false;
            }
        }

        // converts int representation of given log level to string
        private string getStringFromLevel(Level level)
        {
            switch (level)
            {
                case Level.ERROR: return "error";
                case Level.IGNORE: return "ignore";
                case Level.WARNING: return "warning";
            }
            return null;
        }
    }
}

