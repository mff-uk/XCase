using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ConfigWizardUI;

namespace ConfigWizard
{
    public partial class MidPage_WarningError : ConfigWizardUI.InternalWizardPage
    {
        enum Level { IGNORE = 0, WARNING, ERROR }

        // dictionary to keep current values
        private Dictionary<string, int> values = new Dictionary<string, int>
        {
            {"XS_ABSTRACT_NOT_SPECIALIZED", (int)Level.WARNING},
            {"XS_ASSOCIATION_MULTIPLICITY_LOST", (int)Level.WARNING},
            {"XS_ATTRIBUTE_MULTIPLICITY_LOST", (int)Level.WARNING},
            {"XS_ATTRIBUTES_IN_CHOICE", (int)Level.WARNING},
            {"XS_CLASS_NAME_EMPTY", (int)Level.WARNING},
            {"XS_DUPLICATE_ROOT_ELEMENTS", (int)Level.WARNING},
            {"XS_ELEMENT_NAME_MISSING", (int)Level.WARNING},
            {"XS_NO_ROOT", (int)Level.WARNING},
            {"XS_NON_ABSTRACT_CLASS", (int)Level.WARNING},
            {"XS_SPECIALIZED_ATTRIBUTE_GROUP", (int)Level.WARNING},
            {"XS_TRANSLATED_ATTRIBUTE_ALIAS", (int)Level.WARNING},
            {"XS_TRANSLATED_CLASS_NAME", (int)Level.WARNING},
            {"XS_TYPE_TRANSLATED_AS_STRING", (int)Level.WARNING}
        };

        public MidPage_WarningError()
        {
            InitializeComponent();
            values = new Dictionary<string, int>();

            // init log level for each situation
            AbstractNotSpecializedCombo.SelectedIndex = (int)Level.WARNING;
            AssociationMultiplicityLostCombo.SelectedIndex = (int)Level.WARNING;
            AttributeMultiplicityLostCombo.SelectedIndex = (int)Level.WARNING;
            AttributesInChoiceCombo.SelectedIndex = (int)Level.WARNING;
            ClassNameEmptyCombo.SelectedIndex = (int)Level.WARNING;
            DuplicateRootElementsCombo.SelectedIndex = (int)Level.WARNING;
            ElementNameMissingCombo.SelectedIndex = (int)Level.WARNING;
            NonAbstractClassCombo.SelectedIndex = (int)Level.WARNING;
            NoRootCombo.SelectedIndex = (int)Level.WARNING;
            SpecializedAttributeGroupCombo.SelectedIndex = (int)Level.WARNING;
            TranslatedAttributeAliasCombo.SelectedIndex = (int)Level.WARNING;
            TranslatedClassNameCombo.SelectedIndex = (int)Level.WARNING;
            TypeTranslatedAsStringCombo.SelectedIndex = (int)Level.WARNING;
        }

        public Dictionary<string, int> getValues()
        {
            return values;
        }

        #region events handlers
        private void MiddlePage_SetActive(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetWizardButtons(WizardButtons.Back | WizardButtons.Next);
        }

        private void AbstractNotSpecializedCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            values["XS_ABSTRACT_NOT_SPECIALIZED"] = AbstractNotSpecializedCombo.SelectedIndex;
        }

        private void AssociationMultiplicityLostCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            values["XS_ASSOCIATION_MULTIPLICITY_LOST"] = AssociationMultiplicityLostCombo.SelectedIndex;
        }

        private void AttributeMultiplicityLostCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            values["XS_ATTRIBUTE_MULTIPLICITY_LOST"] = AttributeMultiplicityLostCombo.SelectedIndex;
        }

        private void AttributesInChoiceCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            values["XS_ATTRIBUTES_IN_CHOICE"] = AttributesInChoiceCombo.SelectedIndex;
        }

        private void ClassNameEmptyCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            values["XS_CLASS_NAME_EMPTY"] = ClassNameEmptyCombo.SelectedIndex;
        }

        private void DuplicateRootElementsCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            values["XS_DUPLICATE_ROOT_ELEMENTS"] = DuplicateRootElementsCombo.SelectedIndex;
        }

        private void ElementNameMissingCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            values["XS_ELEMENT_NAME_MISSING"] = ElementNameMissingCombo.SelectedIndex;
        }

        private void NoRootCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            values["XS_NO_ROOT"] = NoRootCombo.SelectedIndex;
        }

        private void NonAbstractClassCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            values["XS_NON_ABSTRACT_CLASS"] = NonAbstractClassCombo.SelectedIndex;
        }

        private void SpecializedAttributeGroupCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            values["XS_SPECIALIZED_ATTRIBUTE_GROUP"] = SpecializedAttributeGroupCombo.SelectedIndex;
        }

        private void TranslatedAttributeAliasCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            values["XS_TRANSLATED_ATTRIBUTE_ALIAS"] = TranslatedAttributeAliasCombo.SelectedIndex;
        }

        private void TranslatedClassNameCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            values["XS_TRANSLATED_CLASS_NAME"] = TranslatedClassNameCombo.SelectedIndex;
        }

        private void TypeTranslatedAsStringCombo_SelectedValueChanged(object sender, EventArgs e)
        {
            values["XS_TYPE_TRANSLATED_AS_STRING"] = TypeTranslatedAsStringCombo.SelectedIndex;
        }

        #endregion
    }
}
