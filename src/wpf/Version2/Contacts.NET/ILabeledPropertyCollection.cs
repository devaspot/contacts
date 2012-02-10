/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.Communications.Contacts
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;

    public interface ILabeledPropertyCollection<T> : IList<T>, INotifyCollectionChanged
    {
        [SuppressMessage(
            "Microsoft.Naming",
            "CA1716:IdentifiersShouldNotMatchKeywords",
            MessageId = "Default",
            Justification="The 'default' keyword in the checked languages shouldn't cause ambiguities or compilation problems.  I think this is the correct name for the property.")]
        T Default
        {
            get;
            set;
        }

        int DefaultIndex
        {
            get;
            set;
        }

        ILabelCollection GetLabelsAt(int index);

        string GetNameAt(int index);

        string GetNameAt(params string[] labels);

        int IndexOfLabels(params string[] labels);

        T this[string label] { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional")]
        T this[string label1, string label2] { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional")]
        T this[string label1, string label2, string label3] { get; set; }

        [SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
        T this[params string[] labels] { get; set; }

        void Add(T item, params string[] labels);
    }
}
