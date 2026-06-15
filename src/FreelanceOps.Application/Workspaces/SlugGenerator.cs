using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using FreelanceOps.Application.Abstractions.Workspaces;

namespace FreelanceOps.Application.Workspaces;

public sealed partial class SlugGenerator : ISlugGenerator
{
    public string Generate(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);

            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(character);
        }

        var slug = NonAlphanumericRegex()
            .Replace(builder.ToString().Normalize(NormalizationForm.FormC), "-")
            .Trim('-');

        return string.IsNullOrWhiteSpace(slug) ? "workspace" : slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumericRegex();
}
