using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptDependencyExtension;
using System.IO;
using ScriptDependencyExtension.Http;

namespace ScriptDependencyTests
{
	[TestClass]
	public class FileCombinationTests
	{
		[TestMethod]
		[DeploymentItem("TestFile1.txt")]
		[DeploymentItem("TestFile2.txt")]
		public void ShouldCombineFiles()
		{
			var fileCombiner = new FileCombiner(new HttpContextAdapter(), new string[] {"TestFile1.txt", "TestFile2.txt"});
			var result = fileCombiner.CombineFiles("js");

			Assert.IsNotNull(result);
			Assert.IsFalse(string.IsNullOrWhiteSpace(result.CmbinedFileContents));
			Assert.IsFalse(string.IsNullOrWhiteSpace(result.CombinedFileName));

			// Make sure it contains the combined contents
			Assert.IsTrue(result.CmbinedFileContents.Contains("This is testfile 1"));
			Assert.IsTrue(result.CmbinedFileContents.Contains("This is testfile 2"));
		}
		
		[TestMethod]
		[DeploymentItem("TestFile1.txt")]
		[DeploymentItem("TestFile2.txt")]
		public void ShouldGenerateUniqueCombinedFileNames()
		{
			var fileCombiner1 = new FileCombiner(new HttpContextAdapter(), new string[] { "TestFile1.txt", "TestFile2.txt" });
			var fileCombiner2 = new FileCombiner(new HttpContextAdapter(), new string[] { "TestFile1.txt" });
			var fileCombiner3 = new FileCombiner(new HttpContextAdapter(), new string[] { "TestFile2.txt" });
			var fileCombiner4 = new FileCombiner(new HttpContextAdapter(), new string[] { "TestFile1.txt", "TestFile2.txt" });
			var result1 = fileCombiner1.CombineFiles("js");
			var result2 = fileCombiner2.CombineFiles("js");
			var result3 = fileCombiner3.CombineFiles("js");
			var result4 = fileCombiner4.CombineFiles("js");

			Assert.IsNotNull(result1);
			Assert.IsNotNull(result2);
			Assert.IsNotNull(result3);
			Assert.IsNotNull(result4);

			Assert.AreNotEqual<string>(result1.CombinedFileName, result2.CombinedFileName);
			Assert.AreNotEqual<string>(result1.CombinedFileName, result3.CombinedFileName);
			Assert.AreNotEqual<string>(result2.CombinedFileName, result3.CombinedFileName);

			// These instances combined exactly the same content so the combined file names should
			// be the same.
			Assert.AreEqual<string>(result1.CombinedFileName,result4.CombinedFileName);
		}
	}
}
