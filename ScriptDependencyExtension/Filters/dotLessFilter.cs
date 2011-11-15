using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptDependencyExtension.Http;
using ScriptDependencyExtension.Model;

namespace ScriptDependencyExtension.Filters
{
	public class dotLessFilter : IScriptProcessingFilter
	{
		private dotless.Core.ILessEngine _lessEngine;
		private IHttpContext _context;
		private static IScriptProcessingFilter _filter;
		private static object _lockObject = new object();
		private ScriptDependencyContainer _dependencyContainer;
		
		public dotLessFilter(IHttpContext context, ScriptDependencyContainer dependencyContainer)
		{
			_context = context;
			_dependencyContainer = dependencyContainer;
			var factory = new dotless.Core.EngineFactory(new dotless.Core.configuration.DotlessConfiguration()
			{
				CacheEnabled = true
			});
			_lessEngine = factory.GetEngine();
		}
		public string ProcessScript(string scriptContents, Constants.ScriptType scriptType, Model.ScriptDependencyContainer container)
		{
			if (scriptType != Constants.ScriptType.CSS && scriptType != Constants.ScriptType.dotLess)
			{
				return scriptContents;
			}

			var processedContents = preProcessScriptContents(scriptContents);
			return _lessEngine.TransformToCss(processedContents, null);
		}

		/// <summary>
		/// Searches the script for an Import statement and attempts to resolve the 
		/// location of the import file with one that is specified in the list of
		/// dependencies otherwise the .less processor will barf when it cannot find the
		/// import file.
		/// </summary>
		/// <param name="scriptContents"></param>
		/// <returns></returns>
		private string preProcessScriptContents(string scriptContents)
		{
			int importPosition = scriptContents.IndexOf("@import");
			if (importPosition >= 0)
			{
				int startOfImportName = scriptContents.IndexOf("\"", importPosition)+1;
				int endOfStatement = scriptContents.IndexOf("\";", importPosition);
				var importName = scriptContents.Substring(startOfImportName, endOfStatement - startOfImportName);
				var dependency = _dependencyContainer.FindDependency(importName);
				if (dependency != null)
				{
					var resolvedPath = _context.ResolvePhysicalFilePathFromRelative(dependency.ScriptPath);
					return scriptContents.Replace(string.Format("@import \"{0}\"", importName), string.Format("@import \"{0}\"", resolvedPath));
				}
				else
				{
					throw new ArgumentNullException(string.Format("Could not find Dependency [{0}] for .Less @import",importName));
				}
				//TODO: replace importname with dependency.scriptname
			}

			return scriptContents;
		}

		public static IScriptProcessingFilter GetDotLessProcessingFilter(IHttpContext context, ScriptDependencyContainer dependencyContainer)
		{
			if (_filter != null)
			{
				return _filter;
			}

			lock (_lockObject)
			{
				if (_filter != null)
				{
					return _filter;
				}
				_filter = new dotLessFilter(context, dependencyContainer);
				return _filter;
			}
		}
	}
}
