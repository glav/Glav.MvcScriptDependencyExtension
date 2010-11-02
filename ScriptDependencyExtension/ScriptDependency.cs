using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptDependencyExtension
{
    public class ScriptDependency
    {
        public string ScriptName { get; set; }
        public string ScriptPath { get; set; }
        public List<string> RequiredDependencies { get; set;  }
    }

    public class ScriptName
    {
        public const string jQuery = "jquery";
        public const string jQueryValidate = "jquery-validate";
        public const string jqueryValidateUnobtrusive = "jquery-validate-unobtrusive";

        public const string MicrosoftAjax = "Microsoft-Ajax";
        public const string MicrosoftMvcAjax = "Microsoft-Mvc-Ajax";
        public const string MicrosoftMvcValidation = "Microsoft-Mvc-Validation";

        public const string AllScripts = "allscripts";
    }
}
