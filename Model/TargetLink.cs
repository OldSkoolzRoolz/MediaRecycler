// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers

namespace MediaRecycler;

public partial class TargetLink
{

    public TargetLink()
    {
    }
    public TargetLink(string postId, string postUrl)
    {
        PostId = postId;
        Link = postUrl;
        IsDownloaded = false;
    }







    public int Id { get; set; }

    public string PostId { get; set; }

    public string Link { get; set; }

    public bool IsDownloaded { get; set; }
}
