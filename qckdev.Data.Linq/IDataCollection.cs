using System;
using System.Collections.Generic;

namespace qckdev.Data.Linq
{
    public interface IDataCollection<TSource>
    {

        IEnumerable<TSource> Items { get; set; }

    }
}
