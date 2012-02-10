/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

using System.Collections.Generic;

namespace Microsoft.Communications.Contacts
{
	public interface ILabelCollection : ICollection<string>
	{
		string PropertyName { get; }
		new bool Add(string item);
		bool AddRange(params string[] items);
	}
}
