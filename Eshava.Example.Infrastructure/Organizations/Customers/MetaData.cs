using System;
using System.Collections.Generic;

namespace Eshava.Example.Infrastructure.Organizations.Customers
{
	internal class MetaData
	{
		public int Version { get; set; }
		public IEnumerable<DateTime> Timestamps { get; set; }
	}
}