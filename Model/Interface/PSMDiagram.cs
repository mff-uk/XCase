using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// Platform Specific Model Diagram representation
    /// </summary>
	public class PSMDiagram : Diagram
	{
        private string targetNamespace;
        public string TargetNamespace
        {
            get { return targetNamespace; }
            set { targetNamespace = value;
                NotifyPropertyChanged("TargetNamespace");}
        }

        /// <summary>
		/// Roots collection contains all root PSMSuperordinateComponents of a PSM Diagram (PSMClass, PSMContentContainer)
		/// </summary>
		public ObservableCollection<PSMSuperordinateComponent> Roots { get; private set; }

        public IEnumerable<PSMSuperordinateComponent> RootsWithSpecifications
        {
            get
            {
                List<PSMSuperordinateComponent> result = new List<PSMSuperordinateComponent>();
                foreach (PSMSuperordinateComponent root in Roots)
                {
                    GetSpecificationsRecursive(root, ref result);
                }
                return result;
            }
        }

        public IEnumerable<PSMDiagramReference> DiagramReferences
        {
            get { return DiagramElements.Keys.OfType<PSMDiagramReference>(); }
        }

        private static void GetSpecificationsRecursive(PSMSuperordinateComponent root, ref List<PSMSuperordinateComponent> result)
        {
            result.Add(root);
            PSMClass psmClass = root as PSMClass;
            if (psmClass != null)
            {
                foreach (Generalization specification in psmClass.Specifications)
                {
                    GetSpecificationsRecursive((PSMClass) specification.Specific, ref result);
                }
            }
        }

        

		/// <summary>
		/// Creates new instance of <see cref="PSMDiagram" />. 
		/// </summary>
		/// <param name="caption">caption of the diagram</param>
		public PSMDiagram(string caption)
			: base(caption)
		{
			Roots = new ObservableCollection<PSMSuperordinateComponent>();
		}

    	#region Overrides of Diagram

		public override Diagram Clone()
		{
			return new PSMDiagram(Caption);
		}

		public override void FillCopy(Diagram copyDiagram, Model targetModel, IDictionary<Element, Element> elementCopies, Dictionary<Element, ViewHelper> viewHelperCopies)
		{
			base.FillCopy(copyDiagram, targetModel, elementCopies, viewHelperCopies);

			PSMDiagram copyPsmDiagram = (PSMDiagram) copyDiagram;

		    copyPsmDiagram.TargetNamespace = this.TargetNamespace;

			foreach (PSMClass root in Roots)
			{
				copyPsmDiagram.Roots.Add((PSMClass)elementCopies[root]);
			}
		}

    	/// <summary>
    	/// Removes element from the <see cref="Diagram.DiagramElements"/> collection
    	/// and fires the <see cref="Diagram.ElementRemoved"/> event.
    	/// </summary>
    	/// <param name="element">removed elements</param>
    	/// <seealso cref="Diagram.DiagramElements"/>
    	public override void RemoveModelElement(Element element)
    	{
			PSMElement psmElement = element as PSMElement;
    		if (psmElement != null)
    		{
    			if (psmElement.Diagram != this)
    			{
					throw new InvalidOperationException(string.Format("Element {0} does not belong to diagram {1} or is in inconsistent state.", psmElement, this.Caption));
    			}
    		}
    		base.RemoveModelElement(element);
    		if (psmElement != null)
    		{
				// Clearing the diagram property was removed, because the value is used for removed elements
				// when they are being put back to the diagram during UNDO
				//psmElement.Diagram = null;
    		}
    	}

    	/// <summary>
    	/// Adds an element into the <see cref="Diagram.DiagramElements"/> collection
    	/// and fires the <see cref="Diagram.ElementAdded"/> event.
    	/// </summary>
    	/// <param name="element">added element</param>
    	/// <param name="visualization">visualization of the element in this diagram</param>
    	/// <seealso cref="Diagram.DiagramElements"/>
    	public override void AddModelElement(Element element, ViewHelper visualization)
    	{
			PSMElement psmElement = element as PSMElement;
			if (psmElement != null)
			{
				if (psmElement.Diagram != null && psmElement.Diagram != this)
				{
					throw new InvalidOperationException(string.Format("Element {0} belongs to another diagram ({1}) or is in inconsistent state.", psmElement, psmElement.Diagram.Caption));
				}
				if (psmElement.Diagram == null)
				{
					psmElement.Diagram = this; 
				}
			}
			base.AddModelElement(element, visualization);
    	}

        #endregion
	}
}
