using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Kapusons.Components
{
	/// <summary>
	/// Denotes that an element should be excluded from mapping/serialization.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class| AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
	public class KsNotMappedAttribute : Attribute { }
}
