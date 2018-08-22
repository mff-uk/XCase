using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using NUml.Uml2;
using System.Linq;
using XCase.Model.Implementation;

namespace XCase.Model
{
    /// <summary>
    /// Allows referencing constructs from one diagram in another diagrams.
    /// </summary>
    public class PSMDiagramReference : NamedElement, _ImplVersionedElement
    {
        private PSMDiagram referencedDiagram;
        public PSMDiagram ReferencedDiagram
        {
            get { return referencedDiagram; }
            set
            {
                referencedDiagram = value;
                NotifyPropertyChanged("ReferencedDiagram");
            }
        }

        private PSMDiagram referencingDiagram;
        public PSMDiagram ReferencingDiagram
        {
            get { return referencingDiagram; }
            set
            {
                referencingDiagram = value;
                NotifyPropertyChanged("ReferencingDiagram");
            }
        }

        public NamedElement GetChildByQualifiedName(string qName)
        {
            throw new NotImplementedException();
        }

        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

        private string namespacePrefix;
        public string NamespacePrefix
        {
            get { return namespacePrefix; }
            set
            {
                namespacePrefix = value;
                NotifyPropertyChanged("NamespacePrefix");
            }
        }

        private string xnamespace;
        public string Namespace
        {
            get { return xnamespace; }
            set { xnamespace = value;
                NotifyPropertyChanged("Namespace");}
        }

        private bool local;
        public bool Local
        {
            get { return local; }
            set
            {
                local = value;
                NotifyPropertyChanged("Local");
            }
        }

        private string schemaLocation;
        public string SchemaLocation
        {
            get { return schemaLocation; }
            set
            {
                schemaLocation = value;
                NotifyPropertyChanged("SchemaLocation");
            }
        }

        public string QualifiedName
        {
            get { return this.Name; }
        }

        private string ontologyEquivalent;
        public string OntologyEquivalent
        {
            get
            {
                return ontologyEquivalent;
            }
            set
            {
                ontologyEquivalent = value;
                NotifyPropertyChanged("OntologyEquivalent");
            }
        }

        private VisibilityKind visibility;

        public VisibilityKind Visibility
        {
            get
            {
                return visibility;
            }
            set
            {
                if (visibility != value)
                {
                    visibility = value;
                    NotifyPropertyChanged("Visibility");
                }
            }
        }

        public PSMDiagramReference()
        {
            comments = new ObservableCollection<Comment>();
            comments.CollectionChanged += OnCommentsChanged;
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
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (Comment comment in e.OldItems)
                {
                    _Comment implComment = (_Comment)comment;
                }
            }
        }

        public override string ToString()
        {
            return String.Format("Reference: {0}", SchemaLocation);
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sends the property changed notification to all the registered listeners.
        /// </summary>
        /// <param name="propertyName">Name of the changed property</param>
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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

        #region Implementation of IModelCloneable

        public virtual Element Clone(Model targetModel, ElementCopiesMap createdCopies)
        {
            return new PSMDiagramReference();
        }

        public virtual Element CreateCopy(Model targetModel, ElementCopiesMap createdCopies)
        {
            Element copyElement = Clone(targetModel, createdCopies);
            FillCopy(copyElement, targetModel, createdCopies);
            return copyElement;
        }

        public virtual void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
        {
            PSMDiagramReference copyReference = (PSMDiagramReference)copyElement;

            copyReference.SchemaLocation = this.SchemaLocation;
            copyReference.Local = this.Local;
            copyReference.Namespace = this.Namespace;
            copyReference.Name = this.Name;
            copyReference.NamespacePrefix = this.NamespacePrefix;
        }

        #endregion

        #region Implementation of Element

        public Comment AddComment()
        {
            throw new NotImplementedException();
        }

        public Comment AddComment(string body)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Indicates whether the comment is removed from the model.
        /// </summary>
        protected bool removedFromModel { get; private set; }

        private Dictionary<Version, IVersionedElement> unregisterBranchData;

        protected PSMDiagram removedFrom { get; set; }

        public virtual void RemoveMeFromModel()
        {
            if (removedFromModel)
                throw new InvalidOperationException(string.Format("RemoveMeFromModel called on element {0} that was already removed from model. ", this));
            if (VersionManager != null)
            {
                unregisterBranchData = VersionManager.UnregisterBranch(this);
            }
            removedFromModel = true;
            removedFrom = ReferencingDiagram;

        }

        public virtual void PutMeBackToModel()
        {
            if (!removedFromModel)
                throw new InvalidOperationException(string.Format("PutMeBackToModel called on element {0} that was not removed from model. ", this));
            if (VersionManager != null)
            {
                VersionManager.ReRegisterBranch(this, unregisterBranchData);
            }
            removedFromModel = false;
            ReferencingDiagram = removedFrom;
        }

        public ObservableCollection<StereotypeInstance> appliedSterotypes = new ObservableCollection<StereotypeInstance>();

        public ObservableCollection<StereotypeInstance> AppliedStereotypes
        {
            get { return appliedSterotypes; }
        }

        /// <summary>
        /// Collection of comments attributed to this element.
        /// </summary>
        private readonly ObservableCollection<Comment> comments;

        public ObservableCollection<Comment> Comments
        {
            get { return comments; }
        }

        public StereotypeTarget UMLMetaclass
        {
            get { throw new NotImplementedException(); }
        }

        public Schema Schema
        {
            get { return ReferencingDiagram.Project.Schema; }
        }

        public bool CanBePutToDiagram(Diagram targetDiagram, params Element[] ignoredElements)
        {
            throw new NotImplementedException();
        }

        public bool CanBePutToDiagram(Diagram targetDiagram, IEnumerable<Element> ignoredElements)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}