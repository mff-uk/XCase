using System.Collections.ObjectModel;

namespace XCase.Model
{
    /// <summary>
    /// Represents a nesting join.
    /// </summary>
    public interface NestingJoin
    {
        PSMAssociation Association
        {
            get;
        }
        
        PIMClass CoreClass
        {
            get;
        }

        PIMPath Parent
        {
            get;
        }

        PIMPath Child
        {
            get;
        }

        ObservableCollection<PIMPath> Context
        {
            get;
        }

        PIMPath AddContextPath();

        PIMPath AddContextPath(int index);
    }
}
