namespace Sitecore.Support.Workflows.Simple
{
  using Data;
  using Configuration;
  using Data.Items;
  using Diagnostics;
  using Publishing;
  using SecurityModel;
  using Web;
  using System;
  using System.Collections;
  using global::Sitecore.Workflows.Simple;

  public class PublishAction
  {
    private bool GetDeep(Item actionItem)
    {
      if (actionItem["deep"] != "1")
      {
        return (WebUtil.ParseUrlParameters(actionItem["parameters"])["deep"] == "1");
      }
      return true;
    }

    protected bool GetParents(Item actionItem)
    {
      if (actionItem["parents"] != "1")
      {
        return (WebUtil.ParseUrlParameters(actionItem["parameters"])["parents"] == "1");
      }
      return true;
    }

    private Database[] GetTargets(Item item)
    {
      using (new SecurityDisabler())
      {
        string[] strArray = Settings.GetSetting("ECM.PublishOnDispatchTo", "web").Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        ArrayList list = new ArrayList();
        foreach (string str in strArray)
        {
          if (str.Length > 0)
          {
            Database database = Factory.GetDatabase(str, false);
            if (database != null)
            {
              list.Add(database);
            }
            else
            {
              Log.Warn("Unknown database in PublishAction: " + str, this);
            }
          }
        }
        return (list.ToArray(typeof(Database)) as Database[]);
      }
    }

    public void Process(WorkflowPipelineArgs args)
    {
      Item dataItem = args.DataItem;
      Item innerItem = args.ProcessorItem.InnerItem;
      Database[] targets = this.GetTargets(dataItem);
      if (this.GetParents(innerItem))
      {
        foreach (Database database in targets)
        {
          this.PublishUnpublishedParentItem(database, dataItem.Parent);
        }
      }
      foreach (Database database2 in targets)
      {
        PublishManager.PublishItem(dataItem, new Database[] { database2 }, dataItem.Languages, this.GetDeep(innerItem), false);
      }
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
