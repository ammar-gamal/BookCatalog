using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Text;

namespace BookCatalog.API.Options.Validators;

public class DatabaseOptionsValidations : IValidateOptions<DatabaseOptions>
{
    public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
    {
        if(options is null)
            return ValidateOptionsResult.Fail("DatabaseOptions is missing.");

        if(string.IsNullOrWhiteSpace(options.ConnectionString))
            return ValidateOptionsResult.Fail("The database connection string is missing or empty.");

        SqlConnectionStringBuilder builder;
        try
        {
            builder = new SqlConnectionStringBuilder(options.ConnectionString);
        }
        catch(Exception ex)
        {
            return ValidateOptionsResult.Fail($"The connection string is malformed: {ex.Message}");
        }

        var validationErrors = new StringBuilder();

        if(string.IsNullOrWhiteSpace(builder.DataSource))
            validationErrors.AppendLine("The connection string is missing the data source (server).");

        if(string.IsNullOrWhiteSpace(builder.InitialCatalog))
            validationErrors.AppendLine("The connection string is missing the initial catalog (database name).");

        if(validationErrors.Length > 0)
            return ValidateOptionsResult.Fail(validationErrors.ToString());

        return ValidateOptionsResult.Success;
    }
}