﻿using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tanka.DocsTool
{
    public static class Infra
    {
        public static void Initialize(Options args)
        {
            var debug = args.Debug;
            LoggerFactory = Microsoft.Extensions.Logging
                .LoggerFactory.Create(builder =>
                {
                    builder.AddConsole(console =>
                    {
                        console.DisableColors = false;
                        console.IncludeScopes = true;
                        console.LogToStandardErrorThreshold = LogLevel.Error;

                    });

                    builder.AddFilter(level =>
                    {
                        if (debug)
                            return true;

                        if (level < LogLevel.Information)
                            return false;

                        return true;
                    });
                });
        }

        public static ILoggerFactory LoggerFactory = new NullLoggerFactory(); 

        public static ILogger Logger => LoggerFactory.CreateLogger("main");
    }
}