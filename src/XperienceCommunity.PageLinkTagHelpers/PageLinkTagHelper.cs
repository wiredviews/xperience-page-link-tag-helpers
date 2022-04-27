namespace XperienceCommunity.PageLinkTagHelpers;

[HtmlTargetElement("a", Attributes = "xp-page-link")]
public class PageLinkTagHelper : TagHelper
{
    private readonly IPageRetriever pageRetriever;
    private readonly IPageUrlRetriever urlRetriever;

    [HtmlAttributeName("xp-page-link")]
    public ILinkablePage? Page { get; set; }

    [HtmlAttributeName("xp-page-link-text")]
    public string? LinkText { get; set; } = "";

    [HtmlAttributeName("xp-page-link-query-params")]
    public NameValueCollection? QueryParams { get; set; }

    public PageLinkTagHelper(IPageRetriever pageRetriever, IPageUrlRetriever urlRetriever)
    {
        this.pageRetriever = pageRetriever;
        this.urlRetriever = urlRetriever;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (Page is null)
        {
            return;
        }

        var pages = await pageRetriever
            .RetrieveAsync<TreeNode>(
                q => q.WhereEquals(nameof(TreeNode), Page.NodeGuid),
                cache => cache.Key($"page-link|{Page.NodeGuid}"));

        var page = pages.FirstOrDefault();

        if (page is null)
        {
            return;
        }

        var pageUrl = urlRetriever.Retrieve(page);

        string querystring = QueryParams is not null
            ? QueryParams.ToQueryString()
            : "";

        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("href", pageUrl.RelativePath + querystring);

        var childContent = await output.GetChildContentAsync();

        if (string.IsNullOrWhiteSpace(childContent.GetContent()))
        {
            string linkText = string.IsNullOrWhiteSpace(LinkText)
                ? page.DocumentName
                : LinkText;

            _ = output.PreContent.SetContent(linkText);
        }
    }
}

