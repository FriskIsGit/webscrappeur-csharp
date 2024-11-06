using System.Diagnostics;
using System.Text;
using System.Web;
using NUnit.Framework;
using WebScrapper.scrapper;

namespace WebScrapper.tests;

public class LyricsScrapperTest{
    private static readonly string RESOURCE_DIR_PATH = 
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Desktop/scraper/";

    [Test]
    public void fullGeniusScrape(){
        string input = File.ReadAllText(RESOURCE_DIR_PATH + "genius.html");
        
        HtmlDoc html = new HtmlDoc(input);
        html.ReplaceLineBreakWithNewLine(true);
        html.DelimitTags(false);
        StringBuilder fullExtract = new StringBuilder(512);
        List<Tag> tags = html.FindAll("div",
            ("class", "Lyrics__Container-sc-1ynbvzw-", Compare.VALUE_STARTS_WITH), 
            ("data-lyrics-container", "true", Compare.EXACT));
        Console.WriteLine("SIZE: " + tags.Count);
        
        foreach (var div in tags){
            string extract = html.ExtractText(div);
            fullExtract.Append(extract).Append('\n');
        }

        // Escaping special characters
        string escapedLyrics = escapeHTML(fullExtract.ToString());
        Console.WriteLine(escapedLyrics);
    }
    
    [Test]
    public void fullMusixScrap(){
        string input = File.ReadAllText(RESOURCE_DIR_PATH + "mux.html");
        HtmlDoc html = new HtmlDoc(input);
        html.DelimitTags(true);
        html.ReplaceLineBreakWithNewLine(true);
        int lyricsIndex = input.IndexOf("Lyrics of", StringComparison.Ordinal);
        if (lyricsIndex == -1) {
            Assert.Fail("There are no lyrics for this song");
            return;
        }
        // This webpage is now 'div' infested (approx. 750 divs)
        int outerSongDiv = input.LastIndexOf("<div class", lyricsIndex, StringComparison.Ordinal);
        
        Tag? outerLyricsDiv = html.FindFrom("div", outerSongDiv, ("class", "css", Compare.VALUE_STARTS_WITH));
        if (outerLyricsDiv is null) 
            throw new UnreachableException();
        
        List<Tag> songParts = html.ExtractTags(outerLyricsDiv, "div");
        var lyrics = new StringBuilder(256);
        foreach (var songPart in songParts) {
            string part = html.ExtractText(songPart);
            lyrics.Append(part);
            if (part.Contains("Writer(s)")) {
                break;
            }
        }
        string escapedLyrics = escapeHTML(lyrics.ToString());
        Console.WriteLine(escapedLyrics);
    }
    [Test]
    public void fullLyricsScrap(){
        string input = File.ReadAllText(RESOURCE_DIR_PATH + "lyrics.html");
        HtmlDoc html = new HtmlDoc(input);
        html.DelimitTags(false);
        Tag? tag = html.Find("pre", 
            ("id", "lyric-body-text", Compare.EXACT), 
            ("class", "lyric-body", Compare.VALUE_STARTS_WITH),
            ("dir", "ltr", Compare.EXACT),
            ("data-lang", "en", Compare.EXACT)
            );
        if (tag == null){
            Assert.Fail("First tag not found");
            return;
        }
        string extract = html.ExtractText(tag);
        Console.WriteLine(extract);
    }
    [Test]
    public void youListenerScrap(){
        string input = File.ReadAllText(RESOURCE_DIR_PATH + "youlistener.html");
        HtmlDoc html = new HtmlDoc(input);
        html.DelimitTags(false);
        Tag? firstTag = html.Find("div", ("class", "article-content", Compare.EXACT));
        if (firstTag == null){
            Assert.Fail("First tag not found");
            return;
        }
        Tag? innerTag = html.FindFrom("div", firstTag.StartOffset+1, ("class", "article-content", Compare.EXACT));
        if (innerTag == null){
            Assert.Fail("Inner tag not found");
            return;
        }
        string extract = html.ExtractText(innerTag);
        string escapedLyrics = escapeHTML(extract);
        Console.WriteLine(escapedLyrics);
    }

    public static string escapeHTML(string html) {
        return HttpUtility.HtmlDecode(html);
    }
}