// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Net;
using EnsureThat;
using Microsoft.Health.Fhir.Core.Features.Operations.Export.Models;

namespace Microsoft.Health.Fhir.Api.Features.ActionResults
{
    /// <summary>
    /// Used to return the result of a versions operation.
    /// </summary>
    public class OperationVersionsResult : BaseActionResult<VersionsResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationVersionsResult"/> class.
        /// </summary>
        /// <param name="versionsResult">The result for the versions operation.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        public OperationVersionsResult(VersionsResult versionsResult, HttpStatusCode statusCode)
            : base(versionsResult, statusCode)
        {
            EnsureArg.IsNotNull(versionsResult, nameof(versionsResult));
        }
    }
}
