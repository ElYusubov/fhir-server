﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Hl7.Fhir.ElementModel;
using Microsoft.Health.Fhir.Core.Models;
using Microsoft.Health.Fhir.Tests.Common.FixtureParameters;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.Health.Fhir.Tests.Common
{
    public interface ICustomFhirClient
    {
        Format Format { get; }

        (bool SecurityEnabled, string AuthorizeUrl, string TokenUrl) SecuritySettings { get; }

        HttpClient HttpClient { get; }

        Task RunAsUser(TestUser user, TestApplication clientApplication);

        Task RunAsClientApplication(TestApplication clientApplication);

        Task<FhirResponse<T>> CreateAsync<T>(T resource)
            where T : ResourceElement;

        Task<FhirResponse<T>> CreateAsync<T>(string uri, T resource)
            where T : ResourceElement;

        Task<FhirResponse<T>> ReadAsync<T>(string resourceType, string resourceId)
            where T : ResourceElement;

        Task<FhirResponse<T>> ReadAsync<T>(string uri)
            where T : ResourceElement;

        Task<FhirResponse<T>> VReadAsync<T>(string resourceType, string resourceId, string versionId)
            where T : ResourceElement;

        Task<FhirResponse<T>> UpdateAsync<T>(T resource, string ifMatchVersion = null)
            where T : ResourceElement;

        Task<FhirResponse<T>> UpdateAsync<T>(string uri, T resource, string ifMatchVersion = null)
            where T : ResourceElement;

        Task<FhirResponse> DeleteAsync<T>(T resource)
            where T : ResourceElement;

        Task<FhirResponse> DeleteAsync(string uri);

        Task<FhirResponse> HardDeleteAsync<T>(T resource)
            where T : ResourceElement;

        Task<FhirResponse<ResourceElement>> SearchAsync(string resourceType, string query = null, int? count = null);

        Task<FhirResponse<ResourceElement>> SearchAsync(string url);

        Task<FhirResponse<ResourceElement>> SearchPostAsync(string resourceType, params (string key, string value)[] body);

        Task<string> ExportAsync(Dictionary<string, string> queryParams);

        ////StringContent CreateStringContent(Resource resource);

        Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response);

        Task<FhirResponse<T>> CreateResourceElementResponseAsync<T>(HttpResponseMessage response)
            where T : ResourceElement;

        ////Task<FhirResponse<T>> CreateResponseAsync<T>(HttpResponseMessage response)
        ////    where T : Resource;

        Task SetupAuthenticationAsync(TestApplication clientApplication, TestUser user = null);

        Task<string> GetBearerToken(TestApplication clientApplication, TestUser user);

        List<KeyValuePair<string, string>> GetAppSecuritySettings(TestApplication clientApplication);

        List<KeyValuePair<string, string>> GetUserSecuritySettings(TestApplication clientApplication, TestUser user);

        Task GetSecuritySettings(string fhirServerMetadataUrl);

        ResourceElement GetDefaultObservation();

        ResourceElement GetDefaultPatient();

        ResourceElement GetDefaultOrganization();

        ResourceElement GetEmptyObservation();

        ResourceElement GetEmptyValueSet();

        ResourceElement GetJsonSample(string fileName);

        void Validate(ResourceElement resourceElement);

        ResourceElement UpdateId(ResourceElement resourceElement, string id);

        ResourceElement UpdateVersion(ResourceElement resourceElement, string newVersion);

        ResourceElement UpdateLastUpdated(ResourceElement resourceElement, DateTimeOffset lastUpdated);

        ResourceElement UpdateText(ResourceElement resourceElement, string text);

        ResourceElement UpdatePatientFamilyName(ResourceElement resourceElement, string familyName);

        ResourceElement UpdatePatientAddressCity(ResourceElement resourceElement, string city);

        ResourceElement UpdatePatientGender(ResourceElement resourceElement, string gender);

        ResourceElement UpdateObservationStatus(ResourceElement resourceElement, string status);

        ResourceElement UpdateValueSetStatus(ResourceElement resourceElement, string status);

        ResourceElement UpdateValueSetUrl(ResourceElement resourceElement, string url);

        ResourceElement AddObservationCoding(ResourceElement resourceElement, string system, string code);

        ResourceElement AddMetaTag(ResourceElement resourceElement, string system, string code);

        ResourceElement AddObservationIdentifier(ResourceElement resourceElement, string system, string identifier);

        bool Compare(ResourceElement expected, ITypedElement actual);

        bool Compare(ResourceElement expected, ResourceElement actual);
    }
}
