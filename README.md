# Xperience Page Link Tag Helpers

[![GitHub Actions CI: Build](https://github.com/wiredviews/xperience-page-link-tag-helpers/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/wiredviews/xperience-page-link-tag-helpers/actions/workflows/ci.yml)

[![Publish Packages to NuGet](https://github.com/wiredviews/xperience-page-link-tag-helpers/actions/workflows/publish.yml/badge.svg?branch=main)](https://github.com/wiredviews/xperience-page-link-tag-helpers/actions/workflows/publish.yml)

[![NuGet Package](https://img.shields.io/nuget/v/XperienceCommunity.PageLinkTagHelpers.svg)](https://www.nuget.org/packages/XperienceCommunity.PageLinkTagHelpers)

Kentico Xperience 13.0 ASP.NET Core Tag Helpers that generates links to pages from NodeGUID values

## Dependencies

This package is compatible with ASP.NET Core 3.1+ applications or libraries integrated with Kentico Xperience 13.0.

## How to Use?

1. Install the NuGet package in your ASP.NET Core project (or class library)

   ```bash
   dotnet add package XperienceCommunity.PageLinkTagHelpers
   ```

1. Add the correct `@addTagHelper` directive to your `_ViewImports.cshtml` file:

   ```csharp
    @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
    @addTagHelper *, Kentico.Content.Web.Mvc
    @addTagHelper *, Kentico.Web.Mvc
    @addTagHelper *, DancingGoatCore

    @addTagHelper *, XperienceCommunity.PageLinkTagHelpers
   ```

1. Create an implementation of `ILinkablePage`:

   ```csharp
   public class LinkablePage : ILinkablePage
   {
       public static LinkablePage Home { get; } = new LinkablePage(new Guid("..."));
       public static LinkablePage Store { get; } = new LinkablePage(new Guid("..."));
       public static LinkablePage ContactUs { get; } = new LinkablePage(new Guid("..."));
       public static LinkablePage TermsOfUse { get; } = new LinkablePage(new Guid("..."));

       public Guid NodeGUID { get; }

       protected LinkablePage(Guid nodeGUID) => NodeGUID = nodeGUID;

       public static IReadOnlyList<LinkablePage> All { get; } = new List<LinkablePage>
       {
           Home,
           Store,
           ContactUs,
           TermsOfUse
       };
   }
   ```

1. Register the library with ASP.NET Core DI:

   ```csharp
   public void ConfigureServices(IServiceCollection services)
   {
       // Use default implementations
       services.AddXperienceCommunityPageLinks();

       // or use a custom implementation
       services.AddXperienceCommunityPageLinks<MyCustomLinkablePageLinkRetriever>();
   }
   ```

1. (optional) Add your `LinkablePage` class's namespace to your `_ViewImports.cshtml` file.

1. Use the `xp-page-link` tag helper on an `<a>` element in a Razor View:

   ```html
   <a href="" xp-page-link="LinkablePage.Home">
     <img
       src="/getmedia/10d5e094-d9aa-4edf-940d-098ca69b5f77/logo.png"
       alt="..."
     />
   </a>
   ```

1. (recommended) Create a global event handler to protect the Pages referenced by your `ILinkablePage` implementation:

   ```csharp
   using System;
   using System.Linq;
   using CMS;
   using CMS.Core;
   using CMS.DataEngine;
   using CMS.DocumentEngine;

   [assembly: RegisterModule(typeof(LinkablePageProtectionModule))]

   namespace Sandbox
   {
       /// <summary>
       /// Protects <see cref="LinkablePage"/> instances that represent Pages in the content tree with hard coded <see cref="TreeNode.NodeGUID"/> values.
       /// </summary>
       public class LinkablePageProtectionModule : Module
       {
           public LinkablePageProtectionModule() : base(nameof(LinkablePageProtectionModule)) { }

           protected override void OnInit()
           {
               base.OnInit();

               DocumentEvents.Delete.Before += Delete_Before;
           }

           private void Delete_Before(object sender, DocumentEventArgs e)
           {
               if (LinkablePage.All.Any(p => p.NodeGuid == e.Node.NodeGUID))
               {
                   e.Cancel();

                   var log = Service.Resolve<IEventLogService>();

                   log.LogError(
                       nameof(LinkablePageProtectionModule),
                       "DELETE_PAGE",
                       $"Cannot delete Linkable Page [{e.Node.NodeAliasPath}], as it might be in use. Please first remove the Linkable Page in the application code and re-deploy the application.");
               }
           }
       }
   }
   ```

## Usage

### Simple

Razor (assuming the ContactUs Page has a `DocumentName` of "Contact Us"):

```html
<a
  href=""
  xp-page-link="LinkablePage.ContactUs"></a>
```

Generated HTML will set the child content, `href` and `title` attributes:

```html
<a href="/contact-us" title="Contact Us">Contact Us</a>
```

### Custom Content

Razor (assuming the ContactUs Page has a `DocumentName` of "Contact Us"):

```html
<a
  href=""
  title="Please contact us"
  xp-page-link="LinkablePage.ContactUs">
    <img src="...">
</a>
```

Generated HTML will keep the child content and only set the `href` and `title` attributes:

```html
<a href="/contact-us" title="Please contact us">
    <img src="...">
</a>
```

### Empty Title Attribute with Child Content

Razor (assuming the ContactUs Page has a `DocumentName` of "Contact Us"):

```html
<a
  href=""
  title=""
  xp-page-link="LinkablePage.ContactUs">
    No title necessary!
</a>
```

Generated HTML will not populate the `title` attribute, assuming the child content provides some text:

```html
<a href="/contact-us" title="">
    No title necessary!
</a>
```

### Empty Title Attribute with No Child Content

Razor (assuming the ContactUs Page has a `DocumentName` of "Contact Us"):

```html
<a
  href=""
  title=""
  xp-page-link="LinkablePage.ContactUs">
</a>
```

Generated HTML will populate the `title` for accessibility:

```html
<a href="/contact-us" title="Contact Us">
    Contact Us
</a>
```

### Query String Parameters

Razor:

```html
<a
  href=""
  xp-page-link="LinkablePage.ContactUs"
  title="Please contact us"
  xp-page-link-query-params="@(new NameValueCollection { { "a": "b" } })">
    Contact us for help!
  </a>
```

Generated HTML will include query string parameters in the `href`, and set the `title` attribute/child content as appropriate:

```html
<a href="/contact-us?a=b" title="Please contact us">Contact us for help!</a>
```

## References

### .NET

- [ASP.NET Core Tag Helper](https://docs.microsoft.com/en-US/aspnet/core/mvc/views/tag-helpers/intro?view=aspnetcore-6.0)

### Kentico Xperience

- [Retrieving Pages](https://docs.xperience.io/custom-development/working-with-pages-in-the-api#WorkingwithpagesintheAPI-Retrievingpagesonthelivesite)
- [Displaying Page URLs](https://docs.xperience.io/developing-websites/retrieving-content/displaying-page-content#Displayingpagecontent-GettingpageURLs)
- [Document Events](https://docs.xperience.io/custom-development/handling-global-events/reference-global-system-events#ReferenceGlobalsystemevents-DocumentEvents)
