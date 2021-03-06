﻿using Components;
using Damselfly.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Damselfly.Components.Search
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SystemFileSearchSource : SearchSource
    {
        private readonly string[] _directories = new[]
        {
            @"%SystemRoot%",
            @"%SystemRoot%\system32"
        };

        private readonly string[] _extensions = new[]
        {
            "cpl",
            "msc",
            "exe",
            "cmd",
            "bat"
        };

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string DebuggerDisplay => ToString();

        protected override ConcurrentBag<SearchItem> LoadItems() =>
            new ConcurrentBrag<SearchItem>(
                _directories
                    .ForceUnbufferedPerProcessorParallelism()
                    .SelectMany(x => _extensions.Select(y => new { Directory = x, Extension = y }))
                    .SelectMany(x => GetDirectoryFiles(x.Directory, x.Extension))
                    .SelectMany(x =>
                        SearchItemBuilder
                            .FromFile(x)
                            .Concat(SearchItemBuilder
                                .FromCommand(x))));

        private static IEnumerable<string> GetDirectoryFiles(string directory, string extension) =>
            Directory.GetFiles(
                Environment.ExpandEnvironmentVariables(directory),
                "*." + extension);
    }
}
