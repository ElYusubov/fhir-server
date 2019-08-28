// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using EnsureThat;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Health.Fhir.Api.Features.ContentTypes;
using Microsoft.Health.Fhir.Core.Features.Operations.Export.Models;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.Health.Fhir.Api.Features.Formatters
{
    internal class FhirXmlOutputFormatter : TextOutputFormatter
    {
        private readonly FhirXmlSerializer _fhirXmlSerializer;
        private readonly ILogger<FhirXmlOutputFormatter> _logger;
        private bool _isVersionsResult;

        public FhirXmlOutputFormatter(FhirXmlSerializer fhirXmlSerializer, ILogger<FhirXmlOutputFormatter> logger)
        {
            EnsureArg.IsNotNull(fhirXmlSerializer, nameof(fhirXmlSerializer));

            _fhirXmlSerializer = fhirXmlSerializer;
            _logger = logger;

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);

            SupportedMediaTypes.Add(KnownContentTypes.XmlContentType);
            SupportedMediaTypes.Add(KnownMediaTypeHeaderValues.ApplicationXml);
            SupportedMediaTypes.Add(KnownMediaTypeHeaderValues.TextXml);
            SupportedMediaTypes.Add(KnownMediaTypeHeaderValues.ApplicationAnyXmlSyntax);
        }

        protected override bool CanWriteType(Type type)
        {
            EnsureArg.IsNotNull(type, nameof(type));

            // TODO: Move this to a new formatter.
            if (typeof(VersionsResult).IsAssignableFrom(type))
            {
                _isVersionsResult = true;
                return true;
            }

            return typeof(Resource).IsAssignableFrom(type);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            EnsureArg.IsNotNull(context, nameof(context));
            EnsureArg.IsNotNull(selectedEncoding, nameof(selectedEncoding));

            HttpResponse response = context.HttpContext.Response;
            using (TextWriter textWriter = context.WriterFactory(response.Body, selectedEncoding))
            using (var writer = new XmlTextWriter(textWriter))
            {
                // TODO: Move this to a new formatter.
                if (_isVersionsResult)
                {
                    var namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);

                    var serializer = new XmlSerializer(typeof(VersionsResult));
                    serializer.Serialize(writer, context.Object, namespaces);

                    _isVersionsResult = false;
                }
                else
                {
                    _fhirXmlSerializer.Serialize((Resource)context.Object, writer, context.HttpContext.GetSummaryType(_logger));
                }
            }

            return Task.CompletedTask;
        }
    }
}
