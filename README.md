<a href="https://www.nuget.org/packages/qckdev.Data.Linq"><img src="https://img.shields.io/nuget/v/qckdev.Data.Linq.svg" alt="NuGet Version"/></a>
<a href="https://sonarcloud.io/dashboard?id=qckdev.Data.Linq"><img src="https://sonarcloud.io/api/project_badges/measure?project=qckdev.Data.Linq&metric=alert_status" alt="Quality Gate"/></a>
<a href="https://sonarcloud.io/dashboard?id=qckdev.Data.Linq"><img src="https://sonarcloud.io/api/project_badges/measure?project=qckdev.Data.Linq&metric=coverage" alt="Code Coverage"/></a>
<a><img src="https://hfrances.visualstudio.com/qckdev/_apis/build/status/qckdev.Data.Linq?branchName=main" alt="Azure Pipelines Status"/></a>


# qckdev.Data.Linq

Contains tools for working with IEnumerable and IQueryable objects.

```cs
using System;	
using System.Collections.Generic;

namespace Entities
{
	sealed class TestHeader
	{
		public Guid TestHeaderId { get; set; }
		public string Name { get; set; }
		
		public IEnumerable<TestLine> Lines { get; set; }
	}

	sealed class TestLine
	{
		public Guid TestLineId { get; set; }
		public Guid TestHeaderId { get; set; }
		public string Description { get; set; }

		public TestHeader Header { get; set; }

	}
}
```

```cs
using System;
using System.Linq;
using qckdev.Data.Linq;


context.TestHeaders
	.Include(x => x.Lines)
	.WhereString("First line",
		x => x.Name,
		x => x.Lines.Select(x => x.Description)
	);

```