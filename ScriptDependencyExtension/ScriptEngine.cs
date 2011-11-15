using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptDependencyExtension.Constants;
using ScriptDependencyExtension.Filters;
using ScriptDependencyExtension.Helpers;
using ScriptDependencyExtension.Http;
using ScriptDependencyExtension.Model;

namespace ScriptDependencyExtension
{
	public class ScriptEngine
	{
		private IHttpContext _httpContext;
		private IScriptDependencyLoader _scriptLoader;
		private ScriptCache _scriptCache;
		List<Func<IScriptProcessingFilter>> _filters = new List<Func<IScriptProcessingFilter>>();

		public ScriptEngine(IHttpContext context, IScriptDependencyLoader scriptLoader)
		{
			if (context == null || scriptLoader == null)
				throw new NotImplementedException("HttpContext or ScriptDependencyLoader cannot be NULL");

			_httpContext = context;
			_scriptLoader = scriptLoader;
			_scriptCache = new ScriptCache(context);
			RegisterFilters();
		}

		public ScriptCache ScriptCache
		{
			get { return _scriptCache;  }
		}

		public string RenderDeferredScriptsToBuffer()
		{
			var deferredScripts = _scriptCache.DeferredScripts;
			if (deferredScripts == null || deferredScripts.Count == 0)
				return null;

			StringBuilder emittedScript = new StringBuilder();

			foreach (var scriptName in deferredScripts)
			{
				if (!string.IsNullOrWhiteSpace(scriptName))
				{
					var alreadyRenderedScripts = _scriptCache.ScriptFilesAlreadyRendered;
					if (!alreadyRenderedScripts.Contains(scriptName))
					{
						GenerateDependencyScript(scriptName, emittedScript);

					}
				}
			}

			GenerateCombinedScriptsIfRequired(emittedScript);

			return emittedScript.ToString();

		}

		public void ApplyFiltersToScriptOutput(StringBuilder emittedScript, ScriptType scriptType, ScriptDependencyContainer container)
		{
			_filters.ForEach((filter) =>
			                 	{
			                 		var filterResult = filter().ProcessScript(emittedScript.ToString(),scriptType, container);
									if (!string.IsNullOrWhiteSpace(filterResult))
									{
										emittedScript.Clear();
										emittedScript.Append(filterResult);
									}
			                 	});
		}
		
		private void RegisterFilters()
		{
			_filters.Add(() => dotLessFilter.GetDotLessProcessingFilter(_httpContext,_scriptLoader.DependencyContainer));
			_filters.Add(() => new ScriptMinifierFilter(_httpContext));
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
			if (_scriptCache.IsScriptInListOfScriptsAlreadyRendered(dependency.ScriptName))
				return;

			var nameHelper = new ScriptNameHelper(_httpContext, _scriptLoader.DependencyContainer);

			string fullScriptInclude = null;

			var jsCombineList = _scriptCache.JavascriptFilesToCombineList;
			var cssCombineList = _scriptCache.CssFilesToCombineList;

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
				_scriptCache.JavascriptFilesToCombineList = jsCombineList;
				_scriptCache.CssFilesToCombineList = cssCombineList;
			}
			else
			{
				if (!string.IsNullOrWhiteSpace(fullScriptInclude) &&
					!ScriptNameHelper.HasScriptAlreadyBeenAddedToBuffer(fullScriptInclude, buffer))
				{
					buffer.Append(fullScriptInclude);
				}
                _scriptCache.AddToAlreadyRenderedScripts(dependency.ScriptName);
			}
		}

		public void GenerateCombinedScriptsIfRequired(StringBuilder emittedScript)
		{
			if (_scriptLoader.DependencyContainer.ShouldCombineScripts)
			{
				var cssScripts = _scriptCache.CssFilesToCombineList;
				var jsScripts = _scriptCache.JavascriptFilesToCombineList;

				if (cssScripts != null && cssScripts.Count > 0)
				{
					GenerateCombinedScriptQueryString(cssScripts.ToArray(), emittedScript, ScriptType.CSS);
					_scriptCache.AddToAlreadyRenderedScripts(cssScripts);
					_scriptCache.CssFilesToCombineList = null; // clear the request cache after rendering it
				}
				if (jsScripts != null && jsScripts.Count > 0)
				{
					GenerateCombinedScriptQueryString(jsScripts.ToArray(), emittedScript, ScriptType.Javascript);
					_scriptCache.AddToAlreadyRenderedScripts(jsScripts);
					_scriptCache.JavascriptFilesToCombineList = null; // clear the request cache after rendering it
				}
			}
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
