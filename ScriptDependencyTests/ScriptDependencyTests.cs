using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptDependencyExtension;
using System.Web.Mvc;
using ScriptDependencyExtension.Handler;
using ScriptDependencyExtension.Helpers;
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

            var script1 = ScriptHelper.RequiresScripts(context,ScriptName.jQuery);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(script1.ToString()));
            Assert.IsTrue(script1.ToString().Contains("src='/Scripts/jquery-1.4.1.debug.js" + VERSION_QUERY_STRING));

            var script2 = ScriptHelper.RequiresScripts(context,ScriptName.jqueryValidateUnobtrusive);

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

            var script = ScriptHelper.RequiresScripts(context, ScriptName.AllScripts);
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

            var script = ScriptHelper.RequiresScripts(context, null);
            Assert.IsNotNull(script);
            Assert.AreEqual<MvcHtmlString>(MvcHtmlString.Empty, script);

            script = ScriptHelper.RequiresScripts(context, "");
            Assert.IsNotNull(script);
            Assert.AreEqual<MvcHtmlString>(MvcHtmlString.Empty, script);

            script = ScriptHelper.RequiresScripts(context, string.Empty);
            Assert.IsNotNull(script);
            Assert.AreEqual<MvcHtmlString>(MvcHtmlString.Empty, script);

            script = ScriptHelper.RequiresScripts(context, "   ");
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

            var script = ScriptHelper.RequiresScripts(context, ScriptName.jQuery);
        }

        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void DebugScriptsShouldBeIncludedInDebugMode()
        {
            var mockContext = new MockContext();
            mockContext.HasValidWebContext = true;
            mockContext.IsDebuggingEnabled = true;

            var script1 = ScriptHelper.RequiresScripts(mockContext, ScriptName.jQuery);

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

            var script1 = ScriptHelper.RequiresScripts(mockContext, ScriptName.jQuery);

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

            var script1 = ScriptHelper.RequiresScripts(mockContext, ScriptName.MicrosoftMvcValidation);

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

            var script1 = ScriptHelper.RequiresScripts(mockContext, "PageSpecificStyle");

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

            var script1 = ScriptHelper.RequiresScripts(mockContext, ScriptName.jQuery);
            var script2 = ScriptHelper.RequiresScripts(mockContext, ScriptName.jQuery);

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

			var script1 = ScriptHelper.RequiresScripts(mockContext, "jQuery");

            Assert.IsTrue(!string.IsNullOrWhiteSpace(script1.ToString()));
			Assert.IsTrue(script1.ToString().Contains("src='/Scripts/jquery-1.4.1.min.js" + VERSION_QUERY_STRING));

        }

		[TestMethod]
		[DeploymentItem("ScriptDependencies.xml")]
		[DeploymentItem("jquery-1.4.1.js")]
		[DeploymentItem("jquery-1.4.1.debug.js")]
		public void SHouldNotThrowIfNoScriptFilePresentInDependencyOrSubDependencies()
		{
			var mockContext = new MockContext();
			mockContext.HasValidWebContext = true;
			mockContext.IsDebuggingEnabled = false;
			var loader = new ScriptDependencyLoader(mockContext);
			var engine = new ScriptEngine(mockContext, loader);
			
			loader.Initialise();
			loader.DependencyContainer.ShouldCombineScripts = true;
			
			var builtScript = new StringBuilder();
			var combinedScript = new StringBuilder();
			
			engine.GenerateDependencyScript("no-file-group", builtScript);
			engine.GenerateCombinedScriptsIfRequired(combinedScript);
			
			var generatedUrl = combinedScript.ToString();
			int startPos = generatedUrl.IndexOf("src='");
			Assert.IsTrue(startPos >= 0);
			int endPos = generatedUrl.IndexOf("'></script>");
			Assert.IsTrue(endPos >= 0);
			var urlFragment = generatedUrl.Substring(startPos + 5, endPos - startPos - 5);
			var rawUrl = string.Format("http://localhost{0}", urlFragment);

			mockContext.SetRequestRawUrl(rawUrl);

			var handler = new ScriptServeHandler(mockContext, loader, engine, new TokenisationHelper());
			string contentType;
			var output = handler.ProcessRequestForUrl(rawUrl, out contentType);

			Assert.IsNotNull(contentType);
			Assert.AreEqual<string>(ScriptHelperConstants.ContentType_Javascript,contentType);
			Assert.IsNotNull(output);
			Assert.IsTrue(output.Contains("JavaScript Library")); // this should be present as part of the comments of jQuery which aren't removed by the minifier


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

		[DeploymentItem("ScriptDependencies.xml")]
		[TestMethod]
		public void DeferredScriptsShouldRenderWhenRequested()
		{
			var mockContext = new MockContext();
			mockContext.HasValidWebContext = true;
			mockContext.IsDebuggingEnabled = false;

			ScriptHelper.RequiresScriptsDeferred(mockContext, "jQuery-validate");

			var script = ScriptHelper.RenderDeferredScripts(mockContext);

			Assert.IsTrue(!string.IsNullOrWhiteSpace(script.ToString()));
			Assert.IsTrue(script.ToString().Contains("src='/Scripts/jquery-1.4.1.min.js" + VERSION_QUERY_STRING));
			Assert.IsTrue(script.ToString().Contains("src='/Scripts/jquery.validate.min.js" + VERSION_QUERY_STRING));

		}
		
		[DeploymentItem("ScriptDependencies.xml")]
		[TestMethod]
		public void DeferredScriptsShouldNotIncludePreviouslyRenderedScripts()
		{
			var mockContext = new MockContext();
			mockContext.HasValidWebContext = true;
			mockContext.IsDebuggingEnabled = false;

			// Request a deferred load of a script and its dependencies, in this case jQuery
			ScriptHelper.RequiresScriptsDeferred(mockContext, "jQuery-validate");

			//Now simulate a page requesting that jQuery be loaded in non deferred mode, so it gets 
			//rendered immediatelyso we dont need it rendered again
			var someJunk= ScriptHelper.RequiresScripts(mockContext,"jQuery");

			var script = ScriptHelper.RenderDeferredScripts(mockContext);

			Assert.IsTrue(!string.IsNullOrWhiteSpace(script.ToString()));
			Assert.IsFalse(script.ToString().Contains("src='/Scripts/jquery-1.4.1.min.js" + VERSION_QUERY_STRING));
			Assert.IsTrue(script.ToString().Contains("src='/Scripts/jquery.validate.min.js" + VERSION_QUERY_STRING));

		}
	}


    public class MockContext : IHttpContext
    {
        public bool IsDebuggingEnabled { get; set; }
        public bool HasValidWebContext { get; set; }

		public HttpRequest Request { get; set; }

		public void SetRequestRawUrl(string url)
		{
			var uri = new Uri(url);
			Request = new HttpRequest("dummy.aspx",uri.AbsoluteUri,uri.PathAndQuery);
		}

		private IDictionary<string,object> _globalCache = new Dictionary<string, object>();

        private IDictionary _perRequestItemCache = new Dictionary<object, object>();
        public IDictionary PerRequestItemCache { get { return _perRequestItemCache; } }

        public string ResolveScriptRelativePath(string relativePath)
        {
			if (string.IsNullOrWhiteSpace(relativePath))
				return relativePath;

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

