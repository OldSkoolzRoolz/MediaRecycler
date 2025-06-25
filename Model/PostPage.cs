namespace MediaRecycler;

public partial class PostPage
{
    public PostPage(){}

    public PostPage(string postId, string postUrl)
    {
        PostId = postId;
        Link = postUrl;
    }






    public int Id { get; set; }

    public string PostId { get; set; }

    public string Link { get; set; }

    public bool IsProcessed { get; set; }
}
