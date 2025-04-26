using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EgzamApp.Server.Models
{
    /// <summary>
    /// Converter for storing lists as JSON strings in the database
    /// </summary>
    public class ListToJsonConverter : ValueConverter<List<string>, string>
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ListToJsonConverter() 
            : base(
                v => JsonSerializer.Serialize(v, _options),
                v => JsonSerializer.Deserialize<List<string>>(v, _options) ?? new List<string>())
        {
        }
    }

    /// <summary>
    /// Comparator for list changes for Entity Framework tracking
    /// </summary>
    public class ListComparer : ValueComparer<List<string>>
    {
        public ListComparer() 
            : base(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList())
        {
        }
    }

    /// <summary>
    /// Extension method to configure the Question entity
    /// </summary>
    public static class QuestionEntityConfiguration
    {
        public static void ConfigureQuestion(this EntityTypeBuilder<Question> builder)
        {
            // Configure the Options property to use JSON conversion
            builder.Property(q => q.Options)
                .HasConversion(
                    new ListToJsonConverter(),
                    new ListComparer())
                .HasColumnName("Options");
        }
    }
}
