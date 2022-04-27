# Xperience Page Link Tag Helpers

[![GitHub Actions CI: Build](https://github.com/wiredviews/xperience-page-link-tag-helpers/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/wiredviews/xperience-page-link-tag-helpers/actions/workflows/ci.yml)

[![Publish Packages to NuGet](https://github.com/wiredviews/xperience-page-link-tag-helpers/actions/workflows/publish.yml/badge.svg?branch=main)](https://github.com/wiredviews/xperience-page-link-tag-helpers/actions/workflows/publish.yml)

[![NuGet Package](https://img.shields.io/nuget/v/XperienceCommunity.PageLinkTagHelpers.svg)](https://www.nuget.org/packages/XperienceCommunity.PageLinkTagHelpers)

Kentico Xperience 13.0 ASP.NET Core Tag Helpers that generates links to pages from NodeGUID values

## Dependencies

This package is compatible with ASP.NET Core 6 applications or libraries integrated with Kentico Xperience 13.0.

## How to Use?

1. Install the NuGet package in your ASP.NET Core project (or class library)

   ```bash
   dotnet add package XperienceCommunity.PageLinkTagHelpers
   ```

1. Add the correct `@addTagHelper` directive to your `_ViewImports.cshtml` file:

   `@addTagHelper *, XperienceCommunity.PageLinkTagHelpers`

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

1. (optional) Add your `LinkablePage` class's namespace to your `_ViewImports.cshtml` file:

## Usage

```html
<a
  href=""
  xp-page-link="LinkablePage.ContactUs"
  xp-page-link-text="Contact us for help!"
  xp-page-link-query-params='new NameValueCollection { { "a": "b" } }'
></a>
```

This will generate the following HTML:

```html
<a href="/contact-us?a=b">Contactus for help!</a>
```

## References

### .NET

- [ASP.NET Core Tag Helper](https://docs.microsoft.com/en-US/aspnet/core/mvc/views/tag-helpers/intro?view=aspnetcore-6.0)

### Kentico Xperience
