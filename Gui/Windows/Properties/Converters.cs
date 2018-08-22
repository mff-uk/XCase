using System;
using System.Windows;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Linq;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Controls;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using XCase.Controller.Dialogs;

namespace XCase.Gui
{
    #region General converters

    /// <summary>
    /// Converter interface for converters used within Properties window grids
    /// </summary>
    public abstract class PureConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType,
                   object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public abstract object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture);
    }

    /// <summary>
    ///  Converter for converting to a string
    /// </summary>
    public class ToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                   object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return value.ToString();

            return value;
        }

        public object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    /// <summary>
    /// Converter for displaying of derived PSM class
    /// </summary>
    public class DisplayDerivedClassConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                   object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is PSMClass)
            {
                if (((PSMClass)value).Diagram != null)
                    return ((PSMClass)value).Diagram.Caption + ":" + ((PSMClass)value).Name;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    /// <summary>
    /// Converter for displaying of derived PSM class
    /// </summary>
    public class DisplayRepresentedClassConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                   object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is PIMClass)
            {
                return (value as PIMClass).Name;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    /// <summary>
    /// Converter for replacement of all "," occurences with "\r\n" in the given string
    /// </summary>
    public class CutStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
                   object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                string converted = value.ToString().Replace(",", "\r\n");
                if (converted.EndsWith("\r\n"))
                    converted = converted.Remove(converted.Length - 2);

                return converted;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    #endregion

    #region Converters for AppearanceGrid

    /// <summary>
    /// Converter for numbers
    /// </summary>
    public class NumberConverter : IValueConverter
    {
        virtual public PositionableElementViewHelper viewHelper { get; set; }
        virtual public DiagramController diagramController { get; set; }

        public virtual object Convert(object value, Type targetType,
                      object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Double)
            {
                if (((Double)value).Equals(Double.NaN))
                    return "default";
                else
                    return Math.Round((Double)value);
            }

            return value;
        }

        public virtual object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    /// <summary>
    /// Converter of width of an element
    /// </summary>
    public class WidthConverter : NumberConverter
    {
        public override object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {

            ResizeElementCommand resizeElementCommand = (ResizeElementCommand)ResizeElementCommandFactory.Factory().Create(diagramController);
            resizeElementCommand.ViewHelper = viewHelper;
            resizeElementCommand.Height = viewHelper.Height;

            double d;
            if (double.TryParse((string)value, out d))
            {
                if (d.Equals(viewHelper.Width) || d < 0)
                    return value;

                resizeElementCommand.Width = d;
            }
            else
                return value;

            resizeElementCommand.Execute();

            return value;
        }
    }

    /// <summary>
    /// Converter of height of an element
    /// </summary>
    public class HeightConverter : NumberConverter
    {
        public override object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {

            ResizeElementCommand resizeElementCommand = (ResizeElementCommand)ResizeElementCommandFactory.Factory().Create(diagramController);
            resizeElementCommand.ViewHelper = viewHelper;
            resizeElementCommand.Width = viewHelper.Width;

            double d;
            if (double.TryParse((string)value, out d))
            {
                if (d.Equals(viewHelper.Height) || d < 0)
                    return value;

                resizeElementCommand.Height = d;
            }
            else
                return value;

            resizeElementCommand.Execute();

            return value;
        }
    }

    /// <summary>
    /// Converter of X coordinate of an element
    /// </summary>
    public class XConverter : NumberConverter
    {
        public override object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            MoveElementCommand moveElementCommand = (MoveElementCommand)MoveElementCommandFactory.Factory().Create(diagramController);
            moveElementCommand.ViewHelper = viewHelper;
            moveElementCommand.Y = viewHelper.Y;

            double d;
            if (double.TryParse((string)value, out d))
            {
                if (d.Equals(viewHelper.X))
                    return value;

                moveElementCommand.X = d;
            }
            else
                return value;

            moveElementCommand.Execute();

            return value;
        }
    }

    /// <summary>
    /// Converter of Y coordinate of an element
    /// </summary>
    public class YConverter : NumberConverter
    {
        public override object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            MoveElementCommand moveElementCommand = (MoveElementCommand)MoveElementCommandFactory.Factory().Create(diagramController);
            moveElementCommand.ViewHelper = viewHelper;
            moveElementCommand.X = viewHelper.X;

            double d;
            if (double.TryParse((string)value, out d))
            {
                if (d.Equals(viewHelper.Y))
                    return value;

                moveElementCommand.Y = d;
            }
            else
                return value;

            moveElementCommand.Execute();

            return value;
        }
    }

    #endregion

    #region Converters for Associations

    /// <summary>
    /// Multiplicity converter for PIM association (lower part)
    /// </summary>
    public class ChangeLowerConverter : IValueConverter
    {
        public ModelController modelController;
        public AssociationEnd selectedAssociationEnd;

        public object Convert(object value, Type targetType,
                      object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return value.ToString();

            return value;
        }

        public object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            if (modelController != null && selectedAssociationEnd != null && value != null)
            {

                if (((string)value).Equals(selectedAssociationEnd.Lower.ToString()))
                    return value;

                string newCardinality = ((string)value) + ".." + selectedAssociationEnd.Upper.ToString();

                uint? lower;
                NUml.Uml2.UnlimitedNatural upper;
                MultiplicityElementController.ParseMultiplicityString(newCardinality, out lower, out upper);
                MultiplicityElementController.ChangeMultiplicityOfElement(
                selectedAssociationEnd, selectedAssociationEnd.Association, lower, upper, modelController);

            }

            return value;
        }
    }

    /// <summary>
    /// Multiplicity converter for PIM Association (upper part)
    /// </summary>
    public class ChangeUpperConverter : IValueConverter
    {
        public ModelController modelController;
        public AssociationEnd selectedAssociationEnd;

        public object Convert(object value, Type targetType,
                      object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return value.ToString();

            return value;
        }

        public object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            if (modelController != null && selectedAssociationEnd != null && value != null)
            {
                if (((string)value).Equals(selectedAssociationEnd.Upper.ToString()))
                    return value;

                string newCardinality = selectedAssociationEnd.Lower.ToString() + ".." + ((string)value);

                uint? lower;
                NUml.Uml2.UnlimitedNatural upper;
                MultiplicityElementController.ParseMultiplicityString(newCardinality, out lower, out upper);
                MultiplicityElementController.ChangeMultiplicityOfElement(
                selectedAssociationEnd, selectedAssociationEnd.Association, lower, upper, modelController);
            }

            return value;
        }
    }

    /// <summary>
    /// Multiplicity converter for PSM association (lower part)
    /// </summary>
    public class ChangePSMLowerConverter : IValueConverter
    {

        public PSM_AssociationController controller;

        public object Convert(object value, Type targetType,
                      object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return value.ToString();

            return value;
        }

        public object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            if (controller != null)
            {
                if (((string)value).Equals(controller.PSMAssociation.Lower.ToString()))
                    return value;

                string newCardinality = ((string)value) + ".." + controller.PSMAssociation.Upper.ToString();

                //  uint? lower;
                // NUml.Uml2.UnlimitedNatural upper;

                controller.ChangeMultiplicity(newCardinality);

                //    MultiplicityElementController.ParseMultiplicityString(newCardinality, out lower, out upper);
                //    MultiplicityElementController.ChangeMultiplicityOfElement(
                //    selectedPSMAssociation.MultiplicityString, selectedPSMAssociation, lower, upper, modelController);

            }

            return value;
        }
    }

    /// <summary>
    /// Multiplicity converter for PSM Association (upper part)
    /// </summary>
    public class ChangePSMUpperConverter : IValueConverter
    {

        public PSM_AssociationController controller;

        public object Convert(object value, Type targetType,
                      object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return value.ToString();

            return value;
        }

        public object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            if (controller != null)
            {
                if (((string)value).Equals(controller.PSMAssociation.Upper.ToString()))
                    return value;

                string newCardinality = controller.PSMAssociation.Lower.ToString() + ".." + ((string)value);

                //  uint? lower;
                // NUml.Uml2.UnlimitedNatural upper;

                controller.ChangeMultiplicity(newCardinality);

                //    MultiplicityElementController.ParseMultiplicityString(newCardinality, out lower, out upper);
                //    MultiplicityElementController.ChangeMultiplicityOfElement(
                //    selectedPSMAssociation.MultiplicityString, selectedPSMAssociation, lower, upper, modelController);

            }

            return value;
        }
    }

    /// <summary>
    /// Converter for PIM Association role
    /// </summary>
    public class ChangeRoleConverter : PureConverter
    {
        public ModelController modelController;
        public AssociationEnd selectedAssociationEnd;

        public override object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            if (selectedAssociationEnd != null && modelController != null && value != null)
            {
                // No renaming
                if (selectedAssociationEnd.Name != null && selectedAssociationEnd.Name.Equals((string)value))
                    return value;

                RenameElementCommand<AssociationEnd> command = (RenameElementCommand<AssociationEnd>)RenameElementCommandFactory<AssociationEnd>.Factory().Create(modelController);
                command.NewName = (string)value;
                command.RenamedElement = selectedAssociationEnd;
                command.AssociatedElements.Add(selectedAssociationEnd.Association);
                command.Execute();
            }
            return value;
        }
    }

    /// <summary>
    /// Converter for renaming of PIM Association
    /// </summary>
    public class RenameAssociationConverter : PureConverter
    {
        public AssociationController associationController { get; set; }

        public override object ConvertBack(object value, Type targetType,
                             object parameter, System.Globalization.CultureInfo culture)
        {
            if (associationController != null && (string)value != null)
            {
                // No renaming
                if (associationController.Association.Name != null && associationController.Association.Name.Equals((string)value))
                    return value;
                // No renaming
                if (associationController.Association.Name == null && ((string)value).Equals(""))
                    return value;

                associationController.RenameElement((string)value);
            }
            return value;
        }
    }

    #endregion

    #region Converters for Classes

    /// <summary>
    /// Converter for changing name of a PSM class
    /// </summary>
    public class RenamePSMClassConverter : PureConverter
    {
        /// <summary>
        /// 
        /// </summary>
        public NamedElementController Controller { get; set; }

        public override object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            if (Controller != null && value != null)
            {
                Class c;
                IEnumerable<Class> containingCollection;
                if (Controller is ClassController)
                {
                    c = ((ClassController)Controller).Class;
                    containingCollection = c.Package != null ? c.Package.Classes.Cast<Class>() : null;
                }
                else
                {
                    if (Controller is PSM_ClassController)
                    {
                        c = ((PSM_ClassController)Controller).Class;
                        containingCollection = Controller.DiagramController.Diagram.DiagramElements.OfType<Class>();
                    }
                    else
                    {
                        return value;
                    }
                }

                // No renaming
                if (c.Name.Equals((string)value))
                    return value;

                Controller.RenameElement((string)value, containingCollection);
            }

            return value;
        }
    }

    /// <summary>
    /// Converter for changing name of a PIM class
    /// </summary>
    public class RenamePIMClassConverter : PureConverter
    {
        public ModelController Controller { get; set; }

        /// <summary>
        /// PIM Class to rename
        /// </summary>
        public Class selectedClass;

        public override object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            if (Controller != null && value != null)
            {

                IEnumerable<Class> containingCollection;

                containingCollection = selectedClass.Package != null ? selectedClass.Package.Classes.Cast<Class>() : null;
                // No renaming
                if (selectedClass.Name.Equals((string)value))
                    return value;

                RenameElementCommand<Class> command = (RenameElementCommand<Class>)RenameElementCommandFactory<Class>.Factory().Create(Controller);
                command.RenamedElement = selectedClass;
                command.ContainingCollection = containingCollection;
                command.NewName = (string)value;
                command.Execute();

                //Controller.RenameElement((string)value, containingCollection);
            }

            return value;
        }
    }

    /// <summary>
    /// Converter for changing element name (label) of a PSM class
    /// </summary>
    public class RenameElementConverter : PureConverter
    {
        public NamedElementController Controller { get; set; }

        public override object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            if (Controller != null && value != null)
            {
                // No renaming
                if (Controller.NamedElement.Name != null && Controller.NamedElement.Name.Equals((string)value))
                    return value;

                // PSM Class Element
                if (Controller is PSM_ClassController)
                {
                    ((PSM_ClassController)Controller).ChangeElementName((string)value);
                }
                else
                    // Content Container Element
                    if (Controller is PSM_ContentContainerController)
                    {
                        ((PSM_ContentContainerController)Controller).
                            RenameElement<PSMSubordinateComponent>((string)value, ((PSM_ContentContainerController)Controller).ContentContainer.Parent.Components);
                    }
            }

            return value;
        }
    }

    /// <summary>
    /// Converter for renaming a PIM attribute
    /// </summary>
    public class RenameAttributeConverter : PureConverter
    {
        public ModelController Controller { get; set; }
        public Property selectedAttribute { get; set; }
        public Class selectedClass { get; set; }

        public override object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            if (Controller != null && selectedClass != null && selectedAttribute != null)
            {
                // No renaming
                if (selectedAttribute.Name.Equals((string)value))
                    return value;

                RenameElementCommand<Property> renameElementCommand = (RenameElementCommand<Property>)RenameElementCommandFactory<Property>.Factory().Create(Controller);
                renameElementCommand.NewName = (string)value;
                renameElementCommand.AssociatedElements.Add(selectedClass);
                renameElementCommand.RenamedElement = selectedAttribute;
                renameElementCommand.ContainingCollection = selectedClass.Attributes;
                renameElementCommand.Execute();

            }

            return value;
        }
    }

    /// <summary>
    /// Converter for renaming a PSM attribute alias 
    /// </summary>
    public class RenameAliasConverter : PureConverter
    {
        public NamedElementController Controller { get; set; }
        public PSMAttribute selectedAttribute { get; set; }

        public override object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            if (Controller != null && selectedAttribute != null)
            {
                // No renaming
                if (selectedAttribute.Alias != null && selectedAttribute.Alias.Equals((string)value))
                    return value;

                if (Controller is PSM_ClassController)
                    ((PSM_ClassController)Controller).ChangeAttributeAlias(selectedAttribute, (string)value);
                else
                    if (Controller is PSM_AttributeContainerController)
                        ((PSM_AttributeContainerController)Controller).ChangeAttributeAlias(selectedAttribute, (string)value);


            }
            return value;
        }
    }

    /// <summary>
    /// Converter for renaming an operation of a PIM class
    /// </summary>
    public class RenameOperationConverter : PureConverter
    {
        public ModelController Controller { get; set; }
        public Operation selectedOperation { get; set; }

        public override object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            if (Controller != null && selectedOperation != null)
            {
                // No renaming
                if (selectedOperation.Name.Equals((string)value))
                    return value;

                RenameElementCommand<Operation> renameElementCommand =
    (RenameElementCommand<Operation>)RenameElementCommandFactory<Operation>.Factory().Create(Controller);
                renameElementCommand.NewName = (string)value;
                renameElementCommand.RenamedElement = selectedOperation;
                renameElementCommand.ContainingCollection = selectedOperation.Class.Operations;
                renameElementCommand.AssociatedElements.Add(selectedOperation.Class);
                renameElementCommand.Execute();


                //      ((ClassController)Controller).RenameOperation(selectedOperation, (string)value);

            }
            return value;
        }
    }

    #endregion

    #region Converters for Comments

    /// <summary>
    /// Converter for changing of a comment text
    /// </summary>
    public class ChangeCommentConverter : PureConverter
    {
        public XCaseComment SelectedComment { get; set; }

        public override object ConvertBack(object value, Type targetType,
                        object parameter, System.Globalization.CultureInfo culture)
        {
            if (SelectedComment != null && value != null)
            {
                // no renaming
                if (SelectedComment.CommentText.Equals((string)value))
                    return value;

                SelectedComment.Controller.ChangeComment((string)value);
            }
            return value;
        }
    }

    #endregion
}