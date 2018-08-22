using System;
using System.Collections.Generic;
using XCase.Model;
using Version = XCase.Model.Version;

namespace XCase.Evolution
{
    public class ChangesDetectorContext
    {
        public List<EvolutionChange> DetectedChanges { get; private set; }

        public ChangesDetectorContext()
        {
            ScopeStack = new Stack<EChangeScope>();

            DetectedChanges = new List<EvolutionChange>();
        }

        public Stack<EChangeScope> ScopeStack { get; set; }

        public EChangeScope Scope
        {
            get
            {
                return ScopeStack.Peek();
            }
        }

        public Version OldVersion;

        public Version NewVersion;

        public PSMDiagram Diagram { get; set; }

        #region context fields

        public PSMElement CurrentPSMElement { get; set; }

        public PSMContentContainer CurrentContentContainer { get { return (PSMContentContainer) CurrentPSMElement; } }

        public PSMClass CurrentClass { get { return (PSMClass) CurrentPSMElement; } }

        public PSMAttribute CurrentAttribute { get { return (PSMAttribute) CurrentPSMElement; } }

        public PSMClassUnion CurrentClassUnion { get { return (PSMClassUnion) CurrentPSMElement; } }

        public PSMAssociation CurrentAssociation { get { return (PSMAssociation) CurrentPSMElement; } }

        public PSMContentChoice CurrentContentChoice { get { return (PSMContentChoice) CurrentPSMElement; } }

        public PSMAttributeContainer CurrentAttributeContainer { get { return (PSMAttributeContainer) CurrentPSMElement; } }

        #endregion 
    }
}