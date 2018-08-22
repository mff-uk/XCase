using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XCase.Model;

namespace XCase.Evolution
{
    /// <summary>
    /// Detects new roots of PSM trees in a diagram. The <see cref="NewRoot"/> is a  
    /// new class added to the diagram (not a class that already existed in the diagram)
    /// </summary>
    [ChangeProperties(EChangeScope.Diagram, EEditType.Addition, MayRequireRevalidation = false)]
    public class DiagramRootAddedChange : DiagramChange, ISubelementAditionChange
    {
        public override EEditType EditType
        {
            get { return EEditType.Addition; }
        }

        public PSMElement ChangedSubelement
        {
            get { return NewRoot; }
        }

        public PSMSuperordinateComponent NewRoot
        {
            get; private set;
        }

        public DiagramRootAddedChange(PSMSuperordinateComponent newRoot): 
            base(newRoot != null ? newRoot.Diagram : null)
        {
            NewRoot = newRoot;
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "New root {0} added to diagram {1}", NewRoot, Diagram);
        }

        public override void Verify()
        {
            base.Verify();

            Debug.Assert(NewRoot.Version == NewVersion);
            Debug.Assert(NewRoot.GetInVersion(OldVersion) == null);
            Debug.Assert(Diagram.Roots.Cast<PSMSuperordinateComponent>().Contains(NewRoot));
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMDiagram diagram)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            PSMDiagram diagramOldVersion = (PSMDiagram) diagram.GetInVersion(v1);

            foreach (PSMSuperordinateComponent root in diagram.Roots)
            {
                if (root.GetInVersion(v1) == null)
                {
                    result.Add(new DiagramRootAddedChange(root) {OldVersion = v1, NewVersion = v2});
                }
            }

            return result;
        }

        public override bool InvalidatesAttributes
        {
            get { return false; }
        }

        public override bool InvalidatesContent
        {
            get { return false; }
        }
    }
}