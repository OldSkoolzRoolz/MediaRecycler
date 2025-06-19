// Project Name: MediaRecycler
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers




namespace MediaRecycler.Modules.Interfaces;


public interface IBlogScraper : IScraper
{

    Task BeginScrapingTargetBlogAsync();

    Task DownloadCollectedLinksAsync();

}