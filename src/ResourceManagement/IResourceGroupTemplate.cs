using System.Collections.Generic;

namespace AlmWitt.Web.ResourceManagement
{
	public interface IResourceGroupTemplate : IResourceFilter
	{
		bool MatchesConsolidatedUrl(string consolidatedUrl);
		ResourceType ResourceType { get; }
		bool Minify { get; }
		IEnumerable<IResourceGroup> GetGroups(IEnumerable<IResource> allResources, ResourceMode mode);
		bool TryGetConsolidatedUrl(string virtualPath, out string consolidatedUrl);
	}
}