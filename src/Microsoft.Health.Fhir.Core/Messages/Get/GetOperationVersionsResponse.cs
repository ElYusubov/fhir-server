// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;

namespace Microsoft.Health.Fhir.Core.Messages.Get
{
    public class GetOperationVersionsResponse
    {
        public GetOperationVersionsResponse(string operationVersionsStatement)
        {
            EnsureArg.IsNotNull(operationVersionsStatement, nameof(operationVersionsStatement));

            OperationVersionsStatement = operationVersionsStatement;
        }

        public string OperationVersionsStatement { get; }
    }
}
