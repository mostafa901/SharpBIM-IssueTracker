using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using SharpBIM.GitTracker.Core.GitHttp.Models;

namespace SharpBIM.GitTracker.Core.JsonConverters
{
    public class IssueJsonConv : JsonConverter<IssueModel>
    {
        public override IssueModel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var obj = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
            var instance = new IssueModel();

            if (obj.TryGetProperty(nameof(IssueModel.assignee), out var primaryValue))
            {
                instance.assignee = primaryValue.GetString();
            }
            else if (obj.TryGetProperty(nameof(IssueModel.assignees), out var secondaryValue))
            {
                instance.assignees = JsonSerializer.Deserialize<Account[]>(secondaryValue.GetRawText(), options);
                ;
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