using ImageTagger.UI;
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
    public static class FormatUtil
    {
        //rules:
        //  Allowed chars: letters, numbers, dash and underscore
        //  Possible  cases: CamelCase, pascalCase, kebab-case, snake_case
        public static string FixTag(string tagText)//, TagCasing casingFormat = TagCasing.SnakeCase)
        {
            return TmpFix(tagText);
            //return FixTag(tagText, casingFormat, out _);
        }
        /*
        public static string FixTag( string tagText, TagCasing casingFormat, out List<char> rejectedChars)
        {
            var result = FilterChars(tagText, out rejectedChars);
            result = ChangeToSnakeCasing(result);
            return result;
        }
        */
        public static void FixAllTagsInFiles(ImageFiles imageFiles)//, TagCasing casingFormat = TagCasing.SnakeCase)
        {
            var cancel = false;
            var cancelContext = new CancelDialogDataContext();
            var cancelDialog = new CancelDialog();
            cancelContext.CurrentValue = 0;
            cancelContext.MaxValue = imageFiles.Count;
            cancelContext.PerformAction = () =>
            {
                foreach (var filePath in imageFiles)
                {
                    if (cancel) break;
                    var cleaned = ImageFileUtil.GetImageTags(filePath);
                    ImageFileUtil.ApplyTagsToImage(filePath, cleaned);
                    cancelContext.CurrentValue++;
                }
            };
            cancelContext.OnCancel = (s, e) => { cancel = true; cancelDialog.Close(); };
            cancelContext.OnClosed = (s, e) => { cancel = true; cancelDialog.Close(); };
            cancelDialog.SetContext(cancelContext);
            cancelDialog.ShowDialog();
        }

        public static string FixCategory(string category)
        {
            return category.ToLower();
        }
        private static string TmpFix(string tagText)
        {
            if (tagText.ToUpper().StartsWith("DATA")) return tagText;
            tagText = tagText.ToLower();
            tagText = FilterChars(tagText, out _);
            return tagText;
        }

        private static string ChangeToSnakeCasing(string tagText)
        {
            /*
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
            tagText = tagText.ToLower();*/
            return tagText;
            
        }

        private static bool IsCharValid(char c)
        {
            var result = char.IsLetterOrDigit(c) || c == '{' || c == '}' || c == '[' || c == ']' || c == '"' || c == '!' || c == ':' || c == '.' || c == ',' || c == '*' || c == '%';
            //if snake case result || c == '_';
            // if kebab case || c == '-';
            return result;
        }

        public static bool ContainsInvalidCharacters(string tagText)
        {
            foreach (var c in tagText)
            {
                var isValid = IsCharValid(c);
                if (!isValid) return true;
            }
            return false;
        }

        private static string FilterChars(string tagText, out List<char> rejectedChars)
        {
            var rejectedCharsTmp = new List<char>();
            var filtered = tagText.Where(c =>
            {
                var isValid = IsCharValid(c);
                if (!isValid) rejectedCharsTmp.Add(c);
                return isValid;
            }).ToArray();

            rejectedChars = rejectedCharsTmp;
            return new string(filtered);
        }
    }
}
