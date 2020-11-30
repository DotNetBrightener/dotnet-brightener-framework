using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Newtonsoft.Json;

namespace DotNetBrightener.Core.Utils
{
	/// <summary>
	/// Prevent the property that is marked with this attribute from being converted to dictionary, for mapping purpose
	/// </summary>
	public class DictionaryIgnoreAttribute: Attribute { }

    public static class ObjectToDictionaryHelper
	{
        public static IDictionary<string, object> ToDictionary(this object source, bool nestedProperties = false)
		{
			return source.ToDictionary<object>(nestedProperties);
		}

		public static IDictionary<string, T> ToDictionary<T>(this object source, bool nestedProperties = false)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source),
												"Unable to convert object to a dictionary. The source object is null.");

			var dictionary = new Dictionary<string, T>();

			foreach (var property in source.GetType().GetProperties())
			{
				AddPropertyToDictionary<T>(property, source, dictionary, nestedProperties);
			}
			return dictionary;
		}

		private static void AddPropertyToDictionary<T>(PropertyInfo property, object source, Dictionary<string, T> dictionary,
													   bool nestedProperties = false, string prefix = "")
		{
			// ignore the field's value if they are marked as ignored
			if (property.HasAttribute<JsonIgnoreAttribute>()||
                property.HasAttribute<DictionaryIgnoreAttribute>() || 
                property.HasAttribute<NotMappedAttribute>())
			{
				return;
			}

			// TODO: Putting logic to ignore those fields
            //bool shouldIgnoreId = false;
            //bool shouldIgnoreAuditFields = false;

            //if (source is BaseDtoWithAuditInfo)
            //{
            //    shouldIgnoreId = true;
            //    shouldIgnoreAuditFields = true;
            //}

			object value = null;
			try
			{
				value = property.GetValue(source);
			}
			catch (Exception exception)
			{
				return;
			}

			var propertyName = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

			if (value == null ||
			    property.PropertyType.IsPrimitive ||
			    value is int ||
			    value is bool ||
			    value is string ||
			    value is decimal ||
			    value is float ||
				value is DateTime ||
				value is DateTimeOffset ||
				!nestedProperties)
			{
				if (value == null || !IsOfType<T>(value))
				{
					dictionary.Add(propertyName, default(T));
				}
				else
				{
					dictionary.Add(propertyName, (T)value);
				}
				return;
			}

			var baseType = property.PropertyType.BaseType ?? property.PropertyType;
			if (baseType.FullName.Contains("Dictionary"))
			{
				var dictionaryValue = value as IDictionary<string, object>;
				if (dictionaryValue != null)
				{
					foreach (var dictionaryValueKey in dictionaryValue.Keys)
					{
						dictionary.Add(propertyName + "." + dictionaryValueKey, (T) dictionaryValue[dictionaryValueKey]);
					}
				}
				return;
			}

			foreach (var subProperty in value.GetType().GetProperties())
			{
				AddPropertyToDictionary<T>(subProperty, value, dictionary, nestedProperties, propertyName);
			}
		}

		private static bool IsOfType<T>(object value)
		{
			return value is T;
		}
	}
}