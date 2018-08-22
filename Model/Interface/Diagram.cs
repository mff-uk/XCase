using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using XCase.Model.Serialization;

namespace XCase.Model
{
	/// <summary>
	/// Ancestor of PIM and PSM Diagrams, implements most of the functionality
	/// (adding and removing elements)
	/// </summary>
	public abstract class Diagram : INotifyPropertyChanged, _ImplVersionedElement
	{
        public XCaseGuid Guid { get; set; }

		#region Constructors

		/// <summary>
		/// Creates new instance of <see cref="Diagram" />. 
		/// </summary>
		protected Diagram()
			: this("Undefined")
		{
            Guid = XCaseGuid.NewGuid();
		}

		/// <summary>
		/// Creates new instance of <see cref="Diagram" />. 
		/// </summary>
		/// <param name="capt">caption of the diagram</param>
		protected Diagram(String capt)
		{
			diagramElements = new Dictionary<Element, ViewHelper>();
			caption = capt;
            Guid = XCaseGuid.NewGuid();
		}

		#endregion

		#region Properties

		protected String caption;

		/// <summary>
		/// Name of the diagram
		/// </summary>
		public String Caption
		{
			get
			{
				if (Version != null)
				{
					return String.Format("{0} {1}", caption, Version);
				}
				else
				{
					return caption;
				}
			}
			set
			{
				if (value != String.Empty)
				{
					caption = value;
					NotifyPropertyChanged("Caption");
				}
			}
		}

	    internal String CaptionNoVersion
	    {
	        get
	        {
	            return caption; 
	        }
	    }

		/// <summary>
		/// Underlying collection for <see cref="DiagramElements"/>
		/// </summary>
		protected Dictionary<Element, ViewHelper> diagramElements;

		/// <summary>
		/// Gets representation of elements in the diagram. Elements can be obtained 
		/// by using DiagramElements.Keys. Collection itself maps each elements to 
		/// its ViewHelper. Do not alter the collection directly, use 
		/// <see cref="AddModelElement"/> and <see cref="RemoveModelElement"/> methods 
		/// instead. 
		/// </summary>
		public Dictionary<Element, ViewHelper> DiagramElements
		{
			get { return diagramElements; }
		}

		/// <summary>
		/// References the project that owns this diagram.
		/// </summary>
		protected Project project;

		/// <summary>
		/// Gets or sets the project that owns this diagram.
		/// </summary>
		public Project Project
		{
			get { return project; }
			set { project = value; }
		}

		#endregion

		/// <summary>
		/// Occurs when a property value changes.       
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#region Implementation of IModelCloneable

		public Diagram CreateCopy(Model targetModel, Dictionary<Element, Element> elementsCopies, Dictionary<Element, ViewHelper> viewHelperCopies)
		{
			Diagram copyElement = Clone();
			FillCopy(copyElement, targetModel, elementsCopies, viewHelperCopies);
			return copyElement;
		}

		public virtual void FillCopy(Diagram copyDiagram, Model targetModel, IDictionary<Element, Element> elementCopies, Dictionary<Element, ViewHelper> viewHelperCopies)
		{
			copyDiagram.Caption = this.caption;

			if (targetModel.Schema == this.Project.Schema)
			{
				copyDiagram.Project = this.Project;
			}

			foreach (Element element in DiagramElements.Keys)
			{
				Element elementCopy = elementCopies != null ? elementCopies[element] : element;
				ViewHelper viewHelperCopy = viewHelperCopies[element];
				copyDiagram.AddModelElement(elementCopy, viewHelperCopy);
			}
		}

		public virtual Diagram Clone()
		{
			throw new NotImplementedException("Clone cannot be called in abstract class Diagram. ");
		}

		#endregion

		#region Add and remove element

		protected void NotifyPropertyChanged(string info)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(info));
		}

		/// <summary>
		/// Checks, whether <paramref name="element"/> is present in the diagram. 
		/// </summary>
		/// <param name="element">model element</param>
		/// <returns><code>true</code> if element is present in the diagram, <code>false</code> otherwise</returns>
		/// <seealso cref="DiagramElements"/>
		public bool IsElementPresent(Element element)
		{
			return DiagramElements.ContainsKey(element);
		}

		/// <summary>
		/// Removes element from the <see cref="DiagramElements"/> collection
		/// and fires the <see cref="ElementRemoved"/> event.
		/// </summary>
		/// <param name="element">removed elements</param>
		/// <seealso cref="DiagramElements"/>
		public virtual void RemoveModelElement(Element element)
		{
			if (!IsElementPresent(element))
				return;
			diagramElements.Remove(element);
			NotifyElementRemoved(this, element);
		}

		/// <summary>
		/// Called everytime an element is removed from the diagram.
		/// </summary>
		/// <seealso cref="RemoveModelElement"/>
		public event EventHandler<ElementRemovedEventArgs> ElementRemoved;

		/// <summary>
		/// Fires <see cref="ElementRemoved"/> event. 
		/// </summary>
		/// <param name="sender">sender of the event</param>
		/// <param name="element">removed element</param>
		/// <seealso cref="RemoveModelElement"/>
		public void NotifyElementRemoved(object sender, Element element)
		{
			if (ElementRemoved != null)
				ElementRemoved(sender, new ElementRemovedEventArgs(element));
		}


		/// <summary>
		/// Adds an element into the <see cref="DiagramElements"/> collection
		/// and fires the <see cref="ElementAdded"/> event.
		/// </summary>
		/// <param name="element">added element</param>
		/// <param name="visualization">visualization of the element in this diagram</param>
		/// <seealso cref="DiagramElements"/>
		public virtual void AddModelElement(Element element, ViewHelper visualization)
		{
			if (IsElementPresent(element))
				throw new ArgumentException(string.Format("Element {0} is already present in the diagram.", element));
			diagramElements.Add(element, visualization);
			NotifyElementAdded(this, element, visualization);
		}

		/// <summary>
		/// Called everytime an element is added into the diagram.
		/// </summary>
		/// <seealso cref="AddModelElement"/>
		public event EventHandler<ElementAddedEventArgs> ElementAdded;

		/// <summary>
		/// Fires <see cref="ElementAdded"/> event. 
		/// </summary>
		/// <param name="sender">sender of the event</param>
		/// <param name="element">added element</param>
		/// <param name="visualization">visualization of the element in this diagram</param>
		/// <seealso cref="AddModelElement"/>
		public void NotifyElementAdded(object sender, Element element, ViewHelper visualization)
		{
			if (ElementAdded != null)
				ElementAdded(sender, new ElementAddedEventArgs(element, visualization));
		}

		#endregion

		#region Implementation of IVersionedElement

		private IVersionedElement firstVersion;

		/// <summary>
		/// First version of the current element.
		/// </summary>
		public IVersionedElement FirstVersion
		{
			get { return firstVersion; }
			private set
			{
				firstVersion = value;
				NotifyPropertyChanged("FirstVersion");
			}
		}

		/// <summary>
		/// Version where the element appeared first. 
		/// Returns value of <see cref="_ImplVersionedElement.Version"/> property if 
		/// this is the first version of the element. 
		/// </summary>
		IVersionedElement _ImplVersionedElement.FirstVersion
		{
			get { return FirstVersion; }
			set { FirstVersion = value; }
		}

		private Version version;

		/// <summary>
		/// Version of the element
		/// </summary>
		public Version Version
		{
			get { return version; }
			private set
			{
				version = value;
				NotifyPropertyChanged("Version");
			}
		}

		/// <summary>
		/// Version of the element
		/// </summary>
		Version _ImplVersionedElement.Version
		{
			get { return Version; }
			set { Version = value; }
		}


		VersionManager _ImplVersionedElement.VersionManager
		{
			get { return VersionManager; }
			set { VersionManager = value; }
		}

        public bool IsFirstVersion
        {
            get { return FirstVersion == this; }
        }

	    private VersionManager versionManager;
		public VersionManager VersionManager
		{
			get { return versionManager; }
			private set
			{
				versionManager = value;
				NotifyPropertyChanged("VersionManager");
			}
		}

		#endregion
	}
}
