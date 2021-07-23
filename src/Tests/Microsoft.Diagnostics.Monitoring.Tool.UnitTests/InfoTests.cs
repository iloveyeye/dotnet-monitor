﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.Monitoring.TestCommon;
using Microsoft.Diagnostics.Monitoring.UnitTests.Fixtures;
using Microsoft.Diagnostics.Monitoring.UnitTests.HttpApi;
using Microsoft.Diagnostics.Monitoring.UnitTests.Models;
using Microsoft.Diagnostics.Monitoring.UnitTests.Options;
using Microsoft.Diagnostics.Monitoring.UnitTests.Runners;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Diagnostics.Monitoring.UnitTests
{
    [Collection(DefaultCollectionFixture.Name)]
    public class InfoTests
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITestOutputHelper _outputHelper;

        public InfoTests(ITestOutputHelper outputHelper, ServiceProviderFixture serviceProviderFixture)
        {
            _httpClientFactory = serviceProviderFixture.ServiceProvider.GetService<IHttpClientFactory>();
            _outputHelper = outputHelper;
        }

        /// <summary>
        /// Tests that the info endpoint provides the expected output.
        /// </summary>
        [Theory]
        [InlineData(DiagnosticPortConnectionMode.Connect)]
#if NET5_0_OR_GREATER
        [InlineData(DiagnosticPortConnectionMode.Listen)]
#endif
        public Task InfoEndpointValidationTest(DiagnosticPortConnectionMode mode)
        {
            return ScenarioRunner.SingleTarget(
                _outputHelper,
                _httpClientFactory,
                mode,
                TestAppScenarios.AsyncWait.Name,
                appValidate: async (runner, client) =>
                {
                    // GET /info
                    DotnetMonitorInfo info = await client.GetInfoAsync();

                    Assert.NotNull(info.Version); // Not sure of how to get Dotnet Monitor version from within tests...
                    Assert.Equal(info.RuntimeVersion, Environment.Version.ToString());
                    Assert.Equal(info.DiagnosticPortMode, mode);

                    if (mode == DiagnosticPortConnectionMode.Connect)
                    {
                        Assert.Null(info.DiagnosticPortName);
                    }
                    else if (mode == DiagnosticPortConnectionMode.Listen)
                    {
                        Assert.Equal(runner.DiagnosticPortPath, info.DiagnosticPortName);
                    }

                    await runner.SendCommandAsync(TestAppScenarios.AsyncWait.Commands.Continue);
                });
        }
    }
}