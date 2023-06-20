## C# Lightweight web scrappeur
This project was made in .NET `7.0`, if you know what you're doing feel free to change `<TargetFramework>`

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
The output should be
```bash
Inner text
```
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
Output should be
```bash
StartText
Inner text
Ending text
```

Change the concatenating char
```csharp
doc.SetConcatenatingChar(';')
```

Then the output would be
```bash
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

