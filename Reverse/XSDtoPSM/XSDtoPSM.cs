using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using System.Xml;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System.Windows;

namespace XCase.Reverse
{
    public class XSDtoPSM
    {
        public TextBlock t { get; set; }

        public Label l { get; set; }
        public ProgressBar p { get; set; }
        public bool ResolveSRs = true;
        public bool DeleteUnnecessarySRsMadeByExtensions = true;

        public int ClassesCount = 0;

        //public StreamWriter SW = new StreamWriter("D:\\log.txt");

        public static void ValidationCallbackOne(object sender, ValidationEventArgs args)
        {
            //TODO: Something better
            Console.WriteLine(args.Message);
        }

        List<P_PSMAttribute> GetAttributes(I_PSMHasAttributes current, P_PSMDiagram diagram, int level)
        {
            List<P_PSMAttribute> L = new List<P_PSMAttribute>();
            foreach (P_PSMAttribute A in current.Attributes)
                L.Add(A);
            foreach (XmlQualifiedName N in current.AttrGroupRefs)
            {
                if (diagram.GlobalIDs.ContainsKey(N))
                {
                    foreach (P_PSMAttribute A in GetAttributes(diagram.GlobalIDs[N], diagram, level))
                        L.Add(A);
                }
                else
                {
                    Print("ERROR: Unresolved AttributeGroupRef \"" + N + "\"" + Environment.NewLine, level);
                }
            }
            return L;
        }

        string GetAppInfo(XmlSchemaAnnotation Ann)
        {
            if (Ann == null) return null;
            List<XmlSchemaAppInfo> L = Ann.Items.OfType<XmlSchemaAppInfo>().ToList<XmlSchemaAppInfo>();
            if (L.Count > 0)
            {
                return L[0].Markup[0].InnerText;
            }
            else return null;
        }

        List<string> GetDoc(XmlSchemaAnnotation Ann)
        {
            
            List<string> LS = new List<string>();
            if (Ann == null) return LS;
            List<XmlSchemaDocumentation> L = Ann.Items.OfType<XmlSchemaDocumentation>().ToList<XmlSchemaDocumentation>();
            foreach (XmlSchemaDocumentation D in L)
            {
                foreach (XmlNode N in D.Markup)
                {
                    LS.Add(N.InnerText);
                }
            }
            return LS;
        }
        
        void PostProcess(object current, int level, P_PSMDiagram diagram)
        {
            //TODO: resolve references? (SR + type + attrgroups)
            if (current is I_PSMHasChildren)
            {
                #region Merge AttributeContainers
                P_PSMAttributeContainer first = null;
                List<P_PSMAttributeContainer> temp = new List<P_PSMAttributeContainer>();
                foreach (I_PSMHasParent AC in (current as I_PSMHasChildren).Children) 
                    if (AC is P_PSMAttributeContainer) temp.Add(AC as P_PSMAttributeContainer);
                
                foreach (P_PSMAttributeContainer AC in temp)
                {
                    if (AC != null)
                    {
                        if (first == null) first = AC;
                        else
                        {
                            AC.Parent.Children.Remove(AC);
                            foreach (P_PSMAttribute A in AC.Attributes) first.Attributes.Add(A);
                        }
                    }
                }
                temp = null;
                #endregion

                #region Resolve AttributeGroups
                //RESOLVE ATTRGROUPS BY COPYING - CAN IT BE DONE IN ANOTHER WAY?
                if (current is I_PSMHasAttributes)
                {
                    foreach (XmlQualifiedName N in (current as I_PSMHasAttributes).AttrGroupRefs)
                    {
                        if (diagram.GlobalIDs.ContainsKey(N))
                        {
                            foreach (P_PSMAttribute A in GetAttributes(diagram.GlobalIDs[N], diagram, level))
                            {
                                P_PSMAttribute a = new P_PSMAttribute()
                                {
                                    Alias = GetUniqueAlias(A.Alias, (current as I_PSMHasAttributes).Attributes),
                                    //Alias = "AGREF_" + GetNextInt().ToString() + A.Alias, 
                                    DefaultValue = A.DefaultValue,
                                    FixedValue = A.FixedValue,
                                    Form = A.Form,
                                    type = A.type,
                                    Lower = A.Lower,
                                    Upper = A.Upper
                                };
                                (current as I_PSMHasAttributes).Attributes.Add(a);
                            }
                        }
                        else
                        {
                            Print("ERROR: Unresolved AttributeGroupRef \"" + N + "\"" + Environment.NewLine, level);
                        }
                    }
                    (current as I_PSMHasAttributes).AttrGroupRefs.Clear();
                }
                #endregion

                #region Resolve Groups
                foreach (XmlQualifiedName N in (current as I_PSMHasChildren).GroupRefs)
                {
                    if (diagram.GlobalIDs.ContainsKey(N))
                    {
                        P_PSMClass C = new P_PSMClass() 
                            {   Name = new XmlQualifiedName(GetUniqueName(N.Name, N.Namespace, "_gr", diagram)),
                                //Name = new XmlQualifiedName("GREF_" + GetNextInt().ToString() + N.Name, N.Namespace), 
                                Parent = current as I_PSMHasChildren, SRofType = N };
                        (current as I_PSMHasChildren).Children.Add(C);
                        diagram.UsedNames.Add(C.Name);
                    }
                    else
                    {
                        Print("ERROR: Unresolved GroupRef \"" + N + "\"" + Environment.NewLine, level);
                    }
                }
                #endregion

                if (current is P_PSMClass)
                {
                    P_PSMClass Class = current as P_PSMClass;

                    #region Resolve Extensions
                    //Move the P_PSMClasses "under" the classes they extend and create a SR instead of them
                    if (Class.ExtensionOf != null && !(Class.Parent is P_PSMClass && Class.ExtensionOf == (Class.Parent as P_PSMClass).Name))
                    // Is extension of something and is not moved there yet
                    {
                        if (diagram.GlobalIDs.ContainsKey(Class.ExtensionOf))
                        {
                            P_PSMClass ExtendedClass = diagram.GlobalIDs[Class.ExtensionOf];
                            I_PSMHasChildren Parent = Class.Parent;
                            string EL = Class.ElementLabel;

                            Parent.Children.Remove(Class);
                            ExtendedClass.Children.Add(Class);
                            Class.Parent = ExtendedClass;
                            if (!diagram.GlobalIDs.ContainsKey(Class.Name)) diagram.GlobalIDs.Add(Class.Name, Class);
                            Class.ElementLabel = null;
                            
                            P_PSMClass NewSR = new P_PSMClass()
                            {
                                SRofType = Class.Name,
                                Parent = Parent,
                                ElementLabel = EL,
                                CreatedAsExtensionSR = true,
                                Name = new XmlQualifiedName(GetUniqueName(Class.Name.Name, Class.Name.Namespace, "_e", diagram), Class.Name.Namespace)
                            };

                            Parent.Children.Add(NewSR);

                            PostProcess(NewSR, level, diagram);
                        }
                        else Print("POSTPROCESSING WARNING: Could not resolve ExtensionOf: \"" + Class.ExtensionOf + "\"" + Environment.NewLine, 0);
                    }
                    #endregion

                    #region Register Structural Representatives with the represented classes
                    if (Class.SRofType != null)
                    {
                        if (diagram.GlobalIDs.ContainsKey(Class.SRofType))
                        {
                            diagram.GlobalIDs[Class.SRofType].SRepresentedBy.Add(Class);
                        }
                        else Print("POSTPROCESSING WARNING: Could not resolve SRofType: \"" + Class.SRofType + "\"" + Environment.NewLine, 0);
                    }
                    else if (Class.SRofElemRef != null)
                    {
                        if (diagram.GlobalElementTypes.ContainsKey(Class.SRofElemRef))
                        {
                            XmlQualifiedName N = diagram.GlobalElementTypes[Class.SRofElemRef];
                            if (diagram.GlobalIDs.ContainsKey(N))
                            {
                                diagram.GlobalIDs[N].SRepresentedBy.Add(Class);
                            }
                            else Print("POSTPROCESSING WARNING: Could not resolve SRofType (from SRofElemRef): \"" + N + "\"" + Environment.NewLine, 0);
                        }
                        else Print("POSTPROCESSING WARNING: Could not resolve SRofElemRef: \"" + Class.SRofElemRef + "\"" + Environment.NewLine, 0);
                    }
                }
                #endregion

                #region Remove Dummies
                if (current is P_PSMDummy)
                //Sequence - introduced because of merging ACs in ContentChoices removed the "choice sematics"
                {
                    P_PSMDummy D = current as P_PSMDummy;
                    D.Parent.Children.Remove(D);
                    //Because the "Children" collection can be changed in the process, we need to create a copy
                    List<I_PSMHasParent> Children = D.Children.ToList<I_PSMHasParent>();
                    foreach (I_PSMHasParent child in Children)
                    {
                        D.Children.Remove(child);
                        D.Parent.Children.Add(child);
                        child.Parent = D.Parent;
                        PostProcess(child, level + 1, diagram);
                    }
                }
                #endregion

                //Recursion
                //Because the "Children" collection can be changed in the process, we need to create a copy
                List<I_PSMHasParent> Children2 = (current as I_PSMHasChildren).Children.ToList<I_PSMHasParent>();
                foreach (I_PSMHasParent child in Children2) PostProcess(child, level + 1, diagram);
            }
        }

        void PostProcessCCandComment(object current, int level, P_PSMDiagram diagram)
        {
            if (current is I_PSMHasChildren)
            {
                #region Content Container
                if (current is P_PSMClass && !((current as P_PSMClass).Parent is P_PSMDiagram) && (current as P_PSMClass).Documentation.Count > 0 && (current as P_PSMClass).Documentation[0].Equals("PSM: ContentContainer"))
                {
                    P_PSMClass cPSMClass = current as P_PSMClass;
                    if (cPSMClass.SRepresentedBy.Count == 0)
                    {
                        P_PSMContentContainer CC = new P_PSMContentContainer();
                        CC.ElementLabel = cPSMClass.ElementLabel.Equals("") ? cPSMClass.Name.Name : cPSMClass.ElementLabel;
                        CC.Parent = cPSMClass.Parent;
                        int index = CC.Parent.Children.IndexOf(cPSMClass);
                        CC.Parent.Children.Remove(cPSMClass);
                        CC.Parent.Children.Insert(index, CC);

                        foreach (I_PSMHasParent child in cPSMClass.Children)
                        {
                            CC.Children.Add(child);
                            child.Parent = CC;
                        }

                        if (cPSMClass.Attributes.Count > 0)
                        {
                            P_PSMAttributeContainer AC = new P_PSMAttributeContainer();
                            AC.Parent = CC;
                            CC.Children.Insert(0, AC);
                            foreach (P_PSMAttribute A in cPSMClass.Attributes)
                            {
                                AC.Attributes.Add(A);
                            }
                        }
                        current = CC;
                    }
                }
                if (current is P_PSMClass) ClassesCount++;
                #endregion

                #region Comment
                if (current is P_PSMClass && (current as P_PSMClass).Documentation.Count > 0)
                {
                    P_PSMClass cPSMClass = current as P_PSMClass;
                    foreach (string S in cPSMClass.Documentation)
                    {
                        P_PSMComment C = new P_PSMComment();
                        C.Parent = cPSMClass;
                        C.text = S;
                        cPSMClass.Children.Add(C);
                    }
                }

                #endregion
                List<I_PSMHasParent> Children2 = (current as I_PSMHasChildren).Children.ToList<I_PSMHasParent>();
                foreach (I_PSMHasParent child in Children2) PostProcessCCandComment(child, level + 1, diagram);
            }
        }

        void ProcessAttributeGroup(XmlSchemaAttributeGroup group, P_PSMDiagram diagram, int level)
        {
            P_PSMClass C = new P_PSMClass();
            C.Name = new XmlQualifiedName(group.Name, diagram.TargetNamespace);
            ProcessAttributes(group.Attributes, level, C);
            diagram.GlobalIDs.Add(C.Name, C);
            C.Parent = diagram;
            diagram.Children.Add(C);
        }

        NUml.Uml2.UnlimitedNatural DecToUN(decimal d)
        {
            if (d == Decimal.MaxValue) return NUml.Uml2.UnlimitedNatural.Infinity;
            else return (NUml.Uml2.UnlimitedNatural)d;
        }
        
        void ProcessComplexType(XmlSchemaComplexType type, int level, I_PSMHasChildren Parent, P_PSMDiagram diagram, XmlSchemaElement elem)
        {
            if (elem != null && !elem.SchemaTypeName.IsEmpty)
            //The type of the element has a name => a global complex type, end of recursion <element type="bar"/>
            {
                Print("XmlSchemaElement (of a global type): \"" + elem.Name + "\" Complex type: \"" +
                elem.ElementSchemaType.Name + "\" refname: \"" + elem.RefName.Namespace + ": " +
                elem.RefName.Name + "\" qname: \"" + elem.QualifiedName + "\"" + Environment.NewLine, level);

                if (diagram.GlobalIDs.ContainsKey(elem.ElementSchemaType.QualifiedName)
                    && diagram.GlobalIDs[elem.ElementSchemaType.QualifiedName].Parent == diagram
                    && diagram.GlobalIDs[elem.ElementSchemaType.QualifiedName].ElementLabel == null)
                {   // REDUCES NUMBER OF ELEMENT-COMPLEXTYPE pairs
                    // Root P_PSMClass representing the complexType of the element and not having an element label gets it
                    // and is moved to the place where the the element should be. Works primarily for global elements
                    // as they are processed after all global complex types
                    P_PSMClass candidate = diagram.GlobalIDs[elem.ElementSchemaType.QualifiedName];
                    diagram.Children.Remove(candidate);
                    Parent.Children.Add(candidate);
                    candidate.Parent = Parent;
                    candidate.ElementLabel = elem.Name;
                    candidate.MinOccurs = (uint) elem.MinOccurs;
                    candidate.MaxOccurs = DecToUN(elem.MaxOccurs);
                    if (elem.Parent is XmlSchema)
                        //If this was global element, we need to create the mapping for the case
                        //it is referenced by <element ref="foo"/> somewhere in the schema
                        diagram.GlobalElementTypes.Add(elem.QualifiedName, candidate.Name);
                }
                else
                {
                    // No merging, create a new root P_PSMClass, SR of the P_PSMClass representing
                    // the type from <element type="bar"/>
                     
                    P_PSMClass current = new P_PSMClass()
                    {
                        ElementLabel = elem.Name,
                        AppInfo = GetAppInfo(elem.Annotation),
                        Documentation = GetDoc(elem.Annotation),
                        MinOccurs = (uint)elem.MinOccurs,
                        MaxOccurs = DecToUN(elem.MaxOccurs),
                        Name = new XmlQualifiedName(GetUniqueName(elem.QualifiedName.Name, elem.QualifiedName.Namespace, "_t", diagram), elem.QualifiedName.Namespace)
                    };
                    if (elem.ElementSchemaType != null) current.Documentation.AddRange(GetDoc(elem.ElementSchemaType.Annotation));
                    else if (elem.SchemaType != null) current.Documentation.AddRange(GetDoc(elem.SchemaType.Annotation));
                    if (elem.Parent is XmlSchema)
                    {
                        //Global element, can be referenced by <element ref="foo"/>
                        //We create a mapping in diagram.GlobalElementTypes
                        //and the type will be accessible through GlobalIDs
                        diagram.GlobalElementTypes.Add(elem.QualifiedName, current.Name);
                        diagram.GlobalIDs.Add(current.Name, current);
                    }
                    Parent.Children.Add(current);
                    current.Parent = Parent;
                    current.SRofType = elem.SchemaTypeName;
                    
                    diagram.UsedNames.Add(current.Name);
                }
            }
            else if (elem != null && !elem.RefName.IsEmpty)
                //Non-global element like this: <element ref="foo"/>
                //It cannot be global because ref is not allowed for global elements in XML Schema
            {
                Print("XmlSchemaElementRef: \"" + elem.Name + "\" Complex type: \"" +
                elem.ElementSchemaType.Name + "\" refname: \"" + elem.RefName.Namespace + ": " +
                elem.RefName.Name + "\" qname: \"" + elem.QualifiedName + "\"" + Environment.NewLine, level);

                P_PSMClass current = new P_PSMClass();
                Parent.Children.Add(current);
                current.Parent = Parent;
                /* Structural representative, end of recursion */
                current.SRofElemRef = elem.RefName;
                current.AppInfo = GetAppInfo(elem.Annotation);
                current.Documentation = GetDoc(elem.Annotation);
                if (elem.ElementSchemaType != null) current.Documentation.AddRange(GetDoc(elem.ElementSchemaType.Annotation));
                else if (elem.SchemaType != null) current.Documentation.AddRange(GetDoc(elem.SchemaType.Annotation));
                current.Name =
                    new XmlQualifiedName(
                        GetUniqueName(elem.RefName.Name, elem.QualifiedName.Namespace, "_r", diagram),
                        elem.QualifiedName.Namespace);
                current.MinOccurs = (uint)elem.MinOccurs;
                current.MaxOccurs = DecToUN(elem.MaxOccurs);
                
                //current.Name = new XmlQualifiedName("REF" + GetNextInt().ToString() + "_" + elem.RefName.Name, elem.QualifiedName.Namespace);
                //diagram.GlobalIDs.Add(current.Name, current);
                diagram.UsedNames.Add(current.Name);
            }
            else 
                //The type is global or the type of the element does not have a name (it is embedded)
                //Either global <complexType> or <element ... ><complexType>...
            {

                P_PSMClass current = new P_PSMClass();
                Parent.Children.Add(current);
                current.Parent = Parent;
                ProcessAttributes(type.Attributes, level + 1, current);

                if (elem != null)
                //Complex type inside an element, type.name == null - <element ... ><complexType>...
                {
                    if (elem.ElementSchemaType != null)
                        Print("XmlSchemaElement: \"" + elem.Name + "\" Complex type: \"" +
                        elem.ElementSchemaType.Name + "\" refname: \"" + elem.RefName.Namespace + ": " +
                        elem.RefName.Name + "\" qname: \"" + elem.QualifiedName + "\"" + Environment.NewLine, level);
                    else
                        Print("XmlSchemaElement: \"" + elem.Name + "\" Complex type: \"" +
                        elem.SchemaType.Name + "\" refname: \"" + elem.RefName.Namespace + ": " +
                        elem.RefName.Name + "\" qname: \"" + elem.QualifiedName + "\"" + Environment.NewLine, level);

                    current.Name = 
                        new XmlQualifiedName(
                            GetUniqueName(elem.QualifiedName.Name, elem.QualifiedName.Namespace, "_t", diagram), 
                            elem.QualifiedName.Namespace);
                    current.AppInfo = GetAppInfo(elem.Annotation);
                    current.Documentation = GetDoc(elem.Annotation);
                    if (elem.ElementSchemaType != null) current.Documentation.AddRange(GetDoc(elem.ElementSchemaType.Annotation));
                    else if (elem.SchemaType != null) current.Documentation.AddRange(GetDoc(elem.SchemaType.Annotation));
                    current.MinOccurs = (uint)elem.MinOccurs;
                    current.MaxOccurs = DecToUN(elem.MaxOccurs);
                    
                    if (elem.Parent is XmlSchema) 
                        //Global element, unnamed type (embedded) => Create mapping
                    {
                        diagram.GlobalElementTypes.Add(elem.QualifiedName, current.Name);
                        //Added to fix global <element name="blabal"/> not being able to be target of SR
                        diagram.GlobalIDs.Add(current.Name, current);
                    }
                    diagram.UsedNames.Add(current.Name);
                    current.ElementLabel = elem.Name;
                }
                else //Global Complex Type
                {
                    Print("Complex type: \"" + type.Name + "\"" + Environment.NewLine, 0);
                    //Here we don't generate a unique name
                    //we use the one "reserved" in the beginning of the process
                    //therefore it is already in UsedNames
                    //We only create the mapping for post-processing
                    current.Name = type.QualifiedName;
                    diagram.GlobalIDs.Add(current.Name, current);
                    //diagram.UsedNames.Add(current.Name);
                }

                if (type.ContentType == XmlSchemaContentType.Empty)
                {
                    /* Empty element, end of recursion */
                }
                else if (elem == null || elem.RefName.IsEmpty)
                {
                    //It's type is not "ref" and it has some content
                    if (type.ContentModel == null)
                    {
                        TraverseParticle(type.Particle, level + 1, current, diagram);
                    }
                    else if (type.ContentModel is XmlSchemaComplexContent)
                    {
                        if (type.ContentModel.Content is XmlSchemaComplexContentExtension)
                        {
                            current.ExtensionOf = (type.ContentModel.Content as XmlSchemaComplexContentExtension).BaseTypeName;
                            TraverseParticle(type.ContentTypeParticle, level + 1, current, diagram);

                        }
                        else if (type.ContentModel.Content is XmlSchemaComplexContentRestriction)
                        {
                            current.RestrictionOf = (type.ContentModel.Content as XmlSchemaComplexContentRestriction).BaseTypeName;
                            TraverseParticle(type.ContentTypeParticle, level + 1, current, diagram);
                        }
                        else TraverseParticle(type.Particle, level + 1, current, diagram);
                    }
                    else /* Simple content */
                    {
                        // Merge attributes within simple content
                        if (type.ContentModel.Content is XmlSchemaSimpleContentExtension)
                        {
                            XmlSchemaSimpleContentExtension E = type.ContentModel.Content as XmlSchemaSimpleContentExtension;

                            ProcessAttributes(E.Attributes, level + 1, current);
                            //TODO: groupref inside extension? Restriction?

                        }
                        Print("Simple Content" + Environment.NewLine, level);
                    }
                }
            }
        }

        string GetUniqueName(string name, string ns, string suffix, P_PSMDiagram diagram)
        {
            string N;
            bool u = Model.NameSuggestor<XmlQualifiedName>.IsNameUnique(diagram.UsedNames,
                    ns + name, P => P.Namespace + P.Name);
            if (u) N = name;
            else N = Model.NameSuggestor<XmlQualifiedName>.SuggestUniqueName(
                                diagram.UsedNames, name + suffix, P => P.Name);
            return N;
        }
        string GetUniqueAlias(string alias, ObservableCollection<P_PSMAttribute> atts)
        {
            string A;
            bool u = Model.NameSuggestor<P_PSMAttribute>.IsNameUnique(atts.AsEnumerable<P_PSMAttribute>(), alias, a => a.Alias);
            if (u) A = alias;
            else A = Model.NameSuggestor<P_PSMAttribute>.SuggestUniqueName(atts.AsEnumerable<P_PSMAttribute>(), alias + "_", a => a.Alias);
            return A;
        }

        void ProcessGroup(XmlSchemaGroup G, P_PSMDiagram diagram)
        {
            Print("Group: " + G.Name + Environment.NewLine, 0);
            P_PSMClass C = new P_PSMClass() { Name = G.QualifiedName };
            diagram.GlobalIDs.Add(C.Name, C);
            diagram.UsedNames.Add(C.Name);
            diagram.Children.Add(C);
            C.Parent = diagram;
            TraverseParticle(G.Particle, 1, C, diagram);
        }

        public P_PSMDiagram Process(XmlSchema xschema)
        {
            XmlSchema xs;
            
            XmlSchemaSet set = new XmlSchemaSet();
            set.ValidationEventHandler += new ValidationEventHandler(ValidationCallbackOne);
            set.Add(xschema);
            set.Compile();

            P_PSMDiagram diagram = new P_PSMDiagram();
            diagram.SchemaSet = set;
            
            //Reservation of Global complex type names
            //foreach (XmlSchemaType GlobalType in set.GlobalTypes.Values) diagram.UsedNames.Add(GlobalType.QualifiedName);
            foreach (XmlSchema schema in set.Schemas())
                foreach (XmlSchemaType t in schema.SchemaTypes.Values) diagram.UsedNames.Add(t.QualifiedName);
            
            foreach (XmlSchema schema in set.Schemas())
            {
                xs = schema;
                diagram.TargetNamespace = xs.TargetNamespace;

                Print("Types in SchemaTypes:" + Environment.NewLine + Environment.NewLine, 0);

                //Go through every global type
                foreach (XmlSchemaType type in xs.SchemaTypes.Values)
                {
                    if (type is XmlSchemaComplexType)
                        ProcessComplexType(type as XmlSchemaComplexType, 0, diagram, diagram, null);
                    else if (type is XmlSchemaSimpleType)
                    {
                        Print("Simple type: \"" + type.Name + "\"" + Environment.NewLine, 0);
                        if (!diagram.SimpleTypes.ContainsKey(type.QualifiedName)) diagram.SimpleTypes.Add(type.QualifiedName, type as XmlSchemaSimpleType);
                    }
                }

                foreach (XmlSchemaAttributeGroup AG in xs.AttributeGroups.Values)
                    ProcessAttributeGroup(AG, diagram, 0);

                //TODO: Groups
                foreach (XmlSchemaGroup G in xs.Groups.Values)
                    ProcessGroup(G, diagram);
                
                Print(Environment.NewLine + "Element in Elements:" + Environment.NewLine + Environment.NewLine, 0);
                //Go through every global element
                foreach (XmlSchemaElement el in xs.Elements.Values)
                {
                    TraverseParticle(el, 0, diagram, diagram);
                }
            }
            
            Print("\n\nPOST PROCCESS:\n\n", 0);
            PostProcess(diagram, 0, diagram);

            PostProcessSR(diagram);

            PostProcessCCandComment(diagram, 0, diagram);

            /*temp
            Print("Complex types in diagram " + diagram.GlobalIDs.Count + " :" + Environment.NewLine, 0);
            foreach (XmlQualifiedName N in diagram.GlobalIDs.Keys)
            {
                Print(N + ": " + diagram.GlobalIDs[N].Name + Environment.NewLine, 0);
            }*/
            
            //PrintDiagram(diagram, 0);
            //SW.Close();
            return diagram;
        }

        void PostProcessSR(P_PSMDiagram d)
        {
            List<P_PSMClass> RootClasses = d.Children.OfType<P_PSMClass>().ToList<P_PSMClass>();
            foreach (P_PSMClass rootClass in RootClasses)
            {
                if (DeleteUnnecessarySRsMadeByExtensions && rootClass.CreatedAsExtensionSR && rootClass.SRepresentedBy.Count == 0)
                {
                    //Remove useless SRs created because a global ComplexType is an extension of some other type
                    d.Children.Remove(rootClass);
                    d.GlobalIDs[rootClass.SRofType].SRepresentedBy.Remove(rootClass);
                }
                if (ResolveSRs && rootClass.SRepresentedBy.Count == 1)
                {
                    //Move a root class that is represented only at one place to that place instead of SR
                    P_PSMClass Rep = rootClass.SRepresentedBy[0];
                    //Cycle avoidance
                    if (Rep.Ancestors.Contains(rootClass)) continue;
                    //Element label loss avoidance
                    else if (Rep.ElementLabel != null && rootClass.ElementLabel != null) continue;
                    else if (d.GlobalIDs.ContainsValue(Rep))
                    {
                        //Shouldn't happen, but...
                        Debug.Assert(false, "d.GlobalIDs.ContainsValue(Rep)");
                        continue;
                    }
                    else if (d.GlobalElementTypes.ContainsValue(Rep.Name))
                    {
                        //Shouldn't happen, but...
                        Debug.Assert(false, "d.GlobalElementTypes.ContainsValue(Rep.Name)");
                        continue;
                    }
                    else if (Rep.SRepresentedBy.Count > 0)
                    {
                        //Shouldn't happen, but...
                        Debug.Assert(false, "Rep.SRepresentedBy.Count > 0");
                        continue;
                    }

                    if (Rep.ElementLabel != null && rootClass.ElementLabel == null) rootClass.ElementLabel = Rep.ElementLabel;
                    rootClass.SRepresentedBy.Clear();
                    d.Children.Remove(rootClass);
                    rootClass.Parent = Rep.Parent;
                    rootClass.Documentation = Rep.Documentation; //for CContainers and Comments
                    int index = Rep.Parent.Children.IndexOf(Rep);
                    Rep.Parent.Children.Remove(Rep);
                    Rep.Parent.Children.Insert(index, rootClass);
                }
            }
        }

        void PrintDiagram(object parent, int level)
        {
            if (parent is P_PSMDiagram)
            {
                Print("PSM Diagram" + Environment.NewLine, level);
                foreach (I_PSMHasParent child in (parent as I_PSMHasChildren).Children) PrintDiagram(child, level + 1);
            }
            else if (parent is P_PSMClass)
            {
                P_PSMClass c = parent as P_PSMClass;
                Print("PSM Class: \"" + c.Name + "\" EL: \"" + c.ElementLabel + "\"" + Environment.NewLine, level);
                if (c.SRofType != null) Print("SR of Type: \"" + c.SRofType + "\"" + Environment.NewLine, level);
                if (c.SRofElemRef != null) Print("SR of ElemRef: \"" + c.SRofElemRef + "\"" + Environment.NewLine, level);
                foreach (P_PSMAttribute A in (parent as I_PSMHasAttributes).Attributes)
                    Print("Attribute: \"" + A.Alias + "\" type: \"" + A.type + "\"" + Environment.NewLine, level + 1);
                foreach (I_PSMHasParent child in (parent as I_PSMHasChildren).Children) PrintDiagram(child, level + 1);

            }
            else if (parent is P_PSMAttributeContainer)
            {
                Print("Attribute container" + Environment.NewLine, level);
                foreach (P_PSMAttribute A in (parent as I_PSMHasAttributes).Attributes)
                    Print("Attribute: \"" + A.Alias + "\" type: \"" + A.type + "\"" + Environment.NewLine, level + 1);
            }
            else if (parent is P_PSMContentChoice)
            {
                Print("Content choice" + Environment.NewLine, level);
                foreach (I_PSMHasParent child in (parent as I_PSMHasChildren).Children) PrintDiagram(child, level + 1);
            }
            else
            {
            }
        }

        void ProcessAttributes(XmlSchemaObjectCollection Atts, int level, P_PSMClass current)
        {
            if (Atts.Count > 0)
            {
                Print("Attributes: " +
                    Atts.Count + Environment.NewLine, level);

                foreach (XmlSchemaObject O in Atts)
                {
                    if (O is XmlSchemaAttribute)
                    {
                        XmlSchemaAttribute A = O as XmlSchemaAttribute;
                        Print("Att name: \"" + A.Name + "\" type: \"" +
                        A.SchemaTypeName.Namespace + ": " + A.SchemaTypeName.Name + "\" use: \"" +
                        A.Use + "\"" + Environment.NewLine, level + 1);
                        if (current != null)
                        {
                            P_PSMAttribute a = new P_PSMAttribute()
                            {
                                Alias = A.Name,
                                type = A.SchemaTypeName,
                                DefaultValue = A.DefaultValue,
                                FixedValue = A.FixedValue,
                                Form = A.Form
                            };
                            SetAttributeUse(a, A);
                            current.Attributes.Add(a);
                        }
                    }
                    else if (O is XmlSchemaAttributeGroupRef)
                    {
                        XmlSchemaAttributeGroupRef G = O as XmlSchemaAttributeGroupRef;
                        Print("AttGroupRef: \"" + G.RefName.Name + "\"" + Environment.NewLine, level);

                        /* CANNOT HAVE MULTIPLE REPRESENTED CLASSES - MUST COPY ATTRIBUTES
                        //create SR
                        if (current.SRof != null) Print("Warning: Overwriting SRof \"" + 
                            current.SRof + " with \"" + G.RefName.Name + "\"." + Environment.NewLine, level);
                        current.SRof = G.RefName.Name;*/

                        current.AttrGroupRefs.Add(G.RefName);
                    }
                    else
                    {
                        Print("Unknown object in type.Attributes collection" + Environment.NewLine, level);
                    }
                }
            }
        }

        void SetAttributeUse(P_PSMAttribute A, XmlSchemaAttribute a)
        {
            //TODO: Move to XSDtoPPSM
            switch (a.Use)
            {
                case XmlSchemaUse.Optional:
                    A.Lower = 0;
                    A.Upper = 1;
                    break;
                case XmlSchemaUse.Required:
                    A.Lower = 1;
                    A.Upper = 1;
                    break;
                case XmlSchemaUse.Prohibited:
                    A.Lower = 0;
                    A.Upper = 0;
                    break;
            }
        }
        
        void TraverseGroupInsides(XmlSchemaElement elem, int level, I_PSMHasChildren Parent, P_PSMDiagram diagram)
        {
            if (elem.SchemaTypeName.IsEmpty)
            {
                if (elem.SchemaType is XmlSchemaComplexType)
                    ProcessComplexType(elem.SchemaType as XmlSchemaComplexType, level, Parent, diagram, elem);
                else if (elem.SchemaType is XmlSchemaSimpleType)
                {
                    P_PSMAttributeContainer c = new P_PSMAttributeContainer();
                    Parent.Children.Add(c);
                    c.Parent = Parent;
                    P_PSMAttribute a = new P_PSMAttribute() { Alias = elem.Name, type = elem.SchemaType.BaseXmlSchemaType.QualifiedName };
                    a.Lower = (uint) elem.MinOccurs;
                    a.Upper = DecToUN(elem.MaxOccurs);
                    c.Attributes.Add(a);
                }
                else if (!elem.RefName.IsEmpty)
                {
                    P_PSMClass current = new P_PSMClass();
                    Parent.Children.Add(current);
                    current.Parent = Parent;
                    /* Structural representative, end of recursion */
                    current.SRofElemRef = elem.RefName;
                    current.Name = new XmlQualifiedName(GetUniqueName(elem.RefName.Name, elem.QualifiedName.Namespace, "_g", diagram), elem.QualifiedName.Namespace);
                    diagram.UsedNames.Add(current.Name);
                }
                else
                {
                    Print("WARNING: Unknown SchemaType Type" + Environment.NewLine, level);
                }
            }
            else
            {   //Copied from ProcessComplexType
                Print("XmlSchemaElementRef: \"" + elem.Name + "\" Complex type: \"" +
                elem.SchemaTypeName.Name + "\" refname: \"" + elem.RefName.Namespace + ": " +
                elem.RefName.Name + "\" qname: \"" + elem.QualifiedName + "\"" + Environment.NewLine, level);

                P_PSMClass current = new P_PSMClass();
                Parent.Children.Add(current);
                current.Parent = Parent;
                /* Structural representative, end of recursion */
                current.SRofType = elem.SchemaTypeName;
                current.Name = new XmlQualifiedName(GetUniqueName(elem.SchemaTypeName.Name, elem.SchemaTypeName.Namespace, "_g", diagram), elem.SchemaTypeName.Namespace);
                diagram.UsedNames.Add(current.Name);
            }
        }
        
        void TraverseElement(XmlSchemaElement elem, int level, I_PSMHasChildren Parent, P_PSMDiagram diagram)
        {
            if (elem.ElementSchemaType is XmlSchemaComplexType)
            {
                //Print("TADY: \"" + elem.SchemaType + "\" \"" + elem.SchemaTypeName + "\" \"" + elem.ElementSchemaType + "\"" + Environment.NewLine, 0);
                ProcessComplexType(elem.ElementSchemaType as XmlSchemaComplexType, level, Parent, diagram, elem);
            }
            else if (elem.ElementSchemaType is XmlSchemaSimpleType)
            {
                if ((elem.ElementSchemaType as XmlSchemaSimpleType).BaseXmlSchemaType == null)
                {
                    Print("XmlSchemaElement with BaseSchemaType null: \"", level);
                    P_PSMAttributeContainer c = new P_PSMAttributeContainer();
                    Parent.Children.Add(c);
                    c.Parent = Parent;
                    P_PSMAttribute a = new P_PSMAttribute() { Alias = elem.QualifiedName.Name, type = elem.ElementSchemaType.QualifiedName };
                    //TODO: ANY?
                    a.Lower = (uint)elem.MinOccurs;
                    a.Upper = DecToUN(elem.MaxOccurs);
                    c.Attributes.Add(a);
                }
                else
                {
                    // Each element creates an attribute container... merged in post processing.
                    // Could be done in another way that needs access 2 levels above in
                    // the recursion to realise that there already exists an AC
                    Print("XmlSchemaElement: \"" + elem.Name + "\" Simple type: \""
                        + elem.ElementSchemaType.BaseXmlSchemaType.QualifiedName + "\" refname: \"" + elem.RefName + "\"" + Environment.NewLine, level);
                    P_PSMAttributeContainer c = new P_PSMAttributeContainer();
                    Parent.Children.Add(c);
                    c.Parent = Parent;
                    P_PSMAttribute a = new P_PSMAttribute() { Alias = elem.QualifiedName.Name, type = elem.ElementSchemaType.BaseXmlSchemaType.QualifiedName };
                    a.Lower = (uint)elem.MinOccurs;
                    a.Upper = DecToUN(elem.MaxOccurs);
                    c.Attributes.Add(a);
                }
            }
            else if (elem.ElementSchemaType == null)
                //Special case for elements inside Group...
                TraverseGroupInsides(elem, level, Parent, diagram);
        }

        void TraverseGroupBase(XmlSchemaGroupBase GroupBase, int level, I_PSMHasChildren Current, P_PSMDiagram diagram)
        {
            Print(GroupBaseName(GroupBase) + " Subitems: "
                + GroupBase.Items.Count + Environment.NewLine, level);

            if (GroupBase is XmlSchemaSequence)
            {
                P_PSMDummy D = new P_PSMDummy();
                D.Parent = Current;
                Current.Children.Add(D);

                foreach (XmlSchemaParticle subParticle in GroupBase.Items)
                {
                    //TODO: Vyresit sequence v choice
                    TraverseParticle(subParticle, level + 1, D, diagram);
                }
            }
            else if (GroupBase is XmlSchemaChoice)
            {
                P_PSMContentChoice CC = new P_PSMContentChoice();
                Current.Children.Add(CC);
                CC.Parent = Current;
                foreach (XmlSchemaParticle subParticle in GroupBase.Items)
                {
                    TraverseParticle(subParticle, level + 1, CC, diagram);
                }
            }
            else
            {
                foreach (XmlSchemaParticle subParticle in GroupBase.Items)
                {
                    TraverseParticle(subParticle, level + 1, Current, diagram);
                }
            }
        }

        void TraverseParticle(XmlSchemaParticle particle, int level, I_PSMHasChildren Current, P_PSMDiagram diagram)
        {
            if (particle is XmlSchemaElement)
            {
                TraverseElement(particle as XmlSchemaElement, level, Current, diagram);
            }
            else if (particle is XmlSchemaGroupBase)
            { //xs:all, xs:choice, xs:sequence
                TraverseGroupBase(particle as XmlSchemaGroupBase, level, Current, diagram);
            }
            else if (particle is XmlSchemaAny)
            {
                Print("Any." + Environment.NewLine, level);
            }
            else if (particle is XmlSchemaGroupRef)
            {
                Print("GroupRef: \"" + (particle as XmlSchemaGroupRef).RefName + "\"" + Environment.NewLine, level);
                Current.GroupRefs.Add((particle as XmlSchemaGroupRef).RefName);
            }
            else if (particle == null)
            { 
                Print("Null particle" + Environment.NewLine, level); 
            }
            else
            {
                Print("Unknown XmlSchema particle" + Environment.NewLine, level);
            }
        }

        string GroupBaseName(XmlSchemaGroupBase B)
        {
            if (B == null) return "NULL!";
            if (B is XmlSchemaSequence) return "Sequence:";
            if (B is XmlSchemaChoice) return "Choice:";
            if (B is XmlSchemaAll) return "All:";
            return "Unknown Group Base:";
        }

        void Print(string what, int level)
        {
            /*if (t != null)
            {
                for (int i = 0; i <= level; i++) t.Text += "\t";
                //for (int i = 0; i <= level; i++) SW.Write("\t");
                t.Text += what;
                //SW.Write(what);
            }*/
        }
    }
}
