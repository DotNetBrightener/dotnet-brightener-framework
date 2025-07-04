namespace DotNetBrightener.OAuth;

public class OAuthResponseScripts
{
    internal const string ResponseSerializeStringToken = "$$_DATA_REPLACEMENT_$$";
    internal const string TargetOriginToken            = "$$_TARGET_ORIGIN_REPLACEMENT_$$";

    internal static readonly string OAuthResponseScript = @"
<script>
    if (window.opener) {
        window.opener.postMessage(" + ResponseSerializeStringToken + @", '" + TargetOriginToken + @"');
	}
	window.close();
</script>
<noscript>Unavailable</noscript>
";
}