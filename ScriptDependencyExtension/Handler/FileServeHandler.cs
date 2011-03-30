using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
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
			var listOfFiles = dependencies.Select(d => d.ScriptPath).ToList();
			var combiner = new FileCombiner(contextAdaptor,listOfFiles);
			var combinedContents = combiner.CombineFiles();
			context.Response.Write(combinedContents);
		}
	}
}
