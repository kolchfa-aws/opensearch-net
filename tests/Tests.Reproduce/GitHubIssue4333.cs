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

using System;
using System.Text;
using OpenSearch.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using FluentAssertions;
using OpenSearch.Client;

namespace Tests.Reproduce
{
	public class GitHubIssue4333
	{
		[U]
		public void CanDeserializeYearIntervalUnit()
		{
			var json = @"{
			""took"" : 162,
			""timed_out"" : false,
			""_shards"" : {
				""total"" : 3,
				""successful"" : 3,
				""skipped"" : 0,
				""failed"" : 0
			},
			""hits"" : {
				""total"" : 4089559,
				""max_score"" : 0.0,
				""hits"" : [ ]
			},
			""aggregations"" : {
				""modDate"" : {
					""buckets"" : [
					{
						""key_as_string"" : ""2013-01-01T00:00:00.000Z"",
						""key"" : 1356998400000,
						""doc_count"" : 32
					},
					{
						""key_as_string"" : ""2014-01-01T00:00:00.000Z"",
						""key"" : 1388534400000,
						""doc_count"" : 617
					},
					{
						""key_as_string"" : ""2015-01-01T00:00:00.000Z"",
						""key"" : 1420070400000,
						""doc_count"" : 183
					},
					{
						""key_as_string"" : ""2016-01-01T00:00:00.000Z"",
						""key"" : 1451606400000,
						""doc_count"" : 3479
					},
					{
						""key_as_string"" : ""2017-01-01T00:00:00.000Z"",
						""key"" : 1483228800000,
						""doc_count"" : 1948427
					},
					{
						""key_as_string"" : ""2018-01-01T00:00:00.000Z"",
						""key"" : 1514764800000,
						""doc_count"" : 555748
					},
					{
						""key_as_string"" : ""2019-01-01T00:00:00.000Z"",
						""key"" : 1546300800000,
						""doc_count"" : 1268034
					},
					{
						""key_as_string"" : ""2020-01-01T00:00:00.000Z"",
						""key"" : 1577836800000,
						""doc_count"" : 313039
					}
					],
					""interval"" : ""1y""
				}
			}
		}";

			var bytes = Encoding.UTF8.GetBytes(json);
			var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
			var connectionSettings = new ConnectionSettings(pool, new InMemoryConnection(bytes)).DefaultIndex("default_index");
			var client = new OpenSearchClient(connectionSettings);

			var response = client.Search<object>();

			Func<AutoDateHistogramAggregate> func = () => response.Aggregations.AutoDateHistogram("modDate");

			var agg = func.Should().NotThrow().Subject;
			agg.AutoInterval.Should().Be("1y");
		}
	}
}
