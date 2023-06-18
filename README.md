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
HtmlDoc html = new HtmlDoc(input);
Tag? tag = html.Find("div", ("class", "inner_div"));
if (tag != null){
    string extract = html.ExtractText(tag);
    Console.WriteLine(extract);
}
```
The output should be
```bash
Inner text
```

TODO:
- add more code snippets
- implement html fetch

