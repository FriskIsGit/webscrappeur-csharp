using System.Diagnostics;

namespace WebScrapper.scrapper;

/// <summary>
/// Represents an HTML tag which may include attributes in the form of:
/// <code>&lt;tagname prop1="val1" prop2="val2"&gt; </code>
/// </summary>
public class Tag {
    public string Name { get; }
    public int StartOffset { get; internal set; } //offset at which the given tag begins
    public int EndOffset { get; internal set; } = -1;
    public List<(string, string)> Attributes { get; }

    /// <summary>
    /// Searches for attribute by key, if another pair with the same key exists the first pair's value is returned
    /// <returns> string being the pair's value or null if no match is found </returns>
    /// </summary>
    public string? GetAttribute(string key) {
        if (Attributes.Count == 0) {
            return null;
        }

        foreach (var key_value in Attributes) {
            if (key_value.Item1 == key) {
                return key_value.Item2;
            }
        }

        return null;
    }

    public bool CompareAttributes(params Compare[] comparisons) {
        if (Attributes.Count < comparisons.Length) {
            return false;
        }
        
        // Attributes can be provided in a varying order
        foreach (var predicate in comparisons) {
            if (!AttributesMatchPredicate(predicate)) {
                return false;
            }
        }

        return true;
    }

    // Determines if Attributes match given predicate
    private bool AttributesMatchPredicate(Compare predicate) {
        foreach (var (key, value) in Attributes) {
            switch (predicate.strategy) {
                case ComparisonStrategy.EXACT:
                    if (key == predicate.key && value == predicate.value) {
                        return true;
                    }
                    break;
                case ComparisonStrategy.KEY:
                    if (key == predicate.key) {
                        return true;
                    }
                    break;
                case ComparisonStrategy.VALUE:
                    if (value == predicate.value) {
                        return true;
                    }
                    break;
                case ComparisonStrategy.KEY_PREFIX:
                    if (key.StartsWith(predicate.key)) {
                        return true;
                    }
                    break;
                case ComparisonStrategy.VALUE_PREFIX:
                    if (value.StartsWith(predicate.value)) {
                        return true;
                    }
                    break;
                case ComparisonStrategy.KEY_SUFFIX:
                    if (key.EndsWith(predicate.key)) {
                        return true;
                    }
                    break;
                case ComparisonStrategy.VALUE_SUFFIX:
                    if (value.EndsWith(predicate.value)) {
                        return true;
                    }
                    break;
                case ComparisonStrategy.KEY_AND_VALUE_PREFIX:
                    if (key == predicate.key && value.StartsWith(predicate.value)) {
                        return true;
                    }
                    break;
                case ComparisonStrategy.KEY_AND_VALUE_SUFFIX:
                    if (key == predicate.key && value.EndsWith(predicate.value)) {
                        return true;
                    }
                    break;
                case ComparisonStrategy.KEY_PREFIX_AND_VALUE:
                    if (key.StartsWith(predicate.key) && value == predicate.value) {
                        return true;
                    }
                    break;
                case ComparisonStrategy.KEY_SUFFIX_AND_VALUE:
                    if (key.EndsWith(predicate.key) && value == predicate.value) {
                        return true;
                    }
                    break;
                default:
                    throw new UnreachableException("Not all comparison types have been implemented!");
            }
        }

        return false;
    }

    public Tag(string name) {
        StartOffset = 0;
        Name = name;
        Attributes = new List<(string, string)>();
    }

    public Tag(string name, List<(string, string)> attributes) {
        StartOffset = 0;
        Name = name;
        Attributes = attributes;
    }

    public int EstimateEndOffset() {
        int end = StartOffset + 1 + Name.Length;
        foreach (var attribute in Attributes) {
            end += 1 + attribute.Item1.Length + attribute.Item2.Length + 3;
        }
        return end;
    }
    
    public override string ToString() {
        return "Tag{name=" + Name + ", index=" + StartOffset + "}";
    }
}

/// Attribute comparison wrapper
public struct Compare {
    public string key, value;
    public ComparisonStrategy strategy;
    private Compare(string key, string value, ComparisonStrategy strategy) {
        this.key = key;
        this.value = value;
        this.strategy = strategy;
    }

    public static Compare Exact(string key, string value) {
        return new Compare(key, value, ComparisonStrategy.EXACT);
    }
    
    public static Compare Key(string key) {
        return new Compare(key, "", ComparisonStrategy.KEY);
    }
    
    public static Compare Value(string value) {
        return new Compare("", value, ComparisonStrategy.VALUE);
    }
    
    public static Compare KeyPrefix(string keyPrefix) {
        return new Compare(keyPrefix, "", ComparisonStrategy.KEY_PREFIX);
    }
    
    public static Compare ValuePrefix(string valuePrefix) {
        return new Compare("", valuePrefix, ComparisonStrategy.VALUE_PREFIX);
    }
    
    public static Compare KeySuffix(string keySuffix) {
        return new Compare(keySuffix, "", ComparisonStrategy.KEY_SUFFIX);
    }
    
    public static Compare ValueSuffix(string valueSuffix) {
        return new Compare("", valueSuffix, ComparisonStrategy.VALUE_SUFFIX);
    }
    
    public static Compare KeyAndValuePrefix(string key, string valuePrefix) {
        return new Compare(key, valuePrefix, ComparisonStrategy.KEY_AND_VALUE_PREFIX);
    }
    
    public static Compare KeyPrefixAndValue(string keyPrefix, string value) {
        return new Compare(keyPrefix, value, ComparisonStrategy.KEY_PREFIX_AND_VALUE);
    }
    
    public static Compare KeySuffixAndValue(string keySuffix, string value) {
        return new Compare(keySuffix, value, ComparisonStrategy.KEY_SUFFIX_AND_VALUE);
    }
    
    public static Compare KeyAndValueSuffix(string key, string valueSuffix) {
        return new Compare(key, valueSuffix, ComparisonStrategy.KEY_AND_VALUE_SUFFIX);
    }
}

/// <summary>
/// Comparison policy for each pair of attributes.
/// </summary>
public enum ComparisonStrategy {
    ///<summary> Match both key and value </summary>
    EXACT,

    ///<summary> Match only the key </summary>
    KEY,

    ///<summary> Match only the value </summary>
    VALUE,

    ///<summary> Key must be prefixed with given string to match </summary>
    KEY_PREFIX,
    
    ///<summary> Value must be prefixed with given string </summary>
    VALUE_PREFIX,
    
    ///<summary> Key must be suffixed with given string  </summary>
    KEY_SUFFIX,

    ///<summary> Value must be suffixed with given string </summary>
    VALUE_SUFFIX,

    ///<summary> Key must match fully, value must be prefixed with given string </summary>
    KEY_AND_VALUE_PREFIX,

    ///<summary> Key must be prefixed with given string, value must match fully </summary>
    KEY_PREFIX_AND_VALUE,
    
    ///<summary> Key must match fully, value must be suffixed with given string </summary>
    KEY_AND_VALUE_SUFFIX,

    ///<summary> Key must be suffixed with given string, value must match fully </summary>
    KEY_SUFFIX_AND_VALUE,
}
