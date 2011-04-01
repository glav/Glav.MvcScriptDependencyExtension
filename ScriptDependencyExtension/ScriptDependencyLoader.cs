using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Web;
using ScriptDependencyExtension.Constants;
using ScriptDependencyExtension.Helpers;
using ScriptDependencyExtension.Http;
using ScriptDependencyExtension.Model;

namespace ScriptDependencyExtension
{
	public interface IScriptDependencyLoader
	{
		ScriptDependencyContainer DependencyContainer { get; }
		string DependencyResourceFile { get; set; }
		void LoadDependencies();
		void LoadDependencies(string filename);
		void Initialise();
	}

	internal class ScriptDependencyLoader : IScriptDependencyLoader
	{
		public ScriptDependencyLoader() { }
		public ScriptDependencyLoader(IHttpContext context)
		{
			if (context == null)
				throw new ArgumentNullException("HttpContext cannot be NULL");
			
			_httpContext = context;
		}
		#region properties

		private ScriptDependencyContainer _dependencyContainer = new ScriptDependencyContainer();
        public ScriptDependencyContainer DependencyContainer { get { return _dependencyContainer; } }

        private static object _lockObject = new object();
    	private IHttpContext _httpContext;

        private const string FILENAME = "ScriptDependencies.xml";
        private readonly string[] FILEPATHS = new string[] 
        {
            ".",
            "..",
            ".\\Dependencies",
            "..\\Dependencies",
            AppDomain.CurrentDomain.BaseDirectory,
            HttpContext.Current != null ? HttpContext.Current.Request.PhysicalApplicationPath : string.Empty,
            HttpContext.Current != null ? HttpContext.Current.Request.PhysicalPath : string.Empty,
            Environment.GetFolderPath(Environment.SpecialFolder.Resources),
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            Environment.GetFolderPath(Environment.SpecialFolder.SystemX86),
            Environment.GetFolderPath(Environment.SpecialFolder.Windows)
        };

        public string DependencyResourceFile { get; set; }

		public bool IsInitialised
		{
			get
			{
				return
					(_httpContext.GetItemFromGlobalCache<List<ScriptDependency>>(ScriptHelperConstants.CacheKey_ScriptDependencies) !=
					 null);
			}
		}
		
		#endregion


		internal void FindDependencyFile()
        {
            // look in well known locations for a script dependency file
            foreach (var dirPath in FILEPATHS)
            {
                string tryPath = string.Format("{0}\\{1}", dirPath, FILENAME);
                if (Directory.Exists(dirPath) && File.Exists(tryPath))
                {
                    DependencyResourceFile = tryPath;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(DependencyResourceFile))
                throw new FileNotFoundException(string.Format("{0} Dependency file not found.", FILENAME));
        }

		public void LoadDependencies()
        {
            LoadDependencies(DependencyResourceFile);
        }

		public void LoadDependencies(string filename)
        {
            // If we did not find a file, or the dependency file load ended up loading nothing,
            // then at least load up some known dependencies.
            if (string.IsNullOrWhiteSpace(filename))
                return;

            var xmlFile = XDocument.Load(filename);
            var rootElement = xmlFile.Element(XmlConstants.DependenciesNode);
            var dependencies = from e in rootElement.Elements(XmlConstants.DependencyNode)
                               select e;
            var releaseSuffixEl = rootElement.Attribute(XmlConstants.ReleaseSuffixAttribute);
            var debugSuffixEl = rootElement.Attribute(XmlConstants.DebugSuffixAttribute);
			var scriptVersion = rootElement.Attribute(XmlConstants.ResourceVersionIdentifierAttribute);
        	var versionMoniker = rootElement.Attribute(XmlConstants.VersionMonikerQueryStringName);
			var combineScripts = rootElement.Attribute(XmlConstants.CombineScriptsAttribute);

			_dependencyContainer.VersionIdentifier = scriptVersion !=null ? scriptVersion.Value : "1.0";
            _dependencyContainer.ReleaseSuffix = releaseSuffixEl != null ? releaseSuffixEl.Value : string.Empty;
            _dependencyContainer.DebugSuffix = debugSuffixEl != null ? debugSuffixEl.Value : string.Empty;
        	_dependencyContainer.VersionMonikerQueryStringName = versionMoniker != null ? versionMoniker.Value : "version";
        	_dependencyContainer.ShouldCombineScripts = combineScripts != null
        	                                            	? (combineScripts.Value.ToLower() == "true")
        	                                            	: false;

            ExtractDependencies(dependencies);
        }

        private void ExtractDependencies(IEnumerable<XElement> dependencies)
        {
			if (IsInitialised)
				return;

			ITokenisationHelper tokenHelper = new TokenisationHelper();
            foreach (var dependency in dependencies)
            {
                var scriptDependency = new ScriptDependency();
                var nameAttrib = dependency.Attribute(XmlConstants.NameAttribute);
                if (nameAttrib != null)
                {
                    scriptDependency.ScriptName = nameAttrib.Value.ToLowerInvariant();
                	scriptDependency.ScriptNameToken = tokenHelper.TokeniseString(scriptDependency.ScriptName);
                }

                var typeAttrib = dependency.Attribute(XmlConstants.TypeAttribute);
                if (typeAttrib != null)
                {
                    scriptDependency.TypeOfScript = ScriptType.Javascript;
                    if (typeAttrib.Value.ToLowerInvariant() == XmlConstants.CSSTypeValue)
                    {
                        scriptDependency.TypeOfScript = ScriptType.CSS;
                    }
                }

                var file = dependency.Element(XmlConstants.ScriptFileElement);
                if (file != null)
                {
                    scriptDependency.ScriptPath = file.Value;
                }
                var requiredDependencies = dependency.Element(XmlConstants.RequiredDependenciesNode);
                if (requiredDependencies != null)
                {
                    var names = from rd in requiredDependencies.Elements(XmlConstants.NameElement)
                                select rd;
                    if (names.Count() > 0)
                    {
                        scriptDependency.RequiredDependencies = new List<string>();
                        names.ToList().ForEach(n => scriptDependency.RequiredDependencies.Add(n.Value.ToLowerInvariant()));
                    }
                }
                _dependencyContainer.Dependencies.Add(scriptDependency);
            }
			_httpContext.AddItemToGlobalCache(ScriptHelperConstants.CacheKey_ScriptDependencies,_dependencyContainer.Dependencies);
        }

		public void Initialise()
        {
            if (_dependencyContainer.Dependencies.Count > 0 || IsInitialised)
                return;

            lock (_lockObject)
            {
                if (_dependencyContainer.Dependencies.Count == 0)
                {
                    FindDependencyFile();
                    LoadDependencies();

                }
            }
        }

    }
}
