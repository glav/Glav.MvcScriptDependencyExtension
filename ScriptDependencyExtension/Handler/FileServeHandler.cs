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
	public class FileServeHandler: IHttpHandler
	{
		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			IHttpContext contextAdaptor = new HttpContextAdapter(context);
			ScriptDependencyLoader scriptLoader = new ScriptDependencyLoader(contextAdaptor);
			scriptLoader.Initialise();

			var engine = new ScriptEngine(contextAdaptor, new ScriptDependencyLoader(contextAdaptor));
			var tokenHelper = new TokenisationHelper();
			var dependencies = tokenHelper.GetListOfDependencyNamesFromQueryStringTokens(context.Request.RawUrl,scriptLoader.DependencyContainer);
			var contentType = "application/x-javascript";
			if (dependencies.Count > 0)
			{
				if (dependencies[0].TypeOfScript == ScriptType.CSS)
					contentType = "text/css";
			}
			var listOfFiles = dependencies.Select(d => d.ScriptPath).ToList();
			var combiner = new FileCombiner(contextAdaptor,listOfFiles);
			var combinedContents = combiner.CombineFiles();
			context.Response.ContentType = contentType;
			context.Response.Write(combinedContents);
		}
	}
}
