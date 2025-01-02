using System.Diagnostics;
using System.Text;
using System.Web;
using NUnit.Framework;
using WebScrapper.scrapper;

namespace WebScrapper.tests;

public class LyricsScrapperTest{
    private static readonly string RESOURCE_DIR_PATH = TestUtils.GetProjectRootDirectory() + "/test_pages/";

    [Test]
    public void fullGeniusScrape(){
        string input = File.ReadAllText(RESOURCE_DIR_PATH + "genius.html");
        
        HtmlDoc html = new HtmlDoc(input);
        html.ReplaceLineBreakWithNewLine(true);
        html.DelimitTags(false);
        StringBuilder fullExtract = new StringBuilder(512);
        List<Tag> tags = html.FindAll("div",
            Compare.KeyAndValuePrefix("class", "Lyrics__Container-sc-1ynbvzw-"), 
            Compare.Exact("data-lyrics-container", "true"));
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
        
        Tag? outerLyricsDiv = html.FindFrom("div", outerSongDiv, 
            Compare.KeyAndValuePrefix("class", "css"));
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
            Compare.Exact("id", "lyric-body-text"), 
            Compare.KeyAndValuePrefix("class", "lyric-body"),
            Compare.Exact("dir", "ltr"),
            Compare.Exact("data-lang", "en")
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
        Tag? firstTag = html.Find("div", Compare.Exact("class", "article-content"));
        if (firstTag == null){
            Assert.Fail("First tag not found");
            return;
        }
        Tag? innerTag = html.FindFrom("div", firstTag.StartOffset+1, Compare.Exact("class", "article-content"));
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