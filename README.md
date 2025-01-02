# C# Lightweight web scrapper
Many frameworks nowadays are pretty beefy and contain features most developers won't ever use.
This project was made to facilitate extracting data from websites in a concise yet simple way.
Only the default libraries are used (including `System.Net.Http` since .NET Core 2.1).
The strategy used to parse the webpage is linear as opposed to the usual tree-based approach.
The code has been tested and works most of the time, but it's not guaranteed to work (yet).

# Usage:

## 1. Extracting multi-line text
```html
<div class="outer_div" property="cf_async_73913">
    StartText
    <div class="inner_div">
        Inner text
    </div>
    Ending text
</div>
```

```csharp
HtmlDoc doc = new HtmlDoc(html);
Tag? tag = doc.Find("div", Compare.Value("inner_div"));
if (tag != null){
    string extract = doc.ExtractText(tag);
    Console.WriteLine(extract);
}
```
Output: `Inner text`

---
**Alternatively we can extract text from the outer tag and all its sub-tags.** <br>
Each attribute pair has its own comparison policy <br>
Use `Compare.ValuePrefix` if attributes are obfuscated either intentionally or due to `css` auto-generating gibberish.
    
```csharp
HtmlDoc doc = new HtmlDoc(html);
Tag? tag = doc.Find("div", 
    Compare.Exact("class", "outer_div"),
    Compare.ValuePrefix("cf_async")
);
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
Output:
```
StartTextInner textEnding text
```
---
## 2. Retrieving tags from a tag
```html
<ul>
    <li>item 1</li>
    <li>item 2</li>
    <li>item 3</li>
</ul>
```

```csharp
HtmlDoc doc = new HtmlDoc(input);
Tag? tag = doc.Find("ul");
if (tag == null) {
    return;
}

List<Tag> listElements = doc.ExtractTags(tag, "li");
```
---
## 3. Fetch html from URL with browser headers
```csharp
string html = HtmlDoc.fetchHtml("https://toscrape.com");
HtmlDoc doc = new HtmlDoc(html);
```
---
## 4. Retrieving attribute values
```csharp
Tag? tag = new HtmlDoc(input).Find("a", Compare.Key("href"));
if (tag == null) {
    return;
}
string link = tag.GetAttribute("href");
Console.WriteLine(link);
```