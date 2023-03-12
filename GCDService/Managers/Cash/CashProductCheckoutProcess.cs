using GCDService.Controllers.Product;
using GCDService.Managers.Session;

namespace GCDService.Managers.Cash
{
    public class CashProductCheckoutProcess
    {
        public CashProduct Product { get; init; }
        public UserSession Session { get; init; }
        public CheckoutState State {get; private set; }
        public PaypalTransactionApprovedRequest? ApprovedRequest { get; private set; }
        public PaypalTransactionAuthorizedRequest? AuthorizedRequest { get; set; }

        public CashProductCheckoutProcess(CashProduct product, UserSession session)
        {
            Product = product;
            Session = session;
            State = CheckoutState.CHECKOUT_REQUESTED;
        }

        internal void OnCheckoutSuccess()
        {
            State = CheckoutState.CHECKOUT_APPROVED;
        }

        internal void OnTransactionStarted()
        {
            State = CheckoutState.PAYPAL_TRANSACTION_STARTED;
        }

        public void OnPaypalApprove(PaypalTransactionApprovedRequest request)
        {
            ApprovedRequest = request;
            State = CheckoutState.PAYPAL_TRANSACTION_APPROVED;
        }

        public void OnPaypalAuthorize(PaypalTransactionAuthorizedRequest request)
        {
            AuthorizedRequest = request;
            State = CheckoutState.PAYPAL_TRANSACTION_AUTHORIZED;
        }

        public PaypalTransactionStatus GetTransactionStatus()
        {
            if(AuthorizedRequest!=null) return AuthorizedRequest.Status;
            if(ApprovedRequest!=null) return ApprovedRequest.Status;
            return PaypalTransactionStatus.VOIDED;
        }
        public bool VerifyCompleteness()
        {
            if (State != CheckoutState.PAYPAL_TRANSACTION_AUTHORIZED) return false;
            if(ApprovedRequest ==null ) return false;
            if(AuthorizedRequest == null) return false;
            if(ApprovedRequest.PurchaseUnit.FirstOrDefault()==  null) return false;
            if(ApprovedRequest.PurchaseUnit.First().Id != Product.Id) return false;
            if(ApprovedRequest.TransactionId != AuthorizedRequest.TransactionId) return false;

            return true;
        }
    }

    public enum CheckoutState
    {
        CHECKOUT_REQUESTED,
        CHECKOUT_APPROVED,
        CHECKOUT_CANCELED,

        PAYPAL_TRANSACTION_STARTED,
        PAYPAL_TRANSACTION_CANCELED,
        PAYPAL_TRANSACTION_ERROR,
        PAYPAL_TRANSACTION_APPROVED,
        PAYPAL_TRANSACTION_AUTHORIZED,

        CHECKOUT_COMPLETED
    }
    public enum CheckoutProcessEndReason
    {
        SUCCESS,
        FAIL,
        USER_RELOGIN,
        USER_ALREADY_BUYING,
        MANUAL_CANCEL,
        PAYPAL_CANCEL,
        PAYPAL_ERROR,
        PAYPAL_AUTHORIZED
    }
}
