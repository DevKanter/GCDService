using GCDService.Controllers.Product;
using GCDService.DB;
using GCDService.Managers.Session;
using System.Collections.Generic;
using static GCDService.Managers.Cash.CheckoutProcessEndReason;

namespace GCDService.Managers.Cash
{
    public static class CashProductManager
    {
        private static readonly Dictionary<UserSession, CashProductCheckoutProcess> _activeCheckouts = new();
        private static IEnumerable<CashProduct> _cashProducts = Enumerable.Empty<CashProduct>();
        public static void Initialize()
        {
            _activeCheckouts.Clear();
            _cashProducts = WebsiteDB.GetCashProductList();
        }

        public static GetCashAmountResponse GetCashAmount(UserSession session)
        {
            return new GetCashAmountResponse()
            {
                CashBalance = WebsiteDB.GetCashAmount(session.AccountID)
            };
        }

        public static bool IncreaseCashAmount(UserSession session, int amount)
        {
            return WebsiteDB.IncreaseCashAmount(session.AccountID, amount) == WebsiteDBResult.SUCCESS;
        }
        public static IEnumerable<CashProduct> GetCashProductList()
        {
            return _cashProducts;
        }
        
        public static void OnUserLogin(UserSession session)
        {
            if(_activeCheckouts.TryGetValue(session,out var process))
            {
                _activeCheckouts.Remove(session);
                WebsiteDB.LogCashProductCheckoutProcess(process, USER_RELOGIN);
            }
            
        }
        public static bool OnTryCheckout(int cashProductId, UserSession session, out CashProduct? product)
        {
            product = _cashProducts.Where(p => p.Id == cashProductId).FirstOrDefault();
            if(product == null) 
                throw new Exception("Cant find select cash product!");

            var checkoutProcess = new CashProductCheckoutProcess(product,session);

            if (_activeCheckouts.ContainsKey(session))
            {
                WebsiteDB.LogCashProductCheckoutProcess(checkoutProcess,USER_ALREADY_BUYING);
                throw new Exception("User already in buying process!");
            }

            checkoutProcess.OnCheckoutSuccess();

            _activeCheckouts.Add(session, checkoutProcess);

            return true;
        }

        public static bool OnTransactionStart(UserSession session)
        {
            if (_activeCheckouts.TryGetValue(session, out var process))
            {
                if (process.State < CheckoutState.CHECKOUT_APPROVED)
                    throw new("Wrong Transaction State");
                process.OnTransactionStarted();
            }
            return false;
        }

        public static bool OnManualCancel(UserSession session)
        {
            if (!_activeCheckouts.TryGetValue(session, out var process)) return false;
            if (process.State >= CheckoutState.CHECKOUT_CANCELED) return false;

            WebsiteDB.LogCashProductCheckoutProcess(process,MANUAL_CANCEL);
            _activeCheckouts.Remove(session);
            return true;
        }
        
        public static void OnPaypalCancel(UserSession session, PaypalCancelRequest request)
        {
            if (!_activeCheckouts.TryGetValue(session, out var process)) return;

            WebsiteDB.LogCashProductCheckoutProcess(process, PAYPAL_CANCEL);
            _activeCheckouts.Remove(session);
        }
        public static void OnPaypalError(UserSession session, PaypalErrorRequest request)
        {
            if (!_activeCheckouts.TryGetValue(session, out var process)) return;

            WebsiteDB.LogCashProductCheckoutProcess(process, PAYPAL_ERROR);
            _activeCheckouts.Remove(session);
        }

        public static bool OnPaypalApproved(UserSession session, PaypalTransactionApprovedRequest request)
        {
            if (request.Status != PaypalTransactionStatus.APPROVED)
                throw new("Paypal did not approve the transaction!");
            if (!_activeCheckouts.TryGetValue(session, out var process))
                throw new("CheckoutProcess not found!");
            if (process.State < CheckoutState.PAYPAL_TRANSACTION_STARTED)
                throw new("Wrong Transaction State");
            if (request.PurchaseUnit.FirstOrDefault() == null)
                throw new("No Item Purchased Error!");
            if (process.Product.Id != request.PurchaseUnit.FirstOrDefault()!.Id)
                throw new("Item purchased does not match item selected!");
            if (process.Product.Price != request.PurchaseUnit.FirstOrDefault()!.Price)
                throw new("Price of purchased and selected item do not match!");


            process.OnPaypalApprove(request);
            return true;
        }

        public static bool OnPaypalAuthorize(UserSession session, PaypalTransactionAuthorizedRequest request)
        {
            if (request.Status != PaypalTransactionStatus.COMPLETED)
                throw new("Paypal did not authorize the transaction!");
            if (!_activeCheckouts.TryGetValue(session, out var process))
                throw new("CheckoutProcess not found!");
            if (process.State < CheckoutState.PAYPAL_TRANSACTION_APPROVED)
                throw new("Wrong Transaction State");
            if(process.ApprovedRequest!.TransactionId != request.TransactionId)
                throw new("TransactionId from Authorize does not match the one from Approve!");

            process.OnPaypalAuthorize(request);

            return CompleteProcess(process);
        }

        private static bool CompleteProcess(CashProductCheckoutProcess process)
        {
            if (!process.VerifyCompleteness()) return false;
            WebsiteDB.LogCashProductCheckoutProcess(process, PAYPAL_AUTHORIZED);
            WebsiteDB.IncreaseCashAmount(process.Session.AccountID, process.Product.Amount);

            return true;
        }
    }
}
