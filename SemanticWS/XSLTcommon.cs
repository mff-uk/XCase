using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Controller;
using System.Xml.Linq;
using XCase.Model;

namespace XCase.SemanticWS
{
    abstract public class XSLTcommon
    {
        protected DiagramController controller;

        protected XNamespace nsrdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        protected XNamespace nsrdfs = "http://www.w3.org/2000/01/rdf-schema#";
        protected XNamespace nsxsl = "http://www.w3.org/1999/XSL/Transform";
        protected XNamespace nsxs = "http://www.w3.org/2001/XMLSchema";

        protected XNamespace nsproject;

        protected XElement Stylesheet;
        protected XDocument Stylesheet_doc = new XDocument();
        protected Dictionary<XNamespace, string> Namespaces = new Dictionary<XNamespace, string>();

        protected List<string> templateNames = new List<string>();
        
        protected XSLTcommon(DiagramController C)
        {
            controller = C;
            nsproject = controller.Project.Schema.XMLNamespaceOrDefaultNamespace;
            Namespaces.Add(nsrdf, "rdf");
            Namespaces.Add(nsrdfs, "rdfs");
            Namespaces.Add(nsxsl, "xsl");
            Namespaces.Add(nsxs, "xs");
            Namespaces.Add(nsproject, "d");

            Stylesheet = new XElement(nsxsl + "stylesheet",
                                       new XAttribute("version", "1.0"));
            Stylesheet_doc.Add(Stylesheet);
            Stylesheet.Add(new XElement(nsxsl + "output",
                            new XAttribute("method", "xml"),
                            new XAttribute("version", "1.0"),
                            new XAttribute("encoding", "UTF-8"),
                            new XAttribute("indent", "yes")));
        }

        protected XNamespace GetNamespace(string s)
        {
            int i = s.IndexOf('#');
            if (i == -1)
            {
                if (!Namespaces.ContainsKey(XNamespace.None))
                {
                    Namespaces.Add(XNamespace.None, NameSuggestor<string>.SuggestUniqueName(Namespaces.Values, "ns", S => S));
                }
                return XNamespace.None;
            }
            else
            {
                string sub = s.Substring(0, i);
                if (!Namespaces.ContainsKey(XNamespace.Get(sub)))
                {
                    Namespaces.Add(XNamespace.Get(sub), NameSuggestor<string>.SuggestUniqueName(Namespaces.Values, "ns", S => S));
                }
                return XNamespace.Get(sub);
            }
        }

        protected string CutID(string s)
        {
            if (s.IndexOf('#') == -1) return s;
            else return s.Substring(s.IndexOf('#') + 1);
        }

    }
}
