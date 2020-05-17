namespace Tanka.DocsTool.Navigation
{
    public readonly struct DisplayLink
    {
        public DisplayLink(string? title, Link link)
        {
            Title = title;
            Link = link;
        }


        public string? Title { get; }

        public Link Link { get; }
    }
}