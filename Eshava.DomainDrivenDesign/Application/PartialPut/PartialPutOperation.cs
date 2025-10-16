namespace Eshava.DomainDrivenDesign.Application.PartialPut
{
    public class PartialPutOperation
	{
		public PartialPutOperation(string propertyName, object value, PartialPutOperationType type)
		{
			PropertyName = propertyName;
			Value = value;
			Type = type;
		}

		public string PropertyName { get; }
		public PartialPutOperationType Type { get; }
		public object Value { get; }
	}
}