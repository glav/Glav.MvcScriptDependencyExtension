using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptDependencyExtension.Model
{
    public class ScriptDependencyContainer
    {
        public string ReleaseSuffix { get; set; }
        public string DebugSuffix { get; set;  }
		public string VersionIdentifier { get; set; }
		public string VersionMonikerQueryStringName { get; set; }
		public bool ShouldCombineScripts { get; set; }

        private List<ScriptDependency> _knownDependencies = new List<ScriptDependency>();
        public List<ScriptDependency> Dependencies { get { return _knownDependencies; } }
    }

	public static class ScriptDependencyContainerExtensions
	{
		public static ScriptDependency FindDependency(this ScriptDependencyContainer container, string scriptName )
		{
			var dependency = container.Dependencies.SingleOrDefault(s => s.ScriptName.ToLowerInvariant() == scriptName.ToLowerInvariant());
			return dependency;
		}
	}
}
