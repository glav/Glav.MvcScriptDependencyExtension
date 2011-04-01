using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptDependencyExtension.Constants;
using ScriptDependencyExtension.Model;

namespace ScriptDependencyExtension.Helpers
{
	public interface ITokenisationHelper
	{
		string TokeniseString(string filename);

		/// <summary>
		/// This will create a query string identifying the type of files that are to be combined.
		/// It will be in the form handler.axd?c=123,456,789
		/// Where the numbers represent unique tokens for each script dependency name
		/// </summary>
		/// <returns></returns>
		string GenerateQueryStringRequestForDependencyNames(IEnumerable<string> dependenciesToCombine);

		/// <summary>
		/// Takes a query string (as rendered by a script combine request) and generates the script dependencies names
		/// it represents
		/// </summary>
		/// <param name="queryString"></param>
		/// <returns></returns>
		List<ScriptDependency> GetListOfDependencyNamesFromQueryStringTokens(string queryString, ScriptDependencyContainer scriptContainer);
	}

	public class TokenisationHelper : ITokenisationHelper
	{
		public string TokeniseString(string textToTokenise)
		{
			Decimal nameValue = 0;
			if (!string.IsNullOrWhiteSpace(textToTokenise))
			{
				var normalisedText = textToTokenise.ToLowerInvariant();
				var unicodeBytes = System.Text.UnicodeEncoding.Unicode.GetBytes(normalisedText);
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
					queryString.AppendFormat("{0}=", ScriptHelperConstants.CombinedScriptQueryStringIdentifier);
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
				var queryStringIdentifier = string.Format("{0}=", ScriptHelperConstants.CombinedScriptQueryStringIdentifier);
				int pos = queryString.IndexOf(queryStringIdentifier);
				if (pos >= 0)
				{
					pos += queryStringIdentifier.Length;
					int endPos = queryString.IndexOf("&",pos);
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
