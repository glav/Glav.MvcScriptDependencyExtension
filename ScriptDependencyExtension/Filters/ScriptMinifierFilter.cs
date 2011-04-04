using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Ajax;
using Microsoft.Ajax.Utilities;
using ScriptDependencyExtension.Constants;
using ScriptDependencyExtension.Filters;

namespace ScriptDependencyExtension.Filters
{
	public class ScriptMinifierFilter : IScriptProcessingFilter
	{
		public string ProcessScript(string scriptContents, ScriptType scriptType)
		{
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
