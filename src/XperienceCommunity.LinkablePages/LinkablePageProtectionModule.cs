using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;

namespace XperienceCommunity.LinkablePages
{
    /// <summary>
    /// Protects <see cref="ILinkablePage"/> instances that represent Pages in the content tree with hard coded <see cref="TreeNode.NodeGUID"/> values.
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
            var inventory = Service.Resolve<ILinkablePageInventory>();

            if (inventory.IsLinkablePage(e.Node))
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
