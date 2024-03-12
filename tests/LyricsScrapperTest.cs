using System.Text;
using NUnit.Framework;
using WebScrapper.scrapper;

namespace WebScrapper.tests;

public class LyricsScrapperTest{
    private static readonly string RESOURCE_DIR_PATH = 
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Desktop/scrappeur/";

    [Test]
    public void fullGeniusScrape(){
        string input = File.ReadAllText(RESOURCE_DIR_PATH + "genius.html");
        
        HtmlDoc html = new HtmlDoc(input);
        html.ReplaceLineBreakWithNewLine(true);
        html.DelimitTags(false);
        StringBuilder fullExtract = new StringBuilder(512);
        List<Tag> tags = html.FindAll("div",
            ("class", "Lyrics__Container-sc-1ynbvzw-5 Dzxov"), ("data-lyrics-container", "true"));
        Console.WriteLine("SIZE: " + tags.Count);
        
        foreach (var div in tags){
            string extract = html.ExtractText(div);
            fullExtract.Append(extract).Append('\n');
        }
        
        Console.WriteLine(fullExtract);
    }
    
    [Test]
    public void fullMusixScrap(){
        string input = File.ReadAllText(RESOURCE_DIR_PATH + "musix.html");
        
        HtmlDoc html = new HtmlDoc(input);
        Tag? firstTag = html.Find("span", ("class", "lyrics__content__ok"));
        if (firstTag == null){
            Assert.Fail("First tag not found");
            return;
        }
        
        Tag? mainTag = html.FindFrom("span", firstTag.StartOffset +1,  true, ("class", "lyrics__content__ok"));
        if (mainTag == null){
            Assert.Fail("Main tag not found");
            return;
        }
        string extract1 = html.ExtractText(firstTag);
        string extract2 = html.ExtractText(mainTag);
        Console.WriteLine(extract1 + '\n' + extract2);
    }
    [Test]
    public void fullLyricsScrap(){
        string input = File.ReadAllText(RESOURCE_DIR_PATH + "lyrics.html");
        
        HtmlDoc html = new HtmlDoc(input);
        html.DelimitTags(false);
        Tag? tag = html.Find("pre", 
            ("id", "lyric-body-text"), 
            ("class", "lyric-body"),
            ("dir", "ltr"),
            ("data-lang", "en")
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
        Tag? firstTag = html.Find("div", ("class", "article-content"));
        if (firstTag == null){
            Assert.Fail("First tag not found");
            return;
        }
        Tag? innerTag = html.FindFrom("div", firstTag.StartOffset+1, true, ("class", "article-content"));
        if (innerTag == null){
            Assert.Fail("Inner tag not found");
            return;
        }
        string extract = html.ExtractText(innerTag);
        Console.WriteLine(extract);
    }
}