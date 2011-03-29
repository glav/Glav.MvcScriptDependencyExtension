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

		public FileCombinationResult CombineFiles(string fileExtension)
		{
			var result = new FileCombinationResult();

			result.CmbinedFileContents = CombineFileContents();
			result.CombinedFileName = GenerateFileName(fileExtension);

			return result;
		}

		private string GenerateFileName(string fileExtension)
		{
			var realExtension = new StringBuilder();
			if (!string.IsNullOrWhiteSpace(fileExtension))
			{
				if (fileExtension[0] != '.')
				{
					realExtension.Append('.');
				}
				realExtension.Append(fileExtension);
			}

			string moniker = GenerateUniqueMonikerBasedOnFileNames(_filesToCombine);
			return string.Format("Resource-{0}.combined{1}", moniker, realExtension.ToString());
		}

		private string GenerateUniqueMonikerBasedOnFileNames(IEnumerable<string> filesToCombine)
		{
			Decimal nameValue = 0;
			foreach (var filename in filesToCombine)
			{
				var unicodeBytes = System.Text.UnicodeEncoding.Unicode.GetBytes(filename);
				for (int pos = 0; pos < unicodeBytes.Length; pos++ )
				{
					nameValue += ((int) unicodeBytes[pos]) * (pos+1);
				}
			}
			return nameValue.ToString();
		}

		private string CombineFileContents()
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

	public class FileCombinationResult
	{
		public string CombinedFileName { get; set; }
		public string CmbinedFileContents { get; set; }
	}
}
