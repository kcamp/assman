﻿using System;

namespace Assman.PreCompilation
{
    public class PreCompiledResourceExcluder : IFinderExcluder
    {
        public bool ShouldExclude(IResource resource)
        {
            return resource.VirtualPath.EndsWith(".compiled.js", Comparisons.VirtualPath)
                || resource.VirtualPath.EndsWith(".compiled.css", Comparisons.VirtualPath);
        }
    }
}