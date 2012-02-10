/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

using System.Text;
using System.Collections.Generic;
using System;
using System.IO;

namespace Microsoft.Communications.Contacts
{

	internal interface IContactProperties
	{
		string CreateArrayNode(string collectionName, bool appendNode);
		bool DeleteArrayNode(string nodeName);
		bool DeleteProperty(string propertyName);
		bool DoesPropertyExist(string property);
		ContactProperty GetAttributes(string propertyName);
		Stream GetBinary(string propertyName, out string propertyType);
		DateTime? GetDate(string propertyName);
		string GetLabeledNode(string collection, string[] labelFilter);
		IList<string> GetLabels(string node);
		IEnumerable<ContactProperty> GetPropertyCollection(string collectionName, string[] labelFilter, bool anyLabelMatches);
		string GetString(string propertyName);
		bool IsReadonly { get; }
		bool IsUnchanged { get; }
		Stream SaveToStream();
		void SetBinary(string propertyName, Stream value, string valueType);
		void SetDate(string propertyName, DateTime value);
		void SetString(string propertyName, string value);
		void AddLabels(string node, ICollection<string> labels);
		void ClearLabels(string node);
		bool RemoveLabel(string node, string label);
		string StreamHash { get; }
	}
}
