using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XCase.Model;

namespace XCase.Evolution
{
    [ChangeProperties(EChangeScope.Diagram, EEditType.Migratory, MayRequireRevalidation = false)]
    public class DiagramRootIndexChange: DiagramChange
    {
        public PSMSuperordinateComponent MovedRoot
        {
            get
            {
                return (PSMSuperordinateComponent) Element;
            }
        }

        public PSMSuperordinateComponent MovedRootOldVersion
        {
            get
            {
                return (PSMSuperordinateComponent) MovedRoot.GetInVersion(OldVersion);
            }
        }

        public int OldIndex
        {
            get
            {
                return DiagramOldVersion.Roots.ToList().IndexOf(MovedRootOldVersion);
            }
        }

        public int NewIndex
        {
            get
            {
                return Diagram.Roots.ToList().IndexOf(MovedRoot);
            }
        }

        public override EEditType EditType
        {
            get { return EEditType.Migratory; }
        }

        public DiagramRootIndexChange(PSMSuperordinateComponent movedRoot) : 
            base(movedRoot != null ? movedRoot.Diagram : null)
        {
            Element = movedRoot;
        }

        public override void Verify()
        {
            base.Verify();
            Debug.Assert(OldIndex != NewIndex);
            Debug.Assert(OldIndex >= 0 && NewIndex >= 0);
        }

        public override string ToString()
        {
            return string.Format("Index of root {0} changed from {1} to {2}. ", MovedRoot, OldIndex, NewIndex);
        }

        public static IList<EvolutionChange> Detect(Version v1, Version v2, PSMDiagram diagram)
        {
            List<EvolutionChange> result = new List<EvolutionChange>();

            PSMDiagram diagramOldVersion = (PSMDiagram)diagram.GetInVersion(v1);

            foreach (PSMSuperordinateComponent root in diagram.Roots)
            {
                PSMSuperordinateComponent rootOldVersion = (PSMSuperordinateComponent) root.GetInVersion(v1);

                IList<PSMSuperordinateComponent> oldRoots = diagramOldVersion.Roots.ToList();
                if (rootOldVersion != null 
                    && oldRoots.Contains(rootOldVersion)
                    && oldRoots.IndexOf(rootOldVersion) != diagram.Roots.ToList().IndexOf(root)
                    )
                {
                    result.Add(new DiagramRootIndexChange(root) { OldVersion = v1, NewVersion = v2});
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