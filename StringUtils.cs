namespace QC__Checker
{
    public static class StringUtils
    {
        public static string Normalize(string input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? string.Empty
                : new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToLowerInvariant();
        }
    }
}