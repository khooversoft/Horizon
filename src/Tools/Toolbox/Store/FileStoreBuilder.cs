using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Data;
using Toolbox.Tools;

namespace Toolbox.Store;

public sealed class FileStoreBuilder
{
    private IFileStore? _build;

    private readonly List<Func<IServiceProvider, IFileStore, IFileStore>> _layers = new();
    private Func<IServiceProvider, IFileStore> _root = null!;

    public FileStoreBuilder UseRoot(Func<IServiceProvider, IFileStore> root) { _root = root; return this; }
    public FileStoreBuilder Use(Func<IServiceProvider, IFileStore, IFileStore> layer) { _layers.Add(layer); return this; }

    public IFileStore Build(IServiceProvider sp, string key)
    {
        sp.NotNull();
        key.NotEmpty();

        if (_build is not null) return _build;

        _build = _layers.Aggregate(_root(sp), (inner, layer) => layer(sp, inner));
        return _build;
    }
}


public static class FileStoreBuilderTool
{
    public static FileStoreBuilder UseDatalake(this FileStoreBuilder b) =>
        b.UseRoot(sp => ActivatorUtilities.CreateInstance<FileStoreDatalake>(sp, sp.GetRequiredService<DatalakeStore>()));

    public static FileStoreBuilder WithCache(this FileStoreBuilder b) =>
        b.Use((sp, inner) => ActivatorUtilities.CreateInstance<FileStoreCache>(sp, inner));

    public static FileStoreBuilder WithNamespace(this FileStoreBuilder b, string ns) =>
        b.Use((sp, inner) => ActivatorUtilities.CreateInstance<FileStoreNamespace>(sp, ns, inner));

    //public static FileStoreBuilder WithHash(this FileStoreBuilder b) =>
    //    b.Use((sp, inner) => ActivatorUtilities.CreateInstance<HashFileStore>(sp, inner));
}