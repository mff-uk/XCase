using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using OwlDotNetApi;
using Microsoft.Win32;
using XCase.Model;
using XCase.Controller;
using System.Reflection;

namespace XCase.SemanticWS
{
    public class ImportOWL
    {
        class Assoc
        {
            public string id, from, to;
        }
        class Attr
        {
            public string id, owner, type;
        }

        DiagramController controller;

        public ImportOWL(DiagramController C)
        {
            controller = C;
        }

        private string CutID(string FullID)
        {
            return FullID == null? null : FullID.Substring(FullID.LastIndexOf('#') + 1);
        }
        
        public void OWLtoPIM()
        {
            Dictionary<string, Assoc> Assocs = new Dictionary<string, Assoc>();
            Dictionary<string, Attr> Attrs = new Dictionary<string, Attr>();
            Dictionary<string, PIMClass> Classes = new Dictionary<string, PIMClass>();
            Dictionary<string, Property> Attributes = new Dictionary<string, Property>();
            Dictionary<string, Association> Associations = new Dictionary<string, Association>();
            List<Generalization> Generalizations = new List<Generalization>();
            List<Comment> Comments = new List<Comment>();

            OpenFileDialog D = new OpenFileDialog();
            D.Filter = "OWL File|*.owl|All files|*.*";
            D.Title = "Select OWL file to import";
            D.CheckFileExists = true;
            if (D.ShowDialog() != true) return;
            IOwlParser parser = new OwlXmlParser();

            IOwlGraph graph = parser.ParseOwl(D.FileName);
            ArrayList errors = ((OwlParser)parser).Errors;
            ArrayList warnings = ((OwlParser)parser).Warnings;
            ArrayList messages = ((OwlParser)parser).Messages;

            /*IDictionaryEnumerator nEnumerator = (IDictionaryEnumerator)graph.Nodes.GetEnumerator();
            while (nEnumerator.MoveNext())
            {
                OwlNode node = (OwlNode)graph.Nodes[(nEnumerator.Key).ToString()];
                IOwlClass C = node as IOwlClass;
                if (C == null || C is IOwlRestriction) continue;
                PIMClass pimClass = controller.ModelController.Model.AddClass() as PIMClass;
                //TODO: Catch duplicit IDs
                Classes.Add(C.ID, pimClass);
                pimClass.Name = CutID(C.ID);
                pimClass.OntologyEquivalent = C.ID;
            }*/

            OwlEdgeList TypeEdges = (OwlEdgeList)graph.Edges["http://www.w3.org/1999/02/22-rdf-syntax-ns#type"];
            foreach (OwlEdge E in TypeEdges)
            {
                if (E.ChildNode.ID == "http://www.w3.org/2002/07/owl#ObjectProperty" && !Assocs.ContainsKey(E.ParentNode.ID))
                    Assocs.Add(E.ParentNode.ID, new Assoc() { id = E.ParentNode.ID });
                else if (E.ChildNode.ID == "http://www.w3.org/2002/07/owl#DatatypeProperty" && !Attrs.ContainsKey(E.ParentNode.ID))
                    Attrs.Add(E.ParentNode.ID, new Attr() { id = E.ParentNode.ID });
                else if (E.ChildNode.ID == "http://www.w3.org/2002/07/owl#Class")
                {
                    if (!Classes.ContainsKey(E.ParentNode.ID))
                    {
                        PIMClass pimClass = controller.ModelController.Model.AddClass() as PIMClass;
                        Classes.Add(E.ParentNode.ID, pimClass);
                        pimClass.Name = CutID(E.ParentNode.ID);
                        pimClass.OntologyEquivalent = E.ParentNode.ID;
                        if (E.ParentNode.ID.IndexOf('#') != -1) controller.Project.Schema.XMLNamespace = E.ParentNode.ID.Substring(0, E.ParentNode.ID.IndexOf('#'));
                    }
                }
            }
            OwlEdgeList DomainEdges = (OwlEdgeList)graph.Edges["http://www.w3.org/2000/01/rdf-schema#domain"];
            foreach (OwlEdge E in DomainEdges)
            {
                if (Assocs.ContainsKey(E.ParentNode.ID)) Assocs[E.ParentNode.ID].from = E.ChildNode.ID;
                if (Attrs.ContainsKey(E.ParentNode.ID)) Attrs[E.ParentNode.ID].owner = E.ChildNode.ID;
            }
            OwlEdgeList RangeEdges = (OwlEdgeList)graph.Edges["http://www.w3.org/2000/01/rdf-schema#range"];
            foreach (OwlEdge E in RangeEdges)
            {
                if (Assocs.ContainsKey(E.ParentNode.ID)) Assocs[E.ParentNode.ID].to = E.ChildNode.ID;
                if (Attrs.ContainsKey(E.ParentNode.ID)) Attrs[E.ParentNode.ID].type = E.ChildNode.ID;
            }

            Report R = new Report();

            foreach (Attr A in Attrs.Values)
            {
                if (A.owner == null)
                {
                    R.lb1.Items.Add("Attribute " + A.id + " doesn't have an owner.");
                    
                    continue;
                }
                if (!Classes.ContainsKey(A.owner))
                {
                    R.lb1.Items.Add("Attribute " + A.id + ": Owner " + A.owner + " not found.");

                    continue;
                }
                Property P = Classes[A.owner].AddAttribute();
                P.OntologyEquivalent = A.id;
                Attributes.Add(A.id, P);
                P.Name = CutID(A.id);
                P.Default = CutID(A.type);
            }

            foreach (Assoc A in Assocs.Values)
            {
                if (A.from == null || A.to == null)
                {
                    R.lb2.Items.Add("Association " + A.id + ": doesn't have from or to.");
                    continue;
                }
                List<PIMClass> L = new List<PIMClass>();
                if (Classes.ContainsKey(A.from))
                    L.Add(Classes[A.from]);
                else
                {
                    R.lb2.Items.Add("Association " + A.id + ": From: " + A.from + " doesn't exist.");
                    continue;
                }
                if (Classes.ContainsKey(A.to))
                    L.Add(Classes[A.to]);
                else
                {
                    R.lb2.Items.Add("Association " + A.id + ": To: " + A.to + " doesn't exist.");
                    continue;
                }
                Association Assoc = controller.ModelController.Model.Schema.AssociateClasses(L);
                Assoc.OntologyEquivalent = A.id;
                Assoc.Name = CutID(A.id);
                Associations.Add(A.id, Assoc);
            }
            OwlEdgeList SubClassEdges = (OwlEdgeList)graph.Edges["http://www.w3.org/2000/01/rdf-schema#subClassOf"];
            foreach (OwlEdge E in SubClassEdges)
            {
                if (Classes.ContainsKey(E.ParentNode.ID) && Classes.ContainsKey(E.ChildNode.ID))
                {
                    Generalizations.Add(controller.ModelController.Model.Schema.SetGeneralization(Classes[E.ParentNode.ID], Classes[E.ChildNode.ID]));
                }
                else if (!Classes.ContainsKey(E.ChildNode.ID))
                {
                    R.lb2.Items.Add(E.ParentNode.ID + " subclassOf " + E.ChildNode.ID + ": Child doesn't exist.");
                    continue;
                }
                else if (!Classes.ContainsKey(E.ParentNode.ID))
                {
                    R.lb2.Items.Add(E.ParentNode.ID + " subclassOf " + E.ChildNode.ID + ": Parent doesn't exist.");
                    continue;
                }
            }
            OwlEdgeList CommentEdges = (OwlEdgeList)graph.Edges["http://www.w3.org/2000/01/rdf-schema#comment"];
            if (CommentEdges != null) foreach (OwlEdge E in CommentEdges)
            {
                if (Classes.ContainsKey(E.ParentNode.ID))
                {
                    Comments.Add(Classes[E.ParentNode.ID].AddComment(E.ChildNode.ID));
                }
                else if (Associations.ContainsKey(E.ParentNode.ID))
                {
                    Comments.Add(Associations[E.ParentNode.ID].AddComment(E.ChildNode.ID));
                }
                else if (Attributes.ContainsKey(E.ParentNode.ID))
                {
                    R.lb2.Items.Add("Comment of " + E.ParentNode.ID + ": XCase doesn't support attribute comments.");
                }
                else
                {
                    R.lb2.Items.Add("Comment of " + E.ParentNode.ID + ": Class/Association doesn't exist.");
                    continue;
                }
            }

            int i = 0;
            foreach (PIMClass pimClass in Classes.Values)
            {
                ClassViewHelper VH = new ClassViewHelper(controller.Diagram);
                double rows = Math.Abs(Math.Sqrt(Classes.Count)) + 1;

                VH.X = (i * 200 + 10) % ((int)rows * 200 + 10);
                VH.Y = (i / (int)rows) * 200 + 10;
                controller.Diagram.AddModelElement(pimClass, VH);
                i++;
            }
            foreach (Association A in Associations.Values)
            {
                AssociationViewHelper VH = new AssociationViewHelper(controller.Diagram);
                controller.Diagram.AddModelElement(A, VH);
            }
            foreach (Generalization G in Generalizations)
            {
                GeneralizationViewHelper VH = new GeneralizationViewHelper(controller.Diagram);
                controller.Diagram.AddModelElement(G, VH);
            }
            foreach (Comment C in Comments)
            {
                CommentViewHelper VH = new CommentViewHelper(controller.Diagram);
                controller.Diagram.AddModelElement(C, VH);
            }

            R.Show();

        }
    }
}
