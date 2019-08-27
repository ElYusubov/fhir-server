// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Linq;
using System.Net;
using EnsureThat;
using Hl7.Fhir.Model;
using Microsoft.Health.Fhir.Core.Features.Operations.Export.Models;

namespace Microsoft.Health.Fhir.Api.Features.ActionResults
{
    /// <summary>
    /// Used to return the result of a versions operation.
    /// </summary>
    public class OperationVersionsResult : BaseActionResult<VersionsResult>
    {
        private readonly VersionsResult _versionsResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationVersionsResult"/> class.
        /// </summary>
        /// <param name="versionsResult">The result for the versions operation.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        public OperationVersionsResult(VersionsResult versionsResult, HttpStatusCode statusCode)
            : base(versionsResult, statusCode)
        {
            EnsureArg.IsNotNull(versionsResult, nameof(versionsResult));
            _versionsResult = versionsResult;
        }

        // TODO: Delete?
        ////public override Task ExecuteResultAsync(ActionContext context) // This will have accept header info
        ////{
        ////    // Pull out accept headers and modify result format
        ////    return base.ExecuteResultAsync(context);
        ////}

        protected override object GetResultToSerialize()
        {
            // TODO: determine whether or not this should happen using the accept headers.
            var supportedVersion = new FhirString(_versionsResult.Versions.First());
            var defaultVersion = new FhirString(_versionsResult.DefaultVersion);

            Parameters parameters = new Parameters()
                .Add("version", supportedVersion)
                .Add("default", defaultVersion);

            return parameters;
        }
    }
}
