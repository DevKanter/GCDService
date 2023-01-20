using GCDService.Controllers.Account;
using GCDService.Helpers;
using System.Text.Json;

namespace GCDService.Managers
{
    public static class CryptManager
    {
        public static T DecryptRequest<T>(CryptRequest request)
        {
            if (request.Data == null) throw new ArgumentException("invalid CryptRequest object!");
            var json = RSAHelper.Decrypt(request.Data);
            var decryptRequest = JsonSerializer.Deserialize<T>(json);
            if (decryptRequest == null) throw new ArgumentException("invalid object!");
            return decryptRequest;
        }
    }
}
