namespace Sitecore.Support.Modules.EmailCampaign.Core.Pipelines.DispatchNewsletter
{
  using Configuration;
  using Data;
  using Data.Items;
  using Diagnostics;
  using global::Sitecore.Modules.EmailCampaign.Core.Pipelines.DispatchNewsletter;
  using Publishing;
  using SecurityModel;
  using System;

  public class PublishMessage
  {
    public void Process(DispatchNewsletterArgs args)
    {
      Log.Debug("Message autopublish processor started", this);
      DateTime now = DateTime.Now;
      if (!args.IsTestSend && !args.DedicatedInstance)
      {
        using (new SecurityDisabler())
        {
          Item innerItem = args.Message.InnerItem;
          foreach (string str in Settings.GetSetting("ECM.PublishOnDispatchTo", "web").Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
          {
            Database targetDatabase = Factory.GetDatabase(str);
            this.PublishUnpublishedParentItem(targetDatabase, innerItem);
            PublishManager.PublishItem(innerItem, new Database[] { targetDatabase }, innerItem.Languages, true, false);
          }
        }
      }
      TimeSpan span = (TimeSpan)(DateTime.Now - now);
      Log.Debug("Message autopublish processor finished. Time elapsed " + span.TotalMilliseconds + " ms", this);
    }

    protected void PublishUnpublishedParentItem(Database targetDatabase, Item parentItem)
    {
      if (targetDatabase.GetItem(parentItem.ID) == null)
      {
        this.PublishUnpublishedParentItem(targetDatabase, parentItem.Parent);
        PublishManager.PublishItem(parentItem, new Database[] { targetDatabase }, parentItem.Languages, false, false);
      }
    }
  }
}
