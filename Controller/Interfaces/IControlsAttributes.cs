using System.ComponentModel;
using XCase.Controller.Commands.Helpers;
using XCase.Model;

namespace XCase.Controller.Interfaces
{
    /// <summary>
    /// Marks that this controller manages attributes. Example: ClassController
    /// </summary>
    public interface IControlsAttributes
    {
        void AddNewAttribute(string attributeName);
        void RenameAttribute(Property attribute, string newName);
    	void ChangeAttributeType(Property attribute, ElementHolder<DataType> newType);
        void ChangeAttributeDefaultValue(Property attribute, string newDefaultValue);
        void RemoveAttribute(Property attribute);
        void ShowAttributeDialog(Property attribute);
        IHasAttributes AttributeHolder { get; }
    }
}
