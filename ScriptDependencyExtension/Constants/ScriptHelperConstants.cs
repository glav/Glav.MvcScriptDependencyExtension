using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptDependencyExtension.Constants
{
    internal static class ScriptHelperConstants
    {
        public const string ScriptInclude = "<script type='text/javascript' src='{0}?version={1}'></script>";
        public const string CSSInclude = "<link href='{0}?version={1}' rel='stylesheet' type='text/css' />";
        public const string JSPrefix = ".js";

		public const string ErrorMessage_NoHttpContextAvailable = "HttpContext is NULL or not available";

    }
}
