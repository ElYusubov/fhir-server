﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using EnsureThat;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Health.Extensions.DependencyInjection;
using Microsoft.Health.Fhir.Core.Features.Persistence;

namespace Microsoft.Health.Fhir.Api.Modules
{
    /// <summary>
    /// Installs mediation components in container
    /// </summary>
    public class MediationModule : IStartupModule
    {
        /// <inheritdoc />
        public void Load(IServiceCollection services)
        {
            EnsureArg.IsNotNull(services, nameof(services));

            var coreAssembly = typeof(IFhirRepository).Assembly;

            services.AddMediatR(GetType().Assembly, coreAssembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>));

            Predicate<Type> isPipelineBehavior = y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>);

            services.TypesInSameAssemblyAs<IFhirRepository>()
                .Transient()
                .AsImplementedInterfaces(isPipelineBehavior);
        }
    }
}
