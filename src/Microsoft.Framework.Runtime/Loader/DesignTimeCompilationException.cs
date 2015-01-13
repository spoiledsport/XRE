// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Framework.Runtime
{
    internal class DesignTimeCompilationException : Exception, ICompilationException
    {
        public DesignTimeCompilationException(IList<CompileResponseError> compileResponseErrors)
            : base(string.Join(Environment.NewLine, compileResponseErrors.SelectMany(e => e.Message)))
        {
            CompilationFailures = compileResponseErrors.GroupBy(g => g.FilePath, StringComparer.OrdinalIgnoreCase)
                                                       .Select(g => new CompilationFailure
                                                       {
                                                           SourceFilePath = g.Key,
                                                           Messages = g.Select(m => new CompilationMessage
                                                           {
                                                               Message = m.Message,
                                                               StartLine = m.StartLine,
                                                               StartColumn = m.StartColumn,
                                                               EndLine = m.EndLine,
                                                               EndColumn = m.EndColumn
                                                           })
                                                       })
                                                       .ToArray();
        }

        public IList<string> Errors { get; }

        public IEnumerable<ICompilationFailure> CompilationFailures { get; }
    }
}