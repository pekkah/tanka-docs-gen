namespace Tanka.DocsTool.Navigation
{
    public class NavigationBuilder
    {
        public NavigationBuilder AddLinks(string[] links)
        {
            return this;
        }

        public NavigationBuilder AddLink(string link)
        {
            return this;
        }

        public Link[] Build()
        {
            return new Link[0];
        }
    }
}