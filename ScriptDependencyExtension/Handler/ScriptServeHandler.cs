using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ScriptDependencyExtension.Constants;
using ScriptDependencyExtension.Http;
using ScriptDependencyExtension.Helpers;

namespace ScriptDependencyExtension.Handler
{
	public class ScriptServeHandler: IHttpHandler
	{
		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			IHttpContext contextAdaptor = new HttpContextAdapter(context);
			var scriptLoader = new ScriptDependencyLoader(contextAdaptor);
			scriptLoader.Initialise();

			var engine = new ScriptEngine(contextAdaptor, new ScriptDependencyLoader(contextAdaptor));
			var tokenHelper = new TokenisationHelper();
			var dependencies = tokenHelper.GetListOfDependencyNamesFromQueryStringTokens(context.Request.RawUrl,scriptLoader.DependencyContainer);
			
			var scriptType = dependencies[0].TypeOfScript;
			var contentType = ScriptHelperConstants.ContentType_Javascript;
			if (dependencies.Count > 0)
			{
				if (scriptType == ScriptType.CSS)
					contentType = ScriptHelperConstants.ContentType_CSS;
			}
			
			var listOfFiles = dependencies.Select(d => d.ScriptPath).ToList();
			var combiner = new FileCombiner(contextAdaptor,listOfFiles);
			StringBuilder contents = new StringBuilder(combiner.CombineFiles());
			engine.ApplyFiltersToScriptOutput(contents, scriptType,scriptLoader.DependencyContainer);
			
			context.Response.ContentType = contentType;
			context.Response.Write(contents.ToString());
		}
	}
}
