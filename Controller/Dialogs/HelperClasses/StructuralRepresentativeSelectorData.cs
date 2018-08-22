using XCase.Model;
using System.Collections.Generic;

namespace XCase.Controller.Dialogs
{
    /// <summary>
    /// This class is used for the ListBox of the SelectRepresentantDialog.
    /// It stores all posibilities for PSMClasses to become represented and
    /// root PSMClasses to become reconnected during the AddPSMChildrenMacroCommand.
    /// </summary>
    public class StructuralRepresentativeSelectorData
    {
        /// <summary>
        /// The TreeClasses structure representing the PIMClass to be derived from and all relevant data
        /// </summary>
        public TreeClasses TreeClass { get; set; }

        /// <summary>
        /// All possibilities for represented PSMClass
        /// </summary>
        public List<PSMClass> PossibleRepresented { get; set; }

        /// <summary>
        /// All possibilities for root reconnection
        /// </summary>
        public List<PSMClass> PossibleRoots { get; set; }

        /// <summary>
        /// This property is propagated to the Dialog as IsEnabled for the "root reconnection" section
        /// </summary>
        public bool RootsSelectionEnabled { get; set; }

        /// <summary>
        /// The PSMClass selected by the user to be represented
        /// </summary>
        public PSMClass SelectedRepresentative { get; set; }

        /// <summary>
        /// The root PSMClass selected by the user to be reconnected
        /// </summary>
        public PSMClass SelectedRootClass { get; set; }

        /// <summary>
        /// This property corresponds to the "Structural Representative" RadioButton in the Dialog
        /// </summary>
        public bool Representative { get; set; }

        /// <summary>
        /// This property corresponds to the "Root class" RadioButton in the Dialog
        /// </summary>
        public bool ExistingRootClass { get; set; }
    }
}
