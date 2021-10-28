
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Fabron;
using Fabron.Models;
using Fabron.Providers.PostgreSQL;
using HoneyComb.API.Resources;
using HoneyComb.API.Resources.CronReminders.Models;
using HoneyComb.API.Resources.Reminders.Models;
using HoneyComb.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace HoneyComb.API.Services
{

    public interface IReminderQuerier
    {
        Task<PaginatedList<CronReminder>> ListCronReminderByDisplayNameAsync(string appId, string? pattern, int skip = 0, int take = 20);
        Task<PaginatedList<CronReminderItem>> ListCronReminderItemsAsync(string key, int skip = 0, int take = 20);
        Task<PaginatedList<Reminder>> ListRemindersByDisplayNameAsync(string appId, string? pattern, int skip = 0, int take = 20);
    }

    public class NoopReminderQuerier : IReminderQuerier
    {
        public Task<PaginatedList<CronReminder>> ListCronReminderByDisplayNameAsync(string appId, string? pattern, int skip = 0, int take = 20) => throw new NotImplementedException();
        public Task<PaginatedList<CronReminderItem>> ListCronReminderItemsAsync(string key, int skip = 0, int take = 20) => throw new NotImplementedException();
        public Task<PaginatedList<Reminder>> ListRemindersByDisplayNameAsync(string appId, string? pattern, int skip = 0, int take = 20) => throw new NotImplementedException();
    }

    public class ReminderQuerier : IReminderQuerier
    {
        private readonly ILogger<ReminderQuerier> _logger;
        private readonly PostgreSQLOptions _options;
        private readonly string _sql_SearchCronReminder;
        private readonly string _sql_SearchCronReminderByDisplayName;
        private readonly string _sql_CountCronReminder;
        private readonly string _sql_CountCronReminderByDisplayName;
        private readonly string _sql_SearchReminder;
        private readonly string _sql_SearchReminderByDisplayName;
        private readonly string _sql_CountReminder;
        private readonly string _sql_CountReminderByDisplayName;
        private readonly string _sql_SearchCronReminderItems;
        private readonly string _sql_CountCronReminderItems;

        public ReminderQuerier(
            ILogger<ReminderQuerier> logger,
            IOptions<PostgreSQLOptions> options)
        {
            _logger = logger;
            _options = options.Value;

            _sql_SearchCronReminder = $@"
SELECT data FROM {_options.CronJobIndexesTableName}
WHERE data -> 'Metadata' -> 'Labels' ->> '{Constants.LabelNames.ApplicationId}' = @appId
ORDER BY data-> 'Metadata' -> 'Labels' -> 'CreationTimestamp' DESC
LIMIT @take OFFSET @skip;
";
            _sql_SearchCronReminderByDisplayName = $@"
SELECT data FROM {_options.CronJobIndexesTableName}
WHERE data -> 'Metadata' -> 'Labels' ->> '{Constants.LabelNames.ApplicationId}' = @appId
    AND data -> 'Metadata' -> 'Labels' ->> '{FabronConstants.LabelNames.DisplayName}' LIKE @pattern
ORDER BY data-> 'Metadata' -> 'Labels' -> 'CreationTimestamp' DESC
LIMIT @take OFFSET @skip;
";

            _sql_CountCronReminder = $@"
SELECT count(*) FROM {_options.CronJobIndexesTableName}
WHERE data -> 'Metadata' -> 'Labels' ->> '{Constants.LabelNames.ApplicationId}' = @appId
";
            _sql_CountCronReminderByDisplayName = $@"
SELECT count(*) FROM {_options.CronJobIndexesTableName}
WHERE data -> 'Metadata' -> 'Labels' ->> '{Constants.LabelNames.ApplicationId}' = @appId
    AND data -> 'Metadata' -> 'Labels' ->> '{FabronConstants.LabelNames.DisplayName}' LIKE @pattern
";

            _sql_SearchReminder = $@"
SELECT data FROM {_options.JobIndexesTableName}
WHERE data -> 'Metadata' -> 'Labels' ->> '{Constants.LabelNames.ApplicationId}' = @appId
    AND data -> 'Metadata' -> 'Labels' -> '{FabronConstants.LabelNames.OwnerType}' is null
ORDER BY data -> 'Metadata' -> 'CreationTimestamp' DESC
LIMIT @take OFFSET @skip;
";
            _sql_SearchReminderByDisplayName = $@"
SELECT data FROM {_options.JobIndexesTableName}
WHERE data -> 'Metadata' -> 'Labels' ->> '{Constants.LabelNames.ApplicationId}' = @appId
    AND data -> 'Metadata' -> 'Labels' -> '{FabronConstants.LabelNames.OwnerType}' is null
    AND data -> 'Metadata' -> 'Labels' ->> '{FabronConstants.LabelNames.DisplayName}' LIKE @pattern
ORDER BY data -> 'Metadata' -> 'CreationTimestamp' DESC
LIMIT @take OFFSET @skip;
";

            _sql_CountReminder = $@"
SELECT count(*) FROM {_options.JobIndexesTableName}
WHERE data -> 'Metadata' -> 'Labels' ->> '{Constants.LabelNames.ApplicationId}' = @appId
    AND (data->'Metadata'->'Labels'->'{FabronConstants.LabelNames.OwnerType}') is null
";
            _sql_CountReminderByDisplayName = $@"
SELECT count(*) FROM {_options.JobIndexesTableName}
WHERE data -> 'Metadata' -> 'Labels' ->> '{Constants.LabelNames.ApplicationId}' = @appId
    AND (data->'Metadata'->'Labels'->'{FabronConstants.LabelNames.OwnerType}') is null
    AND data -> 'Metadata' -> 'Labels' ->> '{FabronConstants.LabelNames.DisplayName}' LIKE @pattern;
";

            _sql_SearchCronReminderItems = $@"
SELECT data FROM {_options.JobIndexesTableName}
WHERE data -> 'Metadata' -> 'Labels' ->> '{FabronConstants.LabelNames.OwnerKey}' = @key
    AND data -> 'Metadata' -> 'Labels' ->> '{FabronConstants.LabelNames.OwnerType}' = '{FabronConstants.OwnerTypes.CronJob}'
ORDER BY data -> 'Metadata' -> 'CreationTimestamp' DESC
LIMIT @take OFFSET @skip;
";
            _sql_CountCronReminderItems = $@"
SELECT count(*) FROM {_options.JobIndexesTableName}
WHERE data -> 'Metadata' -> 'Labels' ->> '{FabronConstants.LabelNames.OwnerKey}' = @key
    AND data -> 'Metadata' -> 'Labels' ->> '{FabronConstants.LabelNames.OwnerType}' = '{FabronConstants.OwnerTypes.CronJob}';
";

        }

        public async Task<PaginatedList<CronReminder>> ListCronReminderByDisplayNameAsync(string appId, string? pattern, int skip = 0, int take = 20)
        {
            Task<List<CronReminder>>? searchCronReminderTask = SearchCronReminderByDisplayNameAsync(appId, pattern, skip, take);
            Task<long>? countCronReminderTask = CountCronReminderByDisplayNameAsync(appId, pattern);

            List<CronReminder>? cronReminders = await searchCronReminderTask;
            var count = await countCronReminderTask;

            return new PaginatedList<CronReminder>(cronReminders, count, 1 + skip / take, take);
        }

        private async Task<List<CronReminder>> SearchCronReminderByDisplayNameAsync(string appId, string? pattern, int skip = 0, int take = 20)
        {
            var byDisplayName = !string.IsNullOrEmpty(pattern);
            var result = new List<CronJob>();

            await using var conn = new NpgsqlConnection(_options.ConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(byDisplayName ? _sql_SearchCronReminderByDisplayName : _sql_SearchCronReminder, conn);
            cmd.Parameters.AddWithValue("@appId", appId);
            if (byDisplayName)
            {
                cmd.Parameters.AddWithValue("@pattern", '%' + pattern + '%');
            }

            cmd.Parameters.AddWithValue("@take", take);
            cmd.Parameters.AddWithValue("@skip", skip);

            await using NpgsqlDataReader? reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(JsonSerializer.Deserialize<CronJob>(reader.GetString(0), _options.JsonSerializerOptions)!);
            }

            var cronReminders = result
                .Select(job => job.Map<InvokeHttpRequest>())
                .Select(cronJob => cronJob.ToCronReminder())
                .ToList();

            return cronReminders;
        }

        private async Task<long> CountCronReminderByDisplayNameAsync(string appId, string? pattern)
        {
            var byDisplayName = !string.IsNullOrEmpty(pattern);
            await using var conn = new NpgsqlConnection(_options.ConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(byDisplayName ? _sql_CountCronReminderByDisplayName : _sql_CountCronReminder, conn);
            cmd.Parameters.AddWithValue("@appId", appId);
            if (byDisplayName)
            {
                cmd.Parameters.AddWithValue("@pattern", '%' + pattern + '%');
            }

            var result = await cmd.ExecuteScalarAsync();
            return (long)result!;
        }

        public async Task<PaginatedList<Reminder>> ListRemindersByDisplayNameAsync(string appId, string? pattern, int skip = 0, int take = 20)
        {
            // _logger.LogDebug($"Listing reminders pattern: {pattern}");
            Task<List<Reminder>>? searchReminderTask = SearchReminderByDisplayNameAsync(appId, pattern, skip, take);
            Task<long>? countReminderTask = CountReminderByDisplayNameAsync(appId, pattern);

            List<Reminder>? Reminders = await searchReminderTask;
            var count = await countReminderTask;

            return new PaginatedList<Reminder>(Reminders, count, 1 + skip / take, take);
        }

        private async Task<List<Reminder>> SearchReminderByDisplayNameAsync(string appId, string? pattern, int skip = 0, int take = 20)
        {
            var byDisplayName = !string.IsNullOrEmpty(pattern);
            var result = new List<Job>();

            await using var conn = new NpgsqlConnection(_options.ConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(byDisplayName ? _sql_SearchReminderByDisplayName : _sql_SearchReminder, conn);
            cmd.Parameters.AddWithValue("@appId", appId);
            if (byDisplayName)
            {
                cmd.Parameters.AddWithValue("@pattern", '%' + pattern + '%');
            }

            cmd.Parameters.AddWithValue("@take", take);
            cmd.Parameters.AddWithValue("@skip", skip);

            // _logger.LogInformation($"cmd:\n{cmd.CommandText}\nparams: {string.Join("\n", cmd.Parameters.Select(p => $"{p.ParameterName} : {p.Value}"))}");

            await using NpgsqlDataReader? reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(JsonSerializer.Deserialize<Job>(reader.GetString(0), _options.JsonSerializerOptions)!);
            }

            // _logger.LogInformation("count: " + result.Count);

            var Reminders = result
                .Select(job => job.Map<InvokeHttpRequest, InvokeHttpRequestResult>())
                .Select(job => job.ToReminder())
                .ToList();

            return Reminders;
        }

        private async Task<long> CountReminderByDisplayNameAsync(string appId, string? pattern)
        {
            var byDisplayName = !string.IsNullOrEmpty(pattern);
            await using var conn = new NpgsqlConnection(_options.ConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(byDisplayName ? _sql_CountReminderByDisplayName : _sql_CountReminder, conn);
            cmd.Parameters.AddWithValue("@appId", appId);
            if (byDisplayName)
            {
                cmd.Parameters.AddWithValue("@pattern", '%' + pattern + '%');
            }

            var result = await cmd.ExecuteScalarAsync();
            return (long)result!;
        }

        public async Task<PaginatedList<CronReminderItem>> ListCronReminderItemsAsync(string key, int skip = 0, int take = 20)
        {
            Task<List<CronReminderItem>>? searchCronItemsTask = SearchCronReminderItemsAsync(key, skip, take);
            Task<long>? countCronItemsTask = CountCronReminderItemsAsync(key);
            List<CronReminderItem>? items = await searchCronItemsTask;
            var count = await countCronItemsTask;

            return new PaginatedList<CronReminderItem>(items, count, 1 + skip / take, take);
        }

        private async Task<List<CronReminderItem>> SearchCronReminderItemsAsync(string key, int skip = 0, int take = 20)
        {
            var result = new List<Job>();

            await using var conn = new NpgsqlConnection(_options.ConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(_sql_SearchCronReminderItems, conn);
            cmd.Parameters.AddWithValue("@key", key);
            cmd.Parameters.AddWithValue("@take", take);
            cmd.Parameters.AddWithValue("@skip", skip);

            await using NpgsqlDataReader? reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(JsonSerializer.Deserialize<Job>(reader.GetString(0), _options.JsonSerializerOptions)!);
            }

            var items = result
                .Select(job => job.Map<InvokeHttpRequest, InvokeHttpRequestResult>())
                .Select(job => job.ToCronReminderItem())
                .ToList();

            return items;
        }

        private async Task<long> CountCronReminderItemsAsync(string key)
        {
            await using var conn = new NpgsqlConnection(_options.ConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(_sql_CountCronReminderItems, conn);
            cmd.Parameters.AddWithValue("@key", key);

            var result = await cmd.ExecuteScalarAsync();
            return (long)result!;
        }

    }
}
