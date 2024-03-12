## C# Lightweight web scrapper
Many frameworks nowadays are pretty beefy and contain features most developers won't ever use.
This project was made to facilitate extracting data from websites in a concise yet simple way 
(prior knowledge about the framework is not required to get the job done).
Only the default libraries are used (including `System.Net.Http` since .NET Core 2.1).
The code has been tested and works most of the time, but it's not guaranteed to work as expected every time since html
can be weird.

## Usage:

Let's suppose this is the html we want to scrap
```html
<div class="outer_div" property="value">
    StartText
    <div class="inner_div">
        Inner text
    </div>
    Ending text
</div>
```

This is how we'd do it
```csharp
HtmlDoc doc = new HtmlDoc(html);
Tag? tag = doc.Find("div", ("class", "inner_div"));
if (tag != null){
    string extract = doc.ExtractText(tag);
    Console.WriteLine(extract);
}
```
Output: `Inner text`

---
Alternatively we can extract text from the outer tag and all its sub-tags
```csharp
HtmlDoc doc = new HtmlDoc(html);
Tag? tag = doc.Find("div", ("class", "outer_div"), ("property", "value"));
if (tag != null){
    string extract = doc.ExtractText(tag);
    Console.WriteLine(extract);
}
```
Output:
```
StartText
Inner text
Ending text
```

Change the concatenating char
```csharp
doc.SetConcatenatingChar(';')
```
Output:
```
StartText;Inner text;Ending text
```
Or disable concatenation completely
```csharp
doc.DelimitTags(false)
```
---
### Tips
Retrieve all matching tags at once
```csharp
HtmlDoc doc = new HtmlDoc(html);
List<Tag> tags = doc.FindAll("script");
foreach(var tag in tags){
    string extract = doc.ExtractText(tag);
    Console.WriteLine(extract);
}
```
---
Fetch html from URL with browser headers
```csharp
string html = HtmlDoc.fetchHtml("https://toscrape.com");
HtmlDoc doc = new HtmlDoc(html);
```
Retrieve link from an attribute
```csharp
Tag? tag = new HtmlDoc(input).Find("a");
if (tag == null) {
    return;
}

foreach (var attrib in tag.Attributes) {
    if (attrib.Item1 == "href") {
        Console.WriteLine(attrib.Item2);
    }
}
```

