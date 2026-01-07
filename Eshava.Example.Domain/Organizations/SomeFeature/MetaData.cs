using Eshava.DomainDrivenDesign.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Eshava.Example.Domain.Organizations.SomeFeature
{
	public class MetaData : AbstractValueObject
	{
		public MetaData(int version, IEnumerable<DateTime> timestamps)
		{
			Version = version;
			Timestamps = timestamps;
		}

		[Range(0, Int32.MaxValue)]
		public int Version { get; }
		public IEnumerable<DateTime> Timestamps { get; }
	}
}