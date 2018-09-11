﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Diagnostics;
using Hl7.Fhir.Model;

namespace Microsoft.Health.Fhir.Core.Exceptions
{
    public class ResourceNotFoundException : FhirException
    {
        public ResourceNotFoundException(string message)
            : base(message)
        {
            Debug.Assert(!string.IsNullOrEmpty(message), "Exception message should not be empty");

            Issues.Add(new OperationOutcome.IssueComponent
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.NotFound,
                Diagnostics = message,
            });
        }
    }
}