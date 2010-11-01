using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ScriptDependencyExtension.Http
{
    public class HttpContextAdapter : IHttpContext
    {
        #region Constructors
        public HttpContextAdapter()
        {

        }
        public HttpContextAdapter(HttpContext context)
        {
            _hasValidWebContext = false;
            if (context != null)
            {
                _request = new HttpRequestAdapter(context.Request);
                _isDebuggingEnabled = context.IsDebuggingEnabled;
                _hasValidWebContext = true;
            }
        }

        #endregion

        private bool _hasValidWebContext;
        public bool HasValidWebContext
        {
            get { return _hasValidWebContext; }
        }

        private bool _isDebuggingEnabled;
        public bool IsDebuggingEnabled
        {
            get { return _isDebuggingEnabled; }
        }

        IHttpRequest _request;
        public IHttpRequest Request
        {
            get { return _request; }
        }
    }
}
