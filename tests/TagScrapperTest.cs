using NUnit.Framework;
using WebScrapper.scrapper;

namespace WebScrapper.tests;

[TestFixture]
public class TagScrapperTest{
    [Test]
    public void oneTagOneAttribute(){
        const string input = "<h2 class=\"title is apart\">Internal text</h2>";
        HtmlDoc doc = new HtmlDoc(input);
        Tag? tag = doc.Find("h2", ("class", "title is apart"));
        if (tag == null){
            Assert.Fail("Tag Not Found");
            return;
        }
        
        Assert.AreEqual("h2", tag.Name);
    }
    [Test]
    public void twoAttributes(){
        const string input = "<h2 class=\"title is apart\" href=\"https://link.com/v1.1/end\">Internal text</h2>";
        HtmlDoc doc = new HtmlDoc(input);
        Tag? tag = doc.Find("h2", ("class", "title is apart"), ("href", "https://link.com/v1.1/end"));
        if (tag == null){
            Assert.Fail("Tag Not Found");
            return;
        }
        Assert.AreEqual(2, tag.Attributes.Count);
        Assert.AreEqual("h2", tag.Name);
    }

    [Test]
    public void happyScenario(){
        const string input = "<div class=\"value\">";
        Tag? tag = new HtmlDoc(input).Find("div", ("class", "value"));
        if (tag == null){
            Assert.Fail("Tag Not Found");
        }
    }
    [Test]
    public void spaceInValue(){
        const string input = "<div class=\"split apart\">";
        Tag? tag = new HtmlDoc(input).Find("div", ("class", "split apart"));
        if (tag == null){
            Assert.Fail("Tag Not Found");
        }
    }
    [Test]
    public void classSpace(){
        const string input = "<div class =\"value\">";
        Tag? tag = new HtmlDoc(input).Find("div", ("class", "value"));
        if (tag == null){
            Assert.Fail("Tag Not Found");
        }
    }
    [Test]
    public void twoSpaceEqual(){
        const string input = "<div class = \"value\">";
        Tag? tag = new HtmlDoc(input).Find("div", ("class", "value"));
        if (tag == null){
            Assert.Fail("Tag Not Found");
        }
    }
    [Test]
    public void selfClosing(){
        const string input = "<script src=\"foobar.js\" /> ";
        Tag? tag = new HtmlDoc(input).Find("script", ("src", "foobar.js"));
        if (tag == null){
            Assert.Fail("Tag Not Found");
        }
    }
    [Test]
    public void closed(){
        const string input = "<script src=\"foobar.js\"></script>";
        Tag? tag = new HtmlDoc(input).Find("script", ("src", "foobar.js"));
        if (tag == null){
            Assert.Fail("Tag Not Found");
            return;
        }
        Assert.AreEqual(0, tag.StartOffset);
    }
    [Test]
    public void angleBrackets(){
        const string input = "<script stat=\"put <div> in front\" /> ";
        Tag? tag = new HtmlDoc(input).Find("script", ("stat", "put <div> in front"));
        if (tag == null){
            Assert.Fail("Tag Not Found");
            return;
        }
        Assert.AreEqual(0, tag.StartOffset);
    }
    [Test]
    public void sizeNoQuotes(){
        const string input = "<picture width=512 height=256>";
        Tag? tag = new HtmlDoc(input).Find("picture", ("width", "512"), ("height", "256"));
        if (tag == null){
            Assert.Fail("Tag Not Found");
            return;
        }
        Assert.AreEqual(0, tag.StartOffset);
    }
    [Test]
    public void full(){
        string contents = File.ReadAllText(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Desktop/scrappeur/website.html");
        HtmlDoc doc = new HtmlDoc(contents);
        Tag? starterTag = doc.Find("span", ("class", "lyrics__content__ok"));
        if (starterTag == null){
            Assert.Fail("Starter tag not Found");
            return;
        }
        Tag? nextTag = doc.FindFrom("span", starterTag.StartOffset + 1, ("class", "lyrics__content__ok"));
        if (nextTag == null){
            Assert.Fail("Next tag not Found");
            return;
        }
        Assert.AreNotEqual(starterTag.StartOffset, nextTag.StartOffset);
        
        Console.WriteLine(nextTag.StartOffset);
        Console.WriteLine(contents.Substring(nextTag.StartOffset, 1000));
    }
}