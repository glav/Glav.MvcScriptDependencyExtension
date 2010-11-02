using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Mvc.Html;
using System.Web;
using System.Threading;
using System.IO;
using ScriptDependencyExtension.Http;

namespace ScriptDependencyExtension
{
    public static class ScriptHelper
    {
        private const string SCRIPT_INCLUDE = "<script type='text/javascript' src='{0}'></script>";
        private static ScriptDependencyLoader _scriptLoader = new ScriptDependencyLoader();

        static ScriptHelper()
        {
            _scriptLoader.Initialise();
            WebHttpContext = new HttpContextAdapter(HttpContext.Current);
        }

        public static IHttpContext WebHttpContext {get; set;}

        public static MvcHtmlString RequiresScript(string scriptName)
        {
            if (string.IsNullOrWhiteSpace(scriptName))
                return MvcHtmlString.Empty;

            StringBuilder emittedScript = new StringBuilder();

            // If all scripts are specified, then make a call to the RequiresScripts passing in an array
            // containing all the scripts we read in from the dependency file.
            if (scriptName.ToLowerInvariant() == ScriptName.AllScripts)
            {
                List<string> allScripts = new List<string>();
                _scriptLoader.DependencyContainer.Dependencies.ForEach(s => allScripts.Add(s.ScriptName));

                return RequiresScripts(allScripts.ToArray());
            }

            // First lets see if the required script has any dependencies and include them first
            GenerateDependencyScript(scriptName, emittedScript);

            return MvcHtmlString.Create(emittedScript.ToString());
        }

        public static MvcHtmlString RequiresScripts(params string[] scriptNames)
        {
            // Use this to register multiple scripts and prevent multiple entries of base script like
            // jQuery core.
            StringBuilder emittedScript = new StringBuilder();

            // First lets see if the required script has any dependencies and include them first
            if (scriptNames != null && scriptNames.Length > 0)
            {
                foreach (var scriptName in scriptNames)
                {
                    if (!string.IsNullOrWhiteSpace(scriptName))
                        GenerateDependencyScript(scriptName, emittedScript);
                }
            }

            return MvcHtmlString.Create(emittedScript.ToString());
        }

        private static void GenerateDependencyScript(string scriptName, StringBuilder emittedScript)
        {
            var dependency = _scriptLoader.DependencyContainer.Dependencies.SingleOrDefault(s => s.ScriptName.ToLowerInvariant() == scriptName.ToLowerInvariant());
            if (dependency != null)
            {
                if (dependency.RequiredDependencies != null && dependency.RequiredDependencies.Count > 0)
                {
                    dependency.RequiredDependencies.ForEach(dependencyName =>
                    {
                        var requiredDependency = _scriptLoader.DependencyContainer.Dependencies.Single(d => d.ScriptName == dependencyName.ToLowerInvariant());
                        AddScriptToOutputBuffer(requiredDependency, emittedScript);
                        // Recursively hunt for script dependencies
                        GenerateDependencyScript(requiredDependency.ScriptName, emittedScript);
                    });
                }
                if (!string.IsNullOrWhiteSpace(dependency.ScriptPath))
                {
                    AddScriptToOutputBuffer(dependency, emittedScript);
                }
            }
        }

        private static void AddScriptToOutputBuffer(ScriptDependency dependency, StringBuilder buffer)
        {
            var resolvedPath = ResolveScriptRelativePath(dependency.ScriptPath);
            var scriptNameBasedOnMode = DetermineScriptNameBasedOnDebugOrRelease(resolvedPath);
            var fullScriptInclude = string.Format(SCRIPT_INCLUDE, resolvedPath);
            if (!HasScriptAlreadyBeenAdded(fullScriptInclude, buffer))
            {
                buffer.Append(fullScriptInclude);
            }
        }

        private static object DetermineScriptNameBasedOnDebugOrRelease(string resolvedScriptPath)
        {
            if (WebHttpContext.HasValidWebContext)
            {
                bool scriptIsNamedForRelease = true;
                
                if (resolvedScriptPath.Length > 8)
                {
                    var scriptSuffix = resolvedScriptPath.Substring(resolvedScriptPath.Length - 8, 8);
                    if (scriptSuffix == "debug.js")
                        scriptIsNamedForRelease = false;
                }

                string actualScriptPath = null;
                if (scriptIsNamedForRelease && WebHttpContext.IsDebuggingEnabled)
                    actualScriptPath = ChangeScriptNameToDebug(resolvedScriptPath);
                if (!scriptIsNamedForRelease && !WebHttpContext.IsDebuggingEnabled)
                    actualScriptPath = ChangeScriptNameToRelease(resolvedScriptPath);

                if (File.Exists(actualScriptPath))
                    return actualScriptPath;

                return resolvedScriptPath;
            }
            else
                return resolvedScriptPath;
        }

        private static string ChangeScriptNameToRelease(string resolvedScriptPath)
        {
            var scriptPreffix = resolvedScriptPath.Substring(0,resolvedScriptPath.Length - 8);
            return string.Format("{0}.js", scriptPreffix);
        }

        private static string ChangeScriptNameToDebug(string resolvedScriptPath)
        {
            var scriptPrefix = resolvedScriptPath.Substring(0, resolvedScriptPath.Length - 3);
            return string.Format("{0}.debug.js", scriptPrefix);
        }

        private static bool HasScriptAlreadyBeenAdded(string scriptToCheck, StringBuilder emittedScript)
        {
            var existingScript = emittedScript.ToString().ToLowerInvariant();
            return (existingScript.Contains(scriptToCheck.ToLowerInvariant())); 
        }



        private static string ResolveScriptRelativePath(string relativePath)
        {
            if (WebHttpContext.HasValidWebContext)
            {
                if (relativePath.StartsWith("~"))
                {
                    var url = WebHttpContext.Request.Url;
                    var newPath = (WebHttpContext.Request.ApplicationPath + relativePath.Substring(1)).Replace("//", "/");
                    string newUrl = string.Format("{0}://{1}{2}{3}",
                                                        url.Scheme,
                                                        url.Host,
                                                        url.IsDefaultPort ? string.Empty : string.Format(":{0}", url.Port.ToString()),
                                                        newPath);
                    return newUrl;
                }
            }
            return relativePath.Replace("~", "");
        }

    }
}
