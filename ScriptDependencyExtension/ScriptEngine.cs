using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptDependencyExtension.Constants;
using ScriptDependencyExtension.Helpers;
using ScriptDependencyExtension.Http;

namespace ScriptDependencyExtension
{
	public class ScriptEngine
	{
		private IHttpContext _httpContext;
		private IScriptDependencyLoader _scriptLoader;

		public ScriptEngine(IHttpContext context, IScriptDependencyLoader scriptLoader)
		{
			if (context == null || scriptLoader == null)
				throw new NotImplementedException("HttpContext or ScriptDependencyLoader cannot be NULL");

			_httpContext = context;
			_scriptLoader = scriptLoader;
		}

		#region Properties
		public List<string> JavascriptFilesToCombineList
		{
			get
			{
				List<string> list = null;
				if (_httpContext.PerRequestItemCache.Contains(ScriptHelperConstants.CacheKey_JSFilesToCombineList))
				{
					list = _httpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_JSFilesToCombineList] as List<string>;
				}
				else
				{
					list = new List<string>();
					_httpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_JSFilesToCombineList] = list;
				}
				return list;
			}
			set { _httpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_JSFilesToCombineList] = value; }
		}

		public List<string> CssFilesToCombineList
		{
			get
			{
				List<string> list = null;
				if (_httpContext.PerRequestItemCache.Contains(ScriptHelperConstants.CacheKey_CSSFilesToCombineList))
				{
					list = _httpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_CSSFilesToCombineList] as List<string>;
				}
				else
				{
					list = new List<string>();
					_httpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_CSSFilesToCombineList] = list;
				}
				return list;
			}
			set { _httpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_CSSFilesToCombineList] = value; }
		}

		#endregion

		public void AddScriptToDeferredList(string scriptName)
		{
			List<string> scriptNames;
			if (!_httpContext.PerRequestItemCache.Contains("DeferredScripts"))
			{
				scriptNames = new List<string>();
				_httpContext.PerRequestItemCache.Add("DeferredScripts", scriptNames);
			}
			else
			{
				scriptNames = _httpContext.PerRequestItemCache["DeferredScripts"] as List<string>;
			}
			scriptNames.Add(scriptName);
			_httpContext.PerRequestItemCache["DeferredScripts"] = scriptNames;
		}

		public string RenderDeferredScriptsToBuffer()
		{
			var deferredScripts = _httpContext.PerRequestItemCache["DeferredScripts"] as List<string>;
			if (deferredScripts == null)
				return null;

			StringBuilder emittedScript = new StringBuilder();

			if (_scriptLoader.DependencyContainer.ShouldCombineScripts)
			{
				GenerateCombinedScript(deferredScripts.ToArray(), emittedScript);
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

		public void GenerateDependencyScript(string scriptName, StringBuilder emittedScript)
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

		public void AddScriptToOutputBuffer(ScriptDependency dependency, StringBuilder buffer)
		{
			// If its already been added as part of this request, then dont add it in.
			if (_httpContext.PerRequestItemCache.Contains(dependency.ScriptName))
				return;

			var nameHelper = new ScriptNameHelper(_httpContext, _scriptLoader.DependencyContainer);

			string fullScriptInclude = null;

			var jsCombineList = JavascriptFilesToCombineList;
			var cssCombineList = CssFilesToCombineList;

			var resolvedPath = _httpContext.ResolveScriptRelativePath(dependency.ScriptPath);
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
					_httpContext.PerRequestItemCache.Add(dependency.ScriptName, fullScriptInclude);
				}
			}
		}

		public void GenerateCombinedScript(string[] scriptNames, StringBuilder emittedScript)
		{
			throw new NotImplementedException();
		}
	}
}
