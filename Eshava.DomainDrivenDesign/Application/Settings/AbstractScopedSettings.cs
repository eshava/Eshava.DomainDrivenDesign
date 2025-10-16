namespace Eshava.DomainDrivenDesign.Application.Settings
{
	public abstract class AbstractScopedSettings
	{
		public abstract object GetScopeInformationForLogging();
	}
}