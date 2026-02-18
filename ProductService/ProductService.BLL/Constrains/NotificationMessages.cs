namespace ProductService.BLL.Constants;

public static class NotificationMessages
{
    public const string ProductCreatedTitle = "Product created";
    public const string ProductUpdatedTitle = "Product updated";
    public const string ProductDeletedTitle = "Product deleted";

    public static string GetProductCreatedMessage(string title)
        => $"Your product '{title}' has been successfully published!";

    public static string GetProductUpdatedMessage(string title)
        => $"Your product '{title}' has been successfully updated!";

    public static string GetProductDeletedMessage(string title)
        => $"Your product '{title}' has been successfully deleted.";
}
