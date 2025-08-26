namespace Eds.IdentityService.Domain.FakeDomain.Consts;

public static class FakeConsts
{
    private const string DefaultSorting = "{0}FakeDate desc";

    public static string GetDefaultSorting(bool withEntityName = false)
    {
        return string.Format(DefaultSorting, withEntityName ? $"{TableName}." : string.Empty);
    }

    public const string TableName = "Fakes";
    public const int FakeCodeMaxLength = 500;
}