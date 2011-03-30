using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptDependencyExtension;
using System.Web.Mvc;
using ScriptDependencyExtension.Http;
using ScriptDependencyExtension.Constants;
using System.Collections;

namespace ScriptDependencyTests
{
    [TestClass]
    public class ScriptDependencyTests
    {
    	private const string VERSION_QUERY_STRING = "?v=123";

        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void MultipleScriptsShouldBeLoadedFromASingleDelcaration()
        {
            var context = new MockContext();
            context.HasValidWebContext = true;
            context.IsDebuggingEnabled = true;

            var script1 = ScriptHelper.RequiresScript(context,ScriptName.jQuery);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(script1.ToString()));
            Assert.IsTrue(script1.ToString().Contains("src='/Scripts/jquery-1.4.1.debug.js" + VERSION_QUERY_STRING));

            var script2 = ScriptHelper.RequiresScript(context,ScriptName.jqueryValidateUnobtrusive);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(script2.ToString()));
			Assert.IsTrue(script2.ToString().Contains("src='/Scripts/jquery.validate.debug.js" + VERSION_QUERY_STRING));
			Assert.IsTrue(script2.ToString().Contains("src='/Scripts/jquery.validate.unobtrusive.debug.js" + VERSION_QUERY_STRING));
        }

        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void MultipleScriptsShouldBeLoadedFromMultipleDeclaration()
        {
            var context = new MockContext();
            context.HasValidWebContext = true;
            context.IsDebuggingEnabled = true;

            var script1 = ScriptHelper.RequiresScripts(context, ScriptName.jQuery
                                                        ,ScriptName.MicrosoftMvcValidation,
                                                        ScriptName.jQueryValidate);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(script1.ToString()));
			Assert.IsTrue(script1.ToString().Contains("src='/Scripts/jquery-1.4.1.debug.js" + VERSION_QUERY_STRING));

			Assert.IsTrue(script1.ToString().Contains("src='/Scripts/jquery-1.4.1.debug.js" + VERSION_QUERY_STRING));
			Assert.IsTrue(script1.ToString().Contains("src='/Scripts/jquery.validate.debug.js" + VERSION_QUERY_STRING));
			Assert.IsTrue(script1.ToString().Contains("src='/Scripts/MicrosoftMvcAjax.debug.js" + VERSION_QUERY_STRING));
			Assert.IsTrue(script1.ToString().Contains("src='/Scripts/MicrosoftAjax.debug.js" + VERSION_QUERY_STRING));
        }

        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void AllDependentScriptsShouldBeLoadedWithAllScriptSetting()
        {
            var context = new MockContext();
            context.HasValidWebContext = true;
            context.IsDebuggingEnabled = true;

            var script = ScriptHelper.RequiresScript(context, ScriptName.AllScripts);
			Assert.IsTrue(script.ToString().Contains("src='/Scripts/jquery-1.4.1.debug.js" + VERSION_QUERY_STRING));
			Assert.IsTrue(script.ToString().Contains("src='/Scripts/jquery.validate.debug.js" + VERSION_QUERY_STRING));
			Assert.IsTrue(script.ToString().Contains("src='/Scripts/MicrosoftAjax.debug.js" + VERSION_QUERY_STRING));
        }

        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void RequringAnEmptyScriptNameShouldReturnEmpty()
        {
            var context = new MockContext();
            context.HasValidWebContext = true;
            context.IsDebuggingEnabled = true;

            var script = ScriptHelper.RequiresScript(context, null);
            Assert.IsNotNull(script);
            Assert.AreEqual<MvcHtmlString>(MvcHtmlString.Empty, script);

            script = ScriptHelper.RequiresScript(context, "");
            Assert.IsNotNull(script);
            Assert.AreEqual<MvcHtmlString>(MvcHtmlString.Empty, script);

            script = ScriptHelper.RequiresScript(context, string.Empty);
            Assert.IsNotNull(script);
            Assert.AreEqual<MvcHtmlString>(MvcHtmlString.Empty, script);

            script = ScriptHelper.RequiresScript(context, "   ");
            Assert.IsNotNull(script);
            Assert.AreEqual<MvcHtmlString>(MvcHtmlString.Empty, script);
        }

        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void ShouldOutputDebugVersionOfScript()
        {
            var context = new MockContext();
            context.HasValidWebContext = true;
            context.IsDebuggingEnabled = true;

            var script = ScriptHelper.RequiresScript(context, ScriptName.jQuery);
        }

        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void DebugScriptsShouldBeIncludedInDebugMode()
        {
            var mockContext = new MockContext();
            mockContext.HasValidWebContext = true;
            mockContext.IsDebuggingEnabled = true;

            var script1 = ScriptHelper.RequiresScript(mockContext, ScriptName.jQuery);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(script1.ToString()));
			Assert.IsTrue(script1.ToString().Contains("/Scripts/jquery-1.4.1.debug.js" + VERSION_QUERY_STRING));
        }

        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void ReleaseScriptsShouldBeIncludedInDebugMode()
        {
            var mockContext = new MockContext();
            mockContext.HasValidWebContext = true;
            mockContext.IsDebuggingEnabled = false;

            var script1 = ScriptHelper.RequiresScript(mockContext, ScriptName.jQuery);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(script1.ToString()));
			Assert.IsTrue(script1.ToString().Contains("/Scripts/jquery-1.4.1.min.js" + VERSION_QUERY_STRING));
        }
        
        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void ShouldFindAtLeastOneCSSDependency()
        {
            var mockContext = new MockContext();
            mockContext.HasValidWebContext = true;
            mockContext.IsDebuggingEnabled = false;

            var script1 = ScriptHelper.RequiresScript(mockContext, ScriptName.MicrosoftMvcValidation);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(script1.ToString()));
			Assert.IsTrue(script1.ToString().Contains("/Content/Site.css" + VERSION_QUERY_STRING));
        }
        
        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void ShouldFindTwoCssDependencies()
        {
            var mockContext = new MockContext();
            mockContext.HasValidWebContext = true;
            mockContext.IsDebuggingEnabled = false;

            var script1 = ScriptHelper.RequiresScript(mockContext, "PageSpecificStyle");

            Assert.IsTrue(!string.IsNullOrWhiteSpace(script1.ToString()));
			Assert.IsTrue(script1.ToString().Contains("/Content/Site.css" + VERSION_QUERY_STRING));
			Assert.IsTrue(script1.ToString().Contains("/Content/Page.css" + VERSION_QUERY_STRING));
        }

        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void ShouldNotIncludeDuplicateScripts()
        {
            var mockContext = new MockContext();
            mockContext.HasValidWebContext = true;
            mockContext.IsDebuggingEnabled = false;

            var script1 = ScriptHelper.RequiresScript(mockContext, ScriptName.jQuery);
            var script2 = ScriptHelper.RequiresScript(mockContext, ScriptName.jQuery);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(script1.ToString()));
			Assert.IsTrue(script1.ToString().Contains("/Scripts/jquery-1.4.1.min.js" + VERSION_QUERY_STRING));

            // jQuery has already been included so it should return empty and not include it again
            Assert.IsTrue(string.IsNullOrWhiteSpace(script2.ToString()));
        }

        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void SHouldNotThrowIfNoScriptFilePresentInDependency()
        {
            var mockContext = new MockContext();
            mockContext.HasValidWebContext = true;
            mockContext.IsDebuggingEnabled = false;

            var script1 = ScriptHelper.RequiresScripts(mockContext, "no-file");

            Assert.IsTrue(!string.IsNullOrWhiteSpace(script1.ToString()));
			Assert.IsTrue(script1.ToString().Contains("src='/Scripts/jquery-1.4.1.min.js" + VERSION_QUERY_STRING));

        }

        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void ScriptsShouldRenderInCorrectOrder()
        {
            var mockContext = new MockContext();
            mockContext.HasValidWebContext = true;
            mockContext.IsDebuggingEnabled = false;

            var script = ScriptHelper.RequiresScripts(mockContext, "ScriptOrderTest");

            // Make sure the scripts are in there
            Assert.IsTrue(!string.IsNullOrWhiteSpace(script.ToString()));
            Assert.IsTrue(script.ToString().Contains("HighLevelScript"));
            Assert.IsTrue(script.ToString().Contains("Script1"));
            Assert.IsTrue(script.ToString().Contains("Script2"));

            //Now assert their order
            int posHighLevel = script.ToString().IndexOf("HighLevelScript");
            int posScript1 = script.ToString().IndexOf("Script1");
            int posScript2 = script.ToString().IndexOf("Script2");

            Assert.IsTrue(posHighLevel > posScript2);
            Assert.IsTrue(posScript2 > posScript1);

        }

    }


    public class MockContext : IHttpContext
    {
        public bool IsDebuggingEnabled { get; set; }
        public bool HasValidWebContext { get; set; }

		private IDictionary<string,object> _globalCache = new Dictionary<string, object>();

        private IDictionary _perRequestItemCache = new Dictionary<object, object>();
        public IDictionary PerRequestItemCache { get { return _perRequestItemCache; } }

        public string ResolveScriptRelativePath(string relativePath)
        {
            return relativePath.Replace("~", string.Empty);
        }

		public T GetItemFromGlobalCache<T>(string cacheKey) where T : class
		{
			if (_globalCache.Keys.Contains(cacheKey))
				return _globalCache[cacheKey] as T;

			return null;
		}

		public void AddItemToGlobalCache(string cacheKey, object data)
		{
			if (_globalCache.Keys.Contains(cacheKey))
				_globalCache[cacheKey] = data;
			else
			{
				_globalCache.Add(cacheKey, data);				
			}
		}


		public string ApplicationDirectory
		{
			get { return System.Environment.CurrentDirectory; }
		}

		public string ResolvePhysicalFilePathFromRelative(string relativePath)
		{
			return ResolveScriptRelativePath(relativePath);
		}
	}

}

