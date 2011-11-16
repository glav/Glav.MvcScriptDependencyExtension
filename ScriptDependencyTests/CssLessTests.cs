using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptDependencyExtension;
using System.IO;
using System.Diagnostics;

namespace ScriptDependencyTests
{
	[TestClass]
	public class CssLessTests
	{
		// Note: DeploymentItem is the most retarded attribute in the .net framework. It simply does not work reliably
		// So sometimes this test will or sometimes it will fail, depending on the order in which it is executed as some of the
		// deploymentitem files do not get deployed
		[TestMethod]
		[DeploymentItem(".\\")]
		[DeploymentItem("Test1.css")]
		[DeploymentItem("Test1.less")]
		[DeploymentItem("ScriptDependencies.xml")]
		public void ShouldConvertDotLessCssIntoRegularCss()
		{
			var context = new MockContext();
			ScriptDependencyLoader loader = new ScriptDependencyLoader(context);
			loader.FindDependencyFile();
			Assert.IsFalse(string.IsNullOrWhiteSpace(loader.DependencyResourceFile));

			loader.LoadDependencies();
			Assert.IsNotNull(loader.DependencyContainer);
			Assert.IsNotNull(loader.DependencyContainer.Dependencies);
			Assert.IsTrue(loader.DependencyContainer.Dependencies.Count > 0);

			var engine = new ScriptEngine(context, loader);
			StringBuilder builder = new StringBuilder(File.ReadAllText("Test1.css"));
			engine.ApplyFiltersToScriptOutput(builder, ScriptDependencyExtension.Constants.ScriptType.CSS, loader.DependencyContainer);
			
			var actual = builder.ToString();
			Assert.IsTrue(actual.Contains("div .class1{color:#fff}"));

		}
	}
}
