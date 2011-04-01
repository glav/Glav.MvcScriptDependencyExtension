using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptDependencyExtension.Constants;

namespace ScriptDependencyExtension.Model
{
    public class ScriptDependency
    {
        public string ScriptName { get; set; }
		public string ScriptNameToken { get; set; }
        public string ScriptPath { get; set; }
        public List<string> RequiredDependencies { get; set;  }
        public ScriptType TypeOfScript { get; set; }
    }


}
