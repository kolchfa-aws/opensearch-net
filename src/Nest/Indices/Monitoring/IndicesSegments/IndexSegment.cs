﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nest
{
	[JsonObject]
	public class IndexSegment
	{
		[JsonProperty(PropertyName = "shards")]
		[JsonConverter(typeof(VerbatimDictionaryKeysJsonConverter))]
		public IReadOnlyDictionary<string, ShardsSegment> Shards { get; internal set; } =
			EmptyReadOnly<string, ShardsSegment>.Dictionary;
	}
}
