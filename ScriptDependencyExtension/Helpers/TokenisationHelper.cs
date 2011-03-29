using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptDependencyExtension.Constants;

namespace ScriptDependencyExtension.Helpers
{
	public class TokenisationHelper
	{
		public string TokeniseString(string filename)
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
		public string GenerateQueryStringRequestForDependencyNames(IEnumerable<string> dependenciesToCombine)
		{
			var queryString = new StringBuilder();
			var fileHelper = new TokenisationHelper();
			foreach (var dependencyName in dependenciesToCombine)
			{
				if (queryString.Length > 0)
				{
					queryString.Append(",");
				}
				else
				{
					queryString.AppendFormat("?{0}=", ScriptHelperConstants.CombinedScriptQueryStringIdentifier);
				}
				var tokenForFilename = fileHelper.TokeniseString(dependencyName);
				queryString.Append(tokenForFilename);
			}

			return queryString.ToString();
		}

		/// <summary>
		/// Takes a query string (as rendered by a script combine request) and generates the script dependencies names
		/// it represents
		/// </summary>
		/// <param name="queryString"></param>
		/// <returns></returns>
		public List<ScriptDependency> GetListOfDependencyNamesFromQueryStringTokens(string queryString, ScriptDependencyContainer scriptContainer)
		{
			List<ScriptDependency> dependencyNames = new List<ScriptDependency>();
			var tokenList = ParseQueryStringForTokens(queryString);
			tokenList.ForEach(t =>
			                  	{
			                  		var dep = scriptContainer.Dependencies.Find(s => s.ScriptNameToken == t);
									if (dep != null)
									{
										dependencyNames.Add(dep);
									}
			                  	});
			return dependencyNames;
		}

		private List<string> ParseQueryStringForTokens(string queryString)
		{
			List<string> tokens = new List<string>();
			if (!string.IsNullOrWhiteSpace(queryString) && queryString.Length > 3)
			{
				var queryStringIdentifier = string.Format("?{0}=", ScriptHelperConstants.CombinedScriptQueryStringIdentifier);
				int pos = queryStringIdentifier.IndexOf(queryStringIdentifier);
				if (pos >= 0)
				{
					pos += queryStringIdentifier.Length;
					int endPos = queryString.LastIndexOf("&");
					if (endPos <0)
					{
						endPos = queryString.Length - 1;
					}
					var tokenQueryStringContents = queryString.Substring(pos, (endPos - (pos-1)));
					var arrayOfTokens = tokenQueryStringContents.Split(',');
					tokens.AddRange(arrayOfTokens);
				}
			}
			return tokens;
		}
	}

}
