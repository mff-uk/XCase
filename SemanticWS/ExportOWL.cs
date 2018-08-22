using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Controller;
using Microsoft.Win32;
using System.Xml.Linq;
using XCase.Model;

namespace XCase.SemanticWS
{
    public class ExportOWL
    {
        DiagramController controller;

        XNamespace nsrdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        XNamespace nsowl = "http://www.w3.org/2002/07/owl#";
        XNamespace nsrdfs = "http://www.w3.org/2000/01/rdf-schema#";
        XNamespace nsxs = "http://www.w3.org/2001/XMLSchema#";
        XNamespace nsproject;

        Dictionary<XNamespace, string> Namespaces = new Dictionary<XNamespace, string>();

        XDocument doc = new XDocument();
        XElement rdfElement;

        public ExportOWL(DiagramController C)
        {
            controller = C;
            nsproject = controller.Project.Schema.XMLNamespaceOrDefaultNamespace;

            Namespaces.Add(nsrdf, "rdf");
            Namespaces.Add(nsrdfs, "rdfs");
            Namespaces.Add(nsowl, "owl");
            Namespaces.Add(nsxs, "xs");

            rdfElement = new XElement(nsrdf + "RDF", new XElement(nsowl + "Ontology", new XAttribute(nsrdf + "about", nsproject.NamespaceName)));
            doc.Add(rdfElement);
        }

        string GetName(NamedElement E)
        {
            if (E.OntologyEquivalent == null || E.OntologyEquivalent.Length == 0)
            {
                return E.OntologyEquivalent = nsproject.NamespaceName + '#' + E.Name;
            }
            else
            {
                return E.OntologyEquivalent;
            }
        }

        string CutID(string s)
        {
            if (s.IndexOf('#') == -1) return s;
            else return s.Substring(s.IndexOf('#') + 1);
        }
        
        public void PIMtoOWL()
        {
            //Presumtions
            // - Class, Association and Attribute names are "nice" - can be URI
            
            SaveFileDialog D = new SaveFileDialog();
            D.Filter = "OWL File|*.owl";
            D.Title = "Save OWL file as...";
            D.CheckPathExists = true;
            if (D.ShowDialog() != true) return;
            Model.Model M = controller.ModelController.Model;

            #region Class processing
            foreach (PIMClass C in M.Classes)
            {
                XElement ClassElement = new XElement(nsowl + "Class",
                                new XAttribute(nsrdf + "about", GetName(C)),
                                new XElement(nsrdfs + "label", C.Name));

                foreach (Generalization G in C.Generalizations)
                {
                    ClassElement.Add(new XElement(nsrdfs + "subClassOf",
                                        new XAttribute(nsrdf + "resource", GetName(G.General))));
                }

                foreach (Property P in C.Attributes)
                {
                    if (P.Type == null)
                    {
                        rdfElement.Add(new XElement(nsowl + "DatatypeProperty",
                                          new XAttribute(nsrdf + "about", GetName(P)),
                                          new XElement(nsrdfs + "domain",
                                              new XAttribute(nsrdf + "resource", GetName(C)))));
                    }
                    else
                    {
                        rdfElement.Add(new XElement(nsowl + "DatatypeProperty",
                                          new XAttribute(nsrdf + "about", GetName(P)),
                                          new XElement(nsrdfs + "domain",
                                              new XAttribute(nsrdf + "resource", GetName(C))),
                                          new XElement(nsrdfs + "range",
                                              new XAttribute(nsrdf + "resource", nsxs.NamespaceName + P.Type.Name))));
                    }
                }
                rdfElement.Add(ClassElement);
            }
            #endregion

            #region Association Processing
            foreach (Association A in M.Associations)
            {
                rdfElement.Add(new XElement(nsowl + "ObjectProperty",
                                new XAttribute(nsrdf + "about", GetName(A) + "_1"),
                                new XElement(nsrdfs + "domain",
                                    new XAttribute(nsrdf + "resource", GetName(A.Ends[0].Class))),
                                new XElement(nsrdfs + "range",
                                    new XAttribute(nsrdf + "resource", GetName(A.Ends[1].Class)))));
                rdfElement.Add(new XElement(nsowl + "ObjectProperty",
                                new XAttribute(nsrdf + "about", GetName(A) + "_2"),
                                new XElement(nsrdfs + "range",
                                    new XAttribute(nsrdf + "resource", GetName(A.Ends[0].Class))),
                                new XElement(nsrdfs + "domain",
                                    new XAttribute(nsrdf + "resource", GetName(A.Ends[1].Class)))));
            }
            #endregion

            foreach (KeyValuePair<XNamespace, string> P in Namespaces)
            {
                rdfElement.Add(new XAttribute(XNamespace.Xmlns + P.Value, P.Key.NamespaceName));
            }

            rdfElement.Add(new XAttribute("xmlns", nsproject.NamespaceName));
            doc.Save(D.FileName);
        }
    }
}
