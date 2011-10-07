using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptDependencyExtension.Helpers
{
	//Note: This can be usedin place of the SimpleHashValueGenerator and will provide a much more
	//      unique name, however the length of the names aremuch larger and may cause the resulting URL
	//      using these unique names to be excessive.
	public class MD5HashValueGenerator : IUniqueHashValueGenerator
	{
		public string ComputeHash(string input)
		{
			if (!string.IsNullOrWhiteSpace(input))
			{
				var normalisedText = input.ToLowerInvariant();
				var hasher = System.Security.Cryptography.MD5.Create();
				var byteData = UTF8Encoding.UTF8.GetBytes(normalisedText);

				var hashBytes = hasher.ComputeHash(byteData);
				StringBuilder hashString = new StringBuilder();
				hashBytes.ToList().ForEach(h => hashString.Append(((ushort)h).ToString()));
				return hashString.ToString();
			}
			return string.Empty;

		}
	}
}
