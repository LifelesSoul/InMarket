namespace NotificationService.API.Configuration;

public static class AuthConstants
{
    public const string AdminRole = "Admin";

    public const string OwnerPolicyName = "MustBeOwner";

    public const string Auth0Scheme = "Auth0";

    public const string KeycloakScheme = "Keycloak";

    public const string SelectorScheme = "Auth0OrKeycloak";
}
