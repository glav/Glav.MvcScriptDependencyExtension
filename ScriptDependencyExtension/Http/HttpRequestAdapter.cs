using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ScriptDependencyExtension.Http
{
    public class HttpRequestAdapter : IHttpRequest
    {
        #region Constructors
        public HttpRequestAdapter() {}

        public HttpRequestAdapter(HttpRequest request)
        {
            if (request != null)
            {
                _url = request.Url;
                _applicationPath = request.ApplicationPath;
            }
        }

        #endregion

        private Uri _url;
        public Uri Url
        {
            get { return _url; }
        }

        private string _applicationPath;
        public string ApplicationPath
        {
            get { return _applicationPath; }
        }
    }
}
