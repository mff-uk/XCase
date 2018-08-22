using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using XCase.UMLController.Commands;
using XCase.UMLController.DynamicCompilation;

namespace XCase.UMLController
{
	/// <summary>
	/// Helper class that creates a PropertyBinder for fields that have declared
	/// <see cref="ModelPropertyMappingAttribute"/> or <see cref="ViewHelperPropertyMappingAttribute">
	/// </summary>
	[Obsolete("Use IBindable interface and TypeBindingData instead - see XCaseClass for example")]
	public static class BindingInitializer
	{
		/// <summary>
		/// Creates property bindings for an object. 
		/// </summary>
		/// <param name="target">object whose properties are bound</param>
		/// <param name="propertyContainingSourceAttribute">Type of attribute used to declare a 
		/// property of <paramref name="target"/> whose value is used as a source of binding. See
		/// <see cref="ViewHelperElementAttribute"/> and <see cref="ModelElementAttribute"/>.</param>
		/// <param name="propertyBindingAttribute">Type of attribute used to declare that the property should be bound
		/// to a property of <paramref name="target"/>. See <see cref="ModelPropertyMappingAttribute"/> 
		/// or <see cref="ViewHelperPropertyMappingAttribute"/>.</param>
		public static void InitializeBindings(object target, Type propertyContainingSourceAttribute, Type propertyBindingAttribute)
		{
			if (target is IBindable)
			{
				// v teto properte bude zdroj bindingu
				PropertyInfo propertyContainingSource = FindFieldByAttribute(target, propertyContainingSourceAttribute);

				PropertyInfo[] properties = target.GetType().GetProperties();

				foreach (PropertyInfo info in properties)
				{
					BindField(target, target.GetType(), propertyBindingAttribute, propertyContainingSource, info);
				}
			}
			else
			{
				#region initialize caches

				if (getSourcePropDelegateCache == null)
					getSourcePropDelegateCache = new Dictionary<KeyValuePair<Type, string>, GetHandler>();
				if (setTargetPropDelegateCache == null)
					setTargetPropDelegateCache = new Dictionary<KeyValuePair<Type, string>, SetHandler>();

				#endregion

				PropertyInfo propertyContainingSource = FindFieldByAttribute(target, propertyContainingSourceAttribute);

				PropertyInfo[] properties = target.GetType().GetProperties();

				foreach (PropertyInfo info in properties)
				{
					BindField(target, target.GetType(), propertyBindingAttribute, propertyContainingSource, info);
				}
			}
		}

		private static Dictionary<KeyValuePair<Type, string>, GetHandler> getSourcePropDelegateCache;

		private static Dictionary<KeyValuePair<Type, string>, SetHandler> setTargetPropDelegateCache;

		private static void BindField(object target, Type targetType, Type attributeType, PropertyInfo sourceField, PropertyInfo info)
		{
			if (target.GetType().BaseType != null)
			{
				PropertyInfo baseProp = targetType.BaseType.GetProperty(info.Name);
				if (baseProp != null && baseProp.IsDefined(attributeType, true))
				{
					BindField(target, targetType.BaseType, attributeType, sourceField, baseProp);
				}
			}

			object[] propertyAttributes = info.GetCustomAttributes(attributeType, true);
			foreach (object attr in propertyAttributes)
			{
				if ((attr as PropertyMappingAttribute).PropertyField != null)
				{
					throw new NotImplementedException("Method or operation is not implemented.");
				}

				if ((attr as PropertyMappingAttribute).ConvertToString)
				{
					new StringPropertyBinder(
						(INotifyPropertyChanged) sourceField.GetValue(target, null), target,
						((PropertyMappingAttribute)attr).SourceFieldName, info.Name, getSourcePropDelegateCache, setTargetPropDelegateCache);
				}
				else
				{
					new PropertyBinder(
						(INotifyPropertyChanged)sourceField.GetValue(target, null), target,
						((PropertyMappingAttribute)attr).SourceFieldName, info.Name, getSourcePropDelegateCache, setTargetPropDelegateCache);

				}
			}
		}

		private static PropertyInfo FindFieldByAttribute(object Source, Type attributeType)
		{
			PropertyInfo result = null;

			PropertyInfo[] properties = Source.GetType().GetProperties();

			foreach (PropertyInfo info in properties)
			{
				if (info.IsDefined(attributeType, true))
				{
					if (result != null)
					{
						throw new InvalidOperationException(String.Format(CommandError.CMDERR_MODELELEMENT_DUPLICITY, Source.GetType()));	
					}

					if (!typeof(INotifyPropertyChanged).IsAssignableFrom(info.PropertyType))
					{
						throw new InvalidOperationException(CommandError.CMDERR_MODELELEMENT_NOT_FOUND);
					}
					result = info;		
				}
			}
			return result;
		}
	}
}
