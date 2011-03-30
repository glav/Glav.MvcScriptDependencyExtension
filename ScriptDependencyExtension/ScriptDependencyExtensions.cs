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
		private static IScriptDependencyLoader _scriptLoader;

		public ScriptHelper(IHttpContext context)
		{
			WebHttpContext = context;
		}

		#region properties

		public IHttpContext WebHttpContext { get; set; }


		#endregion

		private static void InitScriptLoader(IHttpContext context)
		{
			if (_scriptLoader == null)
			{
				_scriptLoader = new ScriptDependencyLoader(context);
				_scriptLoader.Initialise();
			}
		}
		#region RequiresScript Helper methods

		public static MvcHtmlString RequiresScript(string scriptName)
		{
			if (HttpContext.Current == null)
				throw new ArgumentNullException(ScriptHelperConstants.ErrorMessage_NoHttpContextAvailable);

			return RequiresScript(new HttpContextAdapter(HttpContext.Current), scriptName);
		}
		internal static MvcHtmlString RequiresScript(IHttpContext context, string scriptName)
		{
			if (string.IsNullOrWhiteSpace(scriptName))
				return MvcHtmlString.Empty;

			InitScriptLoader(context);

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
			InitScriptLoader(context);

			ScriptEngine engine = new ScriptEngine(context, _scriptLoader);
			// Use this to register multiple scripts and prevent multiple entries of base script like
			// jQuery core.
			StringBuilder emittedScript = new StringBuilder();

			// First lets see if the required script has any dependencies and include them first
			if (scriptNames != null && scriptNames.Length > 0)
			{
				foreach (var scriptName in scriptNames)
				{
					if (!string.IsNullOrWhiteSpace(scriptName))
					{
						engine.GenerateDependencyScript(scriptName, emittedScript);
					}
				}

				engine.GenerateCombinedScriptsIfRequired(emittedScript);
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
			InitScriptLoader(context);

			ScriptEngine engine = new ScriptEngine(context, _scriptLoader);

			// First lets see if the required script has any dependencies and include them first
			if (scriptNames != null && scriptNames.Length > 0)
			{
				foreach (var scriptName in scriptNames)
				{
					if (!string.IsNullOrWhiteSpace(scriptName))
						engine.AddScriptToDeferredList(scriptName);
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
			InitScriptLoader(context);

			ScriptEngine engine = new ScriptEngine(context, _scriptLoader);

			return MvcHtmlString.Create(engine.RenderDeferredScriptsToBuffer());

		}
		#endregion

	}
}
