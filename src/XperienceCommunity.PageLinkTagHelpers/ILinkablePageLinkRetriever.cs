using System;
using System.Linq;
using System.Threading.Tasks;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Routing;
using Kentico.Content.Web.Mvc;

namespace XperienceCommunity.PageLinkTagHelpers
{
    /// <summary>
    /// Returns the link URL and text for the given <see cref="TreeNode.NodeGUID" />
    /// </summary>
    public interface ILinkablePageLinkRetriever
    {
        Task<LinkablePageLinkResult?> RetrieveAsync(Guid nodeGUID);
    }

    /// <summary>
    /// Default implementation of <see cref="ILinkablePageLinkRetriever" /> which
    /// returns the page's <see cref="PageUrl.RelativePath" /> for a URL and the <see cref="TreeNode.DocumentName" /> for the text
    /// </summary>
    public class DefaultLinkablePageLinkRetriever : ILinkablePageLinkRetriever
    {
        private readonly IPageRetriever pageRetriever;
        private readonly IPageUrlRetriever pageUrlRetriever;

        public DefaultLinkablePageLinkRetriever(IPageRetriever pageRetriever, IPageUrlRetriever pageUrlRetriever)
        {
            this.pageRetriever = pageRetriever;
            this.pageUrlRetriever = pageUrlRetriever;
        }

        public async Task<LinkablePageLinkResult?> RetrieveAsync(Guid nodeGUID)
        {
            var pages = await pageRetriever
                .RetrieveAsync<TreeNode>(
                    query => query
                        .WhereEquals(nameof(TreeNode.NodeGUID), nodeGUID)
                        // Optimize returned columns, .WithPageUrlPaths() will add back the ones it needs
                        .Columns(nameof(TreeNode.DocumentName))
                        .WithPageUrlPaths(),
                    cache => cache.Key($"page-link|{nodeGUID}"));

            var page = pages.FirstOrDefault();

            if (page is null)
            {
                return null;
            }

            var pageUrl = pageUrlRetriever.Retrieve(page);

            return new LinkablePageLinkResult(pageUrl.RelativePath, page.DocumentName);
        }

        public string GetFallbackLinkText(TreeNode node) => node.DocumentName;
    }

    public class LinkablePageLinkResult
    {
        public string LinkURL { get; }
        public string LinkText { get; }

        public LinkablePageLinkResult(string linkURL, string linkText)
        {
            LinkURL = linkURL;
            LinkText = linkText;
        }
    }
}
