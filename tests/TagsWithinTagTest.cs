﻿using NUnit.Framework;
using WebScrapper.scrapper;

namespace WebScrapper.tests; 

[TestFixture]
public class TagsWithinTagTest {
    [Test]
    public void commentAndExtractionTest() {
        const string input = @"<!DOCTYPE html>
                        <html>
                        <head>
                            <title>Test document</title>
                        </head>
                        <body>
                          <h1>An unordered list:</h1>
                          <ul>
                            <li>This is item 1</li>
                            <!-- Inner comment in between elements-->
                            <li>This is item 2</li>
                            <li>This is item 3</li>
                          </ul>
                        </body>
                        </html>";
        HtmlDoc doc = new HtmlDoc(input);
        Tag? tag = doc.Find("ul");
        if (tag == null) {
            Assert.Fail("Tag is null");
            return;
        }

        List<Tag> listElements = doc.ExtractTags(tag, "li");
        Assert.AreEqual(3, listElements.Count);
        Assert.True(doc.ExtractText(listElements[0]).EndsWith('1'));
        Assert.True(doc.ExtractText(listElements[1]).EndsWith('2'));
        Assert.True(doc.ExtractText(listElements[2]).EndsWith('3'));
    }
    [Test]
    public void extractTagsTest() {
        const string input = @"
                        <ul>
                            <li>This is item 1</li>
                            <li>This is item 2</li>
                            <li key=value>This is item 3</li>
                        </ul>
                        <li>LIST 2, item 1</li>
                        <li>LIST 2, item 2</li>
                        <li>LIST 2, item 3</li>
                        ";
        HtmlDoc doc = new HtmlDoc(input);
        Tag? tag = doc.Find("ul");
        if (tag == null) {
            Assert.Fail("Tag is null");
            return;
        }

        List<Tag> listElements = doc.ExtractTags(tag, "li");
        Assert.AreEqual(3, listElements.Count);
        Assert.True(doc.ExtractText(listElements[0]).EndsWith('1'));
        Assert.True(doc.ExtractText(listElements[1]).EndsWith('2'));
        Assert.True(doc.ExtractText(listElements[2]).EndsWith('3'));
    }
    
    [Test]
    public void divWithinDivTests() {
        const string input = """
                        <div class="wrappers">
                            <div item="1">This is item 1</div>
                            <div item="2">This is item 2</div>
                            <div item="3">This is item 3</div>
                        </div>
                        <div>Other Div1</div>
                        <div>Other Div2</div>
                        <div>Other Div3</div>
                        """;
        HtmlDoc doc = new HtmlDoc(input);
        Tag? tag = doc.Find("div", ("class", "wrappers", Compare.EXACT));
        if (tag == null) {
            Assert.Fail("Tag is null");
            return;
        }

        List<Tag> divElements = doc.ExtractTags(tag, "div");
        Assert.AreEqual(3, divElements.Count);
        int i = 1;
        foreach (var div in divElements) {
            Assert.AreEqual(i.ToString(), div.GetAttribute("item"));
            i++;
        }
        
    }
}