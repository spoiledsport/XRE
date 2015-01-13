// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Framework.DesignTimeHost.Models.OutgoingMessages
{
    public class DiagnosticsMessage
    {
        public FrameworkData Framework { get; set; }

        public IList<string> Warnings { get; set; }

        [JsonIgnore]
        public IList<ICompilationFailure> Errors { get; set; }

        [JsonProperty(PropertyName = "Errors")]
        public IEnumerable<string> ErrorMessages
        {
            get
            {
                return Errors.SelectMany(e => e.Messages)
                             .Select(m => m.Message);
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as DiagnosticsMessage;

            return other != null && 
                 Enumerable.SequenceEqual(Warnings, other.Warnings) &&
                 Enumerable.SequenceEqual(Errors, other.Errors);
        }

        public override int GetHashCode()
        {
            // These objects are currently POCOs and we're overriding equals
            // so that things like Enumerable.SequenceEqual just work.
            return base.GetHashCode();
        }
    }
}