using System;
using System.Collections.Generic;
using System.Linq;

namespace AlmWitt.Web.ResourceManagement
{
	public class DependencyManager
	{
		private readonly IResourceFinder _resourceFinder;
		private IDependencyCache _dependencyCache;
		private readonly IResourceGroupManager _scriptGroups;
		private readonly IResourceGroupManager _styleGroups;
		private readonly IDictionary<string, IDependencyProvider> _parsers = new Dictionary<string, IDependencyProvider>(StringComparer.OrdinalIgnoreCase);

		public DependencyManager(IResourceFinder resourceFinder, IDependencyCache dependencyCache, IResourceGroupManager scriptGroups, IResourceGroupManager styleGroups)
		{
			_resourceFinder = resourceFinder;
			_dependencyCache = dependencyCache;
			_scriptGroups = scriptGroups;
			_styleGroups = styleGroups;
		}

		public void MapProvider(string extension, IDependencyProvider dependencyProvider)
		{
			_parsers[extension] = dependencyProvider;
		}

		public IEnumerable<string> GetDependencies(string virtualPath)
		{	
			IEnumerable<string> cachedDependencies;
			if (_dependencyCache.TryGetDependencies(virtualPath, out cachedDependencies))
				return cachedDependencies;

			var dependencyList = new List<IEnumerable<string>>();
			IEnumerable<IResource> resourcesInGroup;
			if(IsConsolidatedUrl(virtualPath, out resourcesInGroup))
			{
				foreach (var resource in resourcesInGroup)
				{
					AccumulateDependencies(dependencyList, resource);
				}

				//filter out dependencies within the group
				return CollapseDependencies(dependencyList).Where(
						d => !resourcesInGroup.Any(r => r.VirtualPath.Equals(d, StringComparison.OrdinalIgnoreCase)));
			}
			else
			{
				AccumulateDependencies(dependencyList, virtualPath);
				return CollapseDependencies(dependencyList);
			}	
		}

		internal int Comparer(IResource x, IResource y)
		{
			var xDepends = GetDependencies(x);
			var yDepends = GetDependencies(y);

			if (xDepends.Contains(y.VirtualPath, StringComparer.OrdinalIgnoreCase))
				return 1;
			if (yDepends.Contains(x.VirtualPath, StringComparer.OrdinalIgnoreCase))
				return -1;

			return 0;
		}

		private bool IsConsolidatedUrl(string virtualPath, out IEnumerable<IResource> resourcesInGroup)
		{
			if (IsConsolidatedUrl(virtualPath, _scriptGroups, out resourcesInGroup))
				return true;
			if (IsConsolidatedUrl(virtualPath, _styleGroups, out resourcesInGroup))
				return true;

			return false;
		}

		private bool IsConsolidatedUrl(string virtualPath, IResourceGroupManager groupTemplates, out IEnumerable<IResource> resourcesInGroup)
		{
			var group = groupTemplates.GetGroupOrDefault(virtualPath, ResourceMode.Debug, _resourceFinder); //the ResourceMode value shouldn't matter here, we'll use Debug because this code will only be executed on a dev box when you haven't pre-consolidated.
			
			if (group == null)
			{
				resourcesInGroup = null;
				return false;
			}
			
			resourcesInGroup = group.GetResources().SortByDependencies(this);
			return true;
		}

		public IEnumerable<string> GetDependencies(IResource resource)
		{
			IEnumerable<string> cachedDependencies;
			if (_dependencyCache.TryGetDependencies(resource.VirtualPath, out cachedDependencies))
				return cachedDependencies;

			var dependencyList = new List<IEnumerable<string>>();
			AccumulateDependencies(dependencyList, resource);

			return CollapseDependencies(dependencyList);
		}

		public void SetCache(IDependencyCache cache)
		{
			_dependencyCache = cache;
		}

		private void AccumulateDependencies(List<IEnumerable<string>> dependencyList, string virtualPath)
		{
			IEnumerable<string> cachedDependencies;
			if (_dependencyCache.TryGetDependencies(virtualPath, out cachedDependencies))
			{
				dependencyList.Insert(0, cachedDependencies);
				return;
			}
			
			var resource = _resourceFinder.FindResource(virtualPath);
			if(resource == null)
				return;

			AccumulateDependencies(dependencyList, resource);
		}

		private void AccumulateDependencies(List<IEnumerable<string>> dependencyList, IResource resource)
		{
			IEnumerable<string> cachedDependencies;
			if (_dependencyCache.TryGetDependencies(resource, out cachedDependencies))
			{
				dependencyList.Insert(0, cachedDependencies);
				//store in cache so that it will also be indexed by virtual path
				_dependencyCache.StoreDependencies(resource, cachedDependencies);
				return;
			}

			IDependencyProvider provider;
			if (!_parsers.TryGetValue(resource.FileExtension, out provider))
				return;

			var dependencyListEntrySize = dependencyList.Count;
			var dependencies = provider.GetDependencies(resource).ToList();
			if (dependencies.Any())
			{
				dependencyList.Insert(0, dependencies);
				foreach (var dependency in dependencies)
				{
					AccumulateDependencies(dependencyList, dependency);
				}
			}
			var dependenciesForCurrentResource = CollapseDependencies(dependencyList.Take(dependencyList.Count - dependencyListEntrySize));
			_dependencyCache.StoreDependencies(resource, dependenciesForCurrentResource);
		}

		private IEnumerable<string> CollapseDependencies(IEnumerable<IEnumerable<string>> dependencyList)
		{
			return dependencyList.SelectMany(d => d)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();
		}
	}

	public static class DependencyManagerExtensions
	{
		public static IEnumerable<IResource> SortByDependencies(this IEnumerable<IResource> resources, DependencyManager dependencyManager)
		{
			return resources.Sort(dependencyManager.Comparer);
		}
	}
}