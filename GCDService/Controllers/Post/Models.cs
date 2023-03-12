using GCDService.Managers.Request;

namespace GCDService.Controllers.Post
{
    public class GetPostsRequest
    {
        public PostCategory PostCategory { get; set; }
    }

    public class GetDevPostsRequest : AuthRequest
    {
        public PostCategory PostCategory { get; set; }
        
    }
    public class GetPostsResponse
    {
        public IEnumerable<Post> Posts { get; set; } = Array.Empty<Post>();
    }

    public class CreatePostRequest : AuthRequest
    {
        public CreatePostData? Data { get; set; }
    }

    public class CreatePostResponse 
    {
        public bool Success { get; set; }
        public int Code { get; set; }
    }
    public class EditPostRequest: AuthRequest
    {
        public EditPostData? Data { get; set; }
    }
    public class EditPostResponse
    {
        public bool Success { get; set; }
        public int Code { get; set; }
    }
    public class DeletePostRequest : AuthRequest
    {
        public int PostId { get; set; }
    }
    public class DeletePostResponse
    {
        public bool Success { get; set; }
        public int Code { get; set; }
    }
    public class Post
    {
        public int Id { get; set; }
        public PostCategory PostCategory { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Posted { get; set; }
        public DateTime Modified { get; set; }
        public PostVisibility PostVisibility { get; set; }
        public string PostedBy { get; set; } = string.Empty;

    }

    public class CreatePostData
    {
        public PostCategory Category { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public PostVisibility Visibility { get; set; }
    }

    public class EditPostData
    {
        public int PostId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public PostVisibility Visibility { get; set; }
    }

    public enum PostCategory
    {
        INVALID,
        NEWS,
        INFO,
        PATCH,
        EVENT
    }
    public enum PostVisibility
    {
        INVALID,
        EVERYONE,
        DEV
    }
}
