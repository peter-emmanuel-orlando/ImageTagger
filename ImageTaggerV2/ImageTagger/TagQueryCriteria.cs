using ImageTagger.DataModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ImageTagger
{

    public enum OrderDirection
    {
        ASC = 0,
        DESC,
        RANDOM,
    }
    public enum OrderBy
    {
        Date = 0,
        Name,
    }
    public enum FilterBy
    {
        NSFW,
        Explicit,
        Suggestive,
        Untagged,
    }
    public enum FilterState
    {
        Allow = 0,
        Exclusive,
        Exclude
    }

    public class Filter
    {
        public FilterBy filterBy;
        public FilterState FilterState;//allow, exclusive, exclude

        public Filter(FilterBy filterBy, FilterState filterState)
        {
            this.filterBy = filterBy;
            FilterState = filterState;
        }
    }


    public class TagQueryCriteria
    {
        public HashSet<string> anyOfThese { get; set; } = new HashSet<string>();
        public HashSet<string> allOfThese { get; set; } = new HashSet<string>();
        public HashSet<string> noneOfThese { get; set; } = new HashSet<string>();
        public static Dictionary<OrderBy, string> orderByClauses { get; } = new Dictionary<OrderBy, string>();
        public static Dictionary<FilterBy, string> filterByClauses { get; } = new Dictionary<FilterBy, string>();

        public Filter[] filters { get; set; }
        public OrderBy orderBy { get; set; }
        public OrderDirection orderDirection { get; set; } 

        static TagQueryCriteria()
        {

            orderByClauses.Add(OrderBy.Date, @" System.ItemDate");
            orderByClauses.Add(OrderBy.Name, @" System.ItemName");

            filterByClauses.Add(FilterBy.NSFW, @" System.Keywords LIKE 'nsfw'");
            filterByClauses.Add(FilterBy.Explicit, @" System.Keywords LIKE 'explicit'");
            filterByClauses.Add(FilterBy.Suggestive, @" System.Keywords LIKE 'suggestive'");
            filterByClauses.Add(FilterBy.Untagged, @" (System.Keywords = '' OR System.Keywords IS NULL)");
        }
        public TagQueryCriteria(IEnumerable<string> anyOfThese = null, IEnumerable<string> allOfThese = null, IEnumerable<string> noneOfThese = null, OrderBy orderBy = OrderBy.Date, OrderDirection orderDirection = OrderDirection.DESC, params Filter[] filters)
        {
            this.anyOfThese.AddRange(anyOfThese);
            this.allOfThese.AddRange(allOfThese);
            this.noneOfThese.AddRange(noneOfThese);
            this.orderBy = orderBy;
            this.orderDirection = orderDirection;
            this.filters = filters;//new Filter[] { new Filter (FilterBy.Explicit, true) };//
        }

        public bool MatchesCriteria(ImageInfo subject)
        {
            throw new System.NotImplementedException();
        }

        public string GetQueryClause( string searchPath, out bool randomize)
        {
            var evalsToTrue = @"(System.Keywords IS NULL OR System.Keywords IS NOT NULL )";
            var evalsToFalse = @"(System.Keywords IS NULL AND System.Keywords IS NOT NULL )";

            var result = $"SELECT System.ItemPathDisplay, System.Keywords, System.ItemDate FROM SystemIndex WHERE SCOPE='{searchPath}'" +
                @" AND (System.ItemName LIKE '%.jpg' OR System.ItemName LIKE '%.jpeg')";
            result += $" AND  ({evalsToTrue}";
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

            //filter
            foreach (var filter in filters)
            {
                if (filter.FilterState == FilterState.Exclusive)
                    result += $" AND {filterByClauses[filter.filterBy]}";
                if (filter.FilterState == FilterState.Exclude)
                    result += $" AND NOT {filterByClauses[filter.filterBy]}";
            }

            //order
            randomize = orderDirection == OrderDirection.RANDOM;
            if (!randomize)
            {
                result += $" ORDER BY {orderByClauses[orderBy]} {orderDirection.GetName()}";
                foreach (OrderBy item in Enum.GetValues(typeof(OrderBy)))
                {
                    result += $", {orderByClauses[item]} ASC";
                }
            }

            //end query
            //end query
            //end query

            result = result.Replace('*', '%');
            Debug.WriteLine(result);

            return result;
        }
    }
}
