using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptDependencyExtension;
using System.IO;

namespace ScriptDependencyTests
{
    [TestClass]
    public class ScriptDependencyLoaderTests
    {
        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void FileLocatorShouldFindFile()
        {
            ScriptDependencyLoader loader = new ScriptDependencyLoader();

            try
            {
                loader.FindDependencyFile();
                Assert.IsFalse(string.IsNullOrWhiteSpace(loader.DependencyResourceFile));
            }
            catch
            {
                Assert.Fail("No Dependency file found.");
            }
            
        }
        
        [TestMethod]
        [DeploymentItem("ScriptDependencies.xml")]
        public void LoaderShouldLoadDependenciesFromFile()
        {
        	ScriptDependencyLoader loader = new ScriptDependencyLoader(new MockContext());
            loader.FindDependencyFile();
            Assert.IsFalse(string.IsNullOrWhiteSpace(loader.DependencyResourceFile));

            loader.LoadDependencies();
            Assert.IsNotNull(loader.DependencyContainer);
            Assert.IsNotNull(loader.DependencyContainer.Dependencies);
            Assert.IsTrue(loader.DependencyContainer.Dependencies.Count > 0);
        }
        
        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void LoaderShouldThrowIfNoDependenciesFound()
        {
            if (File.Exists("ScriptDependencies.xml"))
                File.Delete("ScriptDependencies.xml");
            ScriptDependencyLoader loader = new ScriptDependencyLoader();
            loader.FindDependencyFile();
        }
    }
}
