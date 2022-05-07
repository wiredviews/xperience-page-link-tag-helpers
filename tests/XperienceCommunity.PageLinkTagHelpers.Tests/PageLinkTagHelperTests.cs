using System.Collections.Specialized;
using AutoFixture.NUnit3;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.Tests;
using FluentAssertions;
using Kentico.Content.Web.Mvc;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Tests.DocumentEngine;

namespace XperienceCommunity.PageLinkTagHelpers.Tests;

[TestFixture]
public class PageLinkTagHelperTests : UnitTests
{
    [SetUp]
    public void Setup()
    {
        DocumentGenerator.RegisterDocumentType<TreeNode>(TreeNode.TYPEINFO.ObjectType);

        Fake().DocumentType<TreeNode>(TreeNode.TYPEINFO.ObjectType);
    }

    [Test, AutoData]
    public async Task ProcessAsync_Will_Not_Modify_The_Target_Anchor_When_No_LinkablePage_Is_Provided(string href)
    {
        var linkRetriever = Substitute.For<ILinkablePageLinkRetriever>();
        var log = Substitute.For<IEventLogService>();

        var tagHelper = new PageLinkTagHelper(linkRetriever, log)
        {
            Page = null
        };

        var tagHelperContext = new TagHelperContext(
            tagName: "a",
            new() { new("href", href) },
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var tagHelperOutput = new TagHelperOutput("a",
            new() { new("href", href) },
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

        tagHelperOutput.TagName.Should().Be("a");
        tagHelperOutput.Attributes.FirstOrDefault(a => a.Name == "href")?.Value.Should().Be(href);
        tagHelperOutput.IsContentModified.Should().BeFalse();

        log.Received(1).LogEvent(Arg.Any<EventLogData>());
    }

    [Test, AutoData]
    public async Task ProcessAsync_Will_Not_Modify_The_Target_Anchor_When_No_Page_Is_Retrieved(string href)
    {
        var page = Substitute.For<ILinkablePage>();
        var linkRetriever = Substitute.For<ILinkablePageLinkRetriever>();
        linkRetriever.RetrieveAsync(Arg.Is(page.NodeGUID)).Returns((LinkablePageLinkResult?)null);

        var log = Substitute.For<IEventLogService>();

        var tagHelper = new PageLinkTagHelper(linkRetriever, log)
        {
            Page = page
        };

        var tagHelperContext = new TagHelperContext(
            tagName: "a",
            new() { new("href", href) },
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var tagHelperOutput = new TagHelperOutput("a",
            new() { new("href", href) },
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

        tagHelperOutput.TagName.Should().Be("a");
        tagHelperOutput.Attributes.FirstOrDefault(a => a.Name == "href")?.Value.Should().Be(href);
        tagHelperOutput.IsContentModified.Should().BeFalse();

        log.Received(1).LogEvent(Arg.Any<EventLogData>());
    }

    [Test, AutoData]
    public async Task ProcessAsync_Will_Not_Modify_The_Target_Anchor_When_No_PageUrl_Is_Retrieved(
        string href
    )
    {
        var page = Substitute.For<ILinkablePage>();
        var linkRetriever = Substitute.For<ILinkablePageLinkRetriever>();
        linkRetriever.RetrieveAsync(Arg.Is(page.NodeGUID)).Throws(new Exception("oops"));

        var log = Substitute.For<IEventLogService>();

        var tagHelper = new PageLinkTagHelper(linkRetriever, log)
        {
            Page = page
        };

        var tagHelperContext = new TagHelperContext(
            tagName: "a",
            new() { new("href", href) },
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var tagHelperOutput = new TagHelperOutput("a",
            new() { new("href", href) },
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

        tagHelperOutput.TagName.Should().Be("a");
        tagHelperOutput.Attributes.FirstOrDefault(a => a.Name == "href")?.Value.Should().Be(href);
        tagHelperOutput.IsContentModified.Should().BeFalse();

        log.Received(1).LogEvent(Arg.Any<EventLogData>());
    }

    [Test, AutoData]
    public async Task ProcessAsync_Will_Set_The_Href_When_A_URL_Is_Retrieved(
        string linkURL,
        string linkText,
        string href
    )
    {
        var page = Substitute.For<ILinkablePage>();
        var linkRetriever = Substitute.For<ILinkablePageLinkRetriever>();
        linkRetriever.RetrieveAsync(Arg.Is(page.NodeGUID)).Returns(new LinkablePageLinkResult(linkURL, linkText));

        var log = Substitute.For<IEventLogService>();

        var tagHelper = new PageLinkTagHelper(linkRetriever, log)
        {
            Page = page
        };

        var tagHelperContext = new TagHelperContext(
            tagName: "a",
            new() { new("href", href) },
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var tagHelperOutput = new TagHelperOutput("a",
            new() { new("href", href) },
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

        tagHelperOutput.TagName.Should().Be("a");
        tagHelperOutput.Attributes.FirstOrDefault(a => a.Name == "href")?.Value.Should().Be(linkURL);
        tagHelperOutput.IsContentModified.Should().BeFalse();
        tagHelperOutput.PreContent.IsModified.Should().BeTrue();
        tagHelperOutput.PreContent.GetContent().Should().Be(linkText);

        log.DidNotReceiveWithAnyArgs().LogEvent(default);
    }

    [Test, AutoData]
    public async Task ProcessAsync_Will_Set_The_Href_With_QueryParams_When_A_URL_Is_Retrieved_And_QueryParams_Are_Provided(
        string linkURL,
        string linkText,
        string href,
        NameValueCollection queryParams
    )
    {
        var page = Substitute.For<ILinkablePage>();
        var linkRetriever = Substitute.For<ILinkablePageLinkRetriever>();
        linkRetriever.RetrieveAsync(Arg.Is(page.NodeGUID)).ReturnsForAnyArgs(new LinkablePageLinkResult(linkURL, linkText));

        var log = Substitute.For<IEventLogService>();

        var tagHelper = new PageLinkTagHelper(linkRetriever, log)
        {
            Page = page,
            QueryParams = queryParams
        };

        var tagHelperContext = new TagHelperContext(
            tagName: "a",
            new() { new("href", href) },
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var tagHelperOutput = new TagHelperOutput("a",
            new() { new("href", href) },
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

        tagHelperOutput.TagName.Should().Be("a");
        tagHelperOutput.Attributes.FirstOrDefault(a => a.Name == "href")?.Value.Should().Be(linkURL + queryParams.ToQueryString());
        tagHelperOutput.IsContentModified.Should().BeFalse();
        tagHelperOutput.PreContent.IsModified.Should().BeTrue();
        tagHelperOutput.PreContent.GetContent().Should().Be(linkText);

        log.DidNotReceiveWithAnyArgs().LogEvent(default);
    }

    [Test, AutoData]
    public async Task ProcessAsync_Will_Use_A_Custom_Link_Text_When_Provided(
        string href,
        string linkURL,
        string linkText,
        string alternateText
    )
    {
        var page = Substitute.For<ILinkablePage>();
        var linkRetriever = Substitute.For<ILinkablePageLinkRetriever>();
        linkRetriever.RetrieveAsync(Arg.Is(page.NodeGUID)).ReturnsForAnyArgs(new LinkablePageLinkResult(linkURL, linkText));

        var log = Substitute.For<IEventLogService>();

        var tagHelper = new PageLinkTagHelper(linkRetriever, log)
        {
            Page = page,
            LinkText = alternateText
        };

        var tagHelperContext = new TagHelperContext(
            tagName: "a",
            new() { new("href", href) },
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var tagHelperOutput = new TagHelperOutput("a",
            new() { new("href", href) },
            (result, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

        tagHelperOutput.TagName.Should().Be("a");
        tagHelperOutput.Attributes.FirstOrDefault(a => a.Name == "href")?.Value.Should().Be(linkURL);
        tagHelperOutput.IsContentModified.Should().BeFalse();
        tagHelperOutput.PreContent.IsModified.Should().BeTrue();
        tagHelperOutput.PreContent.GetContent().Should().Be(alternateText);

        log.DidNotReceiveWithAnyArgs().LogEvent(default);
    }

    [Test, AutoData]
    public async Task ProcessAsync_Will_Not_Set_The_Link_Text_When_There_Is_Child_Content(
        string href,
        string linkURL,
        string linkText,
        string alternateText
    )
    {
        var page = Substitute.For<ILinkablePage>();
        var linkRetriever = Substitute.For<ILinkablePageLinkRetriever>();
        linkRetriever.RetrieveAsync(Arg.Is(page.NodeGUID)).ReturnsForAnyArgs(new LinkablePageLinkResult(linkURL, linkText));

        var log = Substitute.For<IEventLogService>();

        var tagHelper = new PageLinkTagHelper(linkRetriever, log)
        {
            Page = page,
            LinkText = alternateText
        };

        var tagHelperContext = new TagHelperContext(
            tagName: "a",
            new() { new("href", href) },
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));

        var tagHelperOutput = new TagHelperOutput("a",
            new() { new("href", href) },
            (result, encoder) =>
            {
                var content = new DefaultTagHelperContent();
                content.AppendHtml(new HtmlString("<img src='/logo.png' class='hero'>"));

                return Task.FromResult<TagHelperContent>(content);
            });

        await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

        tagHelperOutput.TagName.Should().Be("a");
        tagHelperOutput.Attributes.FirstOrDefault(a => a.Name == "href")?.Value.Should().Be(linkURL);
        tagHelperOutput.IsContentModified.Should().BeFalse();
        tagHelperOutput.PreContent.IsModified.Should().BeFalse();
        tagHelperOutput.PreContent.GetContent().Should().BeEmpty();

        log.DidNotReceiveWithAnyArgs().LogEvent(default);
    }
}
