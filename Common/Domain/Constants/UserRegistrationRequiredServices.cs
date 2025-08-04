namespace Common.Domain.Constants;

public static class UserRegistrationRequiredServices
{
    public static string ReviewsService { get; set; } = nameof(ReviewsService);
    
    public static string OrdersService { get; set; } = nameof(OrdersService);
    
    public static string ChatsService { get; set; } = nameof(ChatsService); 
}
