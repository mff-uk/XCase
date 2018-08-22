using XCase.Controller.Commands.Helpers;
using XCase.Model;
namespace XCase.Controller.Interfaces
{
    /// <summary>
    /// Marks that this controller manages PSM Attributes. Example: PSM_ClassController, PSM_AttributeContainer
    /// </summary>
    public interface IControlsPSMAttributes
    {
        void AddNewAttribute();
        void RenameAttribute(PSMAttribute attribute, string newName);
		void ChangeAttributeType(PSMAttribute attribute, ElementHolder<DataType> newType);
        void ChangeAttributeDefaultValue(PSMAttribute attribute, string newDefaultValue);
        void RemoveAttribute(PSMAttribute attribute);
        void ShowAttributeDialog(PSMAttribute attribute);
        IHasPSMAttributes AttributeHolder { get; }
    	void ChangeAttributeAlias(PSMAttribute attribute, string newAlias);
        void PropagatePIMLess(PSMAttribute attribute);
        void MoveAttributeUp(PSMAttribute attribute);
        void MoveAttributeDown(PSMAttribute attribute);
    }
}
