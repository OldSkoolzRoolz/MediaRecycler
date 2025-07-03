// Project Name: MediaRecycler
// File Name: DataLayer.cs
// Author:  Kyle Crowder
// Github:  OldSkoolzRoolz
// Distributed under Open Source License
// Do not remove file headers




using System.Text.RegularExpressions;

using MediaRecycler.Logging;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;



namespace MediaRecycler.Model;


internal class DataLayer
{



    private static readonly ILogger _logger = _logger;






    internal static async Task InsertPostPageUrlToDbAsync(string postid, string link)
    {

        try
        {
            await using (var db = new MRContext())
            {
                db.PostPages.Add(new PostPage(postid, link));

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // Handle database update exception
                    Log.LogError("Failed to insert duplicate post page URL to database.");
                }

                catch (Exception)
                {
                    // Handle other unexpected exceptions
                    Log.LogError("An unexpected error occurred while inserting post page URL to database.");
                }
            }
        }
        catch (Exception)
        {
            // Log or handle the exception
            Log.LogError("Failed to insert post page URL to database.");
        }
    }






    public static async Task InsertTargetLinkAndMarkPageAsProcessedAsync(string postId, string videoLink)
    {
        using var db = new MRContext();
        db.TargetLinks.Add(new TargetLink { PostId = postId, Link = videoLink });
        var record = db.PostPages.SingleOrDefault(p => p.PostId == postId);

        if (record != null)
        {
            record.IsProcessed = true;
            await db.SaveChangesAsync();
        }
    }






    /// <summary>
    ///     Inserts a target link into the database.
    /// </summary>
    /// <param name="postId">The ID of the post.</param>
    /// <param name="link">The link to be inserted.</param>
    /// <exception cref="ArgumentException">Thrown if postId or link is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the insert operation fails.</exception>
    /// <summary>
    ///     Inserts a target link into the database.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if postId or link is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the insert operation fails.</exception>
    internal static void InsertTargetLinkToDb(string postId, string link)
    {
        // Define a constant for the ArgumentException message
        const string requiredMessage = "Post ID and link are required.";

        // Validate postId and link parameters
        if (string.IsNullOrWhiteSpace(postId) || string.IsNullOrWhiteSpace(link)) throw new ArgumentException(requiredMessage, nameof(postId));

        // Additional validation rules for postId and link
        if (!IsValidPostId(postId) || !IsValidLink(link)) throw new ArgumentException("Invalid post ID or link format.", nameof(postId));

        try
        {
            using var db = new MRContext();
            db.TargetLinks.Add(new TargetLink(postId, link));
            db.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            // Log the error
            Log.LogError(ex, "Duplicate key violation occurred while inserting target link to database.");
        }
        catch (SqlException ex)
        {
            // Log the error
            Log.LogError(ex, "SQL error occurred while inserting target link to database.");
        }
        catch (IOException ex)
        {
            // Log the error
            Log.LogError(ex, "IO error occurred while inserting target link to database.");
        }
        finally
        {
            // Ensure any resources are released
            Log.LogInformation("InsertTargetLinkToDb method completed.");
        }
    }






    private static bool IsValidLink(string link)
    {
        // Implement link validation logic here
        var linkPattern = new Regex(@"^https?://[^\s]+$");
        return linkPattern.IsMatch(link);
    }






    // Additional validation methods
    private static bool IsValidPostId(string postId)
    {
        // Implement post ID validation logic here
        return !string.IsNullOrWhiteSpace(postId);
    }






    internal static async Task MarkPostPageAsProcessedAsync(string postId)
    {
        throw new NotImplementedException();
    }






    internal static async Task UpdatePostPageProcessedFlagAsync(string postid, bool isProcessed)
    {


        await using (var db = new MRContext())
        {
            var page = await db.PostPages.SingleOrDefaultAsync(p => p.PostId == postid);

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

        }
    }






    public static async Task UpdateTargetDownloaded(string ePostId)
    {

        await using (var db = new MRContext())
        {

            var record = await db.TargetLinks.Where(p => p.PostId == ePostId).FirstOrDefaultAsync();

            if (record == null)
            {
                Log.LogError("Record not found in db");
                return;
            }

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
                Log.LogError("Unexpected database error updating a record");
            }
        }

    }

}