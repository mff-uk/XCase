using System;
using XCase.Controller.Commands;
using XCase.Model;

namespace XCase.Controller
{
    public class PSM_DiagramReferenceController : NamedElementController
    {

        public PSM_DiagramReferenceController(PSMDiagramReference element, DiagramController diagramController) : base(element, diagramController)
        {
        }

        public PSMDiagramReference DiagramReference { get { return (PSMDiagramReference) NamedElement;  } }

        public void ChangeLocal(bool local)
        {
            PSMDiagramReferenceChangeLocalCommand command = (PSMDiagramReferenceChangeLocalCommand) PSMDiagramReferenceChangeLocalCommandFactory.Factory().Create(DiagramController);
            command.DiagramReference = this.DiagramReference;
            command.Local = local;
            command.Execute();
        }

        public void ChangeNamespace(string newNamespace)
        {
            PSMDiagramReferenceChangeNamespaceCommand command = (PSMDiagramReferenceChangeNamespaceCommand) PSMDiagramReferenceChangeNamespaceCommandFactory.Factory().Create(DiagramController);
            command.Namespace = newNamespace;
            command.DiagramReference = this.DiagramReference;
            command.Execute();
        }

        public void ChangeNamespacePrefix(string namespacePrefix)
        {
            PSMDiagramReferenceChangeNamespacePrefixCommand command = (PSMDiagramReferenceChangeNamespacePrefixCommand) PSMDiagramReferenceChangeNamespacePrefixCommandFactory.Factory().Create(DiagramController);
            command.NamespacePrefix = namespacePrefix;
            command.DiagramReference = this.DiagramReference;
            command.Execute();
        }

        public void ChangeReferencedDiagram(PSMDiagram referencedDiagram)
        {
            PSMDiagramReferenceChangeReferencedDiagramCommand command = (PSMDiagramReferenceChangeReferencedDiagramCommand) PSMDiagramReferenceChangeReferencedDiagramCommandFactory.Factory().Create(DiagramController);
            command.ReferencedDiagram = referencedDiagram;
            command.DiagramReference = this.DiagramReference;
            command.Execute();
        }

        public void ChangeSchemaLocation(string schemaLocation)
        {
            PSMDiagramReferenceChangeSchemaLocationCommand command = (PSMDiagramReferenceChangeSchemaLocationCommand) PSMDiagramReferenceChangeSchemaLocationCommandFactory.Factory().Create(DiagramController);
            command.SchemaLocation = schemaLocation;
            command.DiagramReference = this.DiagramReference;
            command.Execute();
        }
    }
}