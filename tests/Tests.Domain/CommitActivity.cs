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
using Bogus;
using OpenSearch.Client;
using Tests.Configuration;
using Tests.Domain.Helpers;

namespace Tests.Domain
{
	public class CommitActivity
	{
		public static string TypeName = "commit";

		private string _projectName;

		public Developer Committer { get; set; }
		public double ConfidenceFactor { get; set; }
		public TimeSpan? Duration { get; set; }

		public string Id { get; set; }
		public JoinField Join { get; set; }

		public string Message { get; set; }

		/// <summary>
		/// This is lazy, both project and commits end up in the same index under the same type (_doc)
		/// Quite a few of our tests do script lookups based on this field under the old assumption only a specific type
		/// is searched.
		/// </summary>
		public int? NumberOfCommits { get; set; }

		public string ProjectName
		{
			get => _projectName;
			set
			{
				Join = JoinField.Link<CommitActivity>(value);
				_projectName = value;
			}
		}

		public long SizeInBytes { get; set; }

		[Text]
		public TimeSpan? StringDuration
		{
			get => Duration;
			set => Duration = value;
		}

		public string Type => TypeName;

		// @formatter:off — enable formatter after this line
		public static Faker<CommitActivity> Generator { get; } =
			new Faker<CommitActivity>()
				.UseSeed(TestConfiguration.Instance.Seed)
				.RuleFor(p => p.Id, p => Guid.NewGuid().ToString("N").Substring(0, 8))
				.RuleFor(p => p.ProjectName, p => Project.Projects[Gimme.Random.Number(0, Project.Projects.Count - 1)].Name)
				.RuleFor(p => p.Committer, p => Developer.Developers[Gimme.Random.Number(0, Developer.Developers.Count - 1)])
				.RuleFor(p => p.Message, p => p.Lorem.Paragraph(Gimme.Random.Number(1, 3)))
				.RuleFor(p => p.SizeInBytes, p => p.Random.Number(0, 100000))
				.RuleFor(p => p.NumberOfCommits, f => Gimme.Random.Number(1, 1000))
				.RuleFor(p => p.ConfidenceFactor, p => p.Random.Double())
				.RuleFor(p => p.Duration, p => p.Random.ArrayElement(new TimeSpan?[]
				{
					TimeSpan.MinValue,
					TimeSpan.MaxValue,
					TimeSpan.FromMinutes(7.5),
					TimeSpan.Zero,
					null,
					TimeSpan.FromHours(4.23),
					TimeSpan.FromDays(5),
				}));

		public static IList<CommitActivity> CommitActivities { get; } = Generator.Clone().Generate(1000);
		// @formatter:on — enable formatter after this line
	}
}
