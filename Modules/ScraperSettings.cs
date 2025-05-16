// "Open Source copyrights apply - All code can be reused DO NOT remove author tags"
using System.ComponentModel.DataAnnotations;
namespace Scrapper;


/// <summary>
/// Contains the configurable options for scrapers.
/// This class centralizes all settings that control scraper behavior, selectors, and timeouts.
/// </summary>
public class ScraperSettings
{
    /// <summary>
    /// The default timeout (in milliseconds) for general scraper operations such as HTTP requests.
    /// </summary>
    public int DefaultTimeout { get; set; }

    /// <summary>
    /// The default timeout (in milliseconds) for Puppeteer-based browser automation tasks.
    /// </summary>
    public int DefaultPuppeteerTimeout { get; set; }

    /// <summary>
    /// The URL suffix appended to video archive links to access archived video content.
    /// </summary>
    public string VideoArchiveSuffix { get; set; }

    /// <summary>
    /// The CSS selector used to locate the "next" button or link in paginated content.
    /// Used for navigating through multiple pages of results.
    /// </summary>
    public string PaginationSelector { get; set; }

    /// <summary>
    /// The CSS selector used to identify video source elements within a page.
    /// Used to extract direct video links.
    /// </summary>
    public string VideoLinkSelector { get; set; }

    /// <summary>
    /// The CSS selector used to find links to individual archive entries within search results.
    /// </summary>
    public string ArchiveLinkSelector { get; set; }

    /// <summary>
    /// When set to true, triggers the download module to process the file queue after scraping completes.
    /// </summary>
    public bool StartDownloader { get; set; }


    /// <summary>
    /// The URL of the starting web page for the scraper.
    /// </summary>
    [Required]
    public string StartingWebPage { get; set; }


    /// <summary>
    /// Chrome browser default directory for storing user data.
    /// </summary>
    public string? UserDataDir { get; set; }


}
