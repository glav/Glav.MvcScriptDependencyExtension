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
			var result = fileCombiner.CombineFiles();

			Assert.IsNotNull(result);
			Assert.IsFalse(string.IsNullOrWhiteSpace(result));

			// Make sure it contains the combined contents
			Assert.IsTrue(result.Contains("This is testfile 1"));
			Assert.IsTrue(result.Contains("This is testfile 2"));
		}
		
		[TestMethod]
		[DeploymentItem("TestFile1.txt")]
		[DeploymentItem("TestFile2.txt")]
		public void ShouldGenerateUniqueCombinedFileNames()
		{
			var fileCombiner_Test1AndTest2 = new FileCombiner(new HttpContextAdapter(), new string[] { "TestFile1.txt", "TestFile2.txt" });
			var fileCombiner_Test1 = new FileCombiner(new HttpContextAdapter(), new string[] { "TestFile1.txt" });
			var fileCombiner_Test2 = new FileCombiner(new HttpContextAdapter(), new string[] { "TestFile2.txt" });
			var fileCombiner_Test1AndTest2Copy = new FileCombiner(new HttpContextAdapter(), new string[] { "TestFile1.txt", "TestFile2.txt" });
			var fileCombiner_Test2Copy = new FileCombiner(new HttpContextAdapter(), new string[] { "TestFile2.txt" }); 

			var result_Test1AndTest2 = fileCombiner_Test1AndTest2.CombineFiles();
			var result_Test1 = fileCombiner_Test1.CombineFiles();
			var result_Test2 = fileCombiner_Test2.CombineFiles();
			var result_Test1AndTest2Copy = fileCombiner_Test1AndTest2Copy.CombineFiles();
			var result_Test2Copy = fileCombiner_Test2Copy.CombineFiles();

			Assert.IsNotNull(result_Test1AndTest2);
			Assert.IsNotNull(result_Test1);
			Assert.IsNotNull(result_Test2);
			Assert.IsNotNull(result_Test1AndTest2Copy);
			Assert.IsNotNull(result_Test2Copy);

			Assert.AreNotEqual<string>(result_Test1AndTest2, result_Test1);
			Assert.AreNotEqual<string>(result_Test1AndTest2, result_Test2);
			Assert.AreNotEqual<string>(result_Test1, result_Test2);

			// These instances combined exactly the same content so the combined file names should
			// be the same.
			Assert.AreEqual<string>(result_Test1AndTest2,result_Test1AndTest2Copy);
			Assert.AreEqual<string>(result_Test2, result_Test2Copy);
		}
	}
}
