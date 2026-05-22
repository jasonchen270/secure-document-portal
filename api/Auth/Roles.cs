namespace SecureDocumentPortal.Api.Auth;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Reviewer = "Reviewer";
    public const string Uploader = "Uploader";
}

public static class Policies
{
    public const string Admin = "AdminOnly";
    public const string Reviewer = "ReviewerOrAbove";
    public const string Uploader = "UploaderOrAbove";
}
