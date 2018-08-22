using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XCase.Model;

namespace XCase.Evolution
{
    /// <summary>
    /// Detects root removal in a diagram. The <see cref="RemovedRoot"/> is   
    /// removed from the diagram completely (not moved to another subtree). 
    /// </summary>
    [ChangeProperties(EChangeScope.Diagram, EEditType.Removal)]
    public class DiagramRootRemovedChange : DiagramChange, ISubelementRemovalChange
    {
        public override EEditType EditType
        {
            get { return EEditType.Removal; }
        }

        public PSMElement ChangedSubelement
        {
            get { return RemovedRoot; }
        }

        public PSMSuperordinateComponent RemovedRoot
        {
            get;
            private set;
        }

        public DiagramRootRemovedChange(PSMSuperordinateComponent removedRoot)
            : base(removedRoot != null ? removedRoot.Diagram : null)
        {
            RemovedRoot = removedRoot;
        }

        public override string ToString()
        {
            return string.Format(DispNullFormatProvider, "Root {0} removed from diagram {1}", RemovedRoot, Diagram);
        }

        public override void Verify()
        {
            base.Verify();

            Debug.Assert(RemovedRoot.Version == OldVersion);
            Debug.Assert(RemovedRoot.GetInVersion(NewVersion) == null);
            Debug.Assert(DiagramOldVersion.Roots.Cast<PSMSuperordinateComponent>().Contains(RemovedRoot));
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMDiagram diagram)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            PSMDiagram diagramOldVersion = (PSMDiagram)diagram.GetInVersion(v1);

            foreach (PSMSuperordinateComponent root in diagramOldVersion.Roots)
            {
                if (root.GetInVersion(v2) == null)
                {
                    result.Add(new DiagramRootRemovedChange(root) { OldVersion = v1, NewVersion = v2 });
                }
            }

            return result;
        }

        public override bool InvalidatesAttributes
        {
            get { return true; }
        }

        public override bool InvalidatesContent
        {
            get { return true; }
        }
    }
}