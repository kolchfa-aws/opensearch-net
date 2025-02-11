/* SPDX-License-Identifier: Apache-2.0
*
* The OpenSearch Contributors require contributions made to
* this file be licensed under the Apache-2.0 license or a
* compatible open source license.
*
* Modifications Copyright OpenSearch Contributors. See
* GitHub history for details.
*
*  Licensed to Elasticsearch B.V. under one or more contributor
*  license agreements. See the NOTICE file distributed with
*  this work for additional information regarding copyright
*  ownership. Elasticsearch B.V. licenses this file to you under
*  the Apache License, Version 2.0 (the "License"); you may
*  not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
* 	http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing,
*  software distributed under the License is distributed on an
*  "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
*  KIND, either express or implied.  See the License for the
*  specific language governing permissions and limitations
*  under the License.
*/

using System.Collections.Generic;
using System.Threading.Tasks;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Client;

namespace Tests.Analysis.TokenFilters
{
	public interface ITokenFilterAssertion : IAnalysisAssertion<ITokenFilter, ITokenFilters, TokenFiltersDescriptor> { }

	public abstract class TokenFilterAssertionBase<TAssertion>
		: AnalysisComponentTestBase<TAssertion, ITokenFilter, ITokenFilters, TokenFiltersDescriptor>
			, ITokenFilterAssertion
		where TAssertion : TokenFilterAssertionBase<TAssertion>, new()
	{
		protected override object AnalysisJson => new
		{
			filter = new Dictionary<string, object> { { AssertionSetup.Name, AssertionSetup.Json } }
		};

		protected override IAnalysis FluentAnalysis(AnalysisDescriptor an) =>
			an.TokenFilters(d => AssertionSetup.Fluent(AssertionSetup.Name, d));

		protected override OpenSearch.Client.Analysis InitializerAnalysis() =>
			new OpenSearch.Client.Analysis { TokenFilters = new OpenSearch.Client.TokenFilters { { AssertionSetup.Name, AssertionSetup.Initializer } } };

		// https://youtrack.jetbrains.com/issue/RIDER-19912
		[U] public override Task TestPutSettingsRequest() => base.TestPutSettingsRequest();

		[I] public override Task TestPutSettingsResponse() => base.TestPutSettingsResponse();
	}
}
