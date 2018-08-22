using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using XCase.Controller.DynamicCompilation;

namespace XCase.Controller
{
	/// <summary>
	/// Object that copies value of a property from one object to another object each time the value 
	/// changes. 
	/// </summary>
	public class PropertyBinder
	{
		/// <summary>
		/// Source object
		/// </summary>
		public INotifyPropertyChanged Source { get; private set; }

		/// <summary>
		/// Target object
		/// </summary>
		public object Target { get; private set; }

		/// <summary>
		/// Field of the <see cref="Source"/> that is bound to <see cref="TargetField"/>
		/// </summary>
		public string SourceField { get; private set; }

		/// <summary>
		/// Field of the <see cref="Target"/> that recieves updated values of from <see cref="SourceField"/>
		/// </summary>
		public string TargetField { get; private set; }

		protected GetHandler sourceGetHandler = null;
		
		protected SetHandler targetSetHandler = null;

		/// <summary>
		/// Creates new <see cref="PropertyBinder"/>, bject that copies value of a 
		/// property from one object to another object each time the value 
		/// changes. 
		/// </summary>
		/// <param name="source">Source object</param>
		/// <param name="target">Target object</param>
		/// <param name="sourceField">Field of the <paramref name="source"/> that is bound to <paramref name="targetField"/></param>
		/// <param name="targetField">Field of the <paramref name="target"/> that recieves updated values of from <paramref name="sourceField"/></param>
		public PropertyBinder(INotifyPropertyChanged source, object target, string sourceField, string targetField, Dictionary<KeyValuePair<Type, string>, GetHandler> getCache, Dictionary<KeyValuePair<Type, string>, SetHandler> setCache)
		{
			Source = source;
			Target = target;
			SourceField = sourceField;
			TargetField = targetField;

			Source.PropertyChanged += Source_PropertyChanged;
			KeyValuePair<Type, string> keyGet = new KeyValuePair<Type, string>(source.GetType(), sourceField);
			KeyValuePair<Type, string> keySet = new KeyValuePair<Type, string>(target.GetType(), targetField);

			if (getCache.ContainsKey(keyGet))
			{
				sourceGetHandler = getCache[keyGet];
			}
			else
			{
				PropertyInfo sourceProp = Source.GetType().GetProperty(SourceField);
				sourceGetHandler = DynamicMethodCompiler.CreateGetHandler(Source.GetType(), sourceProp); /**/
				getCache[keyGet] = sourceGetHandler;
			}

			if (getCache.ContainsKey(keySet))
			{
				targetSetHandler = setCache[keySet];
			}
			if (targetSetHandler == null)
			{
				PropertyInfo targetProp = Target.GetType().GetProperty(TargetField); /**/
				targetSetHandler = DynamicMethodCompiler.CreateSetHandler(Target.GetType(), targetProp);/**/
				setCache[keySet] = targetSetHandler;
			}
			
			//Debug.Assert(sourceProp != null, "Source property not found");
			//Debug.Assert(targetProp != null, "Target property not found");

			Source_PropertyChanged(null, new PropertyChangedEventArgs(SourceField));
		}

		protected virtual void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == SourceField)
			{
				//object value = sourceProp.GetValue(Source, null);	
				//targetProp.SetValue(Target, value, null);

				object value = sourceGetHandler(Source);
				targetSetHandler(Target, value);
			}
		}

		/// <summary>
		/// Disconnects the binding
		/// </summary>
		public void UnBind()
		{
			if (Source != null)
			{
				Source.PropertyChanged -= Source_PropertyChanged;
			}
		}


		///<summary>
		///Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
		///</summary>
		///
		///<returns>
		///A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
		///</returns>
		///<filterpriority>2</filterpriority>
		public override string ToString()
		{
			return String.Format("PropertyBinder {{0}.{1} => {2}.{3}", Source, SourceField, Target, TargetField);
		}
	}
}