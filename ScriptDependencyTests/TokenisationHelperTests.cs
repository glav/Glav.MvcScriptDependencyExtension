using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptDependencyExtension;
using ScriptDependencyExtension.Constants;
using ScriptDependencyExtension.Helpers;
using ScriptDependencyExtension.Model;

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
			Assert.IsTrue(queryString.Contains(string.Format("{0}=",ScriptHelperConstants.CombinedScriptQueryStringIdentifier)));
		}

		[TestMethod]
		public void UpperAndLowerCaseTokenisedNamesShouldEqual()
		{
			var helper = new TokenisationHelper();

			Assert.AreEqual<string>(helper.TokeniseString("one"), helper.TokeniseString("One"));
			Assert.AreEqual<string>(helper.TokeniseString("TWO"), helper.TokeniseString("two"));
			Assert.AreEqual<string>(helper.TokeniseString("ThReE"), helper.TokeniseString("tHrEe"));
		}

		[TestMethod]
		public void ShouldenerateDependencyNamesFromTokens()
		{
			ScriptDependencyContainer container = new ScriptDependencyContainer();
			var tokenHelper = new TokenisationHelper();

			var dep1 = new ScriptDependency();
			dep1.ScriptName = "one";
			dep1.ScriptNameToken = tokenHelper.TokeniseString(dep1.ScriptName);
			
			var dep2 = new ScriptDependency();
			dep2.ScriptName = "two";
			dep2.ScriptNameToken = tokenHelper.TokeniseString(dep2.ScriptName);
			
			container.Dependencies.Add(dep1);
			container.Dependencies.Add(dep2);

			var queryString = tokenHelper.GenerateQueryStringRequestForDependencyNames(container.Dependencies.Select(d => d.ScriptName).ToArray());

			var helper = new TokenisationHelper();
			var list = helper.GetListOfDependencyNamesFromQueryStringTokens(queryString,container);

			Assert.IsNotNull(list);
			Assert.IsTrue(list.Count == 2);
			Assert.AreEqual<string>("one", list[0].ScriptName);
			Assert.AreEqual<string>("two", list[1].ScriptName);
		}

		[TestMethod]
		public void ShouldGenerateUniqueTokensForVerySimilarStrings()
		{
			var helper = new TokenisationHelper();

			const string baseCompareValue = "SomeName";

			Assert.AreNotEqual<string>(helper.TokeniseString("1"), helper.TokeniseString("2"));
			Assert.AreNotEqual<string>(helper.TokeniseString("11"), helper.TokeniseString("12"));
			Assert.AreNotEqual<string>(helper.TokeniseString("111"), helper.TokeniseString("112"));
			Assert.AreNotEqual<string>(helper.TokeniseString("111"), helper.TokeniseString("211"));
			Assert.AreNotEqual<string>(helper.TokeniseString("11"), helper.TokeniseString("21"));
			Assert.AreNotEqual<string>(helper.TokeniseString("---1"), helper.TokeniseString("----1"));
			Assert.AreNotEqual<string>(helper.TokeniseString("22"), helper.TokeniseString("223"));
			Assert.AreNotEqual<string>(helper.TokeniseString("12"), helper.TokeniseString("21"));

			for (int i = 0; i < 100; i++)
			{
				var bareToken = i.ToString();
				var compareBareToken = (i + 1).ToString();
				var initialToken = string.Format("{0}__{1}", baseCompareValue, i);
				var secondToken = string.Format("{0}__{1}{1}", baseCompareValue, i);
				var thirdToken = string.Format("{0}_{1}", baseCompareValue, i);
				StringBuilder reversedToken = new StringBuilder();
				initialToken.Reverse().ToList().ForEach(c => reversedToken.Append(c));
				Assert.AreNotEqual<string>(helper.TokeniseString(bareToken), helper.TokeniseString(compareBareToken));
				Assert.AreNotEqual<string>(helper.TokeniseString(initialToken), helper.TokeniseString(secondToken));
				Assert.AreNotEqual<string>(helper.TokeniseString(initialToken), helper.TokeniseString(secondToken));
				Assert.AreNotEqual<string>(helper.TokeniseString(initialToken), helper.TokeniseString(thirdToken));
				Assert.AreNotEqual<string>(helper.TokeniseString(secondToken), helper.TokeniseString(thirdToken));
				Assert.AreNotEqual<string>(helper.TokeniseString(initialToken), helper.TokeniseString(reversedToken.ToString()));

			}

		}
	}
}
