using System.Text.RegularExpressions;

public static class ModelExtensions
{
    private static string imgContainerUrl="https://3dshooter.blob.core.windows.net/images/";
    public static ShortArticle ToShortArticle(this TweetModel t)
    {
        string title=GetTitleFrom(t);
        return new ShortArticle(System.Net.WebUtility.HtmlDecode(t.FullText.Replace(title,"").Trim()), t.CreatedAt, GetImageSrcFrom(t), title,t.DirectLink);
    }

    private static string GetTitleFrom(TweetModel t)
    {
        string firstHashtag=GetFirstHashTag(t.FullText);
        //obligatory hipster hashtag
        return $"#{firstHashtag}";
    }

    private static string GetFirstHashTag(string fullText)
    {
        var regex = new Regex(@"(?<=#)\w+");
        var matches = regex.Matches(fullText);
        foreach (Match m in matches)
        {
            return m.Value;
        }
        return string.Empty;
    }

    private static string GetImageSrcFrom(TweetModel t)
    {
        string imageFileName=ClearFileName(GetFirstHashTag(t.FullText));
        return $"{imgContainerUrl}{imageFileName}.jpg";
    }

    private static string ClearFileName(string fileName)
    {
        foreach (char c in System.IO.Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName;
    }
}