using System;
using XCase.Model;

namespace XCase.Evolution
{
    public abstract class DiagramChange:EvolutionChange
    {
        public PSMDiagram Diagram
        {
            get; private set;
        }

        protected DiagramChange(PSMDiagram diagram)
        {
            Diagram = diagram;
        }

        public PSMDiagram DiagramOldVersion
        {
            get
            {
                return (PSMDiagram) Diagram.GetInVersion(OldVersion);
            }
        }

        public override EChangeScope Scope
        {
            get { return EChangeScope.Diagram; }
        }
    }

    // TODO: Transformation zmena target namespace!
}