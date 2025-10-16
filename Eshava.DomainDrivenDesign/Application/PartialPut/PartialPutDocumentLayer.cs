using System.Collections.Generic;
using System.Linq;

namespace Eshava.DomainDrivenDesign.Application.PartialPut
{
    public class PartialPutDocumentLayer
	{
		public PartialPutDocumentLayer(object id, string propertyName, IEnumerable<PartialPutOperation> operations, IEnumerable<PartialPutDocumentLayer> layers)
		{
			Id = id;
			PropertyName = propertyName;
			Operations = operations?.ToList() ?? new List<PartialPutOperation>();
			Layers = layers?.ToList() ?? new List<PartialPutDocumentLayer>();
		}

		public object Id { get; }
		public string PropertyName { get; }
		public IReadOnlyCollection<PartialPutOperation> Operations { get; private set; }
		public IReadOnlyCollection<PartialPutDocumentLayer> Layers { get; private set; }

		public void Add(string propertyName, object value, PartialPutOperationType operationType = PartialPutOperationType.Replace)
		{
			var operation = new PartialPutOperation(propertyName, value, operationType);
			var operations = Operations.ToList();
			operations.Add(operation);

			Operations = operations;
		}

		public void Add(object id, string propertyName, IEnumerable<PartialPutOperation> operations, IEnumerable<PartialPutDocumentLayer> childLayers)
		{
			var layer = new PartialPutDocumentLayer(id, propertyName, operations, childLayers);
			var layers = Layers.ToList();
			layers.Add(layer);

			Layers = layers;
		}
	}
}