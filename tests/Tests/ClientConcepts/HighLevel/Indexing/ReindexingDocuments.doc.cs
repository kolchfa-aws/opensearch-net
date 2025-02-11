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
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;
using FluentAssertions;
using OpenSearch.Client;
using Tests.Framework;
using Tests.Framework.DocumentationTests;

namespace Tests.ClientConcepts.HighLevel.Indexing
{
	/**[[reindexing-documents]]
	*=== Reindexing documents
	*
	* Sometimes there is a need to reindex documents from one index to another. The client provides two different methods for
	* reindexing documents, `ReindexOnServer` and `Reindex`.
	*/
	public class Reindexing : DocumentationTestBase
	{
		private readonly IOpenSearchClient client = new OpenSearchClient(
			new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")), new InMemoryConnection()));

		/**[[reindex]]
		 * ==== Reindex
		 *
		 * The reindex API of OpenSearch is exposed as the `ReindexOnServer` method (and its asynchronous counterpart, `ReindexOnServerAsync`) on
		 * the client. Simple usage is to define a source index and destination index and wait for the operation to complete
		 *
		 * NOTE: The destination index **must** exist before starting the reindex process
		 */
		public void ReindexOnServer()
		{
			var reindexResponse = client.ReindexOnServer(r => r
				.Source(s => s
					.Index("source_index")
				)
				.Destination(d => d
					.Index("destination_index")
				)
				.WaitForCompletion() // <1> Wait for the reindex process to complete before returning a response.
			);
		}

		/**
		 * In the example above, OpenSearch will wait for the reindex process to complete before returning a response to the client. As such,
		 * ensure that the client is configured with a sufficient request timeout when using `WaitForCompletion`.
		 *
		 * Instead of waiting for the reindex process to complete, reindex can be run asynchronously on OpenSearch, returning a task that
		 * can be used with the task APIs, to get the status of the process or cancel it. The following example demonstrates this approach
		 */
		public void ReindexOnServerWithoutWaitingForCompletion()
		{
			var reindexResponse = client.ReindexOnServer(r => r
					.Source(s => s
						.Index("source_index")
					)
					.Destination(d => d
						.Index("destination_index")
					)
					.WaitForCompletion(false) // <1> Don't wait for the reindex process to complete before returning a response
			);

			var taskId = reindexResponse.Task; // <2> Get the task id from the response to use to check its progress
			var taskResponse = client.Tasks.GetTask(taskId);

			while (!taskResponse.Completed) // <3> Whilst the task isn't completed, keep checking
			{
				Thread.Sleep(TimeSpan.FromSeconds(20)); // <4> Wait some time before fetching and checking the task again
				taskResponse = client.Tasks.GetTask(taskId);
			}

			var completedReindexResponse = taskResponse.GetResponse<ReindexOnServerResponse>(); // <5> Get the completed reindex response from the task response and take some action
		}

		/// hide
		public class Person
		{
			public int Id { get; set; }
			public string FirstName { get; set; }
			public string LastName { get; set; }
		}

		/**[[reindex-with-parameters]]
		 * ==== Reindex with parameters
		 *
		 * The reindex API exposes additional parameters to control the reindex process, such as
		 *
		 * * A query to run on the source index to reindex only a subset of the documents that match a query
		 * * Selecting only a subset of fields from the source documents to reindex into the destination index
		 * * Running an <<pipelines, Ingest pipeline>> on documents to be indexed into the destination index
		 *
		 * The following example demonstrates some of these parameters
		 */
		public void ReindexOnServerWithQuery()
		{
			var reindexResponse = client.ReindexOnServer(r => r
					.Source(s => s
						.Index("source_index")
						.Query<Person>(q => q // <1> Select only a subset of documents to reindex, from the source index
							.Term(m => m
								.Field(f => f.FirstName)
								.Value("Russ")
							)
						)
						.Source<Person>(so => so // <2> Reindex only the first name and last name fields
							.Field(f => f.FirstName)
							.Field(f => f.LastName)
						)
					)
					.Destination(d => d
						.Index("destination_index")
						.Pipeline("my_reindex_pipeline") // <3> Run an ingest pipeline on documents when they are indexed into the destination index
					)
			);
		}

		/**[[reindex-observable]]
		 * ==== Reindex observable
		 *
		 * In addition to `ReindexOnServer`, the client also exposes a `Reindex` method that uses the
		 * https://docs.microsoft.com/en-us/dotnet/standard/events/observer-design-pattern[Observer Design Pattern] to set up a reindex operation
		 * and allows observers to be registered to take action during the reindex process.
		 *
		 * [IMPORTANT]
		 * --
		 * In contrast to the `ReindexOnServer` method, which uses the reindex API of OpenSearch to perform the reindex process
		 * entirely on the server, the `Reindex` method
		 *
		 * 1. retrieves batches of documents over the network from the source index on the server
		 * 2. allows modifications to be performed on the client side
		 * 3. makes requests to bulk index the modified documents to the destination index
		 *
		 * Such an approach can be more flexible than what is provided by the reindex API, at the cost of many more requests to OpenSearch and
		 * higher network traffic. Both approaches have their usages so you should choose the one that best suits your requirements.
		 *
		 * You might be wondering why `ReindexOnServer` that uses the reindex API, and `Reindex` that uses an observable approach, are called as such.
		 * The `Reindex` method existed on the client long before the reindex API existed in OpenSearch. Since the
		 * APIs are quite different on the client, when the reindex API was introduced, it was decided to name it `ReindexOnServer` to not conflict
		 * with the existing method that is still popularly used.
		 * --
		 *
		 * `Reindex` builds on top of <<scrollall-observable, `ScrollAllObservable`>> and <<bulkall-observable, `BulkAllObservable`>> to fetch
		 * documents from, and index documents into OpenSearch, respectively. The following example demonstrates a simple use of `Reindex`
		 */
		public void Reindex()
		{
			var slices = Environment.ProcessorCount; // <1> Number of slices to split each scroll into

			var reindexObserver = client.Reindex<Person>(r => r
				.ScrollAll("5s", slices, s => s // <2> How to fetch documents to be reindexed
					.Search(ss => ss
						.Index("source_index")
					)
				)
				.BulkAll(b => b // <3> How to index fetched documents
					.Index("destination_index")
				)
			)
			.Wait(TimeSpan.FromMinutes(15), response => // <4> Wait up to 15 minutes for the reindex process to complete
			{
				// do something with each bulk response e.g. accumulate number of indexed documents
			});
		}

		/**
		 * An index can be created when using `Reindex`. For example, the source index settings can be retrieved and used
		 * as the basis for index settings of the destination index
		 */
		public void ReindexWithIndexCreation()
		{
			var getIndexResponse = client.Indices.Get("source_index"); // <1> Get the settings for the source index
			var indexSettings = getIndexResponse.Indices["source_index"];

			var lastNameProperty = indexSettings.Mappings.Properties["lastName"]; // <2> Get the mapping for the `lastName` property

			if (lastNameProperty is TextProperty textProperty) // <3> If the `lastName` property is a `text` datatype, add a `keyword` <<multi-fields, multi-field>>
			{
				if (textProperty.Fields == null)
					textProperty.Fields = new Properties();

				textProperty.Fields.Add("keyword", new KeywordProperty());
			}

			var reindexObserver = client.Reindex<Person>(r => r
				.CreateIndex(c => c
					.InitializeUsing(indexSettings) // <4> Use the index settings to create the destination index
				)
				.ScrollAll("5s", Environment.ProcessorCount, s => s
					.Search(ss => ss
						.Index("source_index")
					)
				)
				.BulkAll(b => b
					.Index("destination_index")
				)
			)
			.Wait(TimeSpan.FromMinutes(15), response =>
			{
				// do something with each bulk response e.g. accumulate number of indexed documents
			});
		}

		/**
		 * `Reindex` has an overload that accepts a function for how source documents should be mapped to destination documents. In addition,
		 * further control over reindexing can be achieved by using an observer to subscribe to the reindexing process to take some action on
		 * each successful bulk response, when an error occurs, and when the process has finished. The following example demonstrates these
		 * features.
		 *
		 * IMPORTANT: An observer should not throw exceptions from its interface implementations, such
		 * as `OnNext` and `OnError`. Any exceptions thrown should be expected to go unhandled. In light of this, any exception
		 * that occurs during the reindex process should be captured and thrown outside of the observer, as demonstrated in the
		 * example below. Take a look at the
		 * https://docs.microsoft.com/en-us/dotnet/standard/events/observer-design-pattern-best-practices#handling-exceptions[Observer Design Pattern best practices]
		 * on handling exceptions.
		 */
		public void ReindexWithObserver()
		{
			var reindexObservable = client.Reindex<Person, Person>(
				person => person, // <1> a function to define how source documents are mapped to destination documents
				r => r
				.ScrollAll("5s", Environment.ProcessorCount, s => s
					.Search(ss => ss
						.Index("source_index")
					)
				)
				.BulkAll(b => b
					.Index("destination_index")
				)
			);

			var waitHandle = new ManualResetEvent(false);
			ExceptionDispatchInfo exceptionDispatchInfo = null;

			var observer = new ReindexObserver(
				onNext: response =>
				{
					// do something e.g. write number of pages to console
				},
				onError: exception =>
				{
					exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
					waitHandle.Set();
				},
				onCompleted: () => waitHandle.Set());

			reindexObservable.Subscribe(observer); // <2> Subscribe to the observable, which will initiate the reindex process

			waitHandle.WaitOne(); // <3> Block the current thread until a signal is received

			exceptionDispatchInfo?.Throw(); // <4> If an exception was captured during the reindex process, throw it
		}
	}
}
