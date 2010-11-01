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
        }

        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void ShouldOutputDebugVersionOfScript()
        {
            var context = new MockContext("http://test:1234",".");
            context.HasValidWebContext = true;
            context.IsDebuggingEnabled = true;
            ScriptHelper.WebHttpContext = context;

            var script = ScriptHelper.RequiresScript(ScriptName.jQuery);
        }
    }

    public class MockContext : IHttpContext
    {
        public MockContext(string requestUrl, string requestAppPath)
        {
            Request = new MockRequest(requestUrl, requestAppPath);
        }
        public bool IsDebuggingEnabled {get; set;}
        public IHttpRequest Request {get; set;}
        public bool HasValidWebContext {get; set;}

        public class MockRequest : IHttpRequest
        {
            public MockRequest(string requestUrl, string requestAppPath)
            {
                Url = new Uri(requestUrl);
                ApplicationPath = requestAppPath;
            }
            public Uri Url  {get; set;}
            public string ApplicationPath  {get; set;}
        }
    }

}

