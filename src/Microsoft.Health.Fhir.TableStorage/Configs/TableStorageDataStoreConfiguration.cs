﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

namespace Microsoft.Health.Fhir.TableStorage.Configs
{
    public class TableStorageDataStoreConfiguration
    {
        public string ConnectionString { get; set; }

        public string TableName { get; set; } = "fhirResources";

        public bool AllowTableScans { get; set; } = true;
    }
}
