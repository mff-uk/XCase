using System;

namespace XCase.Evolution
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class ChangePropertiesAttribute : Attribute
    {
        private readonly EChangeScope scope;
        public EChangeScope Scope
        {
            get { return scope; }
        }

        private readonly EEditType editType;

        public EEditType EditType
        {
            get { return editType; }
        }

        /// <summary>
        /// Default is true.
        /// </summary>
        public bool MayRequireRevalidation { get; set; }

        public ChangePropertiesAttribute(EChangeScope scope, EEditType editType)
        {
            this.scope = scope;
            this.editType = editType;
            MayRequireRevalidation = true;
        }
    }
}