using Assman.DependencyManagement;
using Assman.TestSupport;

using NUnit.Framework;

using System.Linq;

namespace Assman
{
	[TestFixture]
	public class TestVisualStudioDependencyProvider
	{
		private VisualStudioScriptDependencyProvider _provider;

		[SetUp]
		public void SetupContext()
		{
			_provider = new VisualStudioScriptDependencyProvider();
		}

		[Test]
		public void TripleWackCommentReferenceElementWithPathAttributeIsParsedAsDependency()
		{
			var resource = StubResource.WithContent(@"///<reference path=""~/scripts/jquery.js"" />");

			var dependencies = _provider.GetDependencies(resource).ToList();

			dependencies.CountShouldEqual(1);
			dependencies[0].ShouldEqual("~/scripts/jquery.js");
		}

		[Test]
		public void ReferenceElementWithNameAndAssemblyAttributeReturnsEmbeddedResourceVirtualPath()
		{
			var resource = StubResource.WithContent(@"///<reference name=""MyScript.js"" assembly=""MyAssembly"" />");

			var dependencies = _provider.GetDependencies(resource).ToList();

			dependencies.CountShouldEqual(1);
			dependencies[0].ShouldEqual("assembly://MyAssembly/MyScript.js");
		}

		[Test]
		public void ReferenceElementeWithNameButMissingAssemblyAttributeReturnsEmbeddedResourceVirtualPathForSystemWebExtensionsAssembly()
		{
			var resource = StubResource.WithContent(@"///<reference name=""MicrosoftAjax.js"" />");

			var dependencies = _provider.GetDependencies(resource).ToList();

			dependencies.CountShouldEqual(1);
			dependencies[0].ShouldEqual("assembly://System.Web.Extensions/MicrosoftAjax.js");
		}

		[Test]
		public void MultipleReferenceElementsAreRecognized()
		{
			var resource = StubResource.WithContent(@"///<reference path=""~/scripts/script1.js"" />

				///<reference path=""~/scripts/script2.js"" />");

			var dependencies = _provider.GetDependencies(resource).ToList();

			dependencies.CountShouldEqual(2);
			dependencies[0].ShouldEqual("~/scripts/script1.js");
			dependencies[1].ShouldEqual("~/scripts/script2.js");
		}

		[Test]
		public void WhenReferencePathPointsToVsDocFile_VsDocPartIsRemoved()
		{
			var resource = StubResource.WithContent(@"///<reference path=""~/scripts/jquery-1.4.4-vsdoc.js"" />");

			var dependencies = _provider.GetDependencies(resource).ToList();

			dependencies.CountShouldEqual(1);
			dependencies[0].ShouldEqual("~/scripts/jquery-1.4.4.js");
		}

	    [Test]
	    public void WhenReferencePathIsRelative_ItIsResolvedRelativeToTheResource()
	    {
	        var resource = StubResource.WithContent(@"///<reference path=""../Shared/MyComponent.js"" />");
	        resource.VirtualPath = "~/Views/MyController/MyView.js";

	        var dependencies = _provider.GetDependencies(resource).ToList();

            dependencies.CountShouldEqual(1);
            dependencies[0].ShouldEqual("~/Views/Shared/MyComponent.js");
	    }
	}
}