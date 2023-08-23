namespace FoundationKit.Helpers.Html;

/// <summary>
/// This helper permit render a sweetalert2 notification in html
/// for mvc proporsals
/// </summary>
public static class NotifyHelper
{
    public static IHtmlContent RenderAlerts(this IHtmlHelper _, ITempDataDictionary tempData)
    {
        var notification = tempData["Notification"]?.ToString();

        return new HtmlString($"<script asp-append-version='true'>{notification}</script>");
    }
}
