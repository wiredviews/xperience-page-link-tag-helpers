using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using XperienceCommunity.LinkablePages;

namespace XperienceCommunity.PageLinkTagHelpers
{
    public static class ServiceCollectionLinkablePageExtensions
    {
        /// <summary>
        /// Adds the default link generation types to DI.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddXperienceCommunityPageLinks(this IServiceCollection services)
        {
            services.TryAddTransient<ILinkablePageLinkRetriever, DefaultLinkablePageLinkRetriever>();

            return services;
        }

        /// <summary>
        /// Adds the custom <typeparamref name="TCustomLinkablePageLinkRetriever" /> type as the implementation
        /// for <see cref="ILinkablePageLinkRetriever" /> for link generation to DI.
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="TCustomLinkablePageLinkRetriever"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddXperienceCommunityPageLinks<TCustomLinkablePageLinkRetriever>(this IServiceCollection services)
            where TCustomLinkablePageLinkRetriever : class, ILinkablePageLinkRetriever
        {
            services.TryAddTransient<ILinkablePageLinkRetriever, TCustomLinkablePageLinkRetriever>();

            return services;
        }

        /// <summary>
        /// Adds the custom <typeparamref name="TCustomLinkablePageInventory" /> type as the implementation
        /// for <see cref="ILinkablePageInventory" /> for link protection to DI.
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="TCustomLinkablePageInventory"></typeparam>
        /// <returns></returns>
        public static IServiceCollection AddXperienceCommunityPageLinksProtection<TCustomLinkablePageInventory>(this IServiceCollection services)
            where TCustomLinkablePageInventory : class, ILinkablePageInventory
        {
            services.TryAddTransient<ILinkablePageInventory, TCustomLinkablePageInventory>();

            return services;
        }
    }
}
