using Syncfusion.Blazor;

namespace Amusing;
   
public class SyncfusionLocalizer : ISyncfusionStringLocalizer
{
    public string GetText(string key)
    {
        // Return an empty string if the resource is not found to avoid null return.
        return ResourceManager.GetString(key) ?? string.Empty;
    }

    public System.Resources.ResourceManager ResourceManager
    {
        get
        {
            // Replace the ApplicationNamespace with your application name.
            return Amusing.Resources.SfResources.ResourceManager;

            //For .Net Maui Blazor App
            // Replace the ApplicationNamespace with your application name.
            //return ApplicationNamespace.LocalizationResources.SfResources.ResourceManager;
        }
    }
}