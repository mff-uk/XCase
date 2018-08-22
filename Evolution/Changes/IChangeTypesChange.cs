using XCase.Model;

namespace XCase.Evolution
{
    public interface IDoubleTargetChange
    {
        PSMElement SecondaryTarget { get; }
    }

    public interface IChangeWithEditTypeOverride
    {
        EEditType EditTypeOverride { get; }
    }

    public interface ICanBeIgnoredOnTarget
    {
        
    }
}