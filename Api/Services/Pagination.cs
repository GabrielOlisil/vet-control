namespace Api.Services;

public static class Pagination
{
    public static int NormalizePage(int? page) => NormalizePage(page.GetValueOrDefault(1));

    public static int NormalizePage(int page) => page < 1 ? 1 : page;

    public static int Offset(int? page, int pageSize) => Offset(NormalizePage(page), pageSize);

    public static int Offset(int page, int pageSize)
    {
        var offset = ((long)NormalizePage(page) - 1) * pageSize;
        return offset > int.MaxValue ? int.MaxValue : (int)offset;
    }
}
