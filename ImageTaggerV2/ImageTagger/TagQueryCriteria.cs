using ImageTagger.DataModels;
using System;
using System.Collections.Generic;

namespace ImageTagger
{
    public class TagQueryCriteria
    {
        public HashSet<string> anyOfThese { get; set; } = new HashSet<string>();
        public HashSet<string> allOfThese { get; set; } = new HashSet<string>();
        public HashSet<string> noneOfThese { get; set; } = new HashSet<string>();

        public TagQueryCriteria(IEnumerable<string> anyOfThese = null, IEnumerable<string> allOfThese = null, IEnumerable<string> noneOfThese = null)
        {
            this.anyOfThese.AddRange(anyOfThese);
            this.allOfThese.AddRange(allOfThese);
            this.noneOfThese.AddRange(noneOfThese);
        }

        public bool MatchesCriteria(ImageInfo subject)
        {
            throw new System.NotImplementedException();
        }

        public string GetQueryClause()
        {
            var evalsToTrue = @"(System.Keywords IS NULL OR System.Keywords IS NOT NULL )";
            var evalsToFalse = @"(System.Keywords IS NULL AND System.Keywords IS NOT NULL )";
            var result = $" ({evalsToTrue}";


            //begin criteria
            //begin criteria
            //begin criteria

            //match any
            result += $" AND ( {evalsToFalse}";
            if (anyOfThese.Count == 0)
                result += $" OR {evalsToTrue}";
            else
                foreach (var any in anyOfThese)
                {
                    result += $" OR System.Keywords LIKE '{any}'";
                }
            result += " )";

            //match all
            result += $" AND ( {evalsToTrue}";
            if (allOfThese.Count == 0)
                result += $" AND {evalsToTrue}";
            else
                foreach (var all in allOfThese)
                {
                    result += $" AND System.Keywords LIKE '{all}'";
                }
            result += " )";

            //match none
            result += $" AND NOT ( {evalsToFalse}";
            if (noneOfThese.Count == 0)
                result += $" OR {evalsToFalse}";
            else
                foreach (var none in noneOfThese)
                {
                    result += $" OR System.Keywords LIKE '{none}'";
                }
            result += " )";

            //end criteria
            //end criteria
            //end criteria

            result += " ) ";
            result.Replace('*', '%');

            return result;
        }
    }
}
