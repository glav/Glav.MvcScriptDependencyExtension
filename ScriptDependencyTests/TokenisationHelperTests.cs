using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptDependencyExtension;
using ScriptDependencyExtension.Constants;
using ScriptDependencyExtension.Helpers;

namespace ScriptDependencyTests
{
	[TestClass]
	public class TokenisationHelperTests
	{
		[TestMethod]
		public void ShouldGenerateTokensForDependencyNames()
		{
			string[] testDependencies = new string[] {"one","two"};

			var helper = new TokenisationHelper();
			var queryString = helper.GenerateQueryStringRequestForDependencyNames(testDependencies);

			Assert.IsFalse(string.IsNullOrWhiteSpace(queryString));
			Assert.IsTrue(queryString.Contains(string.Format("?{0}=",ScriptHelperConstants.CombinedScriptQueryStringIdentifier)));
		}

		[TestMethod]
		public void ShouldenerateDependencyNamesFromTokens()
		{
			List<ScriptDependency> dependencies = new List<ScriptDependency>();
			var tokenHelper = new TokenisationHelper();

			var dep1 = new ScriptDependency();
			dep1.ScriptName = "one";
			dep1.ScriptNameToken = tokenHelper.TokeniseString(dep1.ScriptName);
			
			var dep2 = new ScriptDependency();
			dep2.ScriptName = "two";
			dep2.ScriptNameToken = tokenHelper.TokeniseString(dep2.ScriptName);
			
			dependencies.Add(dep1);
			dependencies.Add(dep2);

			var queryString = tokenHelper.GenerateQueryStringRequestForDependencyNames(dependencies.Select(d => d.ScriptNameToken).ToArray());

			var helper = new TokenisationHelper();
			var list = helper.GetListOfDependencyNamesFromQueryStringTokens(queryString);

			Assert.IsNotNull(list);
			Assert.IsTrue(list.Count == 2);
			Assert.AreEqual<string>("one", list[0]);
			Assert.AreEqual<string>("two", list[0]);
		}
	}
}
