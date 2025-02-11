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
using System.Linq.Expressions;
using System.Runtime.Serialization;
using OpenSearch.Net.Utf8Json;

namespace OpenSearch.Client
{
	/// <summary>
	/// Converts a string to its uppercase equivalent.
	/// </summary>
	[InterfaceDataContract]
	public interface IUppercaseProcessor : IProcessor
	{
		/// <summary>
		/// The field to make uppercase
		/// </summary>
		[DataMember(Name ="field")]
		Field Field { get; set; }

		/// <summary>
		/// If <c>true</c> and <see cref="Field" /> does not exist or is null,
		/// the processor quietly exits without modifying the document. Default is <c>false</c>
		/// </summary>
		[DataMember(Name = "ignore_missing")]
		bool? IgnoreMissing { get; set; }

		/// <summary>
		/// The field to assign the converted value to, by default field is updated in-place
		/// </summary>
		[DataMember(Name = "target_field")]
		Field TargetField { get; set; }
	}

	public class UppercaseProcessor : ProcessorBase, IUppercaseProcessor
	{
		/// <inheritdoc />
		[DataMember(Name ="field")]
		public Field Field { get; set; }

		/// <inheritdoc />
		public bool? IgnoreMissing { get; set; }

		/// <inheritdoc />
		public Field TargetField { get; set; }

		protected override string Name => "uppercase";
	}

	public class UppercaseProcessorDescriptor<T>
		: ProcessorDescriptorBase<UppercaseProcessorDescriptor<T>, IUppercaseProcessor>, IUppercaseProcessor
		where T : class
	{
		protected override string Name => "uppercase";

		Field IUppercaseProcessor.Field { get; set; }
		bool? IUppercaseProcessor.IgnoreMissing { get; set; }
		Field IUppercaseProcessor.TargetField { get; set; }

		/// <inheritdoc cref="IUppercaseProcessor.Field" />
		public UppercaseProcessorDescriptor<T> Field(Field field) => Assign(field, (a, v) => a.Field = v);

		/// <inheritdoc cref="IUppercaseProcessor.Field" />
		public UppercaseProcessorDescriptor<T> Field<TValue>(Expression<Func<T, TValue>> objectPath) =>
			Assign(objectPath, (a, v) => a.Field = v);

		/// <inheritdoc cref="IUppercaseProcessor.TargetField" />
		public UppercaseProcessorDescriptor<T> TargetField(Field field) => Assign(field, (a, v) => a.TargetField = v);

		/// <inheritdoc cref="IUppercaseProcessor.TargetField" />
		public UppercaseProcessorDescriptor<T> TargetField(Expression<Func<T, object>> objectPath) =>
			Assign(objectPath, (a, v) => a.TargetField = v);

		/// <inheritdoc cref="IUppercaseProcessor.IgnoreMissing" />
		public UppercaseProcessorDescriptor<T> IgnoreMissing(bool? ignoreMissing = true) => Assign(ignoreMissing, (a, v) => a.IgnoreMissing = v);
	}
}
