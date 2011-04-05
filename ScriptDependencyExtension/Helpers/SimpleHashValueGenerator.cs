using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptDependencyExtension.Helpers
{
	public class SimpleHashValueGenerator : IUniqueHashValueGenerator
	{
		public string ComputeHash(string input)
		{
			Decimal nameValue = 0;
			if (!string.IsNullOrWhiteSpace(input))
			{
				var normalisedText = input.ToLowerInvariant();
				var unicodeBytes = System.Text.UnicodeEncoding.Unicode.GetBytes(normalisedText);
				for (int pos = 0; pos < unicodeBytes.Length; pos++)
				{
					nameValue += ((int)unicodeBytes[pos]) * (pos + 1);
				}
			}
			return nameValue.ToString();
		}
	}
}
