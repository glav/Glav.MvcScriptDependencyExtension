using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace ScriptDependencyExtension.Http
{
    public interface IHttpContext
    {
        bool IsDebuggingEnabled { get; }
        bool HasValidWebContext { get; }
        string ResolveScriptRelativePath(string relativePath);
        IDictionary PerRequestItemCache { get;  } 

    }
}
