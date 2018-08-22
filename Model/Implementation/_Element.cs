using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using XCase.Model.Serialization;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the base abstract class of all the UML model elements.
    /// </summary>
    /// <typeparam name="NUmlType">
    /// Type of the adapted nUML element, has to inherit from NUml.Uml2.Element
    /// </typeparam>
    internal abstract class _Element<NUmlType> : _ImplElement, _ImplVersionedElement
                                        where NUmlType : NUml.Uml2.Element
    {
        public XCaseGuid Guid { get; set; }

        #region Constructors

        /// <summary>
        /// Creates a new empty element.
        /// Adaptee is set to null, an empty comments collection is created.
        /// </summary>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        /// <param name="_metaclass">
        /// UML metaclass(es) that this element is instance of.
        /// Bitwise combination of StereotypeTarget values.
        /// </param>
        protected _Element(Schema _schema, StereotypeTarget _metaclass)
        {
            adaptedElement = default(NUmlType);
            
            comments = new ObservableCollection<Comment>();
            comments.CollectionChanged += OnCommentsChanged;

            appliedStereotypes = new ObservableCollection<StereotypeInstance>();

            schema = _schema;
            metaclass = _metaclass;

            if (schema.Model != null && schema.Model.VersionManager != null)
            {
                schema.Model.VersionManager.SetAsFirstVersion(this, schema.Model.Version);
            }

#if DEBUG
        	ElementWatcher.InvokeElementCreated(this);
#endif
            Guid = XCaseGuid.NewGuid();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a reference to the adapted nUML element.
        /// </summary>
        public NUmlType Adaptee
        {
            get { return adaptedElement; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends the property changed notification to all the registered listeners.
        /// </summary>
        /// <param name="propertyName">Name of the changed property</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Called whenever the comments collection has changed.
        /// </summary>
        /// <param name="sender">Object that has raised the event</param>
        /// <param name="e">Information about the change</param>
        protected void OnCommentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Comment comment in e.NewItems)
                {
                    _Comment implComment = comment as _Comment;
                    if (implComment == null)
                        throw new ArgumentException("A comment element created outside the model " + 
                            "library has been added to this element!");

                    adaptedElement.OwnedComment.Add(implComment.Adaptee);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Comment comment in e.OldItems)
                {
                    _Comment implComment = (_Comment)comment;

                    adaptedElement.OwnedComment.Remove(implComment.Adaptee);
                }
            }
        }

		/// <summary>
		/// Determines whether the element can be safely inserted in a diagram; this holds
		/// when all other elements that this element depends on are already inserted. For example
		/// a generalization can be inserted in a diagram only when both the general and the specific
		/// class are already present.
		/// </summary>
		/// <param name="targetDiagram">The target diagram</param>
		/// <param name="ignoredElements">elements that should be ignored when needed</param>
		/// <returns></returns>
		public bool CanBePutToDiagram(Diagram targetDiagram, params Element[] ignoredElements)
		{
			return CanBePutToDiagram(targetDiagram, (IEnumerable<Element>)ignoredElements);
		}

		/// <summary>
		/// Determines whether the element can be safely inserted in a diagram; this holds
		/// when all other elements that this element depends on are already inserted. For example
		/// a generalization can be inserted in a diagram only when both the general and the specific
		/// class are already present.
		/// </summary>
		/// <param name="targetDiagram">The target diagram</param>
		/// <param name="ignoredElements">elements that should be ignored when needed</param>
		/// <returns></returns>
    	public virtual bool CanBePutToDiagram(Diagram targetDiagram, IEnumerable<Element> ignoredElements)
    	{
			return true;
    	}

    	#endregion

        #region Element Members

        public Comment AddComment()
        {
            return AddComment("");
        }

        public Comment AddComment(string body)
        {
            Comment cmnt = new _Comment(this, schema);

            comments.Add(cmnt);
            cmnt.Body = body;
            return cmnt;
        }

        /// <summary>
        /// Indicates whether the comment is removed from the model.
        /// </summary>
        protected bool removedFromModel { get; private set; }

        private Dictionary<Version, IVersionedElement> unregisterBranchData;

        public virtual void PutMeBackToModel()
        {
            if (!removedFromModel)
                throw new InvalidOperationException(string.Format("PutMeBackToModel called on element {0} that was not removed from model. ", this));
            if (VersionManager != null)
            {
                VersionManager.ReRegisterBranch(this, unregisterBranchData);
            }
            removedFromModel = false; 
        }

        public virtual void RemoveMeFromModel()
        {
            if (removedFromModel)
                throw new InvalidOperationException(string.Format("RemoveMeFromModel called on element {0} that was already removed from model. ", this));
            if (VersionManager != null)
            {
                unregisterBranchData = VersionManager.UnregisterBranch(this);
            }
            removedFromModel = true; 
        }

		public virtual Element Clone(Model targetModel, ElementCopiesMap createdCopies)
		{
			// TODO udelat neco lepsiho nez InvalidOperationException
			throw new NotImplementedException(string.Format("Clone is not implemented for type {0}.", this.GetType().Name));
		}

		public Element CreateCopy(Model targetModel, ElementCopiesMap createdCopies)
		{
			Element copyElement = Clone(targetModel, createdCopies);
			FillCopy(copyElement, targetModel, createdCopies);
			return copyElement;
		}

    	public virtual void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
    	{
    		
    	}

    	public ObservableCollection<StereotypeInstance> AppliedStereotypes
        {
            get { return appliedStereotypes; }
        }
        
        public ObservableCollection<Comment> Comments
        {
            get { return comments; }
        }

        public virtual StereotypeTarget UMLMetaclass
        {
            get { return metaclass; }
        }

        public Schema Schema
    	{
    		get
    		{
    			return schema;
    		}
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

    	#region _ImplElement Members

        public NUml.Uml2.Element AdaptedElement
        {
            get { return adaptedElement; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Fields

        /// <summary>
        /// References the adapted nUML element.
        /// </summary>
        protected NUmlType adaptedElement;

        /// <summary>
        /// Collection of instances of stereotypes that were applied to this element.
        /// </summary>
        private readonly ObservableCollection<StereotypeInstance> appliedStereotypes;

        /// <summary>
        /// Collection of comments attributed to this element.
        /// </summary>
        private readonly ObservableCollection<Comment> comments;

        /// <summary>
        /// References the Schema instance that is the top
        /// of this model hierarchy.
        /// </summary>
        private readonly Schema schema;

        /// <summary>
        /// UML metaclass(es) which this element is instance of.<br />
        /// Bitwise combination of StereotypeTarget values.
        /// </summary>
        protected StereotypeTarget metaclass;

        #endregion

    }
}
