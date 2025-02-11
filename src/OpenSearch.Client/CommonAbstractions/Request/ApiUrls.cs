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
using System.Collections.Generic;
using System.Linq;

namespace OpenSearch.Client
{
	/// <summary>
	/// Each Request type holds a static instance of this class which creates cached builders for each
	/// of the defined urls in the json spec.
	/// </summary>
	internal class ApiUrls
	{
		private static readonly RouteValues EmptyRouteValues = new();
		private readonly string _errorMessageSuffix;

		/// <summary>
		/// If the spec only defines a single non parameterizable route this allows us to shortcircuit and avoid hitting
		/// the cached string builders.
		/// </summary>
		private readonly string _fixedUrl;

		/// <summary>
		/// Creates a lookup for number of parts <=> list of routes with that number of parts.
		/// <see cref="UrlLookup.Matches"/> allows us to quickly find the right url to use in the list.
		/// </summary>
		public Dictionary<int, List<UrlLookup>> Routes { get; }

		/// <summary> Only intended to be created once per request and stored in a static </summary>
		internal ApiUrls(string[] routes)
		{
			if (routes == null || routes.Length == 0) throw new ArgumentException("urls is null or empty", nameof(routes));
			if (routes.Length == 1 && !routes[0].Contains("{")) _fixedUrl = routes[0];
			else
			{
				foreach (var route in routes)
				{
					var bracketsCount = route.Count(c => c.Equals('{'));
					if (Routes == null) Routes = new Dictionary<int, List<UrlLookup>>();
					if (Routes.ContainsKey(bracketsCount))
						Routes[bracketsCount].Add(new UrlLookup(route));
					else
						Routes.Add(bracketsCount, new List<UrlLookup> { new UrlLookup(route) });
				}
			}

			_errorMessageSuffix = string.Join(",", routes);

			// received multiple urls without brackets we resolve to first
			if (Routes == null) _fixedUrl = routes[0];
		}

		public string Resolve(RouteValues routeValues, IConnectionSettingsValues settings)
		{
			if (_fixedUrl != null) return _fixedUrl;

			var resolved = routeValues.Resolve(settings);

			if (!Routes.TryGetValue(resolved.Count, out var routes))
				throw new Exception($"No route taking {resolved.Count} parameters{_errorMessageSuffix}");

			if (routes.Count == 1)
				return routes[0].ToUrl(resolved);

			//find the first url with N parts that has all provided named parts
			foreach (var u in routes)
			{
				if (u.Matches(resolved))
					return u.ToUrl(resolved);
			}
			throw new Exception($"No route taking {routeValues.Count} parameters{_errorMessageSuffix}");
		}
	}
}
