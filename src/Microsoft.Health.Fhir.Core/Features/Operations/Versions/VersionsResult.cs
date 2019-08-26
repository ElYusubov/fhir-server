// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using EnsureThat;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Core.Features.Operations.Export.Models
{
    /// <summary>
    /// Class used to hold data that needs to be returned to the client when the versions operation completes.
    /// </summary>
    public class VersionsResult
    {
        public VersionsResult(List<string> versions, string defaultVersion)
        {
            EnsureArg.IsNotNull(versions, nameof(versions));
            EnsureArg.IsNotNull(defaultVersion, nameof(defaultVersion));

            Versions = versions;
            DefaultVersion = defaultVersion;
        }

        [JsonProperty("versions")]
        public List<string> Versions { get; }

        [JsonProperty("default")]
        public string DefaultVersion { get; }
    }
}
