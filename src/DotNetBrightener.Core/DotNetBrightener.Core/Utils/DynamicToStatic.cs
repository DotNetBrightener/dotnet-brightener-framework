using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DotNetBrightener.Core.Utils
{
	public static class DynamicToStatic
	{
		public static T GetObjectInstance<T>(object expando) where T : class
		{
			var entity = Activator.CreateInstance<T>();

			//ExpandoObject implements dictionary
			var properties = expando as JObject;

			if (properties == null)
				return entity;
			try
			{
				var e = properties.ToObject<T>();

				return e;
			}
			catch (Exception)
			{
				var jsonstring = JsonConvert.SerializeObject(expando);
				var deserializedObject = JsonConvert.DeserializeObject<T>(jsonstring,
				                                                          new JsonSerializerSettings
				                                                          {
					                                                          DefaultValueHandling = DefaultValueHandling.Ignore
				                                                          });
				return deserializedObject;
			}
		}
	}
}