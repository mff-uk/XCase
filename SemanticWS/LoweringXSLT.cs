using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Controller;
using Microsoft.Win32;
using XCase.Model;
using System.Xml.Linq;
using System.Xml;

namespace XCase.SemanticWS
{
    public class LoweringXSLT : XSLTcommon
    {
        XElement rootTemplate;
        
        public LoweringXSLT(DiagramController C) : base(C)
        {
            rootTemplate = new XElement(nsxsl + "template",
                            new XAttribute("match", "/" + Namespaces[nsrdf] + ":RDF"));
            Stylesheet.Add(rootTemplate);
        }

        protected string GetTemplateName(string prefix)
        {
            string returns;
            XNamespace ns = GetNamespace(prefix);
            string id = CutID(prefix);
            if (NameSuggestor<string>.IsNameUnique(templateNames, Namespaces[ns] + ":" + id, S => S)) returns = Namespaces[ns] + ":" + id;
            else returns = NameSuggestor<string>.SuggestUniqueName(templateNames, Namespaces[ns] + ":" + id, S => S);
            templateNames.Add(returns);
            return returns;
        }

        public void GenerateLoweringXSLT()
        {
            SaveFileDialog D = new SaveFileDialog();
            D.Filter = "XSLT Stylesheet|*.xslt";
            D.CheckPathExists = true;
            D.Title = "Save Lowering XSLT as...";
            if (D.ShowDialog() != true) return;

            foreach (PSMSuperordinateComponent C in (controller.Diagram as PSMDiagram).Roots)
            {
                if (C is PSMClass)
                {
                    string XPath = Namespaces[nsrdf] + ":Description[descendant::" + Namespaces[nsrdfs] + ":Class[@" + Namespaces[nsrdf] + ":resource='" + (C as PSMClass).RepresentedClass.OntologyEquivalent + "'] and not(@" + Namespaces[nsrdf] + ":about = //@" + Namespaces[nsrdf] + ":resource)]/@" + Namespaces[nsrdf] + ":about";
                    string newname = GetTemplateName((C as PSMClass).RepresentedClass.OntologyEquivalent);
                    rootTemplate.Add(new XElement(nsxsl + "call-template", 
                                        new XAttribute("name", newname),
                                        new XElement(nsxsl + "with-param",
                                            new XAttribute("name", "id"),
                                            new XAttribute("select", XPath))));
                    ProcessClass(C as PSMClass, newname);
                }
            }

            foreach (KeyValuePair<XNamespace, string> P in Namespaces)
            {
                Stylesheet.Add(new XAttribute(XNamespace.Xmlns + P.Value, P.Key.NamespaceName));
            }

            Stylesheet_doc.Save(D.FileName);
        }

        void ProcessClass(PSMClass C, string templateName)
        {
            //Presumptions:
            // - every PIM class and PIM associations have ontology-equivalents
            // - PSM associations are mapped to single PIM associations
            // - only PSM Classes, attributes, associations
            // - namespace in id separated by #
            // - pim-less attribute name/alias is "nice" - can be appended to a namespace URI to form another URI
            // - one root

            string XPath = "//" + Namespaces[nsrdf] + ":Description[@" + Namespaces[nsrdf] + ":about=$id]";
            XElement element = new XElement(nsproject + C.ElementName);
            Stylesheet.Add(new XElement(nsxsl + "template", 
                             new XAttribute("name", templateName),
                             new XElement(nsxsl + "param",
                                 new XAttribute("name", "id")),
                             new XElement(nsxsl + "for-each", 
                                 new XAttribute("select", XPath), element)));

            #region Attribute rules generation
            foreach (PSMAttribute A in C.PSMAttributes)
            {
                XNamespace ns;
                string id;
                if (A.RepresentedAttribute == null)
                {
                    ns = nsproject;
                    id = A.AliasOrName;
                }
                else
                {
                    ns = GetNamespace(A.RepresentedAttribute.OntologyEquivalent);
                    id = CutID(A.RepresentedAttribute.OntologyEquivalent);
                }
                element.Add(new XElement(nsxsl + "attribute",
                               new XAttribute("name", Namespaces[nsproject] + ":" + A.AliasOrName),
                               new XElement(nsxsl + "value-of",
                                   new XAttribute("select", Namespaces[ns] + ":" + id))));
            }
            #endregion

            #region Content rules generation

            foreach (PSMSubordinateComponent S in C.Components)
            {
                if (S is PSMAssociation)
                {
                    PSMAssociation A = S as PSMAssociation;
                    if (A.Child is PSMClass)
                    {
                        PSMClass child = A.Child as PSMClass;
                        
                        string newTemplateName = GetTemplateName(child.RepresentedClass.OntologyEquivalent);

                        element.Add(new XElement(nsxsl + "for-each",
                                        new XAttribute("select", Namespaces[GetNamespace(A.NestingJoins[0].Parent.Steps[0].Association.OntologyEquivalent)] + ":" + CutID(A.NestingJoins[0].Parent.Steps[0].Association.OntologyEquivalent)),
                                        new XElement(nsxsl + "call-template",
                                            new XAttribute("name", newTemplateName),
                                            new XElement(nsxsl + "with-param",
                                                new XAttribute("name", "id"),
                                                new XAttribute("select", "@" + Namespaces[nsrdf] + ":resource")))));

                        ProcessClass(child, newTemplateName);
                    }
                }
            }

            #endregion
        }
    }
}
