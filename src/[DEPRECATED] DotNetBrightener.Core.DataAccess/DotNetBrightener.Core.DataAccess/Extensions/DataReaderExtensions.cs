using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DotNetBrightener.Core.DataAccess.Extensions
{
	public static class DataReaderExtensions
	{
		public static IList<T> MapToList<T>(this IDataReader dr) where T : class
		{
			if (typeof(T).IsAbstract || typeof(T).IsInterface)
				throw new InvalidOperationException($"{typeof(T).FullName} must be a concrete class.");

			if (dr == null)
				return null;

			var type = typeof(T);
			var entities = new List<T>();

			var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			var propDict = props.ToDictionary(p => p.Name.ToUpper(), p => p);

			while (dr.Read())
			{
				var newObject = Activator.CreateInstance<T>();

				for (var index = 0; index < dr.FieldCount; index++)
				{
					if (propDict.ContainsKey(dr.GetName(index).ToUpper()))
					{
						var info = propDict[dr.GetName(index).ToUpper()];
						if (info != null && info.CanWrite)
						{
							var val = dr.GetValue(index);

							info.SetValue(newObject, val == DBNull.Value ? null : val, null);
						}

					}
				}

				entities.Add(newObject);
			}

			return entities;

		}
	}
}