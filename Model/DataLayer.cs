// Project Name: ${File.ProjectName}
// Author:  Kyle Crowder 
// Github:  OldSkoolzRoolz
// Distributed under Open Source License 
// Do not remove file headers




using System.Text.RegularExpressions;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;



namespace MediaRecycler.Model;





internal class DataLayer
{

    private static readonly ILogger _logger = Program.Logger;

    internal DataLayer()
    {
    }






    /// <summary>
    /// Inserts a target link into the database.
    /// </summary>
    /// <param name="postId">The ID of the post.</param>
    /// <param name="link">The link to be inserted.</param>
    /// <exception cref="ArgumentException">Thrown if postId or link is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the insert operation fails.</exception>
    /// <summary>
    /// Inserts a target link into the database.
    /// </summary>
    /// <param name="postId">The ID of the post.</param>
    /// <param name="link">The link to be inserted.</param>
    /// <exception cref="ArgumentException">Thrown if postId or link is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the insert operation fails.</exception>
    internal static void InsertTargetLinkToDb(string postId, string link)
    {
        // Define a constant for the ArgumentException message
        const string requiredMessage = "Post ID and link are required.";

        // Validate postId and link parameters
        if (string.IsNullOrWhiteSpace(postId) || string.IsNullOrWhiteSpace(link))
        {
            throw new ArgumentException(requiredMessage, nameof(postId));
        }

        // Additional validation rules for postId and link
        if (!IsValidPostId(postId) || !IsValidLink(link))
        {
            throw new ArgumentException("Invalid post ID or link format.", nameof(postId));
        }

        try
        {
            using var db = new MRContext();
            db.TargetLinks.Add(new TargetLink(postId, link));
            db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            // Log the error
            _logger.LogError(ex, "Duplicate key violation occurred while inserting target link to database.");
        }
        catch (SqlException ex)
        {
            // Log the error
            _logger.LogError(ex, "SQL error occurred while inserting target link to database.");
        }
        catch (IOException ex)
        {
            // Log the error
            _logger.LogError(ex, "IO error occurred while inserting target link to database.");
        }
        finally
        {
            // Ensure any resources are released
            _logger.LogInformation("InsertTargetLinkToDb method completed.");
        }
    }

    // Additional validation methods
    private static bool IsValidPostId(string postId)
    {
        // Implement post ID validation logic here
        return !string.IsNullOrWhiteSpace(postId);
    }

    private static bool IsValidLink(string link)
    {
        // Implement link validation logic here
        var linkPattern = new Regex(@"^https?://[^\s]+$");
        return linkPattern.IsMatch(link);
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
                    _logger.LogError( "Failed to insert duplicate post page URL to database.");
                    return false;
                }
              
                catch (Exception ex)
                {
                    // Handle other unexpected exceptions
                    _logger.LogError("An unexpected error occurred while inserting post page URL to database.");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            // Log or handle the exception
            _logger.LogError("Failed to insert post page URL to database.");
            return false;
        }
    }
















    internal static bool InsertPostPageUrlToDb(string link)
    {
        string? postid = link.Split("/").LastOrDefault();
        return string.IsNullOrWhiteSpace(postid)
            ? throw new ArgumentException("Unable to extract post ID from the link. Use different method overload.")
            : InsertPostPageUrlToDb(postid, link);
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







    public static async Task UpdateTargetDownloaded(string ePostId)
    {

        await using (var db = new MRContext())
        {

            var record = await db.TargetLinks.SingleOrDefaultAsync(p => p.PostId == ePostId);
            if (record == null) return;
            try
            {
                record.IsDownloaded = true;
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
            }
            catch (DbUpdateException)
            {
            }
            catch (Exception)
            {
                Program.Logger.LogError("Unexpected database error updating a record");
            }
        }

    }

}
