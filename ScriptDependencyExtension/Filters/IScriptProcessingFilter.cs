using ScriptDependencyExtension.Constants;

namespace ScriptDependencyExtension.Filters
{
	public interface IScriptProcessingFilter
	{
		string ProcessScript(string scriptContents, ScriptType scriptType);
	}
}
