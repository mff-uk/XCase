using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using XCase.Model.Serialization;

namespace XCase.Model
{
    public class ElementCopiesMap: Dictionary<Element, Element>
    {
        public Dictionary<Element, SubElementCopiesMap> SubElements =
            new Dictionary<Element, SubElementCopiesMap>();

        public void AddSubElementCopy(Element parentElement, Element subElement, Element subElementCopy)
        {
            if (!SubElements.ContainsKey(parentElement))
            {
                SubElements[parentElement] = new SubElementCopiesMap();
            }
            SubElements[parentElement][subElement] = subElementCopy;
        }

        public SubElementCopiesMap GetSubElementsList(Element parentElement)
        {
            if (!SubElements.ContainsKey(parentElement))
            {
                SubElements[parentElement] = new SubElementCopiesMap();
            }
            
            return SubElements[parentElement];
        }
    }

    public class SubElementCopiesMap : Dictionary<Element, Element> { }

	public interface IModelCloneable
	{
		Element Clone(Model targetModel, ElementCopiesMap createdCopies);
		Element CreateCopy(Model targetModel, ElementCopiesMap createdCopies);
		void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies);
	}

    /// <summary>
    /// Abstract base class of all the elements in the UML model.
    /// </summary>
    public interface Element : INotifyPropertyChanged, IVersionedElement, IModelCloneable
	{
        /// <summary>
        /// Adds a new empty comment to this element.
        /// </summary>
        /// <returns>Reference to the new comment</returns>
        Comment AddComment();

        /// <summary>
        /// Adds a new comment with the given body to this element.
        /// </summary>
        /// <param name="body">Body of the comment</param>
        /// <returns>Reference to the new comment</returns>
        Comment AddComment(string body);

        /// <summary>
        /// Reinserts this element to the model after its removal.
        /// </summary>
        /// <exception cref="NotSupportedException">Element is not removed from the model</exception>
        void PutMeBackToModel();

        /// <summary>
        /// Removes this element from the model.
        /// </summary>
        /// <note type="caution">
        /// Related elements (e.g. associations connected to the removed class) ARE NOT REMOVED!
        /// Child elements ARE REMOVED (e.g. attributes of the removed class).
        /// By using this method inappropriately you can introduce inconsistencies in the model!
        /// </note>
        /// <exception cref="NotSupportedException">Element is already removed</exception>
        void RemoveMeFromModel();

        /// <summary>
        /// Gets a collection of instances of stereotypes that were 
        /// applied to this element.
        /// </summary>
        /// <value>
        /// Type: ObservableCollection of XCase.Model.StereotypeInstance<br />
        /// </value>
        ObservableCollection<StereotypeInstance> AppliedStereotypes
        {
            get;
        }
        
        /// <summary>
        /// Gets a collection of comments attributed to this element.
        /// </summary>
        ObservableCollection<Comment> Comments
        {
            get;
        }

        /// <summary>
        /// Gets the UML metaclass (or possibly metaclasses) which this element is instance of.<br />
        /// The metaclasses are given as a bitwise combination of StereotypeTarget values.
        /// </summary>
        StereotypeTarget UMLMetaclass
        {
            get;
        }
		
		/// <summary>
		/// Returns schema of the element
		/// </summary>
		Schema Schema
		{ 
			get;
		}

		/// <summary>
		/// Determines whether the element can be safely inserted in a diagram; this holds
		/// when all other elements that this element depends on are already inserted. For example
		/// a generalization can be inserted in a diagram only when both the general and the specific
		/// class are already present.
		/// </summary>
		/// <param name="targetDiagram">The target diagram</param>
		/// <param name="ignoredElements">Elements that should be ignored when needed</param>
		bool CanBePutToDiagram(Diagram targetDiagram, params Element[] ignoredElements);

		/// <summary>
		/// Determines whether the element can be safely inserted in a diagram; this holds
		/// when all other elements that this element depends on are already inserted. For example
		/// a generalization can be inserted in a diagram only when both the general and the specific
		/// class are already present.
		/// </summary>
		/// <param name="targetDiagram">The target diagram</param>
		/// <param name="ignoredElements">Elements that should be ignored when needed</param>
		bool CanBePutToDiagram(Diagram targetDiagram, IEnumerable<Element> ignoredElements);
	}

	public static class ElementWatcher
	{
		public static event Action<Element> ElementCreated;

		private static readonly List<Element> createdElements = new List<Element>();

		public static IList<Element> CreatedElements
		{
			get
			{
				return createdElements.AsReadOnly();
			}
		}

		public static bool Recording { get; set; }

		public static void ClearRecording()
		{
			createdElements.Clear();
		}

		public static void InvokeElementCreated(Element createdElement)
		{
			if (ElementCreated != null)
			{
				ElementCreated(createdElement);
			}

			if (Recording)
			{
				createdElements.Add(createdElement);
			}
		}
	}
}
