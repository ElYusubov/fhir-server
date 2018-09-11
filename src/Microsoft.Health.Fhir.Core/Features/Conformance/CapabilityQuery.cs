﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Hl7.Fhir.FhirPath;
using Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Core.Features.Conformance
{
    public class CapabilityQuery
    {
        public CapabilityQuery(string fhirPathPredicate)
        {
            EnsureArg.IsNotNull(fhirPathPredicate, nameof(fhirPathPredicate));

            FhirPathPredicate = fhirPathPredicate;
        }

        public string FhirPathPredicate { get; }

        internal bool IsSatisfiedBy(CapabilityStatement statement)
        {
            return statement.Predicate(FhirPathPredicate);
        }
    }
}
