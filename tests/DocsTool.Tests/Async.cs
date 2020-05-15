using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tanka.DocsTool.Tests
{
    internal static class Async
    {
        public static async IAsyncEnumerable<T> From<T>(IEnumerable<T> enumerable)
        {
            await Task.Delay(0);
            foreach (var item in enumerable)
            {
                yield return item;
            }
        }

        public static async Task<ICollection<T>> FromAsync<T>(IAsyncEnumerable<T> enumerable)
        {
            var list = new List<T>();

            await foreach(var item in enumerable)
                list.Add(item);

            return list;
        }
    }
}