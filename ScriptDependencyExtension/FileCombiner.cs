using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ScriptDependencyExtension.Http;

namespace ScriptDependencyExtension
{
	public class FileCombiner
	{
		private List<string> _filesToCombine = new List<string>();
		private IHttpContext _httpContext;

		public FileCombiner(IHttpContext context)
		{
			_httpContext = context;
		}
		public FileCombiner(IHttpContext context, IEnumerable<string> filessToCombine) : this(context) 
		{
			_filesToCombine.AddRange(filessToCombine);	
		}

		public void AddFileToCombine(string filename)
		{
			if (!_filesToCombine.Contains(filename))
				_filesToCombine.Add(filename);
		}

		public string CombineFiles()
		{
			var fileContents = new StringBuilder();
			foreach (var filename in _filesToCombine)
			{
				var fileData = File.ReadAllText(_httpContext.ResolveScriptRelativePath(filename));
				fileContents.Append(fileData);
			}
			return fileContents.ToString();
		}
	}

}
