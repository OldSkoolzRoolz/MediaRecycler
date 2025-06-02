// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Text.Json;



namespace MediaRecycler.Modules.Options;


/// <summary>
///     Contains the configurable options for scrapers.
///     This class centralizes all settings that control scraper behavior, selectors, and timeouts.
/// </summary>
public class Scraping
{



    private static readonly string SettingsFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MediaRecycler", "scraping_settings.json");

    private static Scraping? _defaultInstance;

    public static Scraping Default
    {
        get
        {
            _defaultInstance ??= Load();

            return _defaultInstance;
        }
    }



    /// <summary>
    ///     The default timeout (in milliseconds) for general scraper operations such as HTTP requests.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("30000")]
    public int DefaultTimeout { get; set; }

    /// <summary>
    ///     The default timeout (in milliseconds) for Puppeteer-based browser automation tasks.
    /// </summary>

    [UserScopedSetting]
    [DefaultSettingValue("30000")]
    public int DefaultPuppeteerTimeout { get; set; }

    /// <summary>
    ///     IF set this will be appended to the starting url to go to a sub page of a site.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("")]
    public string? ArchivePageUrlSuffix { get; set; }

    /// <summary>
    ///     The CSS selector used to locate the "next" button or link in paginated content.
    ///     Used for navigating through multiple pages of results.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("")]
    public string? PaginationSelector { get; set; }



    /// <summary>
    ///     This CSS selector is the first one used to generally select the outermost container to separate
    ///     the targets into an array of elements. This will return an array of elements.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("div.dayholder > div.searchpost > a:nth-child(1)")]
    public string? GroupingSelector { get; set; }


    /// <summary>
    ///     This CSS selector should be set to grab the element the you are interested in getting data from.
    ///     It could be a video tag or an image tag. The selector should be as specific as possible and should
    ///     return a single element. NOT a property of an element.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("")]
    public string? TargetElementSelector { get; set; }


    /// <summary>
    ///     This name of the property we are trying to extract.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("href")]
    public string? TargetPropertySelector { get; set; }



    /// <summary>
    ///     When set to true, the downloader will be activated once the scraper has completed.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("false")]
    public bool StartDownloader { get; set; }


    /// <summary>
    ///     The URL of the starting web page for the scraper.
    /// </summary>
    [UserScopedSetting]
    [Required]
    public string? StartingWebPage { get; set; }

    /// <summary>
    ///     Selector for the individual post page URL.
    /// </summary>
    [UserScopedSetting]
    [Required]
    public string SinglePostPageUrl
    {
        get;
        set;
    }






    public void Save()
    {
        var dir = Path.GetDirectoryName(SettingsFilePath);

        if (!Directory.Exists(dir))
        {
            _ = Directory.CreateDirectory(dir!);
        }

        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsFilePath, json);
    }






    public static Scraping Load()
    {
        if (File.Exists(SettingsFilePath))
        {
            var json = File.ReadAllText(SettingsFilePath);
            var loaded = JsonSerializer.Deserialize<Scraping>(json);

            if (loaded != null)
            {
                return loaded;
            }
        }

        return new Scraping();
    }






    public void Reload()
    {
        var loaded = Load();

        // Copy properties from loaded to this
        DefaultTimeout = loaded.DefaultTimeout;
        DefaultPuppeteerTimeout = loaded.DefaultPuppeteerTimeout;
        ArchivePageUrlSuffix = loaded.ArchivePageUrlSuffix;
        PaginationSelector = loaded.PaginationSelector;
        GroupingSelector = loaded.GroupingSelector;
        TargetElementSelector = loaded.TargetElementSelector;
        TargetPropertySelector = loaded.TargetPropertySelector;
        StartDownloader = loaded.StartDownloader;
        StartingWebPage = loaded.StartingWebPage;

    }

}
