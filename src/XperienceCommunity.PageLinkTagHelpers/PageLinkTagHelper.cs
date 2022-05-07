using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using CMS.Core;
using CMS.DocumentEngine;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace XperienceCommunity.PageLinkTagHelpers
{

    /// <summary>
    /// Generates a link to a page based on the page's <see cref="TreeNode.NodeGUID"/>.
    /// </summary>
    [HtmlTargetElement("a", Attributes = "xp-page-link")]
    public class PageLinkTagHelper : TagHelper
    {
        private readonly ILinkablePageLinkRetriever linkRetriever;
        private readonly IEventLogService log;


        [HtmlAttributeName("xp-page-link")]
        public ILinkablePage? Page { get; set; }

        [HtmlAttributeName("xp-page-link-query-params")]
        public NameValueCollection? QueryParams { get; set; }

        public PageLinkTagHelper(ILinkablePageLinkRetriever linkRetriever, IEventLogService log)
        {
            this.log = log;
            this.linkRetriever = linkRetriever;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (Page is null)
            {
                log.LogError(
                    nameof(PageLinkTagHelper),
                    "MISSING_LINKABLE_PAGE",
                    "No Linkable Page has been assigned.",
                    loggingPolicy: LoggingPolicy.ONLY_ONCE);

                return;
            }

            LinkablePageLinkResult? result = null;

            try
            {
                result = await linkRetriever.RetrieveAsync(Page.NodeGUID);
            }
            catch (Exception ex)
            {
                log.LogException(
                    nameof(PageLinkTagHelper),
                    $"PAGE_URL_RETRIEVAL_FAILED_{Page.NodeGUID}",
                    ex,
                    loggingPolicy: LoggingPolicy.ONLY_ONCE);

                return;
            }


            if (result is null)
            {
                log.LogWarning(
                    nameof(PageLinkTagHelper),
                    $"MISSING_PAGE_{Page.NodeGUID}", // Include the NodeGUID in the EventCode because it is part of the cache key for "ONLY_ONCE" events
                    $"Could not find Page [{Page.NodeGUID}]", loggingPolicy: LoggingPolicy.ONLY_ONCE);

                return;
            }

            output.Attributes.SetAttribute("href", result.LinkURL + QueryParams?.ToQueryString() ?? "");

            var childContent = await output.GetChildContentAsync();

            if (!output.Attributes.TryGetAttribute("title", out var titleAttribute))
            {
                output.Attributes.SetAttribute("title", result.LinkText);
            }
            else if (string.IsNullOrWhiteSpace(titleAttribute.Value.ToString()) && childContent.IsEmptyOrWhiteSpace)
            {
                output.Attributes.SetAttribute("title", result.LinkText);
            }

            if (childContent.IsEmptyOrWhiteSpace)
            {
                _ = output.Content.SetContent(result.LinkText);
            }
        }
    }
}
