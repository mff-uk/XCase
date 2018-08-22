using System;
using XCase.Model;

namespace XCase.Evolution
{
    public class AttributeValueGenerator
    {
        readonly Translation.DataGenerator.DataTypeValuesGenerator valuesGenerator = new Translation.DataGenerator.DataTypeValuesGenerator(false);

        public string GenerateValue(PSMAttribute attribute)
        {
            if (!String.IsNullOrEmpty(attribute.Default))
            {
                return attribute.Default;
            }
            else
            {
                return valuesGenerator.GenerateValue(attribute.Type);
            }
        }       
    }
}