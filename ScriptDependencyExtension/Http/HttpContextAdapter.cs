using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace ScriptDependencyExtension.Http
{
	public class HttpContextAdapter : IHttpContext
	{
		HttpContext _context;

		#region Constructors
		public HttpContextAdapter()
		{

		}
		public HttpContextAdapter(HttpContext context)
		{
			_hasValidWebContext = false;
			if (context != null)
			{
				_context = context;
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

		public string ApplicationDirectory
		{
			get
			{
				if (System.Web.HttpContext.Current != null)
				{
					return System.Web.HttpRuntime.AppDomainAppPath;
				}
				else
				{
					return System.Environment.CurrentDirectory;
				}
			}
		}

		public string ResolveScriptRelativePath(string relativePath)
		{
			if (HasValidWebContext && !string.IsNullOrWhiteSpace(relativePath))
			{
				var relReplace = relativePath.Replace("~", _context.Request.ApplicationPath);
				if (!string.IsNullOrWhiteSpace(relReplace))
					return relReplace.Replace("//", "/");
				return relReplace;
			}
			return ResolveRelativePathWithNoHttpContext(relativePath);
		}

		public string ResolvePhysicalFilePathFromRelative(string relativePath)
		{
			if (HasValidWebContext && !string.IsNullOrWhiteSpace(relativePath))
			{
				var relReplace = relativePath.Replace("~", ApplicationDirectory);
				if (!string.IsNullOrWhiteSpace(relReplace))
				{
					return relReplace.Replace("/", "\\").Replace("\\\\", "\\");
				}
				return relReplace;
			}
			return ResolveRelativePathWithNoHttpContext(relativePath);
		}

		private string ResolveRelativePathWithNoHttpContext(string relativePath)
		{
			if (!string.IsNullOrWhiteSpace(relativePath))
				return relativePath.Replace("~", "");

			return relativePath;
		}


		public IDictionary PerRequestItemCache
		{
			get { return _context.Items; }
		}

		public T GetItemFromGlobalCache<T>(string cacheKey) where T : class
		{
			return _context.Cache[cacheKey] as T;
		}

		public void AddItemToGlobalCache(string cacheKey, object data)
		{
			_context.Cache.Add(cacheKey, data, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
		}
	}
}
