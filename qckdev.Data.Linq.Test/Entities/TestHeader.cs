using System;
using System.Collections.Generic;

namespace qckdev.Data.Linq.Test.Entities
{
    sealed class TestHeader
    {

        public Guid TestHeaderId { get; set; }
        public string Name { get; set; }


        public IEnumerable<TestLine> Lines { get; set; }

    }
}
