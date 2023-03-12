using GCDService.Managers.Request;

namespace GCDService.Controllers.Product
{

    public class GetCashAmountRequest : AuthRequest
    {

    }

    public class GetCashAmountResponse
    {
        public int CashBalance { get; set; }
    }
    public class GetCashProductListRequest : AuthRequest
    {
    }
    public class GetCashProductListResponse
    {
        public IEnumerable<CashProduct> CashProducts { get; set; } = Enumerable.Empty<CashProduct>();
    }
    
    public class StartCheckoutRequest : AuthRequest {
        public int CashProductId { get; set; }
    }
    public class StartCheckoutResponse
    {
        public bool Success { get; set; }
        public CashProduct? CashProduct { get; set; }
    }

    public class CheckoutCancelRequest : AuthRequest
    {

    }

    public class StartPaypalTransactionRequest : AuthRequest
    {

    }

    public class PaypalCancelRequest :AuthRequest
    {
        public string TransactionId { get; set; } = string.Empty;
    }

    public class PaypalErrorRequest : AuthRequest
    {
        public string Error { get; set; } = string.Empty;
    }

    public class PaypalTransactionApprovedRequest : AuthRequest
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public PaypalTransactionStatus Status { get; set; }
        public Payer? Payer { get; set; }
        public IEnumerable<PurchaseUnit> PurchaseUnit { get; set; } = Array.Empty<PurchaseUnit>();
    }

    public class PaypalTransactionApprovedResponse
    {
        public bool Success { get; set; }
    }
    public class PaypalTransactionAuthorizedRequest : AuthRequest
    {
        public string TransactionId { get; set; } = string.Empty;
        public PaypalTransactionStatus Status { get; set; }
    }
    public class PaypalTransactionAuthorizedResponse
    {
        public bool Success { get; set; }
    }
    public class CashProduct
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public int Price { get; set; }
        public string ProductName { get; set; } = string.Empty;

    }

    public class Payer
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PayerId { get; set; } = string.Empty;
    }

    public class PurchaseUnit
    {
        public int Price { get; set; }

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }


    public enum PaypalTransactionStatus
    {
        APPROVED,
        SAVED,
        CREATED,
        VOIDED,
        COMPLETED
    }
}
