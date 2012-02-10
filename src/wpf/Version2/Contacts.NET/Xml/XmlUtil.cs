namespace Microsoft.Communications.Contacts.Xml
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Xml;
    using Standard;

    internal static class XmlUtil
    {
        public static string GetBase64String(Stream stream)
        {
            Assert.IsNotNull(stream);

            stream.Position = 0;

            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            return Convert.ToBase64String(bytes, Base64FormattingOptions.InsertLineBreaks);
        }

        public static string GetDateString(DateTime dt)
        {
            return dt.ToString("s", null) + "Z";
        }

        public static DateTime GetModificationDate(XmlNode node)
        {
            Assert.IsNotNull(node);

            var modificationAttribute = (XmlAttribute)node.Attributes.GetNamedItem(SchemaStrings.ModificationDate, SchemaStrings.ContactNamespace);
            Assert.IsNotNull(modificationAttribute);
            return DateTime.Parse(modificationAttribute.Value, null, DateTimeStyles.AdjustToUniversal);
        }

        public static int GetVersion(XmlNode node)
        {
            Assert.IsNotNull(node);

            var versionAttribute = (XmlAttribute)node.Attributes.GetNamedItem(SchemaStrings.Version, SchemaStrings.ContactNamespace);
            Assert.IsNotNull(versionAttribute);
            return int.Parse(versionAttribute.Value, CultureInfo.InvariantCulture);
        }

        public static bool HasNilAttribute(XmlNode node)
        {
            Assert.IsNotNull(node);

            var nilAttribute = node.Attributes.GetNamedItem(SchemaStrings.Nil, SchemaStrings.XsiNamespace) as XmlAttribute;
            if (null == nilAttribute)
            {
                return false;
            }
            return nilAttribute.Value == SchemaStrings.True;
        }

        public static void RemoveNilAttribute(XmlNode node)
        {
            Assert.IsNotNull(node);

            // Doesn't throw on missing attribute.
            // Returns null if it wasn't found.
            node.Attributes.RemoveNamedItem(SchemaStrings.Nil, SchemaStrings.XsiNamespace);
        }

        public static string DateTimeNowString
        {
            get
            {
                return GetDateString(DateTime.UtcNow);
            }
        }
    }
}
