using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Xml;
using System.Xml.Schema;
using XCase.Controller.Commands;
using XCase.Controller;
using XCase.Controller.Commands.Helpers;
using XCase.Model;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using XCase.View;

namespace XCase.Reverse
{
    public class ReverseEngineering
    {
        DiagramController DiagramController;
        PIMClass tempPIMClass;
        public bool ResolveSRs = true;
        public bool DeleteUnnecessarySRsMadeByExtensions = true;
        public bool UseCommands = true;
        public bool Layout = true;
        XSDtoPSM X;
        int currentClassCount = 0;

        public ReverseEngineering(DiagramController diagramController)
        {
            DiagramController = diagramController;
        }
        
        public bool XSDtoPSM(TextBlock t, Label l, ProgressBar p, string filename = null)
        {
            X = new XSDtoPSM();
            X.ResolveSRs = ResolveSRs;
            X.DeleteUnnecessarySRsMadeByExtensions = DeleteUnnecessarySRsMadeByExtensions;
            X.t = t;
            X.l = l;
            X.p = p;

            if (filename == null)
            {
                OpenFileDialog d = new OpenFileDialog();
                d.Filter = "XML Schema files|*.xsd|All files|*.*";
                d.Title = "Select XML Schema file";
                d.CheckPathExists = true;
                d.CheckFileExists = true;
                d.Multiselect = false;
                if (d.ShowDialog() == true)
                {
                    filename = d.FileName;
                }
                else 
                {
                    return false; 
                }
            }
            
            XmlSchema xschema = null;
            XmlTextReader R;
            try
            {
                R = new XmlTextReader(filename);
                xschema = XmlSchema.Read(R, null);
                R.Close();
            }
            catch (Exception E)
            {
                MessageBox.Show(E.Message);

                return false;
            }

            Stopwatch S = new Stopwatch();
            
            S.Start();
            P_PSMDiagram D = X.Process(xschema);
            S.Stop();
            X.t.Text += "Converted XSD to PPSM in " + S.ElapsedMilliseconds.ToString() + "ms" + Environment.NewLine; 
            PPSMtoPSM(D);
            return true;   
        }

        void PPSMtoPSM(P_PSMDiagram D)
        {
            NewModelClassCommand addPIMClass = NewModelClassCommandFactory.Factory().Create(DiagramController.ModelController) as NewModelClassCommand;
            addPIMClass.Package = DiagramController.Project.Schema.Model;
            addPIMClass.Execute();
            tempPIMClass = addPIMClass.CreatedClass.Element;

            X.p.Maximum = X.ClassesCount;
            X.p.Value = 0;
            X.p.Visibility = Visibility.Visible;
            X.l.Visibility = Visibility.Visible;
            if (!Layout) TreeLayout.SwitchOff();
            Stopwatch S = new Stopwatch();
            S.Start();
            if (UseCommands) GeneratePSM(D);
            else
            {
                DiagramController.getUndoStack().Invalidate();
                DiagramController.getRedoStack().Invalidate();
                GeneratePSM2(D);
            }
            S.Stop();
            X.t.Text += "Generated PSM in " + S.ElapsedMilliseconds.ToString() + "ms" + Environment.NewLine;
            
            S = new Stopwatch();
            S.Start();
            if (UseCommands) LinkSRs(D);
            else LinkSRs2(D);
            S.Stop();
            X.t.Text += "Linked SRs in " + S.ElapsedMilliseconds.ToString() + "ms" + Environment.NewLine;
            if (!Layout) TreeLayout.SwitchOn();

            //Only to refresh GUI, which normally happens after each command
            ActivateDiagramCommand A = ActivateDiagramCommandFactory.Factory().Create(DiagramController.ModelController) as ActivateDiagramCommand;
            A.Set(DiagramController.Diagram);
            A.Execute();
        }

        void LinkSRs2(I_PSMHasChildren D)
        {
            if (D is P_PSMClass)
            {
                P_PSMClass C = D as P_PSMClass;
                foreach (P_PSMClass SR in C.SRepresentedBy)
                {
                    (SR.Super as PSMClass).RepresentedPSMClass = (C.Super as PSMClass);

                    /*SetRepresentedPSMClassCommand c = SetRepresentedPSMClassCommandFactory.Factory().Create(DiagramController) as SetRepresentedPSMClassCommand;
                    c.Set(C.Super as PSMClass, SR.Super as PSMClass);
                    c.Execute();*/
                }
            }

            foreach (I_PSMHasParent child in D.Children)
                if (child is I_PSMHasChildren)
                    LinkSRs2(child as I_PSMHasChildren);
        }

        void LinkSRs(I_PSMHasChildren D)
        {
            if (D is P_PSMClass)
            {
                P_PSMClass C = D as P_PSMClass;
                foreach (P_PSMClass SR in C.SRepresentedBy)
                {
                    SetRepresentedPSMClassCommand c = SetRepresentedPSMClassCommandFactory.Factory().Create(DiagramController) as SetRepresentedPSMClassCommand;
                    c.Set(C.Super as PSMClass, SR.Super as PSMClass);
                    c.Execute();
                }
            }

            foreach (I_PSMHasParent child in D.Children) 
                if (child is I_PSMHasChildren) 
                    LinkSRs(child as I_PSMHasChildren);
        }

        void GeneratePSM2(I_PSMHasChildren current)
        {
            foreach (I_PSMHasParent child in current.Children)
            {
                if (child is P_PSMClass)
                {
                    //UPDATE GUI: This is wrong, but better than crash due to detected deadlock:
                    X.l.Content = (++currentClassCount).ToString() + "/" + X.ClassesCount.ToString() + " PSM Classes";
                    X.p.Value = currentClassCount;
                    if (currentClassCount % (Math.Min(25, X.ClassesCount / 10) + 1) == 0)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
                    }
                    
                    P_PSMClass C = child as P_PSMClass;
                    PSMClass psmClass = tempPIMClass.DerivePSMClass();
                    C.Super = psmClass;
                    psmClass.Name = C.Name.Name;
                    psmClass.ElementName = C.ElementLabel;

                    ViewHelper v = new PSMElementViewHelper(DiagramController.Diagram) { X = 0, Y = 0, Height = double.NaN, Width = double.NaN };
                    DiagramController.Diagram.AddModelElement(psmClass, v);
                    psmClass.Diagram = DiagramController.Diagram as PSMDiagram;

                    //Attributes
                    foreach (P_PSMAttribute A in C.Attributes)
                    {
                        Property At = (C.Super as PSMClass).AddAttribute();
                        At.Name = A.Alias;
                        At.Lower = A.Lower;
                        /*if (Type != null && Type.Element != null)
                            createdAttribute.Type = Type.Element;*/
                        At.Upper = A.Upper;
                        At.Default = A.DefaultValue;
                        (At as PSMAttribute).Alias = A.Alias;
                    }
                    
                    if (current is P_PSMDiagram)
                    {
                        (DiagramController.Diagram as PSMDiagram).Roots.Add(psmClass);
                    }
                    else
                    {
                        if (C.ExtensionOf != null)
                        {
                            Generalization generalization = DiagramController.ModelController.Model.Schema.SetGeneralization((current as P_PSMClass).Super as PSMClass, C.Super as PSMClass);

                            DiagramController.Diagram.AddModelElement(generalization, new GeneralizationViewHelper(DiagramController.Diagram));
                            
                        }
                        else
                        {
                            PSMAssociation PSMAssoc = (PSMAssociation)(current as P_PSMBase).Super.AddComponent(PSMAssociationFactory.Instance);
                            PSMAssoc.Child = psmClass;
                            PSMAssoc.Upper = C.MaxOccurs;
                            PSMAssoc.Lower = C.MinOccurs;

                            DiagramController.Diagram.AddModelElement(PSMAssoc, new PSMAssociationViewHelper(DiagramController.Diagram));
                            PSMAssoc.Diagram = DiagramController.Diagram as PSMDiagram;
                            
                        }
                    }
                    GeneratePSM2(C);
                }
                else if (!(current is P_PSMDiagram) && child is P_PSMContentChoice)
                {
                    PSMContentChoice psmChoice = (PSMContentChoice)(current as P_PSMBase).Super.AddComponent(PSMContentChoiceFactory.Instance);
                    DiagramController.Diagram.AddModelElement(psmChoice, new PSMElementViewHelper(DiagramController.Diagram));
                    (child as P_PSMContentChoice).Super = psmChoice;
                    
                    GeneratePSM2(child as P_PSMContentChoice);
                }
                else if (!(current is P_PSMDiagram) && child is P_PSMAttributeContainer)
                {
                    PSMClass owner = null;
                    PSMSuperordinateComponent PSMSuper = null;
                    PSMAttributeContainer psmAttributeContainer = null;
                    if (current is P_PSMClass)
                    {
                        owner = (current as P_PSMClass).Super as PSMClass;
                    }
                    else if (current is P_PSMContentChoice)
                    {
                        PSMSuper = (current as P_PSMContentChoice).Super as PSMSuperordinateComponent;
                        owner = (current as P_PSMContentChoice).P_PSMClass.Super as PSMClass;
                    }
                    else if (current is P_PSMContentContainer)
                    {
                        PSMSuper = (current as P_PSMContentContainer).Super as PSMSuperordinateComponent;
                        owner = (current as P_PSMContentContainer).P_PSMClass.Super as PSMClass;
                    }
                    List<PSMAttribute> PSMAttributes = new List<PSMAttribute>();
                    foreach (P_PSMAttribute A in (child as P_PSMAttributeContainer).Attributes)
                    {
                        Property At = owner.AddAttribute();
                        At.Name = A.Alias;
                        At.Lower = A.Lower;
                        /*if (Type != null && Type.Element != null)
                            createdAttribute.Type = Type.Element;*/
                        At.Upper = A.Upper;
                        At.Default = A.DefaultValue;
                        (At as PSMAttribute).Alias = A.Alias;
                        PSMAttributes.Add(At as PSMAttribute);
                        owner.PSMAttributes.Remove(At as PSMAttribute);

                    }
                    
                    if (PSMSuper != null) psmAttributeContainer = (PSMAttributeContainer)PSMSuper.AddComponent(PSMAttributeContainerFactory.Instance);
                    else psmAttributeContainer = (PSMAttributeContainer)owner.AddComponent(PSMAttributeContainerFactory.Instance);
                    foreach (PSMAttribute attribute in PSMAttributes) psmAttributeContainer.PSMAttributes.Add(attribute);
                    DiagramController.Diagram.AddModelElement(psmAttributeContainer, new PSMElementViewHelper(DiagramController.Diagram));
                }
                else if ((current is P_PSMClass) && child is P_PSMComment)
                {
                    Comment C = (current as P_PSMClass).Super.AddComment(NameSuggestor<Comment>.SuggestUniqueName((current as P_PSMClass).Super.Comments, "Comment", comment => comment.Body));
                    C.Body = (child as P_PSMComment).text;
                    DiagramController.Diagram.AddModelElement(C, new CommentViewHelper(DiagramController.Diagram));
                    
                }
                else if (!(current is P_PSMDiagram) && child is P_PSMContentContainer)
                {
                    P_PSMContentContainer CC = child as P_PSMContentContainer;

                    PSMContentContainer psmContainer = (PSMContentContainer)(current as P_PSMBase).Super.AddComponent(PSMContentContainerFactory.Instance);
                    psmContainer.Name = CC.ElementLabel;
                    CC.Super = psmContainer;
                    DiagramController.Diagram.AddModelElement(psmContainer, new PSMElementViewHelper(DiagramController.Diagram));

                    GeneratePSM2(CC);
                }
            }
        }
        
        void GeneratePSM(I_PSMHasChildren current)
        {
            foreach (I_PSMHasParent child in current.Children)
            {
                //TODO: Macrocommand
                if (child is P_PSMClass)
                {
                    //UPDATE GUI: This is wrong, but better than crash due to detected deadlock:
                    X.l.Content = (++currentClassCount).ToString() + "/" + X.ClassesCount.ToString() + " PSM Classes";
                    X.p.Value = currentClassCount;
                    if (currentClassCount % (Math.Min(25, X.ClassesCount / 10) + 1) == 0)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
                    }

                    P_PSMClass C = child as P_PSMClass;
                    ElementHolder<PSMClass> hldPSMClass = new ElementHolder<PSMClass>();
                    NewPSMClassCommand c0 = NewPSMClassCommandFactory.Factory().Create(DiagramController.ModelController) as NewPSMClassCommand;
                    c0.RepresentedClass = tempPIMClass;
                    c0.CreatedClass = hldPSMClass;
                    c0.Execute();
                    C.Super = hldPSMClass.Element;
                    //TODO commandy
                    hldPSMClass.Element.Name = C.Name.Name;
                    hldPSMClass.Element.ElementName = C.ElementLabel;

                    PSMClassToDiagram_ModelCommand c1_1 = PSMClassToDiagram_ModelCommandFactory.Factory().Create(DiagramController.ModelController) as PSMClassToDiagram_ModelCommand;
                    c1_1.Set(hldPSMClass, new HolderBase<PSMDiagram>(DiagramController.Diagram as PSMDiagram));
                    c1_1.Execute();

                    //Attributes
                    foreach (P_PSMAttribute A in C.Attributes)
                    {
                        NewAttributeCommand c5 = NewAttributeCommandFactory.Factory().Create(DiagramController.ModelController) as NewAttributeCommand;
                        c5.Owner = C.Super as PSMClass;
                        c5.Name = A.Alias;
                        c5.Default = A.DefaultValue;
                        c5.Lower = A.Lower;
                        c5.Upper = A.Upper;
                        ElementHolder<Property> h = new ElementHolder<Property>();
                        c5.createdAttributeHolder = h;
                        //Model.SimpleDataType T = DiagramController.Project.Schema..
                        //c5.Type.Element  //XmlSchemaType.GetBuiltInSimpleType(A.type);
                        c5.Execute();
                        (h.Element as PSMAttribute).Alias = A.Alias;
                    }
                    
                    if (current is P_PSMDiagram)
                    {
                        AddPSMClassToRoots_ModelCommand c1 = AddPSMClassToRoots_ModelCommandFactory.Factory().Create(DiagramController.ModelController) as AddPSMClassToRoots_ModelCommand;
                        c1.Set(hldPSMClass, new HolderBase<PSMDiagram>(DiagramController.Diagram as PSMDiagram));
                        c1.Execute();
                    }
                    else
                    {
                        if (C.ExtensionOf != null)
                        {
                            NewPSMSpecializationCommand c2s = NewPSMSpecializationCommandFactory.Factory().Create(DiagramController.ModelController) as NewPSMSpecializationCommand;
                            c2s.GeneralPSMClass = new ElementHolder<PSMClass>() { Element = (current as P_PSMClass).Super as PSMClass };
                            c2s.SpecificPSMClass = new ElementHolder<PSMClass>() { Element = C.Super as PSMClass };
                            ElementHolder<Generalization> hldGeneralization = new ElementHolder<Generalization>();
                            c2s.CreatedGeneralization = hldGeneralization;
                            c2s.Execute();

                            ElementToDiagramCommand<Generalization, GeneralizationViewHelper> c5s = (ElementToDiagramCommand<Generalization, GeneralizationViewHelper>)ElementToDiagramCommandFactory<Generalization, GeneralizationViewHelper>.Factory().Create(DiagramController);
                            c5s.IncludedElement = hldGeneralization;
                            c5s.Execute();
                        }
                        else
                        {
                            NewPSMAssociationCommand c2 = NewPSMAssociationCommandFactory.Factory().Create(DiagramController.ModelController) as NewPSMAssociationCommand;
                            ElementHolder<PSMAssociation> hldAssoc = new ElementHolder<PSMAssociation>();
                            c2.Lower = C.MinOccurs;
                            c2.Upper = C.MaxOccurs;
                            c2.Set((current as P_PSMBase).Super, hldPSMClass.Element, hldAssoc, null);
                            c2.Execute();

                            ElementToDiagramCommand<PSMAssociation, PSMAssociationViewHelper> c5 = (ElementToDiagramCommand<PSMAssociation, PSMAssociationViewHelper>)ElementToDiagramCommandFactory<PSMAssociation, PSMAssociationViewHelper>.Factory().Create(DiagramController);
                            c5.IncludedElement = hldAssoc;
                            c5.Execute();
                        }
                    }
                    GeneratePSM(C);
                }
                else if (!(current is P_PSMDiagram) && child is P_PSMContentChoice)
                {
                    NewPSMContentChoiceCommand c3 = NewPSMContentChoiceCommandFactory.Factory().Create(DiagramController) as NewPSMContentChoiceCommand;
                    c3.Parent = (current as P_PSMBase).Super;
                    c3.Execute();
                    (child as P_PSMContentChoice).Super = c3.CreatedChoice.Element;
                    GeneratePSM(child as P_PSMContentChoice);
                }
                else if (!(current is P_PSMDiagram) && child is P_PSMAttributeContainer)
                {
                    NewPSMAttributeContainerCommand c4 = NewPSMAttributeContainerCommandFactory.Factory().Create(DiagramController) as NewPSMAttributeContainerCommand;

                    PSMClass owner = null;
                    if (current is P_PSMClass)
                    {
                        owner = c4.PSMClass = (current as P_PSMClass).Super as PSMClass;
                    }
                    else if (current is P_PSMContentChoice)
                    {
                        c4.PSMSuper = (current as P_PSMContentChoice).Super as PSMSuperordinateComponent;
                        owner = c4.PSMClass = (current as P_PSMContentChoice).P_PSMClass.Super as PSMClass;
                    }
                    else if (current is P_PSMContentContainer)
                    {
                        c4.PSMSuper = (current as P_PSMContentContainer).Super as PSMSuperordinateComponent;
                        owner = c4.PSMClass = (current as P_PSMContentContainer).P_PSMClass.Super as PSMClass;
                    }
                    
                    foreach (P_PSMAttribute A in (child as P_PSMAttributeContainer).Attributes)
                    {
                        NewAttributeCommand c5 = NewAttributeCommandFactory.Factory().Create(DiagramController.ModelController) as NewAttributeCommand;
                        c5.Owner = owner;
                        c5.Name = A.Alias;
                        c5.Default = A.DefaultValue;
                        c5.Lower = A.Lower;
                        c5.Upper = A.Upper;
                        ElementHolder<Property> h = new ElementHolder<Property>();
                        c5.createdAttributeHolder = h;
                        c5.Execute();
                        (h.Element as PSMAttribute).Alias = A.Alias;
                        c4.PSMAttributes.Add(h.Element as PSMAttribute);
                    }
                    c4.Execute();
                }
                else if ((current is P_PSMClass) && child is P_PSMComment)
                {
                    NewModelCommentToDiagramCommand C = NewModelCommentaryToDiagramCommandFactory.Factory().Create(DiagramController) as NewModelCommentToDiagramCommand;
                    C.AnnotatedElement = (current as P_PSMClass).Super;
                    C.Text = (child as P_PSMComment).text;
                    C.Set(DiagramController.ModelController, DiagramController.ModelController.Model);
                    C.Execute();
                }
                else if (!(current is P_PSMDiagram) && child is P_PSMContentContainer)
                {
                    P_PSMContentContainer CC = child as P_PSMContentContainer;
                    NewPSMContentContainerCommand C = NewPSMContentContainerCommandFactory.Factory().Create(DiagramController) as NewPSMContentContainerCommand;
                    if (current is P_PSMBase) C.Parent = (current as P_PSMBase).Super;
                    C.Name = CC.ElementLabel;
                    C.Execute();
                    CC.Super = C.CreatedContainer.Element;

                    GeneratePSM(CC);
                }
            }
        }
    }
}
