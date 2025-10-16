namespace Eshava.DomainDrivenDesign.Infrastructure.Settings
{
	public static class CommonSettings
	{
		/// <summary>
		/// Controls whether a delete action or an update action is called in the repositories when a domain model or value object is deactivated.
		/// Hint: The functionality of the soft delete must be implemented yourself. Can be overwritten in every repository
		/// true: Update action is called
		/// false: Delete action is called
		/// </summary>
		public static bool EnableSoftDelete { get; set; } = true;
	}
}