[![NuGet Version](https://img.shields.io/nuget/v/qckdev.Data.Linq.svg)](https://www.nuget.org/packages/qckdev.Data.Linq)
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=qckdev.Data.Linq&metric=alert_status)](https://sonarcloud.io/dashboard?id=qckdev.Data.Linq)
[![Code Coverage](https://sonarcloud.io/api/project_badges/measure?project=qckdev.Data.Linq&metric=coverage)](https://sonarcloud.io/dashboard?id=qckdev.Data.Linq)
![Azure Pipelines Status](https://hfrances.visualstudio.com/qckdev/_apis/build/status/qckdev.Data.Linq?branchName=master)


# qckdev.Data.Linq

Contains tools for working with IEnumerable and IQueryable objects.

## 🛠️ Installation

```bash
dotnet add package qckdev.Data.Linq
```

## ⚡ Quick Start

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

## 🤝 Contributing
Issues and pull requests are welcome! See the contribution guidelines (coming soon).

## 📜 License
This project is licensed under the terms of the [MIT License](LICENSE).
