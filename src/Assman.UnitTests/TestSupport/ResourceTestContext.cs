using System.Linq;

using Assman.Configuration;
using Assman.DependencyManagement;

namespace Assman.TestSupport
{
	public class ResourceTestContext
	{
		private readonly StubResourceFinder _innerFinder;
		private readonly IResourceFinder _outerFinder;
		private readonly ContentFilterPipelineMap _contentFilterPipelineMap;
		private readonly InMemoryDependencyCache _dependencyCache;
		private readonly StubDependencyProvider _dependencyProvider;
		private readonly DependencyManager _dependencyManager;
		private readonly ResourceGroupManager _scriptGroups;
		private readonly ResourceGroupManager _styleGroups;

		public ResourceTestContext(ResourceMode resourceMode = ResourceMode.Debug)
		{
			Mode = resourceMode;
			_innerFinder = new StubResourceFinder();
			_outerFinder = new ResourceModeFilteringFinder(resourceMode, _innerFinder);
			_contentFilterPipelineMap = new ContentFilterPipelineMap();
			_scriptGroups = new ResourceGroupManager(Mode);
			_styleGroups = new ResourceGroupManager(Mode);
			_dependencyCache = new InMemoryDependencyCache();
			_dependencyProvider = new StubDependencyProvider();
			_dependencyManager = new DependencyManager(_outerFinder, _dependencyCache, _scriptGroups, _styleGroups, resourceMode);
			_dependencyManager.MapProvider(".js", _dependencyProvider);
			_dependencyManager.MapProvider(".css", _dependencyProvider);
		}

		public ResourceMode Mode { get; private set; }

		public StubDependencyProvider DependencyProvider
		{
			get { return _dependencyProvider; }
		}

		public ResourceCompiler GetConsolidator()
		{
			return new ResourceCompiler(_contentFilterPipelineMap, _dependencyManager, _scriptGroups, _styleGroups, _outerFinder, Mode);
		}

		public StubResourceBuilder CreateResource(string virtualPath)
		{
			var builder = new StubResourceBuilder(virtualPath, _dependencyProvider);
			_innerFinder.AddResource(builder.Resource);

			return builder;
		}

		public StubResourceGroup CreateGroup(string consolidatedUrl)
		{
			var group = new StubResourceGroup(consolidatedUrl);
			if(group.ResourceType == ResourceType.Script)
				_scriptGroups.Add(new StubResourceGroupTemplate(group));
			else
				_styleGroups.Add(new StubResourceGroupTemplate(group));

			return group;
		}

		public StubResourceGroup CreateGroup(string consolidatedUrl, params string[] resourcesInGroup)
		{
			var group = CreateGroup(consolidatedUrl);
			foreach (var resource in resourcesInGroup)
			{
				CreateResource(resource).InGroup(group);
			}

			return group;
		}

		public void AddGlobalScriptDependencies(params IResource[] globalDependencies)
		{
			_scriptGroups.AddGlobalDependencies(globalDependencies.Select(r => r.VirtualPath));
		}

		public void AddGlobalScriptDependencies(params string[] globalDependencyPath)
		{
			_scriptGroups.AddGlobalDependencies(globalDependencyPath);
		}
	}

	public class StubResourceBuilder
	{
		private readonly StubDependencyProvider _dependencyProvider;
		private readonly StubResource _resource;

		public StubResourceBuilder(string virtualPath, StubDependencyProvider dependencyProvider)
		{
			_dependencyProvider = dependencyProvider;
			_resource = StubResource.WithPath(virtualPath);
		}

		public StubResource Resource
		{
			get { return _resource; }
		}

		public StubResourceBuilder WithDependencies(params IResource[] dependencies)
		{
			_dependencyProvider.SetDependencies(_resource, dependencies.Select(d => d.VirtualPath).ToArray());

			return this;
		}

		public StubResourceBuilder InGroup(StubResourceGroup group)
		{
			group.AddResource(_resource);

			return this;
		}
	}
}