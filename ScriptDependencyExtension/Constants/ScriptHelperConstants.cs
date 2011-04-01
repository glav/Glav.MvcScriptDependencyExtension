using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptDependencyExtension.Constants
{
    internal static class ScriptHelperConstants
    {
        public const string ScriptInclude = "<script type='text/javascript' src='{0}?{1}={2}'></script>";
        public const string CSSInclude = "<link href='{0}?{1}={2}' rel='stylesheet' type='text/css' />";
        public const string JSPrefix = ".js";

		public const string ErrorMessage_NoHttpContextAvailable = "HttpContext is NULL or not available";

		public const string CacheKey_JSFilesToCombineList = "ScriptCombineList-JS";
		public const string CacheKey_CSSFilesToCombineList = "ScriptCombineList-CSS";
		public const string CacheKey_ScriptDependencies = "ScriptDependencies";
		public const string CacheKey_ScriptFilesAlreadyRendered = "ScriptFilesAlreadyRendered";
    	public const string CacheKey_DeferredScripts = "DeferredScripts";

		public const string CombinedScriptQueryStringIdentifier = "scrptcmb";
    	public const string ScriptDependencyHandlerName = "ScriptDependency.axd";

		public const string ContentType_Javascript = "application/x-javascript";
		public const string ContentType_CSS = "text/css";
	}
}
