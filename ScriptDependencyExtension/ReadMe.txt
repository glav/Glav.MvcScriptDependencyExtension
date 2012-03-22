ScriptDependencyExtension Readme/Getting Started
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


The purpose of the script dependency extension is the following:

1. Allow an easy way of describing dependencies between CSS files or Javascript files.
Quite often an application will require multiple javascript files to work properly. Things like
ASP.NET MVC ship with a multitude of script files that are required depending on what features you
use. It can be hard remembering what sets of files are needed for what purposes so it would be nice
to express this in a descriptive, easy to use way. Something like saying include "jQuery-Core" or
"jQuery-validation" and all the necessary scripts are included. Similarly, you can express groups
of dependencies with descriptive names such as "CommonFiles" or "HomePage".

2. Allow easy minification of scripts and CSS files, in addition to combining everything required into
a single file.
This follows best practice recommendations and means your site only ever needs to load 1 javascript file or
1 CSS file for each page

3. Allow a "deferred" rendering of Javascript include files until aspecified point in the page rendering.
This again follows best practice and allows us to include any number of scripts and dependencies during
page creation, within partial views etc, then have a single point at which these "deferred" scripts are all
rendered to the page. This is typically near the bottom of the page so that script resource loading does not
stop the loading and rendering of the page.

The usage is as follows:
* First you need to define your script/css dependencies and what files make them up. This is done via the ScriptDependencies.xml
file. A ScriptDependencies.xml file needs to be present within your web application. Typically this is within 
the root directory of the site but can be within the BIN directory, Dependencies folder (if it exists)
and a number of other well known locations (such as the Windows directory).
The ScriptDependencies.xml file contains all the named script and CSS dependencies of your application.
It's format is shown below:

<?xml version="1.0" encoding="utf-8" ?>
<Dependencies ReleaseSuffix="min" DebugSuffix="" VersionIdentifier="1.0" VersionMonikerQueryStringName="v" 
              CombineScripts="false" MinifyCombinedScriptsInReleaseMode="true"  EnableDotLessSupport="true">

  <Dependency Name="YourDependencyName" Type="js">
    <ScriptFile>~/Scripts/SomeFile.js</ScriptFile>
    <RequiredDependencies>
      <Name>jquery</Name>
      <Name>SomeOtherDependency</Name>
    </RequiredDependencies>
  </Dependency>

  <Dependency Name="jQuery" Type="js">
    <ScriptFile>~/Scripts/jquery.js</ScriptFile>
  </Dependency>

  <Dependency Name="SomeOtherDependency" Type="js">
    <ScriptFile>~/Scripts/SomeOtherDependency.js</ScriptFile>
  </Dependency>

  <Dependency Name="SomeOtherDependency" Type="css">
    <ScriptFile>~/css/SomeCssFile.css</ScriptFile>
  </Dependency>
</Dependencies>

Each <Dependency> element must have a unique name, and a Type of either "js" for javascript
or "css" for CSS files.
Dependencies can be chained using the <RequiredDependencies> element and subsequent <Name>
elements representing the dependency name to also include.

The attributes in the <Dependencies> element above do the following:
--> ReleaseSuffix="min" : If not combining scripts into 1, then this suffix is appended to the 
    javascript file in release mode. So 'SomeScript.js'becomes 'SomeScript.min.js'
--> DebugSuffix="debug": If not combining scripts into 1, then this suffix is appended to the 
    javascript file in debug mode. So 'SomeScript.js'becomes 'SomeScript.debug.js'
--> VersionIdentifier="1.0": This is used as part of the query string of the link to the
    dependency to control browser caching. For example, in conjunction with the 'VersionMonikerQueryStringName'
	property, each request to a resource will have ?v=## appended where ## is the value
	of the version number attribute. The version number can be changed to anything you like.
--> VersionMonikerQueryStringName="v": This is the string used to identify the version
    number argument. So if the value of this property is 'VERSION', then
	?VERSION=## will be appended to the query string where ## is the VersionIdentifer value
	described above.
--> CombineScripts="true/false": This determines if scripts are combined. When renderring
    deferred scripts they are rendered out using separate <script> tags for each resource if
	this attribute is false. If this is set to true, then all deferred scripts are rendered in
	a combined single request using the handler.
--> MinifyCombinedScriptsInReleaseMode="true/false": This determines if scripts are
    automatically minifed when the application is running release mode. Currently the
	default minifier is the Microsoft minifier but this is easily swapped out (however
	requires you to code the implementation/adapter to your alternate implementation)
--> EnableDotLessSupport="true": This will enable parsing of dotLess syntax in your CSS files. There is no
	requirement to have a .less extension or anything like that. dotLess parsing will automatically be run on
	every file.

* Once you have defined your dependencies and the scripts that make them up, 
you need to ensure the handler is in place that generates the combined links.
Add the following handler to your <system.webServer> section as shown below:
  <system.webServer>
    <handlers>
      <add name="ScriptDependencyHandler" path="ScriptDependency.axd" verb="*" type="ScriptDependencyExtension.Handler.ScriptServeHandler" resourceType="Unspecified" preCondition="integratedMode"/>
    </handlers>
 </system.webServer>

 Note: If using older versions of IIS (ie. prior to IIS 7) or using the local web server Cassini,
 you need to still add the handler into the the <system.web> section as shown below
 
 <httpHandlers>
      <add verb="*" type="ScriptDependencyExtension.Handler.ScriptServeHandler" path="ScriptDependency.axd"/>
 </httpHandlers>

 * Finally, you can now reference your scripts in the page using the following syntax:

 --> @ScriptDependencyExtension.ScriptHelper.RequiresScripts("{dependency-name}")
   - This will immediately render scripts to the page that make up the {dependency-name}
   - dependency defined in your ScriptDependencies.xml file.
   - eg. @ScriptDependencyExtension.ScriptHelper.RequiresScripts("Microsoft-Mvc-Ajax" )

--> @ScriptDependencyExtension.ScriptHelper.RequiresScriptsDeferred("{dependency-name}")
  - This will add this dependency (and all it's dependent scripts) to a list that will be
  - rendered later with the 'RenderDeferredScripts' is called.
  - eg. @ScriptDependencyExtension.ScriptHelper.RequiresScriptsDeferred("Microsoft-Mvc-Ajax")

--> @ScriptDependencyExtension.ScriptHelper.RenderDeferredScripts()
  - This will render 1 link that will contain ALL the scripts that have been included as part
  - of any previous 'RequiresScriptsDeferred' calls. This includes calls in the calling page,
  - partial views or anywhere in the page lifecycle prior to rendering. 1 script link will
  - be generated only which will describe all dependencies required.

dotLess Support
~~~~~~~~~~~~~~~
The ScriptHelper supports parsing CSS files with dotLess syntax. One of the features of dotLess is being 
able to @import other files into your CSS file. This feature relies on physical paths and because the
ScriptHelper is running from an ambiguous directory path this is often not resolved. This is catered for
in the script helper by simply defining a script dependency in the ScriptDependencies.xml file with the name
of the file that dotLess needs to import. The ScriptHelper will then look for @import statements and try and
match the name of the import with known dependencies defined in the ScriptDependencies.xml file. It will then
replace the @import statement with the fully qualified path name defined for the dependency.
For example :-
CSS File may contain:
@import "Common.less"

.some-style {
	color: @baseColor;
}

ScriptDependencies.xml should contain:
  <Dependency Name="Common.less" Type="css">
    <ScriptFile>~/Styles/Common.less</ScriptFile>
  </Dependency>

and everything will be resolved fine.




You can contact me via my blog http://weblogs.asp.net/pglavich if you have any questions, 
or via email at glav@aspalliance.com. You can find the full source code to this library
on Bitbucket at http://bitbucket.org/ScriptDependency