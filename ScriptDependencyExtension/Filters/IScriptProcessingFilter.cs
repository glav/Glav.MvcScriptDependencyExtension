using ScriptDependencyExtension.Constants;
using ScriptDependencyExtension.Model;

namespace ScriptDependencyExtension.Filters
{
	public interface IScriptProcessingFilter
	{
		string ProcessScript(string scriptContents, ScriptType scriptType, ScriptDependencyContainer container);
	}
}
