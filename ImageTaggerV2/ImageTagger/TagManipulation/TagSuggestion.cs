using System.Collections.Generic;
using ImageTagger.DataModels;

namespace ImageTagger
{
    public class TagSuggestion : ImageTag
    {
        public TagSuggestion(ImageTag tag, double confidenceLevel, string category) : this(tag.TagName, confidenceLevel, category)
        {
        }
        public TagSuggestion(string tag, double confidenceLevel, string category) : base(tag)
        {
            this.confidenceLevel = confidenceLevel;
            this.category = category;
        }

        public double confidenceLevel { get; }
        public string category { get; }
    }

}



