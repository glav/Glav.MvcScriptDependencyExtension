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
				return SafeGetListFromRequestCache(ScriptHelperConstants.CacheKey_JSFilesToCombineList);
			}
			set { _httpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_JSFilesToCombineList] = value; }
		}
		public List<string> ScriptFilesAlreadyRendered
		{
			get
			{
				return SafeGetListFromRequestCache(ScriptHelperConstants.CacheKey_ScriptFilesAlreadyRendered);
			}
			set { _httpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_ScriptFilesAlreadyRendered] = value; }
		}

		public List<string> CssFilesToCombineList
		{
			get
			{
				return SafeGetListFromRequestCache(ScriptHelperConstants.CacheKey_CSSFilesToCombineList);
			}
			set { _httpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_CSSFilesToCombineList] = value; }
		}

		private List<string> SafeGetListFromRequestCache(string cacheKey)
		{
			List<string> list = null;
			if (_httpContext.PerRequestItemCache.Contains(cacheKey))
			{
				list = _httpContext.PerRequestItemCache[cacheKey] as List<string>;
			}
			if (list == null)
			{
				list = new List<string>();
				_httpContext.PerRequestItemCache[cacheKey] = list;
			}
			return list;
			
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

			foreach (var scriptName in deferredScripts)
			{
				if (!string.IsNullOrWhiteSpace(scriptName))
				{
					var alreadyRenderedScripts = ScriptFilesAlreadyRendered;
					if (!alreadyRenderedScripts.Contains(scriptName))
					{
						GenerateDependencyScript(scriptName, emittedScript);

					}
				}
			}

			GenerateCombinedScriptsIfRequired(emittedScript);

			return emittedScript.ToString();

		}

		/// <summary>
		/// This method will either gather the list of dependencies for a script name and
		/// then ask it to be added to the output
		/// </summary>
		/// <param name="scriptName"></param>
		/// <param name="emittedScript"></param>
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

		/// <summary>
		/// This method actually formats the script and adds it to the buffer or
		/// (if combine scripts is on), adds it to the list of scripts to be combined in the
		/// per request cache
		/// </summary>
		/// <param name="dependency"></param>
		/// <param name="buffer"></param>
		public void AddScriptToOutputBuffer(ScriptDependency dependency, StringBuilder buffer)
		{
			// If its already been added as part of this request, then dont add it in.
			var alreadyRendered = ScriptFilesAlreadyRendered;
			if (alreadyRendered.Contains(dependency.ScriptName))
				return;

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
				    if (!cssCombineList.Contains(dependency.ScriptName))
				        cssCombineList.Add(dependency.ScriptName);
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
					if (!jsCombineList.Contains(dependency.ScriptName))
						jsCombineList.Add(dependency.ScriptName);
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

			if (_scriptLoader.DependencyContainer.ShouldCombineScripts && !string.IsNullOrWhiteSpace(fullScriptInclude))
			{
				JavascriptFilesToCombineList = jsCombineList;
				CssFilesToCombineList = cssCombineList;
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(fullScriptInclude) &&
					!ScriptNameHelper.HasScriptAlreadyBeenAddedToBuffer(fullScriptInclude, buffer))
				{
					buffer.Append(fullScriptInclude);
					AddToAlreadyRenderedScripts(dependency.ScriptName);
					_httpContext.PerRequestItemCache.Add(dependency.ScriptName, fullScriptInclude);
				}
			}
		}

		public void GenerateCombinedScriptsIfRequired(StringBuilder emittedScript)
		{
			if (_scriptLoader.DependencyContainer.ShouldCombineScripts)
			{
				var cssScripts = CssFilesToCombineList;
				var jsScripts = JavascriptFilesToCombineList;

				if (cssScripts != null && cssScripts.Count > 0)
				{
					GenerateCombinedScriptQueryString(cssScripts.ToArray(), emittedScript, ScriptType.CSS);
					AddToAlreadyRenderedScripts(cssScripts);
					CssFilesToCombineList = null; // clear the request cache after rendering it
				}
				if (jsScripts != null && jsScripts.Count > 0)
				{
					GenerateCombinedScriptQueryString(jsScripts.ToArray(), emittedScript, ScriptType.Javascript);
					AddToAlreadyRenderedScripts(jsScripts);
					JavascriptFilesToCombineList = null; // clear the request cache after rendering it
				}
			}
		}

		private void AddToAlreadyRenderedScripts(string scriptName)
		{
			var list = new List<string>();
			list.Add(scriptName);
			AddToAlreadyRenderedScripts(list);
		}
		private void AddToAlreadyRenderedScripts(List<string> renderedScripts)
		{
			var alreadyRendered = ScriptFilesAlreadyRendered;
			renderedScripts.ForEach(s =>
			{
				if (!alreadyRendered.Contains(s))
					alreadyRendered.Add(s);
			});
			if (alreadyRendered.Count > 0)
				ScriptFilesAlreadyRendered = alreadyRendered;

		}


		public void GenerateCombinedScriptQueryString(string[] scriptNames, StringBuilder emittedScript, ScriptType typeOfScript)
		{
			ITokenisationHelper tokenHelper = new TokenisationHelper();
			var queyString = tokenHelper.GenerateQueryStringRequestForDependencyNames(scriptNames);
			var resolvedHandlerLocation =
				_httpContext.ResolveScriptRelativePath(string.Format("~/{0}", ScriptHelperConstants.ScriptDependencyHandlerName));
			if (typeOfScript == ScriptType.CSS)
			{
				emittedScript.AppendFormat(ScriptHelperConstants.CSSInclude,
										   resolvedHandlerLocation,
										   _scriptLoader.DependencyContainer.VersionMonikerQueryStringName,
										   _scriptLoader.DependencyContainer.VersionIdentifier + "&" + queyString);
			}
			else
			{
				emittedScript.AppendFormat(ScriptHelperConstants.ScriptInclude,
										   resolvedHandlerLocation,
										   _scriptLoader.DependencyContainer.VersionMonikerQueryStringName,
										   _scriptLoader.DependencyContainer.VersionIdentifier + "&" + queyString);
			}
		}
	}
}
