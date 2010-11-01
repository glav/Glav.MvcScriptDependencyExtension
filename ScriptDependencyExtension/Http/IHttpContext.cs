using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptDependencyExtension.Http
{
    public interface IHttpContext
    {
        bool IsDebuggingEnabled { get; }
        IHttpRequest Request { get; }
        bool HasValidWebContext { get; }

    }
}
