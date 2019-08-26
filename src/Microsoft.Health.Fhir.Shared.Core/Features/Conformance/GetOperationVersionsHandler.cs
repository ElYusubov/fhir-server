// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using EnsureThat;
using Microsoft.Health.Fhir.Core.Messages.Get;
using Microsoft.Health.Fhir.Core.Models;

namespace Microsoft.Health.Fhir.Shared.Core.Features.Conformance
{
    public class GetOperationVersionsHandler : RequestHandler<GetOperationVersionsRequest, GetOperationVersionsResponse>
    {
        private readonly IModelInfoProvider _provider;

        public GetOperationVersionsHandler(IModelInfoProvider provider)
        {
            EnsureArg.IsNotNull(provider, nameof(provider));

            _provider = provider;
        }

        protected override GetOperationVersionsResponse Handle(GetOperationVersionsRequest request)
        {
            EnsureArg.IsNotNull(request, nameof(request));
            return new GetOperationVersionsResponse(_provider.SupportedVersion);
        }
    }
}
