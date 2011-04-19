using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptDependencyExtension.Constants;
using ScriptDependencyExtension.Http;

namespace ScriptDependencyExtension
{
	public class ScriptCache
	{
		private readonly IHttpContext _httpContext;

		public ScriptCache(IHttpContext context)
		{
			_httpContext = context;
		}

		public List<string> JavascriptFilesToCombineList
		{
			get { return SafeGetListFromRequestCache(ScriptHelperConstants.CacheKey_JSFilesToCombineList); }
			set { _httpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_JSFilesToCombineList] = value; }
		}
		public List<string> ScriptFilesAlreadyRendered
		{
			get { return SafeGetListFromRequestCache(ScriptHelperConstants.CacheKey_ScriptFilesAlreadyRendered); }
			set { _httpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_ScriptFilesAlreadyRendered] = value; }
		}

		public List<string> CssFilesToCombineList
		{
			get { return SafeGetListFromRequestCache(ScriptHelperConstants.CacheKey_CSSFilesToCombineList); }
			set { _httpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_CSSFilesToCombineList] = value; }
		}

		public List<string> DeferredScripts
		{
			get { return SafeGetListFromRequestCache(ScriptHelperConstants.CacheKey_DeferredScripts); }
			set { _httpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_DeferredScripts] = value; }
		}

		public bool IsScriptInListOfScriptsAlreadyRendered(string scriptName)
		{
			return ScriptFilesAlreadyRendered.Contains(scriptName);
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

		public void AddToAlreadyRenderedScripts(List<string> renderedScripts)
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

		public void AddToAlreadyRenderedScripts(string scriptName)
		{
			var list = new List<string>(new string[] {scriptName});
			AddToAlreadyRenderedScripts(list);
		}

		public void AddScriptToDeferredList(string scriptName)
		{
			List<string> scriptNames;
			if (!_httpContext.PerRequestItemCache.Contains(ScriptHelperConstants.CacheKey_DeferredScripts))
			{
				scriptNames = new List<string>();
				_httpContext.PerRequestItemCache.Add(ScriptHelperConstants.CacheKey_DeferredScripts, scriptNames);
			}
			else
			{
				scriptNames = _httpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_DeferredScripts] as List<string>;
			}
			scriptNames.Add(scriptName);
			_httpContext.PerRequestItemCache[ScriptHelperConstants.CacheKey_DeferredScripts] = scriptNames;
		}



	}
}
