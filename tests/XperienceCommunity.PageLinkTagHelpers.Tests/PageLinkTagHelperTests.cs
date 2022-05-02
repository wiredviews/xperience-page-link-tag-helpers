using Microsoft.AspNetCore.Razor.TagHelpers;
using CMS.Core;
using Kentico.Content.Web.Mvc;
using NSubstitute;
using NUnit.Framework;
using FluentAssertions;
using AutoFixture.NUnit3;
using CMS.DocumentEngine;
using CMS.Tests;
using NSubstitute.ExceptionExtensions;
using Tests.DocumentEngine;
using CMS.DataEngine;
using AutoFixture;

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
    public async Task Process_Will_Not_Modify_The_Target_Anchor_When_No_LinkablePage_Is_Provided(string href)
    {
        var pageRetriever = Substitute.For<IPageRetriever>();
        var pageUrlRetriever = Substitute.For<IPageUrlRetriever>();
        var log = Substitute.For<IEventLogService>();

        var tagHelper = new PageLinkTagHelper(pageRetriever, pageUrlRetriever, log)
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
            (result, encoder) =>
            {
                return Task.FromResult<TagHelperContent>(new DefaultTagHelperContent());
            });

        await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

        tagHelperOutput.TagName.Should().Be("a");
        tagHelperOutput.Attributes.FirstOrDefault(a => a.Name == "href")?.Value.Should().Be(href);
        tagHelperOutput.IsContentModified.Should().BeFalse();

        log.Received(1).LogEvent(Arg.Any<EventLogData>());
    }

    [Test, AutoData]
    public async Task Process_Will_Not_Modify_The_Target_Anchor_When_No_Page_Is_Retrieved(string href)
    {
        var page = Substitute.For<ILinkablePage>();
        var pageRetriever = Substitute.For<IPageRetriever>();
        pageRetriever.RetrieveAsync(
            Arg.Any<Action<DocumentQuery<TreeNode>>>(),
            Arg.Any<Action<IPageCacheBuilder<TreeNode>>>(),
            Arg.Any<CancellationToken>()).Returns(new TreeNode[] { });

        var pageUrlRetriever = Substitute.For<IPageUrlRetriever>();
        var log = Substitute.For<IEventLogService>();

        var tagHelper = new PageLinkTagHelper(pageRetriever, pageUrlRetriever, log)
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
            (result, encoder) =>
            {
                return Task.FromResult<TagHelperContent>(new DefaultTagHelperContent());
            });

        await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

        tagHelperOutput.TagName.Should().Be("a");
        tagHelperOutput.Attributes.FirstOrDefault(a => a.Name == "href")?.Value.Should().Be(href);
        tagHelperOutput.IsContentModified.Should().BeFalse();

        log.Received(1).LogEvent(Arg.Any<EventLogData>());
    }

    [Test]
    public async Task Process_Will_Not_Modify_The_Target_Anchor_When_No_PageUrl_Is_Retrieved()
    {
        var fixture = new Fixture();
        string href = fixture.Create<string>();

        var node = TreeNode.New<TreeNode>().With(a => a.DocumentName = "Test");

        var page = Substitute.For<ILinkablePage>();
        var pageRetriever = Substitute.For<IPageRetriever>();
        pageRetriever.RetrieveAsync(
            Arg.Any<Action<DocumentQuery<TreeNode>>>(),
            Arg.Any<Action<IPageCacheBuilder<TreeNode>>>(),
            Arg.Any<CancellationToken>()).Returns(new[] { node });

        var pageUrlRetriever = Substitute.For<IPageUrlRetriever>();
        pageUrlRetriever.Retrieve(Arg.Is(node)).Throws(new Exception("oops"));

        var log = Substitute.For<IEventLogService>();

        var tagHelper = new PageLinkTagHelper(pageRetriever, pageUrlRetriever, log)
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
            (result, encoder) =>
            {
                return Task.FromResult<TagHelperContent>(new DefaultTagHelperContent());
            });

        await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

        tagHelperOutput.TagName.Should().Be("a");
        tagHelperOutput.Attributes.FirstOrDefault(a => a.Name == "href")?.Value.Should().Be(href);
        tagHelperOutput.IsContentModified.Should().BeFalse();

        log.Received(1).LogEvent(Arg.Any<EventLogData>());
    }
}
