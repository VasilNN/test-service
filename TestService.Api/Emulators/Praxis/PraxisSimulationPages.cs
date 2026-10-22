using System.Net;
using System.Text;

namespace TestService.Api.Emulators.Praxis;

/// <summary>
/// The emulator's 3DS confirmation page: the same form as the Praxis sandbox (Decline with an
/// <c>error_code</c> select, Pending, Approve, Verification). Each button POSTs <c>error_code</c>
/// to <c>.../pages/3ds/{tid}/submit?status=approved|declined|pending|verification</c>.
/// </summary>
public static class PraxisSimulationPages
{
    public static string ThreeDsPage(string submitUrl)
    {
        var action = WebUtility.HtmlEncode(submitUrl);
        var options = new StringBuilder();
        foreach (var (code, label) in PraxisTestCases.ThreeDsDeclineCodes)
        {
            options.Append($"<option value=\"{code}\">{WebUtility.HtmlEncode(label)}</option>");
        }

        return $$"""
            <!doctype html>
            <html>
            <head>
            <meta charset="utf-8">
            <title>3DSecure Confirmation Page</title>
            <script>
                const submit3D = (status) => {
                    const form3ds = document.getElementById('sandbox_3ds_form');
                    form3ds.setAttribute('action', '{{action}}?status=' + status);
                    form3ds.submit();
                };
            </script>
            </head>
            <body class="body">
            <div class="container">
                <h1>3DSecure Confirmation Page</h1>
                <h3>Please select transaction status:</h3>
                <div class="form">
                    <form class="action declined-case" id="sandbox_3ds_form" name="sandbox_3ds_form" method="POST" action="#">
                        <select id="error_code" name="error_code" class="select">{{options}}</select>
                        <button type="button" onclick="submit3D('declined')" class="button declined normal">Decline</button>
                    </form>
                    <div class="action">
                        <button type="button" onclick="submit3D('pending')" class="button pending normal">Pending</button>
                        <button type="button" onclick="submit3D('approved')" class="button approve normal">Approve</button>
                        <button type="button" onclick="submit3D('verification')" class="button approve normal">Verification</button>
                    </div>
                </div>
            </div>
            </body>
            </html>
            """;
    }

    /// <summary>Shown after a submit when the transaction has no <c>return_url</c>.</summary>
    public static string Done(string status) =>
        $"<!doctype html><html><head><meta charset=\"utf-8\"><title>3DSecure</title></head><body><p>3DS result submitted: {WebUtility.HtmlEncode(status)}</p></body></html>";
}
