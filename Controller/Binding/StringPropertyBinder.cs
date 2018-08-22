using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using XCase.Controller.DynamicCompilation;

namespace XCase.Controller
{
	/// <summary>
	/// Object that copies value of a property from one object to another object each time the value 
	/// changes. The value is always passed as string.
	/// </summary>
	public class StringPropertyBinder : PropertyBinder
	{
		/// <summary>
		/// Creates new <see cref="StringPropertyBinder"/>, 
		/// object that copies value of a property from one object to another object each time the value 
		/// changes. The value is always passed as string.
		/// </summary>
		/// <param name="source">Source object</param>
		/// <param name="target">Target object</param>
		/// <param name="sourceField">Field of the <paramref name="source"/> that is bound to <paramref name="targetField"/></param>
		/// <param name="targetField">Field of the <paramref name="target"/> that recieves updated values of from <paramref name="sourceField"/></param>
		public StringPropertyBinder(INotifyPropertyChanged source, object target, string sourceField, string targetField, Dictionary<KeyValuePair<Type, string>, GetHandler> getCache, Dictionary<KeyValuePair<Type, string>, SetHandler> setCache)
			: base(source, target, sourceField, targetField, getCache, setCache)
		{
			PropertyInfo targetProp = target.GetType().GetProperty(TargetField);
			if (targetProp.PropertyType != typeof(string))
			{
				throw new ArgumentException(string.Format("StringPropertyBinder only allows properties of type String to be bound. TargetField is of type: {0}.", targetProp.PropertyType));
			}
		}

		protected override void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == SourceField)
			{
				//object value = sourceProp.GetValue(Source, null);
				//targetProp.SetValue(Target, value.ToString(), null);

				object value = sourceGetHandler(Source);
				targetSetHandler(Target, value);
			}
		}

		public override string  ToString()
		{
			return String.Format("StringPropertyBinder {{0}.{1} => {2}.{3}", Source, SourceField, Target, TargetField);
		}
	}
}