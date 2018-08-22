namespace XCase.Model
{
	internal interface _ImplVersionedElement: IVersionedElement
	{
		/// <summary>
		/// Version of the element
		/// </summary>
		new Version Version { get; set; }

		/// <summary>
		/// Version where the element appeared first. 
		/// Returns value of <see cref="Version"/> property if 
		/// this is the first version of the element. 
		/// </summary>
		new IVersionedElement FirstVersion { get; set; }

		new VersionManager VersionManager { get; set; }
	}
}