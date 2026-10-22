using System.Globalization;
using System.Text.Json.Nodes;
using TestService.Api.Emulators;
using TestService.Api.Emulators.Praxis;

namespace TestService.Tests.Praxis;

/// <summary>
/// Builds Praxis requests as BillingServer sends them: card data encrypted with the merchant secret
/// and the request timestamp, signed with <c>GT-Authentication</c>. Also compiled into TestService.Unit.
/// </summary>
public static class PraxisRequestBuilder
{
    public const string TestCard = "4111111111111111";
    public const string TestExpiry = "12/2030";
    public const string CardGateway = "040e154f306f145b84208512d00ef8d9";

    public static JsonObject Sale(PraxisMerchant merchant, long timestamp, string cvv, long amount, string? notificationUrl = null,
        string? returnUrl = "https://merchant.example/return", string transactionType = "sale", string? orderId = null)
    {
        var body = new JsonObject
        {
            ["merchant_id"] = merchant.MerchantId,
            ["application_key"] = merchant.ApplicationKey,
            ["version"] = "1.3",
            ["timestamp"] = timestamp,
            ["transaction_type"] = transactionType,
            ["currency"] = "USD",
            ["amount"] = amount,
            ["cid"] = "cid-1",
            ["locale"] = "en-GB",
            ["gateway"] = merchant.Gateways.FirstOrDefault()?.Id ?? CardGateway,
            ["order_id"] = orderId ?? $"order-{Guid.NewGuid():N}",
            ["notification_url"] = notificationUrl,
            ["return_url"] = returnUrl,
            ["device_data"] = Device(),
            ["customer_data"] = new JsonObject
            {
                ["country"] = "BG",
                ["first_name"] = "Jane",
                ["last_name"] = "Doe",
                ["email"] = "jane.doe@example.com",
                ["city"] = "Testville",
                ["address"] = "1 Test Street"
            }
        };
        body["card_data"] = CardData(merchant.Secret, timestamp, cvv);
        return body;
    }

    public static JsonObject Payout(PraxisMerchant merchant, long timestamp, long amount, string? cardToken, string? customerToken)
    {
        return new JsonObject
        {
            ["merchant_id"] = merchant.MerchantId,
            ["application_key"] = merchant.ApplicationKey,
            ["version"] = "1.3",
            ["timestamp"] = timestamp,
            ["transaction_type"] = "payout",
            ["currency"] = "USD",
            ["amount"] = amount,
            ["cid"] = "cid-1",
            ["locale"] = "en-GB",
            ["gateway"] = merchant.Gateways.FirstOrDefault()?.Id ?? CardGateway,
            ["order_id"] = $"order-{Guid.NewGuid():N}",
            ["notification_url"] = null,
            ["return_url"] = null,
            ["device_data"] = Device(),
            ["card_token"] = cardToken,
            ["customer_token"] = customerToken
        };
    }

    public static JsonObject Refund(PraxisMerchant merchant, long timestamp, long amount, long referenceId) => new()
    {
        ["merchant_id"] = merchant.MerchantId,
        ["application_key"] = merchant.ApplicationKey,
        ["version"] = "1.3",
        ["timestamp"] = timestamp,
        ["transaction_type"] = "refund",
        ["currency"] = "USD",
        ["amount"] = amount,
        ["cid"] = "cid-1",
        ["locale"] = "en-GB",
        ["order_id"] = $"refund-{Guid.NewGuid():N}",
        ["reference_id"] = referenceId
    };

    public static JsonObject Find(PraxisMerchant merchant, long timestamp, long tid) => new()
    {
        ["merchant_id"] = merchant.MerchantId,
        ["application_key"] = merchant.ApplicationKey,
        ["version"] = "1.3",
        ["timestamp"] = timestamp,
        ["tid"] = tid
    };

    public static JsonObject Manage(PraxisMerchant merchant, long timestamp, string intent, long tid) => new()
    {
        ["merchant_id"] = merchant.MerchantId,
        ["application_key"] = merchant.ApplicationKey,
        ["version"] = "1.3",
        ["timestamp"] = timestamp,
        ["intent"] = intent,
        ["tid"] = tid
    };

    public static JsonObject CardData(string secret, long timestamp, string cvv, string card = TestCard, string expiry = TestExpiry)
    {
        var ts = timestamp.ToString(CultureInfo.InvariantCulture);
        return new JsonObject
        {
            ["card_number"] = PraxisCardCipher.Encrypt(card, secret, ts),
            ["card_exp"] = PraxisCardCipher.Encrypt(expiry, secret, ts),
            ["cvv"] = PraxisCardCipher.Encrypt(cvv, secret, ts)
        };
    }

    /// <summary>The <c>GT-Authentication</c> value for a request to <paramref name="path"/>.</summary>
    public static string Sign(string path, JsonNode body, string secret) => path switch
    {
        "api/direct-process" => PraxisSigner.ForDirectProcess(body, secret),
        "direct/refund" => PraxisSigner.ForRefund(body, secret),
        "agent/find-transaction" => PraxisSigner.ForFindTransaction(body, secret),
        "agent/manage-transaction" => PraxisSigner.ForManageTransaction(body, secret),
        _ => throw new ArgumentOutOfRangeException(nameof(path), path, null)
    };

    /// <summary>An emulator request for <paramref name="path"/>, signed with <paramref name="secret"/> unless a signature is given.</summary>
    public static EmulatorRequest Request(string environment, string path, JsonNode body, string secret, string? signature = null) => new()
    {
        Environment = environment,
        Path = path,
        Method = "POST",
        Body = body.ToJsonString(),
        Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Content-Type"] = "application/json",
            [PraxisSigner.HeaderName] = signature ?? Sign(path, body, secret)
        }
    };

    private static JsonObject Device() => new()
    {
        ["user_agent"] = "Mozilla/5.0 (test)",
        ["accept_header"] = "text/html",
        ["language"] = "en-US",
        ["ip_address"] = "203.0.113.10",
        ["timezone_offset"] = -120,
        ["color_depth"] = "24",
        ["pixel_depth"] = "24",
        ["pixel_ratio"] = "1",
        ["screen_height"] = 1080,
        ["screen_width"] = 1920,
        ["java_enabled"] = 0,
        ["javascript_enabled"] = 1
    };
}
