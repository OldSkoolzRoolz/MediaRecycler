// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using System.ComponentModel;

using Microsoft.EntityFrameworkCore;



namespace MediaRecycler.Model;





internal class DataLayer
{

    private readonly BindingSource _postPagesBindingSource;
    private readonly BindingSource _postLinksBindingSource;
    private readonly BindingList<string> _postPages;







    internal DataLayer()
    {
        _postPages = new BindingList<string>();
        _postPagesBindingSource = new BindingSource();
        _postLinksBindingSource = new BindingSource(new MRContext().TargetLinks, "Link");
        _postPagesBindingSource.DataSource = new MRContext().PostPages;
    }







    internal static void InsertTargetLinkToDb(string postId, string link)
    {
        if (string.IsNullOrWhiteSpace(postId) || string.IsNullOrWhiteSpace(link))
        {
            throw new ArgumentException("Post ID and link are required.");
        }

        using (var db = new MRContext())
        {
            db.TargetLinks.Add(new TargetLink(postId, link));

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                // Handle database update exception
                throw new InvalidOperationException("Failed to insert target link to database.", ex);
            }
        }
    }







    internal static bool InsertPostPageUrlToDb(string postid, string link)
    {
        if (string.IsNullOrWhiteSpace(postid) || string.IsNullOrWhiteSpace(link))
        {
            throw new ArgumentException("Post ID and link are required.");
        }

        try
        {
            using (var db = new MRContext())
            {
                db.PostPages.Add(new PostPage(postid, link));

                try
                {
                    db.SaveChanges();
                    return true;
                }
                catch (DbUpdateException ex)
                {
                    // Handle database update exception
                    throw new InvalidOperationException("Failed to insert post page URL to database.", ex);
                }
            }
        }
        catch (Exception ex)
        {
            // Log or handle the exception
            throw new InvalidOperationException("Failed to insert post page URL to database.", ex);
        }
    }
















    internal static bool InsertPostPageUrlToDb(string link)
    {
        var postid = link.Split("/").LastOrDefault();
        if (string.IsNullOrWhiteSpace(postid))
        {
            throw new ArgumentException("Unable to extract post ID from the link. Use different method overload.");
        }

        return InsertPostPageUrlToDb(postid, link);
    }

    internal static void UpdatePostPageProcessedFlag(int iid)
    {
        using (var db = new MRContext())
        {
            var page = db.PostPages.FirstOrDefault(p => p.Id == iid);
            if (page != null)
            {
                page.IsProcessed = true;
                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    // Handle database update exception
                    throw new InvalidOperationException("Failed to update post page processed flag.", ex);
                }
            }
        }
    }

    internal static async Task UpdatePostPageProcessedFlagAsync(string postid, bool isProcessed)
    {
        if (string.IsNullOrWhiteSpace(postid))
        {
            throw new ArgumentException("Post ID is required.");
        }

        using (var db = new MRContext())
        {
            var page = await db.PostPages.SingleOrDefaultAsync(p => p.PostId == postid.ToString());
            if (page != null)
            {
                page.IsProcessed = isProcessed;
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    // Handle database update exception
                    throw new InvalidOperationException("Failed to update post page processed flag asynchronously.", ex);
                }
            }
            else
            {
                // Handle the case where the post page is not found
                throw new InvalidOperationException("Post page not found.");
            }
        }
    }


}
