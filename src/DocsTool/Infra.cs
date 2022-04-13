using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tanka.DocsTool
{
    public static class Infra
    {
        public static void Initialize(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory;
        }

        public static ILoggerFactory LoggerFactory = new NullLoggerFactory(); 

        public static ILogger Logger => LoggerFactory.CreateLogger("main");
    }
}