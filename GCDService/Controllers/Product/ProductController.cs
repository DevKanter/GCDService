using GCDService.Controllers.Account;
using GCDService.Controllers.Product;
using GCDService.DB;
using GCDService.Managers;
using GCDService.Managers.Cash;
using GCDService.Managers.Request;
using Microsoft.AspNetCore.Mvc;

namespace GCDService.Controllers.Post
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ProductController : ControllerBase
    {

        [HttpPost]
        public GetCashAmountResponse GetCashAmount(CryptRequest crypt)
        {
            var request = RequestManager.ParseAndAuthenticate<GetCashAmountRequest>(crypt, out var session);

            return CashProductManager.GetCashAmount(session);
        }
        [HttpPost]
        public GetCashProductListResponse GetCashProductList(CryptRequest crypt)
        {
            var request = RequestManager.ParseAndAuthenticate<GetCashProductListRequest>(crypt, out var session);
            if (session ==null) throw new Exception("Not Authorized!");
            return new GetCashProductListResponse
            {
                CashProducts = CashProductManager.GetCashProductList()
            };
        }

        [HttpPost]
        public StartCheckoutResponse StartCheckout(CryptRequest crypt)
        {
            var request = RequestManager.ParseAndAuthenticate<StartCheckoutRequest>(crypt, out var session);

            var result = CashProductManager.OnTryCheckout(request.CashProductId, session, out var product);
            return new StartCheckoutResponse()
            {
                Success = result,
                CashProduct = product
            };

        }

        [HttpPost]
        public bool StartPaypalTransaction(CryptRequest crypt)
        {
            var request = RequestManager.ParseAndAuthenticate<StartPaypalTransactionRequest>(crypt, out var session);

            CashProductManager.OnTransactionStart(session);

            return true;
        }

        [HttpPost]
        public bool CancelCheckout(CryptRequest crypt)
        {
            var request = RequestManager.ParseAndAuthenticate<CheckoutCancelRequest>(crypt, out var session);

            return CashProductManager.OnManualCancel(session);
        }

        [HttpPost]
        public bool PaypalCancelRequest(CryptRequest crypt)
        {
            var request = RequestManager.ParseAndAuthenticate<PaypalCancelRequest>(crypt, out var session);

            CashProductManager.OnPaypalCancel(session,request);
            return true;
        }

        [HttpPost]
        public bool PaypalErrorRequest(CryptRequest crypt)
        {
            var request = RequestManager.ParseAndAuthenticate<PaypalErrorRequest>(crypt, out var session);

            CashProductManager.OnPaypalError(session,request);
            return true;
        }

        [HttpPost]
        public PaypalTransactionApprovedResponse OnPaypalTransactionApproved(PaypalTransactionApprovedRequest request)
        {
            var authSuccess = RequestManager.Authenticate<PaypalTransactionApprovedRequest>(request, out var session);
            if (!authSuccess) throw new Exception("Not Authorized!");

            CashProductManager.OnPaypalApproved(session,request);
            return new PaypalTransactionApprovedResponse()
            {
                Success = true
            };
        }


        [HttpPost]
        public PaypalTransactionAuthorizedResponse OnPaypalTransactionAuthorized(CryptRequest crypt)
        {
            var request = RequestManager.ParseAndAuthenticate<PaypalTransactionAuthorizedRequest>(crypt, out var session);

            CashProductManager.OnPaypalAuthorize(session, request);
            return new PaypalTransactionAuthorizedResponse()
            {
                Success = true
            };
        }


    }
}
