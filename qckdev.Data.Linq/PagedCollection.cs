using System;
using System.Collections.Generic;
using System.Text;

namespace qckdev.Data.Linq
{
    public sealed class PagedCollection<TSource> : IDataCollection<TSource>
    {

        public int CurrentPage { get; set; }
        public int Pages { get; set; }
        public int Total { get; set; }
        public IEnumerable<TSource> Items { get; set; }

    }
}
