namespace XCase.Model
{
    /// <summary>
    /// A comment is a textual annotation that can be attached to a set of elements.
    /// </summary>
    public interface Comment : Element
    {
        /// <summary>
        /// Gets a reference to the element annotated by this comment.
        /// </summary>
        Element AnnotatedElement
        {
            get;
        }
        
        /// <summary>
        /// Gets or sets the string that is the comment.
        /// </summary>
        string Body
        {
            get;
            set;
        }
    }
}
