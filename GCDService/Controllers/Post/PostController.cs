using GCDService.Controllers.Account;
using GCDService.DB;
using GCDService.Managers;
using GCDService.Managers.Request;
using Microsoft.AspNetCore.Mvc;

namespace GCDService.Controllers.Post
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class PostController : ControllerBase
    {
        [HttpPost]
        public GetPostsResponse GetPosts(CryptRequest crypt)
        {
            var request = CryptManager.DecryptRequest<GetPostsRequest>(crypt);

            var result = WebsiteDB.GetPosts(request.PostCategory);

            return new GetPostsResponse { Posts = result };
        }
        [HttpPost]
        public CreatePostResponse CreatePost(CreatePostRequest request)
        {
            var authSuccess = RequestManager.Authenticate<CreatePostRequest>(request, out var session);
            if (!authSuccess) throw new Exception("Not Authorized!");
            var result = WebsiteDB.AddPost(request!.Data,session!.AccountID);

            return new CreatePostResponse() { Success = result == WebsiteDBResult.SUCCESS };
        }
        [HttpPost]
        public DeletePostResponse DeletePost(CryptRequest crypt)
        {
            var request = RequestManager.ParseAndAuthenticate<DeletePostRequest>(crypt, out var session);

            return new DeletePostResponse() { Success = WebsiteDB.DeletePost(request.PostId) == WebsiteDBResult.SUCCESS };

        }

    }
}
