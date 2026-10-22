using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using TestService.Api.Emulators.Transactions;

namespace TestService.Api.Emulators.Praxis;

/// <summary>
/// JSON bodies per Praxis endpoint, with the key sets (and key order) of the captured examples.
/// Values come from the transaction; <c>transaction.amount</c> holds the final amount and the
/// processed amount the requested one.
/// </summary>
public static class PraxisResponses
{
    public const string Version = "1.3";

    /// <summary>Keys of <see cref="EmulatedTransaction.ProviderData"/> used by the Praxis emulator.</summary>
    public static class Keys
    {
        public const string CustomerToken = "customer_token";
        public const string Country = "country";
        public const string FirstName = "first_name";
        public const string LastName = "last_name";
        public const string CardToken = "card_token";
        public const string CardType = "card_type";
        public const string CardNumber = "card_number";
        public const string CardExp = "card_exp";
        public const string CardIssuerName = "card_issuer_name";
        public const string CardIssuerCountry = "card_issuer_country";
        public const string AuthToken = "auth_token";
        public const string Cid = "cid";
        public const string Gateway = "gateway";
        public const string PaymentMethod = "payment_method";
        public const string PaymentProcessor = "payment_processor";
        public const string WithdrawalRequestId = "withdrawal_request_id";
        public const string Variable1 = "variable1";
        public const string Variable2 = "variable2";
        public const string Variable3 = "variable3";
    }

    private enum Shape
    {
        DirectProcess,
        Refund,
        FindTransaction,
        ManageTransaction,
        Notification
    }

    private static readonly JsonSerializerOptions WriteOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static string Serialize(JsonNode node) => node.ToJsonString(WriteOptions);

    /// <summary><c>api/direct-process</c> (sale or payout) response.</summary>
    public static JsonObject DirectProcess(EmulatedTransaction tx, long timestamp) => new()
    {
        ["status"] = 0,
        ["description"] = "Ok",
        ["redirect_url"] = tx.RedirectUrl,
        ["customer"] = Customer(tx),
        ["session"] = Session(tx),
        ["transaction"] = Transaction(tx, Shape.DirectProcess),
        ["timestamp"] = timestamp,
        ["version"] = Version
    };

    /// <summary><c>direct/refund</c> response.</summary>
    public static JsonObject Refund(EmulatedTransaction tx, long timestamp) => new()
    {
        ["status"] = 0,
        ["description"] = "Ok",
        ["customer"] = Customer(tx),
        ["session"] = Session(tx),
        ["transaction"] = Transaction(tx, Shape.Refund),
        ["version"] = Version,
        ["timestamp"] = timestamp
    };

    /// <summary><c>agent/find-transaction</c> response.</summary>
    public static JsonObject FindTransaction(EmulatedTransaction tx, long timestamp) => new()
    {
        ["status"] = 0,
        ["description"] = "Ok",
        ["customer"] = Customer(tx),
        ["session"] = Session(tx),
        ["transaction"] = Transaction(tx, Shape.FindTransaction),
        ["version"] = Version,
        ["timestamp"] = timestamp
    };

    /// <summary><c>agent/manage-transaction</c> response: only status, description, transaction, version, timestamp.</summary>
    public static JsonObject ManageTransaction(EmulatedTransaction tx, long timestamp) => new()
    {
        ["status"] = 0,
        ["description"] = "Ok",
        ["transaction"] = Transaction(tx, Shape.ManageTransaction),
        ["version"] = Version,
        ["timestamp"] = timestamp
    };

    /// <summary>Notification body posted to the transaction's <c>notification_url</c>.</summary>
    public static JsonObject Notification(EmulatedTransaction tx, PraxisMerchant merchant, long timestamp) => new()
    {
        ["merchant_id"] = merchant.MerchantId,
        ["application_key"] = merchant.ApplicationKey,
        ["customer"] = Customer(tx),
        ["session"] = Session(tx),
        ["transaction"] = Transaction(tx, Shape.Notification),
        ["version"] = Version,
        ["timestamp"] = timestamp
    };

    /// <summary>A rejection: <c>{"status":..,"description":..}</c>.</summary>
    public static JsonObject Error(int status, string description) => new()
    {
        ["status"] = status,
        ["description"] = description
    };

    /// <summary>A <c>status 1 Validation Error</c> with one message for one field.</summary>
    public static JsonObject ValidationError(string field, string message) => new()
    {
        ["status"] = 1,
        ["description"] = "Validation Error",
        ["errors"] = new JsonObject { [field] = new JsonArray(message) }
    };

    private static JsonObject Customer(EmulatedTransaction tx) => new()
    {
        ["customer_token"] = tx.Data(Keys.CustomerToken),
        ["country"] = tx.Data(Keys.Country),
        ["first_name"] = tx.Data(Keys.FirstName),
        ["last_name"] = tx.Data(Keys.LastName),
        ["avs_alert"] = 0,
        ["verification_alert"] = 0
    };

    private static JsonObject Session(EmulatedTransaction tx) => new()
    {
        ["auth_token"] = tx.Data(Keys.AuthToken),
        ["intent"] = tx.TransactionType == "sale" ? "payment" : "withdrawal",
        ["session_status"] = tx.SessionStatus,
        ["order_id"] = tx.OrderId,
        ["currency"] = tx.Currency,
        ["amount"] = tx.Amount,
        ["conversion_rate"] = "1.000000",
        ["processed_currency"] = tx.Currency,
        ["processed_amount"] = tx.ProcessedAmount ?? tx.RequestedAmount,
        ["payment_method"] = tx.Data(Keys.PaymentMethod),
        ["gateway"] = tx.Data(Keys.Gateway),
        ["cid"] = tx.Data(Keys.Cid),
        ["variable1"] = tx.Data(Keys.Variable1),
        ["variable2"] = tx.Data(Keys.Variable2),
        ["variable3"] = tx.Data(Keys.Variable3)
    };

    private static JsonObject Transaction(EmulatedTransaction tx, Shape shape)
    {
        var transaction = new JsonObject
        {
            ["transaction_type"] = tx.TransactionType,
            ["transaction_status"] = tx.Status.ToWire(),
            ["tid"] = tx.Tid,
            ["transaction_id"] = tx.Tid.ToString(),
            ["currency"] = tx.Currency,
            ["amount"] = tx.Amount,
            ["conversion_rate"] = "1.000000",
            ["processed_currency"] = tx.ProcessedCurrency,
            ["processed_amount"] = tx.ProcessedAmount,
            ["fee"] = 0,
            ["fee_included"] = 0,
            ["fee_type"] = "flat",
            ["payment_method"] = tx.Data(Keys.PaymentMethod),
            ["payment_processor"] = tx.Data(Keys.PaymentProcessor),
            ["gateway"] = tx.Data(Keys.Gateway),
            ["card"] = new JsonObject
            {
                // The payout response carries no card token (sandbox).
                ["card_token"] = tx.TransactionType == "payout" ? null : tx.Data(Keys.CardToken),
                ["card_type"] = tx.Data(Keys.CardType),
                ["card_number"] = tx.Data(Keys.CardNumber),
                ["card_exp"] = tx.Data(Keys.CardExp),
                ["card_issuer_name"] = tx.Data(Keys.CardIssuerName),
                ["card_issuer_country"] = tx.Data(Keys.CardIssuerCountry)
            },
            ["wallet"] = null
        };

        if (shape != Shape.FindTransaction)
        {
            transaction["is_async"] = tx.TransactionType == "refund" ? null : (tx.IsAsync ? 1 : 0);
        }

        transaction["is_cascade"] = 0;
        transaction["cascade_level"] = shape is Shape.FindTransaction or Shape.Notification ? 0 : null;
        transaction["reference_id"] = tx.ReferenceId;
        transaction["withdrawal_request_id"] = tx.Data(Keys.WithdrawalRequestId);
        transaction["created_by"] = "INTERNET";
        transaction["edited_by"] = null;
        transaction["status_code"] = tx.StatusCode;
        transaction["status_details"] = tx.StatusDetails;

        if (shape == Shape.DirectProcess && tx.RequiresAuthentication)
        {
            transaction["redirect_url"] = tx.RedirectUrl;
        }
        if (shape == Shape.FindTransaction)
        {
            transaction["is_3d"] = tx.Is3d ? 1 : 0;
        }

        return transaction;
    }
}
