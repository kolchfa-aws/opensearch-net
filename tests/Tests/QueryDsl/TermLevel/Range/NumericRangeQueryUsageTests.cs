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

using OpenSearch.Client;
using Tests.Core.ManagedOpenSearch.Clusters;
using Tests.Domain;
using Tests.Framework.EndpointTests.TestState;

namespace Tests.QueryDsl.TermLevel.Range
{
	public class RangeQueryUsageTests : QueryDslUsageTestsBase
	{
		public RangeQueryUsageTests(ReadOnlyCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override ConditionlessWhen ConditionlessWhen => new ConditionlessWhen<INumericRangeQuery>(q => q.Range as INumericRangeQuery)
		{
			q => q.Field = null,
			q =>
			{
				q.GreaterThan = null;
				q.GreaterThanOrEqualTo = null;
				q.LessThan = null;
				q.LessThanOrEqualTo = null;
			}
		};

		protected override QueryContainer QueryInitializer => new NumericRangeQuery
		{
			Name = "named_query",
			Boost = 1.1,
			Field = "description",
			GreaterThan = 1.0,
			GreaterThanOrEqualTo = 1.1,
			LessThan = 2.1,
			LessThanOrEqualTo = 2.0,
			Relation = RangeRelation.Within
		};

		protected override object QueryJson => new
		{
			range = new
			{
				description = new
				{
					_name = "named_query",
					boost = 1.1,
					gt = 1.0,
					gte = 1.1,
					lt = 2.1,
					lte = 2.0,
					relation = "within"
				}
			}
		};

		protected override QueryContainer QueryFluent(QueryContainerDescriptor<Project> q) => q
			.Range(c => c
				.Name("named_query")
				.Boost(1.1)
				.Field(p => p.Description)
				.GreaterThan(1.0)
				.GreaterThanOrEquals(1.1)
				.LessThan(2.1)
				.LessThanOrEquals(2.0)
				.Relation(RangeRelation.Within)
			);
	}
}
