using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace WebScrapper.scrapper;

public class HtmlDoc {
    private static readonly HttpClient Client = new();
    private readonly string html;
    private readonly int len;
    private char concatChar;
    private bool delimitTags = true;
    private bool brToNewline = false;

    public HtmlDoc(string contents) {
        html = contents;
        len = html.Length;
        concatChar = '\n';
    }

    public void SetConcatenatingChar(char given) {
        concatChar = given;
    }

    //<br> -> '\n'
    public void ReplaceLineBreakWithNewLine(bool enabled) {
        brToNewline = enabled;
    }

    /// <summary>During extraction - raw text can be found in both tags and subtags. The text will be concatenated
    /// using the <c>concatChar</c> (a delimiter)</summary>
    /// <param name="enabled"> whether to use a delimiter when concatenating text extracted from two or more tags</param>
    /// <returns>The raw Tag object or null if not found</returns>
    public void DelimitTags(bool enabled) {
        delimitTags = enabled;
    }

    /// <summary>Find a tag by name matching given attributes.</summary>
    /// <param name="tag"> the tag name to find</param>
    /// <param name="attributes"> the attributes to look for</param>
    /// <returns>The raw Tag object or null if not found</returns>
    public Tag? Find(string tag, params (string, string)[] attributes) {
        return FindFrom(tag, 0, false, attributes);
    }

    /// <summary>Find a tag by name. The tag may or may not contain attributes.</summary>
    /// <param name="tag"> the tag name to find</param> 
    /// <returns>The raw Tag object or null if not found</returns>
    public Tag? Find(string tag) {
        return FindFrom(tag, 0, true);
    }

    public List<Tag> FindAllFrom(string tag, int from, bool matchTagOnly, params (string, string)[] attributes) {
        List<Tag> tags = new List<Tag>();
        int cursor = from;
        while (cursor < len) {
            Tag? traverserTag = FindFrom(tag, cursor, matchTagOnly, attributes);
            if (traverserTag == null) {
                break;
            }

            tags.Add(traverserTag);
            if (traverserTag.EndOffset == -1) {
                cursor = traverserTag.StartOffset + 1;
            }
            else {
                cursor = traverserTag.EndOffset + 1;
            }
        }

        return tags;
    }

    public List<Tag> FindAll(string tag, params (string, string)[] attributes) {
        return FindAllFrom(tag, 0, false, attributes);
    }
    public List<Tag> FindAll(string tag) {
        return FindAllFrom(tag, 0, true);
    }

    /// <summary>Linearly searches for a tag in a given HtmlDoc returning the first Tag that matches predicates</summary>
    /// <param name="tag"> the tag name to find</param> 
    /// <param name="from"> the index to search from</param> 
    /// <param name="matchTagOnly"> whether to skip comparing <c>attributes</c> </param>
    /// <param name="attributes"> (key, value) pairs of strings representing tag attributes to match against only if <c>match_attrib</c> is true</param>
    /// <returns>The <c>Tag</c> object or <c>null</c> if not found</returns>
    public Tag? FindFrom(string tag, int from, bool matchTagOnly, params (string, string)[] attributes) {
        for (int i = from; i < len; i++) {
            char chr = html[i];
            switch (chr) {
                case '<':
                    bool closing = i + 1 < len && html[i + 1] == '/';
                    if (closing) {
                        i++;
                        continue;
                    }

                    bool hasAttributes = false;
                    //parse tag
                    int j = i + 1;
                    for (; j < len; j++) {
                        if (html[j] == ' ') {
                            hasAttributes = true;
                            break;
                        }

                        if (html[j] == '>') {
                            break;
                        }
                    }

                    string tagName = html[(i + 1)..j];
                    Tag parsedTag = new Tag(tagName);
                    int end = -1;
                    if (hasAttributes) {
                        end = parseAttributes(parsedTag, j + 1);
                    }

                    if ((matchTagOnly && parsedTag.Name == tag) || parsedTag.Matches(tag, attributes)) {
                        parsedTag.StartOffset = i;
                        return parsedTag;
                    }

                    if (hasAttributes) {
                        i = end;
                    }

                    break;
            }
        }

        return null;
    }

    //returns '>' index where tag ends
    private int parseAttributes(Tag parsedTag, int from) {
        //from cursor should be placed after tag name
        StringBuilder name = new StringBuilder();
        StringBuilder value = new StringBuilder();
        bool afterEqual = false, inQuoteVal = false;
        for (int i = from; i < len; i++) {
            char chr = html[i];
            switch (chr) {
                case '=':
                    afterEqual = true;
                    break;
                case ' ':
                    if (afterEqual) {
                        if (value.Length > 0 && !inQuoteVal) {
                            parsedTag.Attributes.Add((name.ToString(), value.ToString()));
                            afterEqual = false;
                            name.Clear();
                            value.Clear();
                        }
                        else if (inQuoteVal) {
                            value.Append(' ');
                        }
                    }

                    break;
                case '"':
                    if (afterEqual) {
                        inQuoteVal = !inQuoteVal;
                    }

                    break;
                case '/':
                    if (!inQuoteVal) {
                        //assume it's a self-closing tag
                        int endOffset = i + 1;
                        for (int j = i + 1; j < len; j++) {
                            if (html[j] != '>')
                                continue;
                            
                            endOffset = j + 1;
                            break;
                        }

                        if (name.Length != 0 && value.Length != 0) {
                            parsedTag.Attributes.Add((name.ToString(), value.ToString()));
                        }
                        return endOffset;
                    }
                    
                    if (afterEqual) {
                        value.Append(chr);
                    }
                    else {
                        name.Append(chr);
                    }

                    break;
                case '>':
                    if (!inQuoteVal) {
                        if (name.Length > 0 && value.Length > 0) {
                            parsedTag.Attributes.Add((name.ToString(), value.ToString()));
                        }

                        return i;
                    }

                    if (afterEqual) {
                        value.Append(chr);
                    }
                    else {
                        name.Append(chr);
                    }

                    break;
                default:
                    if (afterEqual) {
                        value.Append(chr);
                    }
                    else {
                        name.Append(chr);
                    }

                    break;
            }
        }

        return -1;
    }

    /// <summary>
    /// Extracts text from given tag and all its sub-tags beginning at <c>StartOffset</c>. The extraction will begin
    /// at the top level tag and will continue until either the tag is closed or the end of document is reached.
    /// Text extracted from sub-tags will be concatenated using the specified delimiter that can be set by calling
    /// <c>SetConcatenatingChar()</c>. The <c>EndOffset</c> field of the tag will be set to the index where parsing
    /// finished if its value was -1. </summary>
    /// <param name="tag">the tag to extract text from</param> 
    /// <returns>The raw extracted html</returns>
    public string ExtractText(Tag tag) {
        if (tag.StartOffset < 0 || tag.StartOffset >= len) {
            return "";
        }

        bool append = false, inQuotes = false;
        bool concatenate = false;
        Stack<string> stack = new Stack<string>();
        StringBuilder text = new StringBuilder();
        for (int i = tag.StartOffset; i < len; i++) {
            char chr = html[i];
            switch (chr) {
                case ' ':
                    if (append) {
                        //based on default case
                        if (delimitTags && concatenate && text.Length > 0) {
                            text.Append(concatChar);
                        }

                        text.Append(' ');
                        concatenate = false;
                    }

                    break;
                case '"':
                    if (append) {
                        text.Append('"');
                    }
                    else {
                        inQuotes = !inQuotes;
                    }

                    break;
                //cannot exist in text in this form, must be a character code
                case '<':
                    bool closing = i + 1 < len && html[i + 1] == '/';
                    if (closing) {
                        i++;
                    }

                    bool hasAttributes = false;
                    int tagEnd = i + 1;
                    for (int j = tagEnd; j < len; j++) {
                        char c = html[j];
                        switch (c) {
                            case ' ':
                                if (closing) {
                                    // handle closing tags with a whitespace </div >, they shouldn't have attributes
                                    continue;
                                }

                                hasAttributes = true;
                                tagEnd = j;
                                goto exitLoop;
                            case '>':
                                tagEnd = j;
                                goto exitLoop;
                        }
                    }

                    exitLoop:

                    string anyTag = html[(i + 1)..tagEnd];
                    if (brToNewline && anyTag.StartsWith(Tags.LineBreak)) {
                        text.Append('\n');
                    }

                    //move cursor to '>' or ' ' before attributes
                    i = tagEnd;
                    bool voidTag = anyTag[^1] == '/'; //last char
                    if (closing) {
                        // while stack is not exhausted
                        while (stack.Count > 0 && stack.Pop() != anyTag) { }

                        if (stack.Count == 0) {
                            if (tag.EndOffset == -1)
                                tag.EndOffset = i;
                            return text.ToString();
                        }
                    }
                    else if (!voidTag) {
                        stack.Push(anyTag);
                    }

                    append = !hasAttributes;
                    concatenate = true;
                    break;
                case '>':
                    concatenate = true;
                    append = true;
                    break;
                default:
                    if (append) {
                        if (delimitTags && concatenate && text.Length > 0) {
                            text.Append(concatChar);
                        }

                        concatenate = false;
                        text.Append(chr);
                    }

                    break;
            }
        }

        //if unclosed should exit due to length here
        if (tag.EndOffset == -1)
            tag.EndOffset = len;
        return text.ToString();
    }

    public static string fetchHtml(string url) {
        Client.Timeout = TimeSpan.FromSeconds(6);
        var getRequest = new HttpRequestMessage {
            RequestUri = new Uri(url),
            Method = HttpMethod.Get,
        };
        getRequest.Headers.UserAgent.ParseAdd("Mozilla/5.0 Gecko/20100101");
        getRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
        getRequest.Headers.AcceptLanguage.ParseAdd("en-US;q=0.7");
        getRequest.Headers.Add("Set-GPC", "1");
        var response = Client.Send(getRequest);
        if (response.StatusCode == HttpStatusCode.OK) {
            string contentOk = response.Content.ReadAsStringAsync().Result;
            return contentOk ?? "";
        }

        Console.WriteLine("Response: " + response.StatusCode);
        string content = response.Content.ReadAsStringAsync().Result;
        return content ?? "";
    }
}