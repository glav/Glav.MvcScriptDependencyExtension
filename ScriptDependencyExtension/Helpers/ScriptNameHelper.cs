using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptDependencyExtension.Constants;
using ScriptDependencyExtension.Http;

namespace ScriptDependencyExtension.Helpers
{
	public class ScriptNameHelper
	{
		private IHttpContext _httpContext;
		private ScriptDependencyContainer _scriptContainer;

		public ScriptNameHelper(IHttpContext context, ScriptDependencyContainer container)
		{
			if (context == null || container == null)
			{
				throw new NotImplementedException("HttpContext or ScriptDependencyContainer cannot be NULL");
			}

			_httpContext = context;
			_scriptContainer = container;
		}

		public string DetermineScriptNameBasedOnDebugOrRelease(string resolvedScriptPath)
		{
			if (string.IsNullOrWhiteSpace(resolvedScriptPath))
				return null;

			if (_httpContext.HasValidWebContext)
			{
				ScriptState scriptState = ScriptState.Original;

				var debugSuffix = string.Format("{0}.js", _scriptContainer.DebugSuffix);
				if (!string.IsNullOrWhiteSpace(debugSuffix) && resolvedScriptPath.Length > debugSuffix.Length)
				{
					var scriptSuffix = resolvedScriptPath.Substring(resolvedScriptPath.Length - debugSuffix.Length, debugSuffix.Length);
					if (scriptSuffix == debugSuffix)
						scriptState = ScriptState.Debug;
				}

				var releaseSuffix = string.Format("{0}.js", _scriptContainer.ReleaseSuffix);
				if (!string.IsNullOrWhiteSpace(releaseSuffix) && resolvedScriptPath.Length > releaseSuffix.Length)
				{
					var scriptSuffix = resolvedScriptPath.Substring(resolvedScriptPath.Length - releaseSuffix.Length, releaseSuffix.Length);
					if (scriptSuffix == releaseSuffix)
						scriptState = ScriptState.Release;
				}

				string actualScriptPath = null;
				if (scriptState != ScriptState.Debug && _httpContext.IsDebuggingEnabled)
					actualScriptPath = ChangeScriptNameToDebug(resolvedScriptPath);
				if (scriptState != ScriptState.Release && !_httpContext.IsDebuggingEnabled)
					actualScriptPath = ChangeScriptNameToRelease(resolvedScriptPath);

				if (!string.IsNullOrWhiteSpace(actualScriptPath))
					return actualScriptPath;

				return resolvedScriptPath;
			}
			else
				return resolvedScriptPath;
		}

		public string ChangeScriptNameToRelease(string resolvedScriptPath)
		{
			var scriptPreffix = resolvedScriptPath.Substring(0, resolvedScriptPath.Length - ScriptHelperConstants.JSPrefix.Length);
			if (string.IsNullOrWhiteSpace(_scriptContainer.ReleaseSuffix))
				return resolvedScriptPath;
			return string.Format("{0}.{1}.js", scriptPreffix, _scriptContainer.ReleaseSuffix);
		}

		public string ChangeScriptNameToDebug(string resolvedScriptPath)
		{
			var scriptPrefix = resolvedScriptPath.Substring(0, resolvedScriptPath.Length - ScriptHelperConstants.JSPrefix.Length);
			if (string.IsNullOrWhiteSpace(_scriptContainer.DebugSuffix))
				return resolvedScriptPath;

			return string.Format("{0}.{1}.js", scriptPrefix, _scriptContainer.DebugSuffix);
		}

		public static bool HasScriptAlreadyBeenAdded(string scriptToCheck, StringBuilder emittedScript)
		{
			var existingScript = emittedScript.ToString().ToLowerInvariant();
			return (existingScript.Contains(scriptToCheck.ToLowerInvariant()));
		}
	}
}
