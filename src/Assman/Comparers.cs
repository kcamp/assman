﻿using System;
using System.Collections.Generic;

namespace Assman
{
    public static class Comparers
    {
        private static readonly IEqualityComparer<IResource> _resource = new ResourceEqualityComparer(); 
        
        public static IEqualityComparer<IResource> Resource
        {
            get { return _resource; }
        }
        
        public static IEqualityComparer<string> VirtualPath
        {
            get { return StringComparer.OrdinalIgnoreCase; }
        }

        public static IEqualityComparer<string> FileSystemPath
        {
            get { return StringComparer.OrdinalIgnoreCase; }
        }

        public static IEqualityComparer<string> RegistryNames
        {
            get { return StringComparer.OrdinalIgnoreCase; }
        }

        private class ResourceEqualityComparer : IEqualityComparer<IResource>
        {
            public bool Equals(IResource x, IResource y)
            {
                return VirtualPath.Equals(x.VirtualPath, y.VirtualPath);
            }

            public int GetHashCode(IResource obj)
            {
                return VirtualPath.GetHashCode(obj.VirtualPath);
            }
        }
    }

    public static class Comparisons
    {
        public static bool EqualsVirtualPath(this string virtualPath1, string virtualPath2)
        {
            return virtualPath1.Equals(virtualPath2, VirtualPath);
        }
        
        public static StringComparison VirtualPath
        {
            get { return StringComparison.OrdinalIgnoreCase; }
        }
    }
}