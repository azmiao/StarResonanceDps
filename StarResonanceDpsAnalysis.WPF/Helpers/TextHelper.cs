namespace StarResonanceDpsAnalysis.WPF.Helpers;

internal static class TextHelper
{
    public static bool IsFullWidthText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        foreach (var ch in text)
        {
            if (ch > 0x7F)
            {
                return true;
            }
        }

        return false;
    }
}
