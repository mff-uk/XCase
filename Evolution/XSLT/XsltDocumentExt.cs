using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Linq;

namespace XCase.Evolution.Xslt
{
    public static class XsltDocumentExt
    {
        public const string NS_XSLT = "http://www.w3.org/1999/XSL/Transform";

        #region 

        public static XmlAttribute XslGroupStartingWithAttribute(this XmlDocument document, XPathExpr value, bool consequentInOneGroup)
        {
            if (!consequentInOneGroup)
            {
                XmlAttribute xslGroupStartingWithAttribute = document.CreateAttribute("group-starting-with");
                xslGroupStartingWithAttribute.Value = value;
                return xslGroupStartingWithAttribute;
            }
            else
            {
                XmlAttribute xslGroupStartingWithAttribute = document.CreateAttribute("group-starting-with");
                xslGroupStartingWithAttribute.Value = string.Format("{0}[not(preceding-sibling::{0}[1] is preceding-sibling::*[1])]", value);
                return xslGroupStartingWithAttribute;
            }
        }

        #endregion 

        public static XmlElement XslElement(this XmlElement element, string elementName)
        {
            XmlElement xmlElement = element.OwnerDocument.CreateElement("xsl", elementName, NS_XSLT);
            element.AppendChild(xmlElement);
            return xmlElement;
        }

        public static XmlElement XslTemplate(this XmlElement element, XPathExpr matchXPath, params string[] parameters)
        {
            XmlElement templateElement = element.XslElement("template");
            templateElement.AddAttributeWithValue("match", matchXPath);
            if (parameters != null) 
                foreach (string parameter in parameters)
                {
                    XmlElement param = templateElement.XslElement("param");
                    param.AddAttributeWithValue("name", parameter);
                }
            return templateElement; 
        }

        public static XmlElement XslNamedTemplate(this XmlElement element, string name, params string[] parameters)
        {
            XmlElement templateElement = element.XslElement("template");
            templateElement.AddAttributeWithValue("name", name);
            if (parameters != null)
                foreach (string parameter in parameters)
                {
                    XmlElement param = templateElement.XslElement("param");
                    param.AddAttributeWithValue("name", parameter);
                }
            return templateElement;
        }

        public static XmlElement XslStylesheet(this XmlDocument document, string version)
        {
            XmlElement xmlElement = document.CreateElement("xsl", "stylesheet", NS_XSLT);
            xmlElement.AddAttributeWithValue("version", version);
            document.AppendChild(xmlElement);
            return xmlElement;
        }

        

        public static XmlElement XslCallTemplate(this XmlElement element, string templateName, params TemplateParameter[] parameters)
        {
            return XslCallTemplate(element, templateName, (IEnumerable<TemplateParameter>)parameters);
        }

        public static XmlElement XslCallTemplate(this XmlElement element, string templateName, IEnumerable<TemplateParameter> parameters)
        {
            XmlElement callTemplateElement = element.XslElement("call-template");
            callTemplateElement.AddAttributeWithValue("name", templateName);
            if (parameters != null)
                foreach (TemplateParameter keyValuePair in parameters)
                {
                    XmlElement param = callTemplateElement.XslElement("with-param");
                    param.AddAttributeWithValue("name", keyValuePair.Name);
                    param.AddAttributeWithValue("select", keyValuePair.Value);
                }
            return callTemplateElement;
        }

        public static XmlElement XslAttribute(this XmlElement element, string name, string value)
        {
            XmlElement attributeElement = element.XslElement("attribute");
            attributeElement.AddAttributeWithValue("name", name);
            if (value != null)
            {
                XmlText valueText = element.OwnerDocument.CreateTextNode(value);
                attributeElement.AppendChild(valueText);
            }
            return attributeElement;
        }

        public static XmlElement XslApplyTemplates(this XmlElement element, XPathExpr selectXPath, params TemplateParameter[] parameters)
        {
            return XslApplyTemplates(element, selectXPath, (IEnumerable<TemplateParameter>)parameters);
        }

        private static XmlElement XslApplyTemplates(this XmlElement element, XPathExpr selectXPath, IEnumerable<TemplateParameter> parameters)
        {
            XmlElement applyTemplatesElement = element.XslElement("apply-templates");
            applyTemplatesElement.AddAttributeWithValue("select", selectXPath);
            if (parameters != null)
                foreach (TemplateParameter keyValuePair in parameters)
                {
                    XmlElement param = applyTemplatesElement.XslElement("with-param");
                    param.AddAttributeWithValue("name", keyValuePair.Name);
                    param.AddAttributeWithValue("select", keyValuePair.Value);
                }
            return applyTemplatesElement;
        }

        public static XmlElement XslCopy(this XmlElement element)
        {
            return element.XslElement("copy");
        }

        public static XmlElement XslCopyOf(this XmlElement element, XPathExpr selectXPath)
        {
            XmlElement copyOfElement = element.XslElement("copy-of");
            copyOfElement.AddAttributeWithValue("select", selectXPath);
            return copyOfElement;
        }

        public static XmlElement XslForEach(this XmlElement element, XPathExpr selectXPath)
        {
            XmlElement forEachElement = element.XslElement("for-each");
            forEachElement.AddAttributeWithValue("select", selectXPath);
            return forEachElement;
        }

        public static XmlElement XslForEachGroup(this XmlElement element, XPathExpr selectXPath)
        {
            XmlElement copyOfElement = element.XslElement("for-each-group");
            copyOfElement.AddAttributeWithValue("select", selectXPath);
            return copyOfElement;
        }

        public static XmlElement XslValueOf(this XmlElement element, XPathExpr selectXPath)
        {
            XmlElement valueOfElement = element.XslElement("value-of");
            valueOfElement.AddAttributeWithValue("select", selectXPath);
            return valueOfElement;
        }

        public static XmlElement XslVariable(this XmlElement element, string name, XPathExpr selectXPath)
        {
            XmlElement variableElement = element.XslElement("variable");
            variableElement.AddAttributeWithValue("name", name);
            if (selectXPath != null)
                variableElement.AddAttributeWithValue("select", selectXPath);
            return variableElement;
        }

        public static XmlElement XslIf(this XmlElement element, XPathExpr test)
        {
            XmlElement valueOfElement = element.XslElement("if");
            valueOfElement.AddAttributeWithValue("test", test);
            return valueOfElement;
        }

        public static XmlElement XslChoose(this XmlElement element)
        {
            XmlElement choose = element.XslElement("choose");
            return choose;
        }

        public static XmlElement XslWhen(this XmlElement element, XPathExpr test)
        {
            XmlElement option = element.XslElement("when");
            option.AddAttributeWithValue("test", test);
            return option;
        }

        public static XmlElement XslOtherwise(this XmlElement element)
        {
            XmlElement otherwise = element.XslElement("otherwise");
            return otherwise;
        }

        private static string getSupportingTemplateName(string nameBase, bool groupAware, IEnumerable<TemplateParameter> parameters)
        {
            string template = nameBase;
            if (groupAware)
                template += "GroupAware";
            if (parameters.Any(p => p.Name == ParamOnly))
                template += "Explicit";
            return template;
        }

        public static void CallCopyAttributes(this XmlElement element, bool groupAware, params TemplateParameter[] parameters)
        {
            CallCopyAttributes(element, groupAware, (IEnumerable<TemplateParameter>)parameters);
        }

        public static void CallCopyAttributes(this XmlElement element, bool groupAware, IEnumerable<TemplateParameter> parameters)
        {
            Debug.Assert(!groupAware || parameters.Any(kvp => kvp.Name == ParamCurrentGroup));
            string template = getSupportingTemplateName("copyAttributes", groupAware, parameters);
            element.XslCallTemplate(template, parameters);
        }

        public static void CallCopyContent(this XmlElement element, bool groupAware, params TemplateParameter[] parameters)
        {
            Debug.Assert(!groupAware || parameters.Any(kvp => kvp.Name == ParamCurrentGroup));
            string template = getSupportingTemplateName("copyContent", groupAware, parameters);
            element.XslCallTemplate(template, parameters);
        }

        public static void CallProcessContent(this XmlElement element, bool groupAware, params TemplateParameter[] parameters)
        {
            Debug.Assert(!groupAware || parameters.Any(kvp => kvp.Name == ParamOnly));
            string template = getSupportingTemplateName("processContent", groupAware, parameters);
            element.XslCallTemplate(template, parameters);
        }

        public static void AddFallbackRules(this XmlElement element)
        {
            XmlElement templateElement = element.XslTemplate(new XPathExpr("* | text() | @*"));
            XmlText textNode = templateElement.OwnerDocument.CreateTextNode("NO MATCHING RULE! (");
            templateElement.AppendChild(textNode);
            templateElement.AppendChild(templateElement.XslValueOf(new XPathExpr("name()")));
            textNode = templateElement.OwnerDocument.CreateTextNode("). ");
            templateElement.AppendChild(textNode);   
        }

        public const string ParamExcept = "except";

        public const string ParamOnly = "only";

        public const string ParamCurrentGroup = "cg";
        
        public const string ParamAttributes = "attributes";

        public static void AddCopyAttributesTemplate(this XmlElement element)
        {
            XmlElement namedTemplateElement = element.XslNamedTemplate("copyAttributes", ParamExcept, ParamOnly);
            XmlElement choose = namedTemplateElement.XslChoose();
            XmlElement opt1 = choose.XslWhen(new XPathExpr("$except and $only"));
            opt1.CreateTextElement("ERROR: parameters EXCEPT and ONLY can not be used together");
            XmlElement opt2 = choose.XslWhen(new XPathExpr("$except"));
            opt2.XslCopyOf(new XPathExpr("./@* except ($except)"));
            XmlElement opt3 = choose.XslWhen(new XPathExpr("$only"));
            opt3.XslCopyOf(new XPathExpr("$only"));
            XmlElement otherwise = choose.XslOtherwise();
            otherwise.XslCopyOf(new XPathExpr("./@*"));

            XmlElement namedTemplateElement2 = element.XslNamedTemplate("copyAttributesExplicit", ParamOnly);
            namedTemplateElement2.XslCopyOf(new XPathExpr("$only"));
        }

        public static void AddCopyAttributesTemplateGroupAware(this XmlElement element)
        {
            XmlElement namedTemplateElement = element.XslNamedTemplate("copyAttributesGroupAware", ParamExcept, ParamOnly, ParamCurrentGroup);
            XmlElement choose = namedTemplateElement.XslChoose();
            XmlElement opt1 = choose.XslWhen(new XPathExpr("$except and $only"));
            opt1.CreateTextElement("ERROR: parameters EXCEPT and ONLY can not be used together");
            XmlElement opt2 = choose.XslWhen(new XPathExpr("$except"));
            opt2.XslCopyOf(new XPathExpr("$cg[self::attribute()] except ($except)"));
            XmlElement opt3 = choose.XslWhen(new XPathExpr("$only"));
            opt3.XslCopyOf(new XPathExpr("$only"));
            XmlElement otherwise = choose.XslOtherwise();
            otherwise.XslCopyOf(new XPathExpr("$cg[self::attribute()]"));

            XmlElement namedTemplateElement2 = element.XslNamedTemplate("copyAttributesGroupAwareExplicit", ParamOnly, ParamCurrentGroup);
            namedTemplateElement2.XslCopyOf(new XPathExpr("$only"));
        }

        public static void AddCopyContentTemplate(this XmlElement element)
        {
            XmlElement namedTemplateElement = element.XslNamedTemplate("copyContent", ParamExcept, ParamOnly);
            XmlElement choose = namedTemplateElement.XslChoose();
            XmlElement opt1 = choose.XslWhen(new XPathExpr("$except and $only"));
            opt1.CreateTextElement("ERROR: parameters EXCEPT and ONLY can not be used together");
            XmlElement opt2 = choose.XslWhen(new XPathExpr("$except"));
            opt2.XslCopyOf(new XPathExpr("./* except ($except)"));
            XmlElement opt3 = choose.XslWhen(new XPathExpr("$only"));
            opt3.XslCopyOf(new XPathExpr("$only"));
            XmlElement otherwise = choose.XslOtherwise();
            otherwise.XslCopyOf(new XPathExpr("./*"));

            XmlElement namedTemplateElement2 = element.XslNamedTemplate("copyContentExplicit", ParamOnly);
            namedTemplateElement2.XslCopyOf(new XPathExpr("$only"));
        }

        public static void AddCopyContentTemplateGroupAware(this XmlElement element)
        {
            XmlElement namedTemplateElement = element.XslNamedTemplate("copyContentGroupAware", ParamExcept, ParamOnly, ParamCurrentGroup);
            XmlElement choose = namedTemplateElement.XslChoose();
            XmlElement opt1 = choose.XslWhen(new XPathExpr("$except and $only"));
            opt1.CreateTextElement("ERROR: parameters EXCEPT and ONLY can not be used together");
            XmlElement opt2 = choose.XslWhen(new XPathExpr("$except"));
            opt2.XslCopyOf(new XPathExpr("$cg[self::element()] except ($except)"));
            XmlElement opt3 = choose.XslWhen(new XPathExpr("$only"));
            opt3.XslCopyOf(new XPathExpr("$only"));
            XmlElement otherwise = choose.XslOtherwise();
            otherwise.XslCopyOf(new XPathExpr("$cg[self::element()]"));

            XmlElement namedTemplateElement2 = element.XslNamedTemplate("copyContentGroupAwareExplicit", ParamOnly, ParamCurrentGroup);
            namedTemplateElement2.XslCopyOf(new XPathExpr("$only"));
        }

        public static void AddProcessContentTemplate(this XmlElement element)
        {
            XmlElement namedTemplateElement = element.XslNamedTemplate("processContent", ParamExcept, ParamOnly);
            XmlElement choose = namedTemplateElement.XslChoose();
            XmlElement opt1 = choose.XslWhen(new XPathExpr("$except and $only"));
            opt1.CreateTextElement("ERROR: parameters EXCEPT and ONLY can not be used together");
            XmlElement opt2 = choose.XslWhen(new XPathExpr("$except"));
            opt2.XslApplyTemplates(new XPathExpr("./* except ($except)"));
            XmlElement opt3 = choose.XslWhen(new XPathExpr("$only"));
            opt3.XslApplyTemplates(new XPathExpr("$only"));
            XmlElement otherwise = choose.XslOtherwise();
            otherwise.XslApplyTemplates(new XPathExpr("./*"));

            XmlElement namedTemplateElement2 = element.XslNamedTemplate("processContentExplicit", ParamOnly);
            namedTemplateElement2.XslApplyTemplates(new XPathExpr("$only"));
        }

        public static void AddProcessContentTemplateGroupAware(this XmlElement element)
        {
            XmlElement namedTemplateElement = element.XslNamedTemplate("processContentGroupAware", ParamExcept, ParamOnly, ParamCurrentGroup);
            XmlElement choose = namedTemplateElement.XslChoose();
            XmlElement opt1 = choose.XslWhen(new XPathExpr("$except and $only"));
            opt1.CreateTextElement("ERROR: parameters EXCEPT and ONLY can not be used together");
            XmlElement opt2 = choose.XslWhen(new XPathExpr("$except"));
            opt2.XslApplyTemplates(new XPathExpr("$cg[self::element()] except ($except)"));
            XmlElement opt3 = choose.XslWhen(new XPathExpr("$only"));
            opt3.XslApplyTemplates(new XPathExpr("$only"));
            XmlElement otherwise = choose.XslOtherwise();
            otherwise.XslApplyTemplates(new XPathExpr("$cg[self::element()]"));

            XmlElement namedTemplateElement2 = element.XslNamedTemplate("processContentGroupAwareExplicit", ParamOnly, ParamCurrentGroup);
            namedTemplateElement2.XslApplyTemplates(new XPathExpr("$only"));
        }
    }

    public struct TemplateParameter
    {
        public string Name { get; set; }
        public XPathExpr Value { get; set; }

        public TemplateParameter(string name, XPathExpr value) : this()
        {
            Name = name;
            Value = value;
        }
    }
}