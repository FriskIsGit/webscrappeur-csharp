using NUnit.Framework;
using WebScrapper.scrapper;

namespace WebScrapper.tests;

[TestFixture]
public class TagScrapperTest {
    [Test]
    public void oneTagOneAttribute() {
        const string input = "<h2 class=\"title is apart\">Internal text</h2>";
        HtmlDoc doc = new HtmlDoc(input);
        Tag? tag = doc.Find("h2", Compare.Exact("class", "title is apart"));
        if (tag == null) {
            Assert.Fail("Tag Not Found");
            return;
        }

        Assert.AreEqual("h2", tag.Name);
    }

    [Test]
    public void twoAttributes() {
        const string input = "<h2 class=\"title is apart\" href=\"https://link.com/v1.1/end\">Internal text</h2>";
        HtmlDoc doc = new HtmlDoc(input);
        Tag? tag = doc.Find("h2", Compare.Exact("class", "title is apart"),
            Compare.Exact("href", "https://link.com/v1.1/end"));
        if (tag == null) {
            Assert.Fail("Tag Not Found");
            return;
        }

        Assert.AreEqual(2, tag.Attributes.Count);
        Assert.AreEqual("h2", tag.Name);
    }

    [Test]
    public void happyScenario() {
        const string input = "<div class=\"value\">";
        Tag? tag = new HtmlDoc(input).Find("div", Compare.Exact("class", "value"));
        if (tag == null) {
            Assert.Fail("Tag Not Found");
        }
    }

    [Test]
    public void spaceInValue() {
        const string input = "<div class=\"split apart\">";
        Tag? tag = new HtmlDoc(input).Find("div", Compare.Exact("class", "split apart"));
        if (tag == null) {
            Assert.Fail("Tag Not Found");
        }
    }

    [Test]
    public void classSpace() {
        const string input = "<div class =\"value\">";
        Tag? tag = new HtmlDoc(input).Find("div", Compare.Exact("class", "value"));
        if (tag == null) {
            Assert.Fail("Tag Not Found");
        }
    }

    [Test]
    public void twoSpaceEqual() {
        const string input = "<div class = \"value\">";
        Tag? tag = new HtmlDoc(input).Find("div", Compare.Exact("class", "value"));
        if (tag == null) {
            Assert.Fail("Tag Not Found");
        }
    }

    [Test]
    public void selfClosing() {
        const string input = "<script src=\"foobar.js\" /> ";
        Tag? tag = new HtmlDoc(input).Find("script", Compare.Exact("src", "foobar.js"));
        if (tag == null) {
            Assert.Fail("Tag Not Found");
        }
    }

    [Test]
    public void closed() {
        const string input = "<script src=\"foobar.js\"></script>";
        Tag? tag = new HtmlDoc(input).Find("script", Compare.Exact("src", "foobar.js"));
        if (tag == null) {
            Assert.Fail("Tag Not Found");
            return;
        }

        Assert.AreEqual(0, tag.StartOffset);
    }

    [Test]
    public void angleBrackets() {
        const string input = "<script stat=\"put <div> in front\" /> ";
        Tag? tag = new HtmlDoc(input).Find("script", Compare.Exact("stat", "put <div> in front"));
        if (tag == null) {
            Assert.Fail("Tag Not Found");
            return;
        }

        Assert.AreEqual(0, tag.StartOffset);
    }

    [Test]
    public void sizeNoQuotes() {
        const string input = "<picture width=512 height=256>";
        Tag? tag = new HtmlDoc(input).Find("picture", 
            Compare.Exact("width", "512"), Compare.Exact("height", "256"));
        if (tag == null) {
            Assert.Fail("Tag Not Found");
            return;
        }

        Assert.AreEqual(0, tag.StartOffset);
    }

    [Test]
    public void attributesWithManySpacesVar1() {
        const string input = "<picture width =  512      height=256>";
        Tag? tag = new HtmlDoc(input).Find("picture", 
            Compare.Exact("width", "512"),
            Compare.Exact("height", "256"));
        if (tag == null) {
            Assert.Fail("Tag Not Found");
            return;
        }

        Assert.AreEqual(0, tag.StartOffset);
    }

    [Test]
    public void checkKeyOnly() {
        const string input = "<a><myTag href=\"https://s.domain.com\"/></a>";
        Tag? tag = new HtmlDoc(input).Find("myTag", Compare.Key("href"));
        if (tag == null) {
            Assert.Fail("Tag Not Found");
            return;
        }

        Assert.AreEqual(1, tag.Attributes.Count);
        Assert.AreEqual("href", tag.Attributes[0].Item1);
    }
    
    [Test]
    public void ensureAttributes() {
        const string input = "<a2><input src=\"script.js\" /></a2>";
        Tag? tag = new HtmlDoc(input).Find("input", Compare.Exact("src", "script.js"));
        if (tag == null) {
            Assert.Fail("Tag Not Found");
            return;
        }

        Assert.AreEqual(1, tag.Attributes.Count);
        Assert.AreEqual("script.js", tag.Attributes[0].Item2);
    }
    
    [Test]
    public void noSpaceSelfClosing() {
        const string input = "<p img=\"pic.jpg\"/>";
        Tag? tag = new HtmlDoc(input).Find("p");
        if (tag == null) {
            Assert.Fail("Tag is null");
            return;
        }

        Assert.AreEqual(1, tag.Attributes.Count);
        Assert.AreEqual("pic.jpg", tag.Attributes[0].Item2);
    }
    
    [Test]
    public void extractAttribute() {
        const string input = "<a alt=\"noise\" href=\"https://www.w3schools.com\">Visit W3Schools</a>";
        Tag? tag = new HtmlDoc(input).Find("a");
        if (tag == null) {
            Assert.Fail("Tag is null");
            return;
        }

        foreach (var attrib in tag.Attributes) {
            if (attrib.Item1 == "href") {
                Assert.AreEqual("https://www.w3schools.com", attrib.Item2);
                return;
            }
        }
    }
    [Test]
    public void getAttrib() {
        const string input = "<a href=\"https://link.com\">Visit the LINK</a>";
        Tag? tag = new HtmlDoc(input).Find("a");
        if (tag == null) {
            Assert.Fail("Tag is null");
            return;
        }

        string? value = tag.GetAttribute("href");
        Assert.AreEqual(value, "https://link.com");
    }
    [Test]
    public void emptyValue() {
        const string input = "<a empty=\"\" next=\"1234\">Enjoy our website</a>";
        Tag? tag = new HtmlDoc(input).Find("a");
        if (tag == null) {
            Assert.Fail("Tag is null");
            return;
        }

        string? valueNext = tag.GetAttribute("next");
        Assert.AreEqual(valueNext, "1234");
        string? valueEmpty = tag.GetAttribute("empty");
        Assert.AreEqual(valueEmpty, "");
    }
    
    [Test]
    public void whitespaceSeparatedValue() {
        const string input = "<a faulty= \"ok\">Enjoy our website</a>";
        Tag? tag = new HtmlDoc(input).Find("a");
        if (tag == null) {
            Assert.Fail("Tag is null");
            return;
        }

        string? valueNext = tag.GetAttribute("faulty");
        Assert.AreEqual(valueNext, "ok");
    }
    
    [Test]
    public void trailingSpacesAfterValues() {
        const string input = "<a unclosedValue=\"ok\"   another = \"smth\"  >Enjoy our website</a>";
        Tag? tag = new HtmlDoc(input).Find("a");
        if (tag == null) {
            Assert.Fail("Tag is null");
            return;
        }

        string? valueNext = tag.GetAttribute("unclosedValue");
        Assert.AreEqual(valueNext, "ok");
        
        string? another = tag.GetAttribute("another");
        Assert.AreEqual(another, "smth");
    }
}