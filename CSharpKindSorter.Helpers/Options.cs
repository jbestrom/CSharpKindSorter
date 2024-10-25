using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using LightJson;
using LightJson.Serialization;
using Microsoft.CodeAnalysis;

namespace CSharpKindSorter.Helpers;

public class Options
{
	private const string CONFIG_FILE_NAME = "csharpkindsorter.json";
	public string[] AccessOrder { get; set; }
	public bool? Alphabetical { get; set; }
	public bool? ConstFirst { get; set; }
	public string[] KindOrder { get; set; }
	public bool? ReadonlyFirst { get; set; }
	public bool? OverrideFirst { get; set; }
	public bool? StaticFirst { get; set; }

	public static Options GetOptions(ImmutableArray<AdditionalText> additionalFiles)
	{
		var defaultOptions = GetDefaultOptions();
		var configFile = additionalFiles.FirstOrDefault(file => Path.GetFileName(file.Path).Equals(CONFIG_FILE_NAME));
		var text = configFile?.GetText();

		return text == null ? defaultOptions : GetOptions(text.ToString());
	}

	public static Options GetOptions(string text)
	{
		var defaultOptions = GetDefaultOptions();
		try
		{
			var json = JsonReader.Parse(text);
			var options = new Options
			{
				KindOrder = json["KindOrder"].AsJsonArray?.Select(item => item.AsString).ToArray() ?? defaultOptions.KindOrder,
				AccessOrder = json["AccessOrder"].AsJsonArray?.Select(item => item.AsString).ToArray() ?? defaultOptions.AccessOrder,
				ConstFirst = json["ConstFirst"].IsBoolean ? json["ConstFirst"].AsBoolean : defaultOptions.ConstFirst,
				StaticFirst = json["StaticFirst"].IsBoolean ? json["StaticFirst"].AsBoolean : defaultOptions.StaticFirst,
				ReadonlyFirst = json["ReadonlyFirst"].IsBoolean ? json["ReadonlyFirst"].AsBoolean : defaultOptions.ReadonlyFirst,
				OverrideFirst = json["OverrideFirst"].IsBoolean ? json["OverrideFirst"].AsBoolean : defaultOptions.OverrideFirst,
				Alphabetical = json["Alphabetical"].IsBoolean ? json["Alphabetical"].AsBoolean : defaultOptions.Alphabetical
			};

			return options;
		}
		catch (Exception)
		{
			return defaultOptions;
		}
	}

	public static string SerializeOptions(Options options)
	{
		// Create a JsonObject to hold the options data
		var jsonObject = new JsonObject
		{
			["KindOrder"] = new JsonArray(options.KindOrder.Select(kind => new JsonValue(kind)).ToArray()),
			["AccessOrder"] = new JsonArray(options.AccessOrder.Select(access => new JsonValue(access)).ToArray()),
			["ConstFirst"] = options.ConstFirst,
			["StaticFirst"] = options.StaticFirst,
			["ReadonlyFirst"] = options.ReadonlyFirst,
			["OverrideFirst"] = options.OverrideFirst,
			["Alphabetical"] = options.Alphabetical
		};

		// Serialize the JsonObject to a JSON string
		return jsonObject.ToString(pretty: true);
	}

	private static Options GetDefaultOptions()
	{
		return new Options
		{
			KindOrder =
			[
				"Fields", "Constructors", "Finalizers", "Delegates",
				"Events", "Enums", "Interfaces", "Properties", "Operators", "Indexers", "Methods",
				"Structs", "Classes"
			],
			AccessOrder = ["public", "public explicit", "internal", "protected internal", "protected", "private"],
			ConstFirst = true,
			StaticFirst = true,
			ReadonlyFirst = true,
			OverrideFirst = false,
			Alphabetical = true
		};
	}
}