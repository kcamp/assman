using System;
using System.Collections.Generic;
using System.Linq;

using Assman.Configuration;

namespace Assman.TestSupport
{
	public class StubResourceGroupTemplate : IResourceGroupTemplate
	{
		private readonly List<IResourceGroup> _groups = new List<IResourceGroup>();

		public List<IResourceGroup> Groups
		{
			get { return _groups; }
		}

		public StubResourceGroupTemplate(IResourceGroup group)
		{
			if(group != null)
				_groups.Add(group);
		}

		public bool IsMatch(IResource resource)
		{
			return Groups.Any(group => group.Contains(resource));
		}

		public bool IsMatch(string virtualPath)
		{
			return Groups.Any(group => group.GetResources().Any(r => r.VirtualPath.EqualsVirtualPath(virtualPath)));
		}

		public bool MatchesConsolidatedUrl(string consolidatedUrl)
		{
			return Groups.Any(group => group.ConsolidatedUrl.EqualsVirtualPath(consolidatedUrl));
		}

		public ResourceType ResourceType { get; set; }

		public ResourceModeCondition Minify { get; set; }

		public ResourceModeCondition Consolidate { get; set; }

		public IEnumerable<IResourceGroup> GetGroups(IEnumerable<IResource> allResources, ResourceMode mode)
		{
			return from @group in _groups
				   select CreateGroup(@group.ConsolidatedUrl, allResources.Where(@group.Contains), mode);
		}

		public bool TryGetConsolidatedUrl(string virtualPath, ResourceMode resourceMode, out string consolidatedUrl)
		{
			throw new NotImplementedException();
		}

		private IResourceGroup CreateGroup(string consolidatedUrl, IEnumerable<IResource> resources, ResourceMode mode)
		{
			return new ResourceGroup(consolidatedUrl, resources)
			{
				Minify = this.Minify.IsTrue(mode)
			};
		}
	}
}