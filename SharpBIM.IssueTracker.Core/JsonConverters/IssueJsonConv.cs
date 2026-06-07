using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using SharpBIM.IssueTracker.Core.GitHttp.Models;

namespace SharpBIM.IssueTracker.Core.JsonConverters
{
    public class IssueJsonConv : JsonConverter<IssueModel>
    {
        public override IssueModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Parse the input as JsonDocument
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var root = jsonDoc.RootElement;

            // Create new JsonSerializerOptions without this converter
            var defaultOptions = new JsonSerializerOptions(options);
            var thisConverter = defaultOptions.Converters.FirstOrDefault(c => c is IssueJsonConv);
            if (thisConverter is not null)
                defaultOptions.Converters.Remove(thisConverter);

            // Deserialize normally, avoiding infinite recursion
            var instance = JsonSerializer.Deserialize<IssueModel>(root.GetRawText(), defaultOptions)
                           ?? new IssueModel();

            // Override logic for assignee/assignees
            if (root.TryGetProperty(nameof(IssueModel.assignee), out var assigneeProp) &&
                assigneeProp.ValueKind != JsonValueKind.Null)
            {
                instance.assignee = JsonSerializer.Deserialize<Account>(assigneeProp.GetRawText(), options);
            }
            else if (root.TryGetProperty(nameof(IssueModel.assignees), out var assigneesProp) &&
                     assigneesProp.ValueKind == JsonValueKind.Array)
            {
                instance.assignees = JsonSerializer.Deserialize<Account[]>(assigneesProp.GetRawText(), options);
            }

            return instance;
        }



        public override void Write(Utf8JsonWriter writer, IssueModel value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            if (value.assignee != null)
            {
                writer.WritePropertyName(nameof(IssueModel.assignee));
                JsonSerializer.Serialize(writer, value.assignee, options);
            }
            else if (value.assignees != null)
            {
                writer.WritePropertyName(nameof(IssueModel.assignees));
                JsonSerializer.Serialize(writer, value.assignees, options);
            }

            writer.WriteEndObject();
        }
    }
}