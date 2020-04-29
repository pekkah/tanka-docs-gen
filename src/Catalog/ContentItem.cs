using System.Collections.Generic;
using System.Threading.Tasks;
using Tanka.FileSystem;

namespace Catalog
{
    public class ContentItem
    {
        public string Type { get; }
        
        public File File { get; }
        
        public Directory Directory { get; }
    }

    public static class ContentTypes
    {
        public const string Document = "md";
    }

    public interface IFileClassifier
    {
        public object Classify(File file);
    }

    public class Catalog
    {
        public Catalog()
        {
            
        }

        public Task Build()
        {
            
        }
    }
}