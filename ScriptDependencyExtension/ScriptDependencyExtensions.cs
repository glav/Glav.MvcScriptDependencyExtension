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
using ScriptDependencyExtension.Constants;

namespace ScriptDependencyExtension
{
	public class ScriptHelper
	{
		private static ScriptDependencyLoader _scriptLoader = new ScriptDependencyLoader();

		public ScriptHelper(IHttpContext context)
		{
			_scriptLoader.Initialise(context);
			WebHttpContext = context;
		}

		#region properties

		public IHttpContext WebHttpContext { get; set; }

		public List<string> JavascriptFilesToCombineList
		{
			get
			{
				List<string> list = null;
				if (WebHttpContext.PerRequestItemCache.Contains(ScriptHelperConstants.CacheKey_JSFilesToCombineList))
				{
					list = WebHttpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_JSFilesToCombineList] as List<string>;
				}
				else
				{
					list = new List<string>();
					WebHttpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_JSFilesToCombineList] = list;
				}
				return list;
			}
			set { WebHttpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_JSFilesToCombineList] = value; }
		}

		public List<string> CssFilesToCombineList
		{
			get
			{
				List<string> list = null;
				if (WebHttpContext.PerRequestItemCache.Contains(ScriptHelperConstants.CacheKey_CSSFilesToCombineList))
				{
					list = WebHttpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_CSSFilesToCombineList] as List<string>;
				}
				else
				{
					list = new List<string>();
					WebHttpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_CSSFilesToCombineList] = list;
				}
				return list;
			}
			set { WebHttpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_CSSFilesToCombineList] = value; }
		}

		#endregion

		#region RequiresScript Helper methods

		public static MvcHtmlString RequiresScript(string scriptName)
		{
			if (HttpContext.Current == null)
				throw new ArgumentNullException(ScriptHelperConstants.ErrorMessage_NoHttpContextAvailable);

			return RequiresScript(new HttpContextAdapter(HttpContext.Current), scriptName);
		}
		internal static MvcHtmlString RequiresScript(IHttpContext context, string scriptName)
		{
			ScriptHelper helper = new ScriptHelper(context);

			if (string.IsNullOrWhiteSpace(scriptName))
				return MvcHtmlString.Empty;

			StringBuilder emittedScript = new StringBuilder();

			// If all scripts are specified, then make a call to the RequiresScripts passing in an array
			// containing all the scripts we read in from the dependency file.
			if (scriptName.ToLowerInvariant() == ScriptName.AllScripts)
			{
				List<string> allScripts = new List<string>();
				_scriptLoader.DependencyContainer.Dependencies.ForEach(s => allScripts.Add(s.ScriptName));

				return RequiresScripts(context, allScripts.ToArray());
			}

			return RequiresScripts(context, new string[] { scriptName });
		}

		public static MvcHtmlString RequiresScripts(params string[] scriptNames)
		{
			if (HttpContext.Current == null)
				throw new ArgumentNullException(ScriptHelperConstants.ErrorMessage_NoHttpContextAvailable);

			return RequiresScripts(new HttpContextAdapter(HttpContext.Current), scriptNames);
		}

		internal static MvcHtmlString RequiresScripts(IHttpContext context, params string[] scriptNames)
		{
			ScriptHelper helper = new ScriptHelper(context);

			// Use this to register multiple scripts and prevent multiple entries of base script like
			// jQuery core.
			StringBuilder emittedScript = new StringBuilder();

			// First lets see if the required script has any dependencies and include them first
			if (scriptNames != null && scriptNames.Length > 0)
			{
				if (_scriptLoader.DependencyContainer.ShouldCombineScripts)
				{
					helper.GenerateCombinedScript(emittedScript);
				}
				else
				{
					foreach (var scriptName in scriptNames)
					{
						if (!string.IsNullOrWhiteSpace(scriptName))
						{
							helper.GenerateDependencyScript(scriptName, emittedScript);
						}
					}
				}
			}

			return MvcHtmlString.Create(emittedScript.ToString());
		}

		#endregion

		#region RequiresScriptDeferred helper method

		public static MvcHtmlString RequiresScriptsDeferred(string scriptName)
		{
			if (HttpContext.Current == null)
				throw new ArgumentNullException(ScriptHelperConstants.ErrorMessage_NoHttpContextAvailable);

			RequiresScriptsDeferred(new HttpContextAdapter(HttpContext.Current), new string[] { scriptName });
			return MvcHtmlString.Empty;  // Razor needs something returned to it otherwise it has issues rendering
		}

		public static MvcHtmlString RequiresScriptsDeferred(params string[] scriptNames)
		{
			if (HttpContext.Current == null)
				throw new ArgumentNullException(ScriptHelperConstants.ErrorMessage_NoHttpContextAvailable);

			RequiresScriptsDeferred(new HttpContextAdapter(HttpContext.Current), scriptNames);
			return MvcHtmlString.Empty;  // Razor needs something returned to it otherwise it has issues rendering
		}

		internal static void RequiresScriptsDeferred(IHttpContext context, params string[] scriptNames)
		{
			ScriptHelper helper = new ScriptHelper(context);

			// First lets see if the required script has any dependencies and include them first
			if (scriptNames != null && scriptNames.Length > 0)
			{
				foreach (var scriptName in scriptNames)
				{
					if (!string.IsNullOrWhiteSpace(scriptName))
						helper.AddScriptToDeferredList(scriptName);
				}
			}
		}

		public static MvcHtmlString RenderDeferredScripts()
		{
			if (HttpContext.Current == null)
				throw new ArgumentNullException(ScriptHelperConstants.ErrorMessage_NoHttpContextAvailable);

			return RenderDeferredScripts(new HttpContextAdapter(HttpContext.Current));
		}

		internal static MvcHtmlString RenderDeferredScripts(IHttpContext context)
		{
			ScriptHelper helper = new ScriptHelper(context);

			return MvcHtmlString.Create(helper.RenderDeferredScriptsToBuffer());

		}
		#endregion

		private void AddScriptToDeferredList(string scriptName)
		{
			List<string> scriptNames;
			if (!WebHttpContext.PerRequestItemCache.Contains("DeferredScripts"))
			{
				scriptNames = new List<string>();
				WebHttpContext.PerRequestItemCache.Add("DeferredScripts", scriptNames);
			}
			else
			{
				scriptNames = WebHttpContext.PerRequestItemCache["DeferredScripts"] as List<string>;
			}
			scriptNames.Add(scriptName);
			WebHttpContext.PerRequestItemCache["DeferredScripts"] = scriptNames;
		}

		private string RenderDeferredScriptsToBuffer()
		{
			var deferredScripts = WebHttpContext.PerRequestItemCache["DeferredScripts"] as List<string>;
			if (deferredScripts == null)
				return null;

			StringBuilder emittedScript = new StringBuilder();

			if (_scriptLoader.DependencyContainer.ShouldCombineScripts)
			{
				GenerateCombinedScript(emittedScript);
			}
			else
			{
				foreach (var scriptName in deferredScripts)
				{
					if (!string.IsNullOrWhiteSpace(scriptName))
						GenerateDependencyScript(scriptName, emittedScript);
				}
			}

			return emittedScript.ToString();

		}

		private void GenerateDependencyScript(string scriptName, StringBuilder emittedScript)
		{
			var dependency = _scriptLoader.DependencyContainer.FindDependency(scriptName);
			if (dependency != null)
			{
				if (dependency.RequiredDependencies != null && dependency.RequiredDependencies.Count > 0)
				{
					dependency.RequiredDependencies.ForEach(dependencyName =>
					                                        	{
					                                        		var requiredDependency =
					                                        			_scriptLoader.DependencyContainer.FindDependency(dependencyName);
						// Recursively hunt for script dependencies
						GenerateDependencyScript(requiredDependency.ScriptName, emittedScript);
						AddScriptToOutputBuffer(requiredDependency, emittedScript);
					});
				}
				if (!string.IsNullOrWhiteSpace(dependency.ScriptPath))
				{
					AddScriptToOutputBuffer(dependency, emittedScript);
				}
			}
		}

		private void AddScriptToOutputBuffer(ScriptDependency dependency, StringBuilder buffer)
		{
			// If its already been added as part of this request, then dont add it in.
			if (WebHttpContext.PerRequestItemCache.Contains(dependency.ScriptName))
				return;

			var nameHelper = new ScriptNameHelper(WebHttpContext, _scriptLoader.DependencyContainer);

			string fullScriptInclude = null;

			var jsCombineList = JavascriptFilesToCombineList;
			var cssCombineList = CssFilesToCombineList;

			var resolvedPath = WebHttpContext.ResolveScriptRelativePath(dependency.ScriptPath);
			if (dependency.TypeOfScript == ScriptType.CSS)
			{
				if (_scriptLoader.DependencyContainer.ShouldCombineScripts)
				{
					if (!cssCombineList.Contains(dependency.ScriptPath))
						cssCombineList.Add(dependency.ScriptPath);
				}
				else
				{
					fullScriptInclude = string.Format(ScriptHelperConstants.CSSInclude, resolvedPath,
					                                  _scriptLoader.DependencyContainer.VersionMonikerQueryStringName,
					                                  _scriptLoader.DependencyContainer.VersionIdentifier);
				}
			}
			else
			{
				if (_scriptLoader.DependencyContainer.ShouldCombineScripts)
				{
					if (!jsCombineList.Contains(dependency.ScriptPath))
						jsCombineList.Add(dependency.ScriptPath);
				}
				else
				{
					var scriptNameBasedOnMode = nameHelper.DetermineScriptNameBasedOnDebugOrRelease(resolvedPath);
					if (!string.IsNullOrWhiteSpace(scriptNameBasedOnMode))
						fullScriptInclude = string.Format(ScriptHelperConstants.ScriptInclude, scriptNameBasedOnMode,
						                                  _scriptLoader.DependencyContainer.VersionMonikerQueryStringName,
						                                  _scriptLoader.DependencyContainer.VersionIdentifier);
				}
			}
			if (_scriptLoader.DependencyContainer.ShouldCombineScripts)
			{
				JavascriptFilesToCombineList = jsCombineList;
				CssFilesToCombineList = cssCombineList;
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(fullScriptInclude) &&
				    !ScriptNameHelper.HasScriptAlreadyBeenAdded(fullScriptInclude, buffer))
				{
					buffer.Append(fullScriptInclude);
					WebHttpContext.PerRequestItemCache.Add(dependency.ScriptName, fullScriptInclude);
				}
			}
		}

		private void GenerateCombinedScript(StringBuilder emittedScript)
		{
			throw new NotImplementedException();
		}

	}
}
