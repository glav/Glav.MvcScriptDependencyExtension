using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptDependencyExtension.Constants;

namespace ScriptDependencyExtension
{
	public interface IScriptProcessingFilter
	{
		string ProcessScript(string scriptContents, ScriptType scriptType);
	}
}
