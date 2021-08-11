using System;

namespace qckdev.Data.Linq.Test.Entities
{
    sealed class TestLine
    {

        public Guid TestLineId { get; set; }
        public Guid TestHeaderId { get; set; }
        public string Description { get; set; }
        public bool Disabled { get; set; }


        public TestHeader Header { get; set; }

    }
}
