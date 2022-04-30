using System.Collections.Specialized;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Routing;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace XperienceCommunity.PageLinkTagHelpers;

/// <summary>
/// Generates a link to a page based on the page's <see cref="TreeNode.NodeGUID"/>.
/// </summary>
[HtmlTargetElement("a", Attributes = "xp-page-link")]
public class PageLinkTagHelper : TagHelper
{
    private readonly IPageRetriever pageRetriever;
    private readonly IPageUrlRetriever urlRetriever;
    private readonly IEventLogService log;


    [HtmlAttributeName("xp-page-link")]
    public ILinkablePage? Page { get; set; }

    [HtmlAttributeName("xp-page-link-text")]
    public string? LinkText { get; set; } = "";

    [HtmlAttributeName("xp-page-link-query-params")]
    public NameValueCollection? QueryParams { get; set; }

    public PageLinkTagHelper(IPageRetriever pageRetriever, IPageUrlRetriever urlRetriever, IEventLogService log)
    {
        this.log = log;
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
                query => query
                    .WhereEquals(nameof(TreeNode), Page.NodeGUID)
                    .Columns(nameof(TreeNode.NodeID))
                    .WithPageUrlPaths(),
                cache => cache.Key($"page-link|{Page.NodeGUID}"));

        var linkedPage = pages.FirstOrDefault();

        if (linkedPage is null)
        {
            log.LogWarning(
                nameof(PageLinkTagHelper), 
                $"MISSING_PAGE_{Page.NodeGUID}", // Include the NodeGUID in the EventCode because it is part of the cache key for "ONLY_ONCE" events
                $"Could not find Page [{Page.NodeGUID}]", loggingPolicy: LoggingPolicy.ONLY_ONCE);

            return;
        }

        var pageUrl = urlRetriever.Retrieve(linkedPage);

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
                ? linkedPage.DocumentName
                : LinkText;

            _ = output.PreContent.SetContent(linkText);
        }
    }
}

