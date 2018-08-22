using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Model;

namespace XCase.Controller.Commands.Helpers
{
    /// <summary>
    /// Sometimes, you need to cast the Element in the holder from one type to another. You cannot do this directly.
    /// This command is meant to be placed in the Commands queue of a MacroCommand in the place where you need to 
    /// cast the type of the Element within the holder. Input are two holders of different type. When the command is 
    /// executed, it simply casts the element and copies it from the source Holder to the target Holder
    /// </summary>
    /// <typeparam name="SourceElementType">Source Holder</typeparam>
    /// <typeparam name="TargetElementType">Target Holder</typeparam>
    public class HolderConvertorCommand<SourceElementType, TargetElementType> : CommandBase
        where SourceElementType : class, Element 
        where TargetElementType : class, Element
    {
        private ElementHolder<SourceElementType> SourceHolder;
        private ElementHolder<TargetElementType> TargetHolder;

        public HolderConvertorCommand(ElementHolder<SourceElementType> sourceHolder, ElementHolder<TargetElementType> targetHolder)
        {
            Description = CommandDescription.HOLDER_CONVERTOR;
            SourceHolder = sourceHolder;
            TargetHolder = targetHolder;
        }

        public override bool CanExecute()
        {
            return true;
        }

        internal override void CommandOperation()
        {
            TargetHolder.Element = SourceHolder.Element as TargetElementType;
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            return OperationResult.OK;
        }
    }
}
