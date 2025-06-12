// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"




using System.ComponentModel.DataAnnotations;
using System.Configuration;



namespace MediaRecycler.Modules.Options;


/// <summary>
///     Contains the configurable options for scrapers.
///     This class centralizes all settings that control scraper behavior, selectors, and timeouts.
/// </summary>
[SettingsGroupName("ScrapingOptions")]
[SettingsProvider(typeof(LocalFileSettingsProvider))]
public class ScrapingOptions : ApplicationSettingsBase
{

    /// <summary>
    ///     The default timeout (in milliseconds) for general scraper operations such as HTTP requests.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("30000")]
    public int DefaultTimeout
    {
        get => (int)this[nameof(DefaultTimeout)];
        set => this[nameof(DefaultTimeout)] = value;
    }

    /// <summary>
    ///     The default timeout (in milliseconds) for Puppeteer-based browser automation tasks.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("30000")]
    public int DefaultPuppeteerTimeout
    {
        get => (int)this[nameof(DefaultPuppeteerTimeout)];
        set => this[nameof(DefaultPuppeteerTimeout)] = value;
    }

    /// <summary>
    ///     IF set this will be appended to the starting url to go to a sub page of a site.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue(DefaultArchivePageUrlSuffix)]
    public string ArchivePageUrlSuffix
    {
        get => (string)this[nameof(ArchivePageUrlSuffix)];
        set => this[nameof(ArchivePageUrlSuffix)] = value ?? string.Empty;
    }

    /// <summary>
    ///     The CSS selector used to locate the "next" button or link in paginated content.
    ///     Used for navigating through multiple pages of results.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue(DefaultPaginationSelector)]
    public string PaginationSelector
    {
        get => (string)this[nameof(PaginationSelector)];
        set => this[nameof(PaginationSelector)] = value ?? string.Empty;
    }

    /// <summary>
    ///     This CSS selector is the first one used to generally select the outermost container to separate
    ///     the targets into an array of elements. This will return an array of elements.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue(DefaultGroupingSelector)]
    public string GroupingSelector
    {
        get => (string)this[nameof(GroupingSelector)];
        set => this[nameof(GroupingSelector)] = value ?? string.Empty;
    }

    /// <summary>
    ///     This CSS selector should be set to grab the element the you are interested in getting data from.
    ///     It could be a video tag or an image tag. The selector should be as specific as possible and should
    ///     return a single element. NOT a property of an element.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue(DefaultTargetElementSelector)]
    public string TargetElementSelector
    {
        get => (string)this[nameof(TargetElementSelector)];
        set => this[nameof(TargetElementSelector)] = value ?? string.Empty;
    }

    /// <summary>
    ///     This name of the property we are trying to extract.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue(DefaultTargetPropertySelector)]
    public string TargetPropertySelector
    {
        get => (string)this[nameof(TargetPropertySelector)];
        set => this[nameof(TargetPropertySelector)] = value ?? string.Empty;
    }

    /// <summary>
    ///     When set to true, the downloader will be activated once the scraper has completed.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("false")]
    public bool StartDownloader
    {
        get => (bool)this[nameof(StartDownloader)];
        set => this[nameof(StartDownloader)] = value;
    }

    /// <summary>
    ///     The URL of the starting web page for the scraper.
    /// </summary>
    [UserScopedSetting]
    [Required]
    public string StartingWebPage
    {
        get => (string)this[nameof(StartingWebPage)];
        set => this[nameof(StartingWebPage)] = value ?? string.Empty;
    }

    /// <summary>
    ///     Selector for the individual post page URL.
    /// </summary>
    [UserScopedSetting]
    [DefaultSettingValue("")]
    public string SinglePostPageUrl
    {
        get => (string)this[nameof(SinglePostPageUrl)];
        set => this[nameof(SinglePostPageUrl)] = value ?? string.Empty;
    }

    /// <summary>
    ///     Gets the default instance of the <see cref="ScrapingOptions" /> class.
    ///     This property provides a thread-safe, synchronized instance of <see cref="ScrapingOptions" />
    ///     that can be used to access and modify scraping-related configuration settings.
    /// </summary>
    public static ScrapingOptions Default { get; } = (ScrapingOptions)Synchronized(new ScrapingOptions());

    // Constants for default values
    private const string DefaultArchivePageUrlSuffix = "archive/4";
    private const string DefaultGroupingSelector = "div.dayholder > div.searchpost > a:nth-child(1)";
    private const string DefaultPaginationSelector = "ul.pagination a[rel='next']";
    private const string DefaultTargetElementSelector = "";
    private const string DefaultTargetPropertySelector = "href";






    /// <summary>
    ///     Saves the current scraper settings to the persistent storage.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the settings fail to save due to a configuration error.
    /// </exception>
    public void SaveSettings()
    {

        try
        {
            Validate();
            Save();
        }
        catch (ConfigurationErrorsException e)
        {
            throw new InvalidOperationException("Failed to save Settings.", e);
        }
    }






    /// <summary>
    ///     Validates the current configuration of the <see cref="ScrapingOptions" /> instance.
    ///     Ensures that all required properties are properly set and meet their constraints.
    /// </summary>
    /// <exception cref="ValidationException">
    ///     Thrown when one or more properties of the <see cref="ScrapingOptions" /> instance
    ///     do not meet their validation requirements.
    /// </exception>
    public void Validate()
    {
        // Validate DefaultTimeout
        if (DefaultTimeout <= 0)
        {
            throw new ValidationException("DefaultTimeout must be greater than zero.");
        }

        // Validate DefaultPuppeteerTimeout
        if (DefaultPuppeteerTimeout <= 0)
        {
            throw new ValidationException("DefaultPuppeteerTimeout must be greater than zero.");
        }

        // Validate ArchivePageUrlSuffix (allow empty, but not null)
        ValidateString(ArchivePageUrlSuffix, nameof(ArchivePageUrlSuffix));

        // Validate PaginationSelector (allow empty, but not null)
        ValidateString(PaginationSelector, nameof(PaginationSelector));

        // Validate GroupingSelector (allow empty, but not null)
        ValidateString(GroupingSelector, nameof(GroupingSelector));

        // Validate TargetElementSelector (allow empty, but not null)
        ValidateString(TargetElementSelector, nameof(TargetElementSelector));

        // Validate TargetPropertySelector (allow empty, but not null)
        ValidateString(TargetPropertySelector, nameof(TargetPropertySelector));

        // Validate StartDownloader (bool, no validation needed)

        // Validate StartingWebPage (required, not null or empty)
        ValidateString(StartingWebPage, nameof(StartingWebPage), false);

        // Validate SinglePostPageUrl (allow empty, but not null)
        ValidateString(SinglePostPageUrl, nameof(SinglePostPageUrl));
    }






    private void ValidateString(string value, string propertyName, bool allowEmpty = true)
    {
        if (value == null)
        {
            throw new ValidationException($"{propertyName} cannot be null.");
        }

        if (!allowEmpty && string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{propertyName} is required and cannot be empty.");
        }
    }

}
