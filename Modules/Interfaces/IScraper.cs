// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




namespace MediaRecycler.Modules.Interfaces;


public interface IScraper
{

    ValueTask CancelAsync();

    ValueTask DisposeAsync();

}