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

using System.Runtime.Serialization;

namespace OpenSearch.Client
{
	[DataContract]
	public class QueryCacheStats
	{
		[DataMember(Name ="cache_count")]
		public long CacheCount { get; set; }

		[DataMember(Name ="cache_size")]
		public long CacheSize { get; set; }

		[DataMember(Name ="evictions")]
		public long Evictions { get; set; }

		[DataMember(Name ="hit_count")]
		public long HitCount { get; set; }

		[DataMember(Name ="memory_size_in_bytes")]
		public long MemorySizeInBytes { get; set; }

		[DataMember(Name ="miss_count")]
		public long MissCount { get; set; }

		[DataMember(Name ="total_count")]
		public long TotalCount { get; set; }
	}
}
