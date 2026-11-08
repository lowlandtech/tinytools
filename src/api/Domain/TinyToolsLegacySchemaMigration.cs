using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace LowlandTech.TinyTools.Api.Domain;

/// <summary>Moves a previously installed unprefixed TinyTools schema in place.</summary>
internal static class TinyToolsLegacySchemaMigration
{
    public const string InitialMigrationId = "20261108012126_InitialTinyToolsSchema";

    private static readonly string[] LegacyTables =
    [
        "Packages",
        "StepIntents",
        "TemplateFlows",
        "Templates",
        "Imports",
        "TemplateSteps",
        "TemplateTemplatePackage",
        "TemplateVersions",
    ];

    public static async Task ApplyAsync(TinyToolsDbContext database, CancellationToken cancellationToken)
    {
        var applied = await database.Database.GetAppliedMigrationsAsync(cancellationToken);
        if (!applied.Contains(InitialMigrationId, StringComparer.Ordinal))
            return;

        var provider = database.Database.ProviderName;
        var sqlite = string.Equals(provider, "Microsoft.EntityFrameworkCore.Sqlite", StringComparison.Ordinal);
        var postgres = string.Equals(provider, "Npgsql.EntityFrameworkCore.PostgreSQL", StringComparison.Ordinal);
        if (!sqlite && !postgres)
            throw new NotSupportedException($"TinyTools schema migration does not support provider '{provider}'.");

        var connection = database.Database.GetDbConnection();
        var openedHere = connection.State != System.Data.ConnectionState.Open;
        if (openedHere)
            await connection.OpenAsync(cancellationToken);

        try
        {
            foreach (var table in LegacyTables)
            {
                if (!await TableExistsAsync(connection, table, sqlite, cancellationToken)
                    || await TableExistsAsync(connection, $"TinyTools_{table}", sqlite, cancellationToken))
                    continue;

                await using var rename = connection.CreateCommand();
                rename.CommandText = sqlite
                    ? $"ALTER TABLE {Quote(table)} RENAME TO {Quote($"TinyTools_{table}")}" 
                    : $"ALTER TABLE {Quote(TinyToolsDbContext.Schema)}.{Quote(table)} RENAME TO {Quote($"TinyTools_{table}")}";
                await rename.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        finally
        {
            if (openedHere)
                await connection.CloseAsync();
        }
    }

    private static async Task<bool> TableExistsAsync(
        DbConnection connection,
        string table,
        bool sqlite,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sqlite
            ? "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name"
            : "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = @schema AND table_name = @name";
        var name = command.CreateParameter();
        name.ParameterName = sqlite ? "$name" : "name";
        name.Value = table;
        command.Parameters.Add(name);
        if (!sqlite)
        {
            var schema = command.CreateParameter();
            schema.ParameterName = "schema";
            schema.Value = TinyToolsDbContext.Schema;
            command.Parameters.Add(schema);
        }

        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
    }

    private static string Quote(string identifier) => $"\"{identifier.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
