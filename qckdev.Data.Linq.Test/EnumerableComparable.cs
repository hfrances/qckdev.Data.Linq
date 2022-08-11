using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace qckdev.Data.Linq.Test
{
    sealed class EnumerableComparable<TEntity> : IComparable<EnumerableComparable<TEntity>>, IEquatable<EnumerableComparable<TEntity>>
    {

        public IEnumerable<TEntity> Enumerable { get; }

        public EnumerableComparable(IEnumerable<TEntity> collection)
        {
            this.Enumerable = collection;
        }

        public int CompareTo([AllowNull] EnumerableComparable<TEntity> other)
        {
            IComparer<TEntity> elementComparer = Comparer<TEntity>.Default;

            using (IEnumerator<TEntity> iterator1 = this.Enumerable.GetEnumerator())
            using (IEnumerator<TEntity> iterator2 = other.Enumerable.GetEnumerator())
            {
                while (true)
                {
                    bool next1 = iterator1.MoveNext();
                    bool next2 = iterator2.MoveNext();

                    if (!next1 && !next2) // Both sequences finished
                    {
                        return 0;
                    }
                    if (!next1) // Only the first sequence has finished
                    {
                        return -1;
                    }
                    if (!next2) // Only the second sequence has finished
                    {
                        return 1;
                    }

                    // Both are still going, compare current elements
                    int comparison = elementComparer.Compare(iterator1.Current, iterator2.Current);
                    // If elements are non-equal, we're done
                    if (comparison != 0)
                    {
                        return comparison;
                    }
                }
            }
        }

        public bool Equals([AllowNull] EnumerableComparable<TEntity> other)
            => this.CompareTo(other) == 0;
    }
}
