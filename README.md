## C# Lightweight web scrapper
Many frameworks nowadays are pretty beefy and contain features most developers won't ever use.
This project was made to facilitate extracting data from websites in a concise yet simple way 
(prior knowledge about the framework is not required to get the job done).
Only the default libraries are used (including `System.Net.Http` since .NET Core 2.1).
The code has been tested and works most of the time, but it's not guaranteed to work as expected every time since html
can be weird.

## Usage:

1. Let's suppose you want to extract multi-line text from the html below
```html
<div class="outer_div" property="random73913">
    StartText
    <div class="inner_div">
        Inner text
    </div>
    Ending text
</div>
```

It could be done like so:
```csharp
HtmlDoc doc = new HtmlDoc(html);
Tag? tag = doc.Find("div", ("class", "inner_div", Compare.EXACT));
if (tag != null){
    string extract = doc.ExtractText(tag);
    Console.WriteLine(extract);
}
```
Output: `Inner text`

---
#### 2. Alternatively we can extract text from the outer tag and all its sub-tags. <br>
Each attribute pair has its own comparison policy and follows the format: `(key, value, comparison_policy)` <br>
Use `Compare.VALUE_STARTS_WITH` if attributes are obfuscated either intentionally or due to `css` auto-generating gibberish.
    
```csharp
HtmlDoc doc = new HtmlDoc(html);
Tag? tag = doc.Find("div", 
    ("class", "outer_div", Compare.EXACT),
    ("property", "random", Compare.VALUE_STARTS_WITH)
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
#### 3. Retrieve all matching tags at once
```csharp
HtmlDoc doc = new HtmlDoc(html);
List<Tag> tags = doc.FindAll("script");
foreach(var tag in tags){
    string extract = doc.ExtractText(tag);
    Console.WriteLine(extract);
}
```
---
#### 4. Fetch html from URL with browser headers
```csharp
string html = HtmlDoc.fetchHtml("https://toscrape.com");
HtmlDoc doc = new HtmlDoc(html);
```
---
#### 5. Retrieve link from an attribute
```csharp
Tag? tag = new HtmlDoc(input).Find("a", ("href", "", Compare.KEY_ONLY));
if (tag == null) {
    return;
}
string link = tag.GetAttribute("href");
Console.WriteLine(link);
```

