using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Ajax;
using Microsoft.Ajax.Utilities;
using ScriptDependencyExtension.Constants;
using ScriptDependencyExtension.Filters;
using ScriptDependencyExtension.Model;
using ScriptDependencyExtension.Http;

namespace ScriptDependencyExtension.Filters
{
	public class ScriptMinifierFilter : IScriptProcessingFilter
	{
		private IHttpContext _context;

		public ScriptMinifierFilter(IHttpContext context)
		{
			_context = context;
		}
		public string ProcessScript(string scriptContents, ScriptType scriptType, ScriptDependencyContainer container)
		{
			if (!container.ShouldMinifyScriptsInReleaseMode || _context.IsDebuggingEnabled)
				return scriptContents;

			var minifier = new Microsoft.Ajax.Utilities.Minifier();
			switch (scriptType)
			{
				case ScriptType.Javascript:
					return minifier.MinifyJavaScript(scriptContents);
					break;
				case ScriptType.CSS:
					return minifier.MinifyStyleSheet(scriptContents);
					break;
				default:
					return scriptContents;
			}
		}
	}
}
