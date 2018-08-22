using System;
using System.Collections.Generic;
using System.Linq;

namespace XCase.Model.Implementation
{
    /// <summary>
    /// Implementation of the UML Comment metaclass adapter.
    /// </summary>
    internal class _Comment : _Element<NUml.Uml2.Comment>, Comment
    {
        #region Constructors

        /// <summary>
        /// Creates a new empty comment.
        /// </summary>
        /// <param name="annotated">
        /// Reference to the element annotated by this comment
        /// </param>
        /// <param name="_schema">
        /// Reference to the Schema instance that is the top
        /// of this model hierarchy.
        /// </param>
        public _Comment(Element annotated, Schema _schema)
            : base(_schema, StereotypeTarget.Comment)
        {
            annotatedElement = annotated;
            adaptedElement = NUml.Uml2.Create.Comment();
        }

        #endregion

        #region Element Members

        public override void PutMeBackToModel()
        {
            base.PutMeBackToModel();

            if (!annotatedElement.Comments.Contains(this))
                annotatedElement.Comments.Add(this);
        }

        public override void RemoveMeFromModel()
        {
            base.RemoveMeFromModel();

            annotatedElement.Comments.Remove(this);
        }

        public override bool CanBePutToDiagram(Diagram targetDiagram, IEnumerable<Element> ignoredElements)
        {
            return targetDiagram.IsElementPresent(AnnotatedElement) || ignoredElements.Contains(AnnotatedElement) || AnnotatedElement is Model;
        }

        #region clone and copy

        public override Element Clone(Model targetModel, ElementCopiesMap createdCopies)
        {
            Element _annotatedElement;
            if (targetModel != this.Schema.Model || createdCopies.ContainsKey(AnnotatedElement))
            {
                _annotatedElement = createdCopies[AnnotatedElement];
            }
            else
            {
                _annotatedElement = AnnotatedElement;
            }
            Comment clone = new _Comment(_annotatedElement, targetModel.Schema);

            return clone;
        }

        public override void FillCopy(Element copyElement, Model targetModel, ElementCopiesMap createdCopies)
        {
            base.FillCopy(copyElement, targetModel, createdCopies);

            Comment copyComment = (Comment)copyElement;
            copyComment.Body = this.Body;
        }

        #endregion

        #endregion

        #region Comment Members

        public Element AnnotatedElement
        {
            get { return annotatedElement; }
        }

        public string Body
        {
            get
            {
                return adaptedElement.Body;
            }
            set
            {
                if (adaptedElement.Body != value)
                {
                    adaptedElement.Body = value;
                    NotifyPropertyChanged("Body");
                }
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// References the element annotated by this comment.
        /// </summary>
        protected Element annotatedElement;

        #endregion

        public override string ToString()
        {
            if (!String.IsNullOrEmpty(Body))
            {
                return string.Format("Comment: {0}", Body.Substring(0, Math.Min(10, Body.Length)));
            }
            else
            {
                return ("Comment (Empty)");
            }
        }
    }
}
