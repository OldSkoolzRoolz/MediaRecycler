#region Header

// Project Name: MediaRecycler
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers

#endregion



using System.ComponentModel.DataAnnotations;
using System.Configuration;



namespace MediaRecycler.Modules.Options;


/// <summary>
///     Contains the configurable options for scrapers.
///     This class centralizes all settings that control scraper behavior, selectors, and timeouts.
/// </summary>
public class Scraping : ApplicationSettingsBase
{



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
    [DefaultSettingValue("")]
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
    ///     This CSS selector should be set to grab a property of a single element.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("")]
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
    ///     Chrome browser default directory for storing user data.
    ///     This is used by Puppeteer for browser automation tasks. and should be set to a valid path.
    ///     It will store cookies and other data to avoid logging in every time.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("d:\\chromeuserdata")]
    public string? UserDataDir { get; set; }

}
