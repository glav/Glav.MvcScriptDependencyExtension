using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptDependencyExtension;
using System.Web.Mvc;
using ScriptDependencyExtension.Http;

namespace ScriptDependencyTests
{
    [TestClass]
    public class ScriptDependencyTests
    {
        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void MultipleScriptsShouldBeLoadedFromASingleDelcaration()
        {
            var script1 = ScriptHelper.RequiresScript(ScriptName.jQuery);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(script1.ToString()));
            Assert.IsTrue(script1.ToString().Contains("src='/Scripts/jquery-1.4.1.js'"));

            var script2 = ScriptHelper.RequiresScript(ScriptName.jqueryValidateUnobtrusive);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(script2.ToString()));
            Assert.IsTrue(script2.ToString().Contains("src='/Scripts/jquery-1.4.1.js'"));
            Assert.IsTrue(script2.ToString().Contains("src='/Scripts/jquery.validate.js'"));
            Assert.IsTrue(script2.ToString().Contains("src='/Scripts/jquery.validate.unobtrusive.js'"));
        }

        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void MultipleScriptsShouldBeLoadedFromMultipleDeclaration()
        {
            var script1 = ScriptHelper.RequiresScripts(ScriptName.jQuery
                                                        ,ScriptName.MicrosoftMvcValidation,
                                                        ScriptName.jQueryValidate);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(script1.ToString()));
            Assert.IsTrue(script1.ToString().Contains("src='/Scripts/jquery-1.4.1.js'"));

            Assert.IsTrue(script1.ToString().Contains("src='/Scripts/jquery-1.4.1.js'"));
            Assert.IsTrue(script1.ToString().Contains("src='/Scripts/jquery.validate.js'"));
            Assert.IsTrue(script1.ToString().Contains("src='/Scripts/MicrosoftMvcAjax.js'"));
            Assert.IsTrue(script1.ToString().Contains("src='/Scripts/MicrosoftAjax.js'"));
        }

        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void AllDependentScriptsShouldBeLoadedWithAllScriptSetting()
        {
            var script = ScriptHelper.RequiresScript(ScriptName.AllScripts);
            Assert.IsTrue(script.ToString().Contains("src='/Scripts/jquery-1.4.1.js'"));
            Assert.IsTrue(script.ToString().Contains("src='/Scripts/jquery.validate.js'"));
            Assert.IsTrue(script.ToString().Contains("src='/Scripts/MicrosoftAjax.js'"));
        }

        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void RequringAnEmptyScriptNameShouldReturnEmpty()
        {
            var script = ScriptHelper.RequiresScript(null);
            Assert.IsNotNull(script);
            Assert.AreEqual<MvcHtmlString>(MvcHtmlString.Empty, script);
            
            script = ScriptHelper.RequiresScript("");
            Assert.IsNotNull(script);
            Assert.AreEqual<MvcHtmlString>(MvcHtmlString.Empty, script);
            
            script = ScriptHelper.RequiresScript(string.Empty);
            Assert.IsNotNull(script);
            Assert.AreEqual<MvcHtmlString>(MvcHtmlString.Empty, script);

            script = ScriptHelper.RequiresScript("   ");
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
            ScriptHelper.WebHttpContext = context;

            var script = ScriptHelper.RequiresScript(ScriptName.jQuery);
        }

        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void DebugScriptsShouldBeIncludedInDebugMode()
        {
            var mockContext = new MockContext();
            mockContext.HasValidWebContext = true;
            mockContext.IsDebuggingEnabled = true;
            ScriptHelper.WebHttpContext = mockContext;

            var script1 = ScriptHelper.RequiresScript(ScriptName.jQuery);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(script1.ToString()));
            Assert.IsTrue(script1.ToString().Contains("/Scripts/jquery-1.4.1.debug.js'"));
        }
        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void ReleaseScriptsShouldBeIncludedInDebugMode()
        {
            var mockContext = new MockContext();
            mockContext.HasValidWebContext = true;
            mockContext.IsDebuggingEnabled = false;
            ScriptHelper.WebHttpContext = mockContext;

            var script1 = ScriptHelper.RequiresScript(ScriptName.jQuery);

            Assert.IsTrue(!string.IsNullOrWhiteSpace(script1.ToString()));
            Assert.IsTrue(script1.ToString().Contains("/Scripts/jquery-1.4.1.min.js'"));
        }
    }


    public class MockContext : IHttpContext
    {
        public bool IsDebuggingEnabled { get; set; }
        public bool HasValidWebContext { get; set; }

        public string ResolveScriptRelativePath(string relativePath)
        {
            return relativePath.Replace("~", string.Empty);
        }

    }

}

