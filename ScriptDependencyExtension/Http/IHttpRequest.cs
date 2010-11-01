using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptDependencyExtension.Http
{
    public interface IHttpRequest
    {
        Uri Url { get; }
        string ApplicationPath { get; }
    }
}
