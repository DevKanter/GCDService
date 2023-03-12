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
        public GetPostsResponse GetDevPosts(CryptRequest crypt)
        {
            var request = RequestManager.ParseAndAuthenticate<GetDevPostsRequest>(crypt, out var session);

            var getAccountResult = WebsiteDB.GetAccountInfo(session.AccountID, out var response);
            if (getAccountResult != WebsiteDBResult.SUCCESS) return new GetPostsResponse { };
            
            if (response!.AccountType <= (int) AccountType.WEBSITE_ADMIN)
            {
                return new GetPostsResponse
                {
                    Posts = WebsiteDB.GetPosts(request.PostCategory, true)
                };
            }
            return new GetPostsResponse {};
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

        [HttpPost]
        public EditPostResponse EditPost(EditPostRequest request)
        {
            var authSuccess = RequestManager.Authenticate<EditPostRequest>(request, out var session);
            if (!authSuccess) throw new Exception("Not Authorized!");
            var result = WebsiteDB.EditPost(request!.Data);

            return new EditPostResponse() { Success = result == WebsiteDBResult.SUCCESS };
        }
    }
}
