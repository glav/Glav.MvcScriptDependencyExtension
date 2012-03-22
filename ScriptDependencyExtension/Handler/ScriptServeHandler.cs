using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ScriptDependencyExtension.Constants;
using ScriptDependencyExtension.Http;
using ScriptDependencyExtension.Helpers;
using ScriptDependencyExtension.Model;

namespace ScriptDependencyExtension.Handler
{
	public class ScriptServeHandler: IHttpHandler
	{
		private static ScriptEngine _scriptEngine;
		private static IHttpContext _contextAdapter;
		private static IScriptDependencyLoader _scriptLoader;
		private static object _lockObject = new object();
		private static ITokenisationHelper _tokenHelper;

		public ScriptServeHandler() { }

		/// <summary>
		/// This constructor is for testability
		/// </summary>
		/// <param name="contextAdapter"></param>
		/// <param name="dependencyLoader"></param>
		/// <param name="engine"></param>
		/// <param name="tokenisationHelper"></param>
		public ScriptServeHandler(IHttpContext contextAdapter, IScriptDependencyLoader dependencyLoader, ScriptEngine engine, ITokenisationHelper tokenisationHelper   )
		{
			_contextAdapter = contextAdapter;
			_scriptLoader = dependencyLoader;
			_scriptEngine = engine;
			_tokenHelper = tokenisationHelper;
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		public void EnsureHandlersAreInitialised(HttpContext context)
		{
			if (_scriptEngine != null && _contextAdapter != null && _scriptLoader != null && _tokenHelper != null)
			{
				return;
			}

			lock (_lockObject)
			{
				if (_scriptEngine == null || _contextAdapter == null || _scriptLoader == null || _tokenHelper == null)
				{
					_contextAdapter = new HttpContextAdapter(context);
					_scriptLoader = new ScriptDependencyLoader(_contextAdapter);
					_scriptLoader.Initialise();
					_scriptEngine = new ScriptEngine(_contextAdapter, _scriptLoader);
					_tokenHelper = new TokenisationHelper();
				}
			}
		}

		public void ProcessRequest(HttpContext context)
		{
			EnsureHandlersAreInitialised(context);

			string contentType = null;
			var scriptToRender = ProcessRequestForUrl(context.Request.RawUrl, out contentType);
			context.Response.ContentType = contentType;
			context.Response.Write(scriptToRender);
		}

		public string ProcessRequestForUrl(string requestUrl,out string contentType)
		{
			var dependencies = _tokenHelper.GetListOfDependencyNamesFromQueryStringTokens(requestUrl,
																						 _scriptLoader.DependencyContainer);
			var scriptType = dependencies[0].TypeOfScript;
			contentType = ScriptHelperConstants.ContentType_Javascript;
			if (dependencies.Count > 0)
			{
				if (scriptType == ScriptType.CSS)
					contentType = ScriptHelperConstants.ContentType_CSS;
			}

			string scriptToRender = null;
			if (!_contextAdapter.IsDebuggingEnabled)
			{
				scriptToRender = _contextAdapter.GetItemFromGlobalCache<string>(requestUrl);
			}
			if (string.IsNullOrWhiteSpace(scriptToRender))
			{
				//var engine = new ScriptEngine(contextAdaptor, scriptLoader);
				var listOfFiles = dependencies.Select(d => d.ScriptPath).ToList();
				var combiner = new FileCombiner(_contextAdapter, listOfFiles);
				StringBuilder contents = new StringBuilder(combiner.CombineFiles());
				_scriptEngine.ApplyFiltersToScriptOutput(contents, scriptType, _scriptLoader.DependencyContainer);
				scriptToRender = contents.ToString();

				// We do an extra check here just in case the script has been added to the cachein between
				// the last check and this one
				if (string.IsNullOrWhiteSpace(_contextAdapter.GetItemFromGlobalCache<string>(requestUrl)))
					_contextAdapter.AddItemToGlobalCache(requestUrl, scriptToRender);
			}
			return scriptToRender;
		}

	}
}
