using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tanka.FileSystem;

namespace Tanka.DocsTool.Pages
{
    public class PageComposer
    {
        public Task<Page> Compose(ISiteModel site, File htmlFile)
        {
            return null;
        }
    }

    public class Page
    {

    }

    public interface ISiteModel
    {
        public ISiteNavigation Navigation { get; }
    }

    public interface ISiteNavigation
    {
        
    }
}
