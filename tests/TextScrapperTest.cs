using System.Text;
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
                             "Text<div class=\"inner_div\">Inner text</div>Ending  text</div>";
        HtmlDoc html = new HtmlDoc(input);
        Tag? tag = html.Find("div", ("class", "inner_div"));
        if (tag == null){
            Assert.Fail("Tag not found");
            return;
        }
        string extract = html.ExtractText(tag);
        Assert.AreEqual("Inner text", extract);
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
    public void outOfBounds(){
        const string input = "<picture width=512 height=256>alternative text</picture>";
        HtmlDoc html = new HtmlDoc(input);
        Tag? tag = html.Find("picture", ("width", "512"), ("height", "256"));
        if (tag == null){
            Assert.Fail("Tag Not Found");
            return;
        }
        tag.StartOffset = 80;
        Assert.AreEqual("", html.ExtractText(tag));
    }
    [Test]
    public void oneSpace(){
        const string input = "<picture id=84 >1 space</picture>";
        HtmlDoc html = new HtmlDoc(input);
        Tag? tag = html.Find("picture", ("id", "84"));
        if (tag == null){
            Assert.Fail("Tag Not Found");
            return;
        }
        Assert.AreEqual("1 space", html.ExtractText(tag));
    }
    [Test]
    public void threeSpaces(){
        const string input = "<picture id=84   >3 space</picture>";
        HtmlDoc html = new HtmlDoc(input);
        Tag? tag = html.Find("picture", ("id", "84"));
        if (tag == null){
            Assert.Fail("Tag Not Found");
            return;
        }
        Assert.AreEqual("3 space", html.ExtractText(tag));
    }
    [Test]
    public void startWithSpace(){
        const string input = "<picture id=84> 123z</picture>";
        HtmlDoc html = new HtmlDoc(input);
        Tag? tag = html.Find("picture", ("id", "84"));
        if (tag == null){
            Assert.Fail("Tag Not Found");
            return;
        }
        Assert.AreEqual(" 123z", html.ExtractText(tag));
    }
    [Test]
    public void startAndEndWithSpace(){
        const string input = "<picture id=84> 123z </picture>";
        HtmlDoc html = new HtmlDoc(input);
        Tag? tag = html.Find("picture", ("id", "84"));
        if (tag == null){
            Assert.Fail("Tag Not Found");
            return;
        }
        Assert.AreEqual(" 123z ", html.ExtractText(tag));
    }
    [Test]
    public void startWithSpaceNoAttrib(){
        const string input = "<picture> 123z</picture>";
        HtmlDoc html = new HtmlDoc(input);
        Tag? tag = html.Find("picture");
        if (tag == null){
            Assert.Fail("Tag Not Found");
            return;
        }
        Assert.AreEqual(" 123z", html.ExtractText(tag));
    }
    
    [Test]
    public void brAsNewLines1(){
        const string input = "<div>abc<br>xyz</div>";
        HtmlDoc html = new HtmlDoc(input);
        html.ReplaceLineBreakWithNewLine(true);
        html.DelimitTags(false);

        Tag? tag = html.Find("div");
        if (tag == null){
            Assert.Fail("First tag not found");
            return;
        }
        string extract = html.ExtractText(tag);
        Assert.AreEqual("abc\nxyz", extract);
    }
    [Test]
    public void brAsNewLines2(){
        const string input = "<div>abc<br/>xyz</div>";
        HtmlDoc html = new HtmlDoc(input);
        html.ReplaceLineBreakWithNewLine(true);
        html.DelimitTags(false);

        Tag? tag = html.Find("div");
        if (tag == null){
            Assert.Fail("First tag not found");
            return;
        }
        string extract = html.ExtractText(tag);
        Assert.AreEqual("abc\nxyz", extract);
    }
    [Test]
    public void attributesWithManySpacesVar2(){
        const string input = "<picture hey  =  \" 125z\"   245=f321    >";
        Tag? tag = new HtmlDoc(input).Find("picture", ("hey", " 125z"), ("245", "f321"));
        if (tag == null){
            Assert.Fail("Tag Not Found");
            return;
        }
        Assert.AreEqual(0, tag.StartOffset);
    }
}