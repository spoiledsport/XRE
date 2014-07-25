// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Framework.Runtime
{
    [AssemblyNeutral]
    public interface IMetadataProjectReference : IMetadataReference
    {
        string ProjectPath { get; }

        IProjectBuildResult GetDiagnostics();

        IList<ISourceReference> GetSources();

        void EmitReferenceAssembly(Stream stream);

        IProjectBuildResult EmitAssembly(Stream assemblyStream, Stream pdbStream);

        IProjectBuildResult EmitAssembly(string outputPath);
    }
}
