# Xperience Page Link Tag Helpers

[![GitHub Actions CI: Build](https://github.com/wiredviews/xperience-page-link-tag-helpers/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/wiredviews/xperience-page-link-tag-helpers/actions/workflows/ci.yml)

[![Publish Packages to NuGet](https://github.com/wiredviews/xperience-page-link-tag-helpers/actions/workflows/publish.yml/badge.svg?branch=main)](https://github.com/wiredviews/xperience-page-link-tag-helpers/actions/workflows/publish.yml)

## Packages

### LinkablePages

[![NuGet Package](https://img.shields.io/nuget/v/XperienceCommunity.LinkablePages.svg)](https://www.nuget.org/packages/XperienceCommunity.LinkablePages)

Kentico Xperience 13.0 custom module to protect pages referenced in code from deletion and shared abstractions for linkable pages.

This package is compatible with .NET Standard 2.0 libraries integrated with Kentico Xperience 13.0.

### PageLinkTagHelpers

[![NuGet Package](https://img.shields.io/nuget/v/XperienceCommunity.PageLinkTagHelpers.svg)](https://www.nuget.org/packages/XperienceCommunity.PageLinkTagHelpers)

Kentico Xperience 13.0 ASP.NET Core Tag Helper that generates links to predefined pages using their NodeGUID values, and extension methods for registering dependencies in ASP.NET Core.

This package is compatible with ASP.NET Core 3.1+ applications or libraries integrated with Kentico Xperience 13.0.

## How to Use?

1. Install the `XperienceCommunity.LinkablePages` NuGet package in a class library shared by your ASP.NET Core and CMS applications:

   ```bash
   dotnet add package XperienceCommunity.LinkablePages
   ```

1. In the shared class library, create a class implementing the `ILinkablePage` where you define pages that are available to the tag helper:

   > Populate the `Guid` values with the `NodeGUID` of each page in the content tree that you need to link to in your application.

   ```csharp
   public namespace Sandbox.Shared
   {
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
   }
   ```

1. In the shared class library, create an implementation of the `ILinkablePageInventory` interface, which will be used to determine which Pages in the application should be protected:

   ```csharp
   public class LinkablePageInventory : ILinkablePageInventory
   {
       public bool IsLinkablePage(TreeNode page)
       {
           return LinkablePage.All.Any(p => p.NodeGUID == page.NodeGUID);
       }
   }
   ```

1. Install the `XperienceCommunity.PageLinkTagHelpers` NuGet package in your ASP.NET Core project:

   ```bash
   dotnet add package XperienceCommunity.PageLinkTagHelpers
   ```

1. Add the `@addTagHelper` directive to your `_ViewImports.cshtml` file:

   > (optional) Add your `LinkablePage` class's namespace to your `_ViewImports.cshtml` file (e.g., `Sandbox.Shared`).

   ```razor
   // Add this using to make LinkablePages easy to access in Views
   @using Sandbox.Shared

   @addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
   @addTagHelper *, Kentico.Content.Web.Mvc
   @addTagHelper *, Kentico.Web.Mvc
   @addTagHelper *, DancingGoatCore

   // Add this directive to use the Tag Helper
   @addTagHelper *, XperienceCommunity.PageLinkTagHelpers
   ```

1. Register the library with ASP.NET Core DI:

   ```csharp
   public void ConfigureServices(IServiceCollection services)
   {
       // Use standard registration
       services
         .AddXperienceCommunityPageLinks()
         .AddXperienceCommunityPageLinksProtection<LinkablePageInventory>();

       // or use a custom implementation of ILinkablePageLinkRetriever
       services
         .AddXperienceCommunityPageLinks<MyCustomLinkablePageLinkRetriever>()
         .AddXperienceCommunityPageLinksProtection<LinkablePageInventory>();
   }
   ```

1. Add the data protection custom module registration to your ASP.NET Core application (in `Startup.cs` or wherever you register your dependencies):

   ```csharp
   [assembly: RegisterModule(typeof(LinkablePageProtectionModule))]
   ```

1. Use the `xp-page-link` tag helper in an `<a>` element in a Razor View:

   ```html
   <a href="" xp-page-link="LinkablePage.Home">
     <img
       src="/getmedia/10d5e094-d9aa-4edf-940d-098ca69b5f77/logo.png"
       alt="..."
     />
   </a>
   ```

1. Create a custom module class in your CMS application to register the `LinkablePageProtectionModule` and the `ILinkablePageInventory` implementation:

   ```csharp
   // Registers this custom module class
   [assembly: RegisterModule(typeof(DependencyRegistrationModule))]

   // Registers the library's custom module class
   [assembly: RegisterModule(typeof(LinkablePageProtectionModule))]

   namespace CMSApp
   {
       public class DependencyRegistrationModule : Module
       {
           public DependencyRegistrationModule() : base(nameof(DependencyRegistrationModule))
           {

           }

           protected override void OnPreInit()
           {
               base.OnPreInit();

               // Registers the ILinkablePageInventory implementation used by the LinkablePageProtectionModule
               Service.Use<ILinkablePageInventory, LinkablePageInventory>();
           }
       }
   }
   ```

1. Use Kentico Xperience's [Content Staging](https://docs.xperience.io/x/cgeRBg) to keep your `LinkablePage` `NodeGUID` values valid between different environments.

## Usage

### Simple

Razor (assuming the ContactUs Page has a `DocumentName` of "Contact Us"):

```html
<a href="" xp-page-link="LinkablePage.ContactUs"></a>
```

Generated HTML will set the child content, `href` and `title` attributes:

```html
<a href="/contact-us" title="Contact Us">Contact Us</a>
```

### Custom Content

Razor (assuming the ContactUs Page has a `DocumentName` of "Contact Us"):

```html
<a href="" title="Please contact us" xp-page-link="LinkablePage.ContactUs">
  <img src="..." />
</a>
```

Generated HTML will keep the child content and only set the `href` and `title` attributes:

```html
<a href="/contact-us" title="Please contact us">
  <img src="..." />
</a>
```

### Empty Title Attribute with Child Content

Razor:

```html
<a href="" title="" xp-page-link="LinkablePage.ContactUs">
  No title necessary!
</a>
```

Generated HTML will not populate the `title` attribute, assuming the child content provides some text:

```html
<a href="/contact-us" title=""> No title necessary! </a>
```

### Empty Title Attribute with No Child Content

Razor (assuming the ContactUs Page has a `DocumentName` of "Contact Us"):

```html
<a href="" title="" xp-page-link="LinkablePage.ContactUs"> </a>
```

Generated HTML will populate the `title` for accessibility:

```html
<a href="/contact-us" title="Contact Us"> Contact Us </a>
```

### Query String Parameters

Razor:

```html
<a
  href=""
  xp-page-link="LinkablePage.ContactUs"
  title="Please contact us"
  xp-page-link-query-params="@(new() { { "parameter": "value" } })">
    Contact us for help!
</a>
```

Generated HTML will include query string parameters in the `href`, and set the `title` attribute/child content as appropriate:

```html
<a href="/contact-us?parameter=value" title="Please contact us">
   Contact us for help!
</a>
```

## Contributions

If you discover a problem, please [open an issue](https://github.com/wiredviews/xperience-page-link-tag-helpers/issues/new).

If you would like contribute to the code or documentation, please [open a pull request](https://github.com/wiredviews/xperience-page-link-tag-helpers/compare).

## References

### .NET

- [ASP.NET Core Tag Helper](https://docs.microsoft.com/en-US/aspnet/core/mvc/views/tag-helpers/intro?view=aspnetcore-6.0)

### Kentico Xperience

- [Retrieving Pages](https://docs.xperience.io/x/vwyRBg#WorkingwithpagesintheAPI-Retrievingpagesonthelivesite)
- [Displaying Page URLs](https://docs.xperience.io/x/Kw2RBg#Displayingpagecontent-GettingpageURLs)
- [Document Events](https://docs.xperience.io/x/2AyRBg#ReferenceGlobalsystemevents-DocumentEvents)
