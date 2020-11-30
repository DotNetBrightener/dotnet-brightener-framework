using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DotNetBrightener.Core.Utils
{
	public class JsonSerializerOptions
	{
		public static readonly JsonSerializerSettings PreventLoopJsonSerializer = new JsonSerializerSettings
		{
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore
		};


		public static readonly JsonSerializerSettings PreventLoopJsonSerializerWithCamelCase = new JsonSerializerSettings
		{
			ContractResolver = new CamelCasePropertyNamesContractResolver(),
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore
		};
	}
}