using NUnit.Framework;
using WebScrapper.scrapper;

namespace WebScrapper.tests;

[TestFixture]
public class TextScrapperTest{
    [Test]
    public void test1(){
        const string input = "<h2 div=\"title is apart\">Internal text</h2>";
        HtmlDoc html = new HtmlDoc(input);
        Tag? tag = html.Find("h2", ("div", "title is apart"));
        if (tag == null){
            Assert.Fail("Tag not found");
            return;
        }
        string extract = html.ExtractText(tag);
        Assert.AreEqual("Internal text", extract);
    }
    [Test]
    public void innerScrape(){
        const string input = "<div class = \"outer_div\" property=\"value\">Start" +
                             "Text<div class=\"inner_div\">Super inner text</div>Ending  text</div>";
        HtmlDoc html = new HtmlDoc(input);
        Tag? tag = html.Find("div", ("class", "inner_div"));
        if (tag == null){
            Assert.Fail("Tag not found");
            return;
        }
        string extract = html.ExtractText(tag);
        Assert.AreEqual("Super inner text", extract);
    }
    [Test]
    public void multiScrape(){
        const string input = "<div class = \"outer_div\" property=\"value\">Start" +
                             "Text<div class=\"inner_div\">Super inner text</div>Ending  text</div>";
        HtmlDoc html = new HtmlDoc(input);
        Tag? tag = html.Find("div", ("class", "outer_div"), ("property", "value"));
        if (tag == null){
            Assert.Fail("Tag not found");
            return;
        }
        string extract = html.ExtractText(tag);
        Console.WriteLine(extract);
        Assert.AreEqual("StartText\nSuper inner text\nEnding  text", extract);
    }
    [Test]
    public void fullWebsiteScrape(){
        string testPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Desktop/out.html";
        string input = File.ReadAllText(testPath);
        
        HtmlDoc html = new HtmlDoc(input);
        Tag? tag = html.Find("div", 
            ("class", "Lyrics__Container-sc-1ynbvzw-5 Dzxov"), ("data-lyrics-container", "true"));
        if (tag == null){
            Assert.Fail("Tag not found");
            return;
        }
        Console.WriteLine(tag);
        string extract = html.ExtractText(tag);
        Console.WriteLine(extract);
    }
}