using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageTagger
{
    public enum TagCasing
    {
        None = 0,
        CamelCase,
        PascalCase,
        KebabCase,
        SnakeCase

        // these two are not convertable
        //LowerCase,
        //UpperCase,
    }
    public static class TagFormatUtil
    {
        //rules:
        //  Allowed chars: letters, numbers, dash and underscore
        //  Possible  cases: CamelCase, pascalCase, kebab-case, snake_case
        public static string Fix( string tagText, TagCasing casingFormat, out List<char> rejectedChars)
        {
            var result = FilterChars(tagText, out rejectedChars);
            result = ChangeToSnakeCasing(result);
            return result;
        }

        private static string ChangeToSnakeCasing(string tagText)
        {
            //doing snake casing
            var tmp = tagText.Select((c, index) =>
            {
                if (char.IsUpper(c) && index != 0)
                    return "_" + c;
                else if (c == '-')
                    return "_";
                else
                    return "" + c;
            }).ToArray();
            tagText =  string.Join("", tmp);
            tagText = tagText.ToLower();
            return tagText;
            
        }



        private static string FilterChars(string tagText, out List<char> rejectedChars)
        {
            var rejectedCharsTmp = new List<char>();
            var filtered = tagText.Where(c =>
            {
                var isValid = char.IsLetterOrDigit(c) || c == '-' || c == '_';
                if (!isValid) rejectedCharsTmp.Add(c);
                return isValid;
            }).ToArray();

            rejectedChars = rejectedCharsTmp;
            return new string(filtered);
        }
    }
}
