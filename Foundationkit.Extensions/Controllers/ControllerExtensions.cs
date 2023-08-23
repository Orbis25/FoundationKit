namespace Foundationkit.Extensions.Controllers;

public static class ControllerExtensions
{
    /// <summary>
    /// Show a alert with
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="title"></param>
    /// <param name="type"></param>
    /// <param name="message"></param>
    public static void SendNotification(this Controller controller, string title, MvcCoreNotification type = MvcCoreNotification.Success, string? message = default, string? config = default)
    {
        if (!string.IsNullOrEmpty(config))
        {
            controller.TempData["Notification"] = $"Swal.fire({{" +
                $"title:'{title}'," +
                $"icon:'{type.ToString().ToLower()}'," +
                $"html:'{message}'," +
                $"...{config}}})";
        }
        else
        {
            controller.TempData["Notification"] = $"Swal.fire('{title}','{message}','{type.ToString().ToLower()}')";
        }
    }
}