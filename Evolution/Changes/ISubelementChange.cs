using XCase.Model;

namespace XCase.Evolution
{
    public interface ISubelementChange
    {
        PSMElement ChangedSubelement { get; }
        bool InvalidatesAttributes { get; }
        bool InvalidatesContent { get; }
    }

    public interface ISubelementAditionChange : ISubelementChange
    {

    }

    public interface ISubelementRemovalChange : ISubelementChange
    {

    }
}