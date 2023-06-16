using System.Text;

namespace WebScrapper.scrapper;

public class HtmlDoc{
    private readonly string html;
    private readonly int len;
    private char concatenatingChar;
    public HtmlDoc(string contents){
        html = contents;
        len = html.Length;
        concatenatingChar = '\n';
    }

    public void SetConcatenatingChar(char given){
        concatenatingChar = given;
    }
    
    public Tag? Find(string tag, params (string, string)[] attributes){
        return FindFrom(tag, 0, attributes);
    }
    public Tag? Find(string tag){
        return FindFrom(tag, 0);
    }

    public Tag? FindFrom(string tag, int from, params (string, string)[] attributes){
        for (int i = from; i < len; i++){
            char chr = html[i];
            switch (chr){
                case '<':
                    bool closing = i + 1 < len && html[i + 1] == '/';
                    if (closing){
                        i++;
                        continue;
                    }

                    bool hasAttributes = false;
                    //parse tag
                    int j = i + 1;
                    for (; j < len; j++){
                        if (html[j] == ' '){
                            hasAttributes = true;
                            break;
                        }
                        if (html[j] == '>'){
                            break;
                        }
                    }

                    string tagName = html[(i+1)..j];
                    Tag parsedTag = new Tag(tagName);
                    int end = -1;
                    if (hasAttributes){
                        end = parseAttributes(parsedTag, j + 1);
                    }

                    if (parsedTag.Matches(tag, attributes)){
                        parsedTag.StartOffset = i;
                        return parsedTag;
                    }

                    if (hasAttributes){
                        i = end;
                    }
                    break;
            }
        }
        return null;
    }

    //returns '>' index where tag ends
    private int parseAttributes(Tag parsedTag, int from){
        //from cursor should be placed after tag name
        StringBuilder name = new StringBuilder();
        StringBuilder value = new StringBuilder();
        bool afterEqual = false, inQuoteVal = false;
        for (int i = from; i < len; i++){
            char chr = html[i];
            switch (chr){
                case '=':
                    afterEqual = true;
                    break;
                case ' ':
                    if (afterEqual){
                        if (value.Length > 0 && !inQuoteVal){
                            parsedTag.Attributes.Add((name.ToString(), Unstringify(value)));
                            afterEqual = false;
                            name.Clear();
                            value.Clear();
                        }
                        else if(inQuoteVal){
                            value.Append(' ');
                        }
                    }
                    break;
                case '"':
                    if (afterEqual){
                        inQuoteVal = !inQuoteVal;
                    }
                    break;
                case '/':
                    if (!afterEqual && !inQuoteVal && name.Length == 0 && value.Length == 0){
                        //assume it's a self-closing tag
                        int endOffset = i+1;
                        for (int j = i+1; j < len; j++){
                            if (html[j] == '>'){
                                endOffset = j+1;
                                break;
                            }
                        }
                        return endOffset;
                    }
                    if (afterEqual){
                        value.Append(chr);
                    }
                    else{
                        name.Append(chr);
                    }
                    break;
                case '>':
                    if (!inQuoteVal){
                        //consume current pair and exit
                        parsedTag.Attributes.Add((name.ToString(), Unstringify(value)));
                        return i; 
                    }
                    if (afterEqual){
                        value.Append(chr);
                    }
                    else{
                        name.Append(chr);
                    }
                    break;
                default:
                    if (afterEqual){
                        value.Append(chr);
                    }
                    else{
                        name.Append(chr);
                    }
                    break;
            }
        }
        return -1;
    }
    /// <summary>
    /// Extracts text from given tag and all its sub-tags. <br/>
    /// Text extracted from sub-tags will be concatenated using the specified concatenating char
    /// which can be set by calling <code>HtmlDoc.SetConcatenatingChar()</code>
    /// </summary>
    /// <param name="tag"></param>
    /// <returns>The raw extracted html</returns>
    public string ExtractText(Tag tag){
        if (Tags.ForbiddenToClose(tag.Name)){
            //throw new ArgumentException("This tag cannot enclose any text");
            return "";
        }

        bool append = false, inQuotes = false;
        bool concatenate = false;
        Stack<string> stack = new Stack<string>();
        StringBuilder text = new StringBuilder();
        for (int i = tag.StartOffset; i < len; i++){
            char chr = html[i];
            switch (chr){
                case ' ':
                    if (append){
                        text.Append(' ');
                    }
                    break;
                case '"':
                    if (append){
                        text.Append('"');
                    }
                    else{
                        inQuotes = !inQuotes;
                    }
                    break;
                //cannot exist in text in this form, must be a character code
                case '<':
                    bool closing = i + 1 < len && html[i + 1] == '/';
                    if (closing){
                        i++;
                    }
                    
                    bool hasAttributes = false;
                    int tagEnd = i+1;
                    for (int j = tagEnd; j < len; j++){
                        char c = html[j];
                        switch (c){
                            case ' ':
                                if (closing){
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
                    
                    string anyTag = html[(i+1)..tagEnd];
                    //move cursor to '>' or ' ' before attributes
                    i = tagEnd;
                    if (closing){
                        // while stack is not exhausted
                        while (stack.Count > 0 && stack.Pop() != anyTag){
                        }
                        if (stack.Count == 0){
                            return text.ToString();
                        }
                    }else if (!Tags.ForbiddenToClose(anyTag)){
                        stack.Push(anyTag);
                    }
                    //else don't push it because it will never be closed

                    if (hasAttributes){
                        append = false;
                    }
                    concatenate = true;
                    break;
                case '>':
                    concatenate = true;
                    append = true;
                    break;
                default:
                    if (append){
                        if (concatenate && text.Length > 0){
                            concatenate = false;
                            text.Append(concatenatingChar);
                        }
                        else{
                            //this ensures we don't prepend the character when length is 0
                            concatenate = false;
                        }
                        
                        text.Append(chr);
                    }
                    break;
            }
        }
        //if unclosed should exit due to length here
        return text.ToString();
    }
    
    private static string Unstringify(string str){
        if (str[0] == '"' && str[^1] == '"'){
            return str[1..^1];
        }

        return str;
    }
    private static string Unstringify(StringBuilder str){
        if (str.Length < 2 || str[0] != '"' || str[^1] != '"') 
            return str.ToString();

        return str.ToString(1, str.Length-2);
    }
}