using System;
using XCase.Model;

namespace XCase.Evolution
{
    [Flags]
    public enum EChangeScope
    {
        Class = 2 << 1,
        ClassUnion = 2 << 2,
        Association = 2 << 3,
        ContentChoice = 2 << 4, 
        ContentContainer = 2 << 5,
        AttributeContainer = 2 << 6,
        Attribute = 2 << 7, 
        Diagram = 2 << 8,
        /// <summary>
        /// PSM class or attribute container
        /// </summary>
        HasAttributes = Class | AttributeContainer,
        /// <summary>
        /// Psm class or content choice or content container; 
        /// elements that have <see cref="PSMSuperordinateComponent.Components">Components collection</see>
        /// </summary>
        Superordinate = Class | ContentChoice | ContentContainer,
        /// <summary>
        /// <para>Elements that have <see cref="PSMSubordinateComponent.Parent">Parent</see> and 
        /// can be placed in <see cref="PSMSuperordinateComponent.Components">Components collection</see> of 
        /// a <see cref="PSMSuperordinateComponent" />.
        /// </para>
        /// <para>
        /// Association, attribute container, content choice, content container
        /// </para>
        /// </summary>
        Subordinate = Association | AttributeContainer | ContentChoice | ContentContainer,
        /// <summary>
        /// Class or clas union.
        /// </summary>
        AssociationChild = Class | ClassUnion
    }
}