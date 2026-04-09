using System.Text;

namespace NeedApp.Infrastructure.Persistence;

/// <summary>
/// Helpers for Vietnamese diacritics-insensitive + case-insensitive search.
/// </summary>
public static class SearchHelper
{
    // All Vietnamese lowercase diacritical characters → their ASCII equivalents.
    // Since we apply lower() in SQL before translate(), we only need lowercase mappings.
    private const string VnFrom = "àáảãạăằắẳẵặâầấẩẫậèéẻẽẹêềếểễệìíỉĩịòóỏõọôồốổỗộơờớởỡợùúủũụưừứửữựỳýỷỹỵđ";
    private const string VnTo   = "aaaaaaaaaaaaaaaaaeeeeeeeeeeeiiiiiooooooooooooooooouuuuuuuuuuuyyyyyd";

    // Full mapping (both upper and lowercase) for C# side use
    private const string VnFromFull =
        "àáảãạăằắẳẵặâầấẩẫậèéẻẽẹêềếểễệìíỉĩịòóỏõọôồốổỗộơờớởỡợùúủũụưừứửữựỳýỷỹỵđ" +
        "ÀÁẢÃẠĂẰẮẲẴẶÂẦẤẨẪẬÈÉẺẼẸÊỀẾỂỄỆÌÍỈĨỊÒÓỎÕỌÔỒỐỔỖỘƠỜỚỞỠỢÙÚỦŨỤƯỪỨỬỮỰỲÝỶỸỴĐ";
    private const string VnToFull =
        "aaaaaaaaaaaaaaaaaeeeeeeeeeeeiiiiiooooooooooooooooouuuuuuuuuuuyyyyyd" +
        "AAAAAAAAAAAAAAAAAEEEEEEEEEEEIIIIIOOOOOOOOOOOOOOOOUUUUUUUUUUUYYYYYD";

    /// <summary>The FROM chars for PostgreSQL translate(lower(col), from, to).</summary>
    public static string SqlTranslateFrom => VnFrom;

    /// <summary>The TO chars for PostgreSQL translate(lower(col), from, to).</summary>
    public static string SqlTranslateTo => VnTo;

    /// <summary>
    /// Remove diacritical marks from Vietnamese text (C# side).
    /// </summary>
    public static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            var idx = VnFromFull.IndexOf(c);
            sb.Append(idx >= 0 ? VnToFull[idx] : c);
        }
        return sb.ToString();
    }
}
