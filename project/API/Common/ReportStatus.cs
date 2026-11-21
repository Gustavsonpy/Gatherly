namespace API.Common
{
    public static class ReportStatus
    {
        public const string Pending = "Pending";
        public const string Resolved = "Resolved";
        public const string Dismissed = "Dismissed";

        public static readonly IReadOnlyList<string> All = [Pending, Resolved, Dismissed];

        public static bool IsValid(string? status) => status is not null && All.Contains(status);
    }
}
