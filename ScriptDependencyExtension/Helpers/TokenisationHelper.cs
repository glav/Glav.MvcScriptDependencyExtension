using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptDependencyExtension.Helpers
{
	public class TokenisationHelper
	{
		public string TokeniseFileName(string filename)
		{
			Decimal nameValue = 0;
			if (!string.IsNullOrWhiteSpace(filename))
			{
				var unicodeBytes = System.Text.UnicodeEncoding.Unicode.GetBytes(filename);
				for (int pos = 0; pos < unicodeBytes.Length; pos++)
				{
					nameValue += ((int)unicodeBytes[pos]) * (pos + 1);
				}
			}
			return nameValue.ToString();
		}

		/// <summary>
		/// This will create a query string identifying the type of files that are to be combined.
		/// It will be in the form handler.axd?c=123,456,789
		/// Where the numbers represent unique tokens for each script dependency name
		/// </summary>
		/// <returns></returns>
		public string GenerateQueryStringRequestForFiles(IEnumerable<string> dependenciesToCombine)
		{
			var queryString = new StringBuilder();
			var fileHelper = new TokenisationHelper();
			foreach (var filename in dependenciesToCombine)
			{
				if (queryString.Length > 0)
				{
					queryString.Append(",");
				}
				else
				{
					queryString.Append("?c=");
				}
				var tokenForFilename = fileHelper.TokeniseFileName(filename);
				queryString.Append(tokenForFilename);
			}

			return queryString.ToString();
		}

	}
}
