using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Model;
using XCase.Controller.Commands.Helpers;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Propagates a PIMLess PSM Attribute to the PIMClass represented by a PSMClass containing the PIMLess attribute.
    /// </summary>
    public class PropagatePIMLessMacroCommand : MacroCommand<ModelController>
    {
        public PropagatePIMLessMacroCommand(ModelController controller)
            : base(controller)
        {
            Description = CommandDescription.PROPAGATE_PIMLESS;
        }

        public void Set(PSMAttribute attribute)
        {
            //Only PIMLess attributes
            if (attribute.RepresentedAttribute != null) return;

            ElementHolder<Property> AttributeHolder = new ElementHolder<Property>();
            NewAttributeCommand c1 = NewAttributeCommandFactory.Factory().Create(Controller) as NewAttributeCommand;
            c1.createdAttributeHolder = AttributeHolder;
            c1.Owner = attribute.Class.RepresentedClass;
            c1.Lower = attribute.Lower;
            c1.Upper = attribute.Upper;
            c1.Default = attribute.Default;
            c1.Name = NameSuggestor<Property>.SuggestUniqueName(attribute.Class.RepresentedClass.Attributes, attribute.Name ?? attribute.Alias, property => property.Name);
            c1.Type = new ElementHolder<DataType>(attribute.Type);
            Commands.Add(c1);

            RenameElementCommand<Property> c2 = RenameElementCommandFactory<Property>.Factory().Create(Controller) as RenameElementCommand<Property>;
            c2.ContainingCollection = attribute.Class.Attributes;
            c2.RenamedElement = attribute;
            c2.NewName = c1.Name;
            Commands.Add(c2);

            PropagatePIMLessCommand c3 = PropagatePIMLessCommandFactory.Factory().Create(Controller) as PropagatePIMLessCommand;
            c3.Set(new ElementHolder<PSMAttribute>(attribute), AttributeHolder);
            Commands.Add(c3);
        }
    }

    #region PropagatePIMLessMacroCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="PropagatePIMLessMacroCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class PropagatePIMLessMacroCommandFactory : ModelCommandFactory<PropagatePIMLessMacroCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private PropagatePIMLessMacroCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of PropagatePIMLessMacroCommand
        /// <param name="modelController">Associated model controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new PropagatePIMLessMacroCommand(modelController);
        }
    }

    #endregion
}
