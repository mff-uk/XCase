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
    public class LiftingXSLT : XSLTcommon
    {
        XElement RDFElement;
        
        public LiftingXSLT(DiagramController C) : base(C)
        {
            RDFElement = new XElement(nsrdf + "RDF");

            Stylesheet.Add(new XElement(nsxsl + "template",
                            new XAttribute("match", "/"),
                            RDFElement));
        }

        public void GenerateLiftingXSLT()
        {
            SaveFileDialog D = new SaveFileDialog();
            D.Filter = "XSLT Stylesheet|*.xslt";
            D.CheckPathExists = true;
            D.Title = "Save Lifting XSLT as...";
            if (D.ShowDialog() != true) return;

            foreach (PSMSuperordinateComponent C in (controller.Diagram as PSMDiagram).Roots)
            {
                if (C is PSMClass)
                {
                    string newname = GetTemplateName((C as PSMClass).ElementName);
                    RDFElement.Add(new XElement(nsxsl + "for-each",
                                    new XAttribute("select", Namespaces[nsproject] + ":" + (C as PSMClass).ElementName),
                                    new XElement(nsxsl + "call-template",
                                        new XAttribute("name", newname))));
                    ProcessClass(C as PSMClass, newname);
                }
            }

            foreach(KeyValuePair<XNamespace, string> P in Namespaces)
            {
                Stylesheet.Add(new XAttribute(XNamespace.Xmlns + P.Value, P.Key.NamespaceName));
            }
            
            Stylesheet_doc.Save(D.FileName);
        }

        protected string GetTemplateName(string name)
        {
            string returns;
            if (NameSuggestor<string>.IsNameUnique(templateNames, name, S => S)) returns = name;
            else returns = NameSuggestor<string>.SuggestUniqueName(templateNames, name , S => S);
            templateNames.Add(returns);
            return returns;
        }

        void ProcessClass(PSMClass C, string templateName)
        {
            //Presumptions:
            // - every PIM class and PIM association has an ontology-equivalent
            // - PSM associations are mapped to single PIM associations
            // - only PSM Classes, attributes, associations
            // - namespace in id separated by #
            // - each instance of a class has an id attribute
            // - pim-less attribute name/alias is "nice" - can be appended to a namespace URI to form another URI
            
            #region Generation of RDF class header
            XElement rdfDescription = new XElement(nsrdf + "Description");
            XElement template = new XElement(nsxsl + "template",
                new XAttribute("name", templateName),
                rdfDescription);
            Stylesheet.Add(template);

            rdfDescription.Add(new XElement(nsxsl + "attribute",
                                new XAttribute("name", Namespaces[nsrdf] + ":about"), 
                                new XElement(nsxsl + "value-of", 
                                  new XAttribute("select", "concat('" + C.RepresentedClass.OntologyEquivalent + "', @id)"))));

            rdfDescription.Add(new XElement(nsrdfs + "Class",
                                new XElement(nsxsl + "attribute",
                                  new XAttribute("name", Namespaces[nsrdf] + ":resource"),
                                  new XElement(nsxsl + "text", C.RepresentedClass.OntologyEquivalent))));
            #endregion

            #region Generation of attribute transformation

            foreach (PSMAttribute A in C.PSMAttributes)
            {
                rdfDescription.Add(new XElement(
                                                A.RepresentedAttribute == null
                                                ? nsproject + A.AliasOrName
                                                : GetNamespace(A.RepresentedAttribute.OntologyEquivalent) + CutID(A.RepresentedAttribute.OntologyEquivalent),
                                    new XElement(nsxsl + "value-of",
                                        new XAttribute("select", "@" + A.AliasOrName))));
            }

            #endregion

            #region Generation of content transformation

            foreach (PSMSubordinateComponent S in C.Components)
            {
                if (S is PSMAssociation)
                {
                    PSMAssociation A = S as PSMAssociation;
                    if (A.Child is PSMClass)
                    {
                        PSMClass child = A.Child as PSMClass;
                        string newRefName = GetTemplateName(child.ElementName + "-ref");
                        rdfDescription.Add(new XElement(nsxsl + "for-each",
                                            new XAttribute("select", Namespaces[nsproject] + ":" + child.ElementName),
                                            new XElement(nsxsl + "call-template",
                                                new XAttribute("name", newRefName))));

                        string newname = GetTemplateName(child.ElementName);
                        Stylesheet.Add(new XElement(nsxsl + "template",
                                        new XAttribute("name", newRefName),
                                        new XElement(GetNamespace(A.NestingJoins[0].Parent.Steps[0].Association.OntologyEquivalent) + CutID(A.NestingJoins[0].Parent.Steps[0].Association.OntologyEquivalent),
                                            new XElement(nsxsl + "attribute",
                                                new XAttribute("name", "rdf:resource"),
                                                new XElement(nsxsl + "value-of",
                                                    new XAttribute("select", "concat('" + child.RepresentedClass.OntologyEquivalent + "', @id)"))))));

                        template.Add(new XElement(nsxsl + "for-each",
                                        new XAttribute("select", Namespaces[nsproject] + ":" + child.ElementName),
                                        new XElement(nsxsl + "call-template",
                                            new XAttribute("name", newname))));

                        ProcessClass(child, newname);
                    }
                }
            }

            #endregion
        }
    }
}
