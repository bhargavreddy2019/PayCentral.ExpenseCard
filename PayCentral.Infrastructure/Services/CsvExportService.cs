using PayCentral.Application.Common.Interfaces;
using System.Reflection;
using System.Text;

namespace PayCentral.Infrastructure.Services;

public class CsvExportService : ICsvExportService
{
    public string Export<T>(IEnumerable<T> data)
    {
        var sb = new StringBuilder();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Header row
        sb.AppendLine(string.Join(",", properties.Select(p => p.Name)));

        // Data rows
        foreach (var item in data)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                var str = value?.ToString() ?? string.Empty;
                // Escape commas and quotes
                if (str.Contains(',') || str.Contains('"'))
                    str = $"\"{str.Replace("\"", "\"\"")}\"";
                return str;
            });
            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }
}