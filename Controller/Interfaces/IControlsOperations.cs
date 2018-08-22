using XCase.Model;
namespace XCase.Controller.Interfaces
{
    /// <summary>
    /// Marks that this controller manages operations. Example: ClassController, PSM_ClassController
    /// </summary>
    public interface IControlsOperations
    {
        void AddNewOperation(string operationName);
        void RemoveOperation(Operation operation);
        void RenameOperation(Operation operation, string newName);
        IHasOperations OperationHolder { get; }
    }
}
