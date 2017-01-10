namespace Sitecore.Support.Hooks
{
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Diagnostics;
  using Sitecore.Events.Hooks;
  using Sitecore.Globalization;
  using Sitecore.SecurityModel;

  public class AddingECMAutopublish : IHook
  {
    [UsedImplicitly]
    public AddingECMAutopublish()
    {
    }

    public void Initialize()
    {
      using (new SecurityDisabler())
      {
        //Thread.Sleep(60000);
        var databaseName = "master";
        var parentItemPath = "/sitecore/system/Workflows/Analytics Workflow/Deployed";
        var typeFieldName = "Type";
        var paramFieldName = "Parameters";

        // protects from refactoring-related mistakes
        var type = typeof(Sitecore.Support.Workflows.Simple.PublishAction);

        var typeName = type.FullName;
        var assemblyName = type.Assembly.GetName().Name;
        var typeFieldValue = $"{typeName}, {assemblyName}";

        var paramString = "deep=1&parents=1";

        var database = Factory.GetDatabase(databaseName);
        var parentItem = database.GetItem(parentItemPath);

        if (parentItem == null)
        {
          // no analytics installed
          return;
        }


        if (database.GetItem(parentItemPath + "/auto publish") != null)
        {
          // already installed
          return;
        }

        Log.Info($"Installing {assemblyName}", this);

        var item = parentItem.Add("auto publish", new TemplateID(new ID("{66882E97-C8AA-4E37-8901-7A8AA35ED2ED}")));

        item.Editing.BeginEdit();
        item[typeFieldName] = typeFieldValue;
        item[paramFieldName] = paramString;
        item.Editing.EndEdit();

        // Creating language versions:
        var target = database.GetItem(item.ID, Language.Parse("da"));
        target.Editing.BeginEdit();
        target.Versions.AddVersion();
        target.Editing.EndEdit();
        target = database.GetItem(item.ID, Language.Parse("ja-JP"));
        target.Editing.BeginEdit();
        target.Versions.AddVersion();
        target.Editing.EndEdit();
        target = database.GetItem(item.ID, Language.Parse("de-DE"));
        target.Editing.BeginEdit();
        target.Versions.AddVersion();
        target.Editing.EndEdit();
      }
    }
  }
}