using System;
using System.Collections.Generic;
using System.IO;

using Assman.Configuration;

namespace Assman.Registration
{
    public class DependencyResolvingResourceRegistry : IReadableResourceRegistry
    {
        private readonly IResourceRegistry _inner;
        private readonly AssmanContext _context;

        public DependencyResolvingResourceRegistry(IResourceRegistry inner, AssmanContext context)
        {
            _inner = inner;
            _context = context;
        }

        public bool TryResolvePath(string resourcePath, out IEnumerable<string> resolvedResourcePaths)
        {
            return _inner.TryResolvePath(resourcePath, out resolvedResourcePaths);
        }

        public void Require(string resourcePath)
        {
            var canonicalPath = ToCanonicalPath(resourcePath);
            foreach(var dependency in _context.GetResourceDependencies(canonicalPath))
            {
                _inner.Require(dependency);
            }
            _inner.Require(canonicalPath);
        }

        public void RegisterInlineBlock(Action<TextWriter> block, object key)
        {
            _inner.RegisterInlineBlock(block, key);
        }

        public bool IsInlineBlockRegistered(object key)
        {
            return _inner.IsInlineBlockRegistered(key);
        }

        public IEnumerable<string> GetIncludes()
        {
            return _inner.AsReadable().GetIncludes();
        }

        public IEnumerable<Action<TextWriter>> GetInlineBlocks()
        {
            return _inner.AsReadable().GetInlineBlocks();
        }

        private string ToCanonicalPath(string path)
        {
            //TODO: This implementation is naive, consider doing what the ConsolidatingResourceRegistry does (and eliminate the duplication)
            if (path.StartsWith("/"))
                return "~" + path;

            return path;
        }
    }
}