// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




namespace MediaRecycler.Modules.Interfaces;


public interface IBlogScraper : IScraper
{

    Task BeginScrapingTargetBlogAsync();




    /// <summary>
    /// 
    /// </summary>
    /// <param name="collectedUrls"></param>
    /// <returns></returns>
    Task DownloadCollectedLinksAsync();





    /// <summary>
    ///     Extracts URLs from the page that contain videos, but does not retrieve the actual video content.
    /// </summary>
    /// <returns></returns>
    Task ExtractTargetLinksAsync();






}