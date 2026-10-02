using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public static class DatalakeExtensions
{
    public static async Task<Option<DatalakeStorePathDetail>> GetPathDetail(this DataLakeFileClient fileClient, ILogger logger, CancellationToken token = default)
    {
        fileClient.NotNull();
        logger.NotNull();

        logger.LogDebug("Getting path {path} properties", fileClient.Path);
        using var metric = logger.LogDuration("dataLakeStore-getPathProperties");

        try
        {
            Response<bool> exist = await fileClient.ExistsAsync();
            if (!exist.HasValue || !exist.Value)
            {
                logger.LogDebug("File does not exist, path={path}", fileClient.Path);
                return new Option<DatalakeStorePathDetail>(StatusCode.NotFound);
            }

            var result = await fileClient.GetPropertiesAsync(cancellationToken: token);
            return result.Value.ConvertTo(fileClient.Path).ToOption();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to GetPathProperties for file {path}", fileClient.Path);
            return (StatusCode.NotFound, ex.Message);
        }
    }

    public static async Task<Option<DatalakeStorePathDetail>> GetPathDetailOrCreate(this DataLakeFileClient fileClient, ILogger logger, CancellationToken token = default)
    {
        using var metric = logger.LogDuration("dataLakeStore-getPathPropertiesOrCreate");

        var properties = await fileClient.GetPathDetail(logger, token);
        if (properties.IsOk()) return properties;

        await fileClient.CreateIfNotExistsAsync(PathResourceType.File);
        return await fileClient.GetPathDetail(logger, token);
    }

    public static DatalakeStorePathDetail ConvertTo(this PathItem subject, string path)
    {
        subject.NotNull();
        path.NotEmpty();

        return new DatalakeStorePathDetail
        {
            Path = path,
            IsFolder = subject.IsDirectory ?? false,
            LastModified = subject.LastModified,
            ContentLength = subject.ContentLength ?? 0,
            CreatedOn = subject.CreatedOn,
            ETag = subject.ETag.ToString(),
        };
    }

    public static DatalakeStorePathDetail ConvertTo(this PathProperties subject, string path)
    {
        subject.NotNull();
        path.NotEmpty();

        return new DatalakeStorePathDetail
        {
            Path = path,
            IsFolder = false,
            LastModified = subject.LastModified,
            ContentLength = subject.ContentLength,
            CreatedOn = subject.CreatedOn,
            ETag = subject.ETag.ToString(),
            LeaseStatus = subject.LeaseStatus switch
            {
                DataLakeLeaseStatus.Locked => LeaseStatus.Locked,
                DataLakeLeaseStatus.Unlocked => LeaseStatus.Unlocked,
                _ => LeaseStatus.Unlocked,
            },
            LeaseDuration = subject.LeaseDuration switch
            {
                DataLakeLeaseDuration.Fixed => LeaseDuration.Fixed,
                DataLakeLeaseDuration.Infinite => LeaseDuration.Infinite,
                _ => LeaseDuration.Infinite,
            },
        };
    }

    public static async Task<Option> ForceDelete(this DatalakeStore store, string path)
    {
        store.NotNull();
        path.NotEmpty();

        var deleteOption = await store.Delete(path);
        if (deleteOption.IsOk() || !deleteOption.IsLocked()) return StatusCode.OK;

        await store.BreakLease(path);

        var result = await store.Delete(path);
        return result;
    }

    public static async Task<Option<string>> ForceSet(this DatalakeStore keyStore, string path, DataETag data)
    {
        keyStore.NotNull();
        path.NotEmpty();

        var writeOption = await keyStore.Set(path, data.StripETag());
        if (writeOption.IsOk() || !writeOption.IsLocked()) return StatusCode.OK;

        await keyStore.BreakLease(path);

        var result = await keyStore.Set(path, data);
        return result;
    }

    public static async Task<Option<string>> Add<T>(this DatalakeStore keyStore, string key, T value)
    {
        var data = value.ToDataETag();
        return await keyStore.NotNull().Add(key, data);
    }

    public static async Task<Option<string>> Set<T>(this DatalakeStore keyStore, string key, T value)
    {
        var data = value.ToDataETag();
        return await keyStore.NotNull().Set(key, data);
    }

    public static Task<Option<T>> Get<T>(this DatalakeStore keyStore, string key) => Get<T>(keyStore, key, data => data.ToObject<T>().NotNull());

    public static async Task<Option<T>> Get<T>(this DatalakeStore keyStore, string key, Func<DataETag, Option<T>> converter)
    {
        keyStore.NotNull();
        converter.NotNull();

        var getOption = await keyStore.Get(key);
        if (getOption.IsError()) return getOption.ToOptionStatus<T>();

        DataETag data = getOption.Return();
        return converter(data);
    }
}
