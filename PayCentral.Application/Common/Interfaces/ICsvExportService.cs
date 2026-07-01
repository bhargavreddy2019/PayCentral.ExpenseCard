namespace PayCentral.Application.Common.Interfaces;

public interface ICsvExportService
{
    string Export<T>(IEnumerable<T> data);
}