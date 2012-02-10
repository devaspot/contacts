
namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Text.RegularExpressions;
    using Standard;
    using Xml;

    internal class PropertyNameInfo
    {
        private struct _PTRegex
        {
            public readonly Regex Regex;
            public readonly PropertyNameTypes Type;

            public _PTRegex(PropertyNameTypes typeMap, string regex)
            {
                Regex = new Regex(regex);
                Type = typeMap;
            }
        }

        private const string _GroupNamespace = "namespace";
        private const string _GroupLevel1 = "level1";
        private const string _GroupLevel2 = "level2";
        private const string _GroupLevel3 = "level3";
        private const string _GroupIndex = "index";

        // All properties must begin with an alphabetic character.
        // Only '_' and ',' are allowed non-alphanumeric characters in a property name.
        // private const string _PropertyNameRegex = @"[a-zA-Z][a-zA-Z0-9_,]*";

        // These are insanely difficult to read when catenating string aliases.
        // Just inlining the regex to improve readabilty.
        private static readonly _PTRegex[] _PropertyTypeRegexes = new[]
        {
            new _PTRegex(PropertyNameTypes.SimpleExtensionTopLevel,             @"^\[(?<namespace>[a-zA-Z][a-zA-Z0-9_,]*)\](?<level1>[a-zA-Z][a-zA-Z0-9_,]*)$"),
            new _PTRegex(PropertyNameTypes.SimpleExtensionNode,                 @"^\[(?<namespace>[a-zA-Z][a-zA-Z0-9_,])*\](?<level1>[a-zA-Z][a-zA-Z0-9_,]*)/(?<level2>[a-zA-Z][a-zA-Z0-9_,]*)\[(?<index>[1-9][0-9]*)\]$"),
            new _PTRegex(PropertyNameTypes.SimpleExtensionHierarchicalProperty, @"^\[(?<namespace>[a-zA-Z][a-zA-Z0-9_,]*)\](?<level1>[a-zA-Z][a-zA-Z0-9_,]*)/(?<level2>[a-zA-Z][a-zA-Z0-9_,]*)\[(?<index>[1-9][0-9]*)\]/(?<level3>[a-zA-Z][a-zA-Z0-9_,]*)$"),
            new _PTRegex(PropertyNameTypes.SimpleExtensionCreationProperty,     @"^\[(?<namespace>[a-zA-Z][a-zA-Z0-9_,]*):(?<level2>[a-zA-Z][a-zA-Z0-9_,]*)\](?<level1>[a-zA-Z][a-zA-Z0-9_,]*)$"),
            new _PTRegex(PropertyNameTypes.SchematizedNode,                     @"^(?<level1>[a-zA-Z][a-zA-Z0-9_,]*)/(?<level2>[a-zA-Z][a-zA-Z0-9_,]*)\[(?<index>[1-9][0-9]*)\]$"),
            new _PTRegex(PropertyNameTypes.SchematizedHierarchicalProperty,     @"^(?<level1>[a-zA-Z][a-zA-Z0-9_,]*)/(?<level2>[a-zA-Z][a-zA-Z0-9_,]*)\[(?<index>[1-9][0-9]*)\]/(?<level3>[a-zA-Z][a-zA-Z0-9_,]*)$"),
            new _PTRegex(PropertyNameTypes.SchematizedTopLevelProperty,         @"^(?<level1>[a-zA-Z][a-zA-Z0-9_,]*)$"),
            // This RegEx is the same as a schematized top-level property.  Distinguish between the two by checking for a known collection.
            //private const Regex _SchematizedCollectionRegex = new Regex(@"^(?<level1>[a-zA-Z][a-zA-Z0-9_,]*)$");
        };

        public PropertyNameInfo(string propertyName)
        {
            Assert.IsNeitherNullNorEmpty(propertyName);

            SourceString = propertyName;

            Match match = null;

            foreach (_PTRegex regex in _PropertyTypeRegexes)
            {
                match = regex.Regex.Match(propertyName);
                if (match.Success)
                {
                    Type = regex.Type;

                    // If this looks like a schematized top level property, check whether it's a collection.
                    Assert.AreNotEqual(PropertyNameTypes.SchematizedCollectionName, Type);
                    if (PropertyNameTypes.SchematizedTopLevelProperty == Type)
                    {
                        if (!string.IsNullOrEmpty(SchemaStrings.TryGetCollectionNodeName(propertyName)))
                        {
                            Type = PropertyNameTypes.SchematizedCollectionName;
                        }
                    }
                    break;
                }

                match = null;
            }

            if (null == match)
            {
                throw new SchemaException("The given property name is improperly formatted.");
            }

            SimpleExtensionNamespace = match.Groups[_GroupNamespace].Value;
            Level1 = match.Groups[_GroupLevel1].Value;
            Level2 = match.Groups[_GroupLevel2].Value;
            Level3 = match.Groups[_GroupLevel3].Value;
            Index = match.Groups[_GroupIndex].Value;
        }

        public string SourceString { get; private set; }
        public string SimpleExtensionNamespace { get; private set; }
        public string Level1 { get; private set; }
        public string Level2 { get; private set; }
        public string Level3 { get; private set; }
        public string Index { get; private set; }
        public PropertyNameTypes Type { get; private set; }

        public override string ToString()
        {
            return string.Format(null, "{0}:\n\tType: {1}\n\tNamespace: {2}\n\tLevel1: {3}\n\tLevel2: {4}\n\tLevel3: {5}\n\tIndex: {6}",
                SourceString, Type, SimpleExtensionNamespace, Level1, Level2, Level3, Index);
        }
    }

    internal enum PropertyNameTypes
    {
        /// <summary>
        /// Invalid property type.
        /// </summary>
        None = 0,
        /// <summary>
        /// simple extension top level property, e.g. "[WOW]Server"
        /// </summary>
        SimpleExtensionTopLevel,
        /// <summary>
        /// simple extension hierarchical property, e.g. "[WOW]ServerCollection/Server[1]/Url"
        /// </summary>
        SimpleExtensionHierarchicalProperty,
        /// <summary>
        /// simple extension node, e.g. "[WOW]ServerCollection/Server[1]"
        /// </summary>
        SimpleExtensionNode,
        /// <summary>
        /// simple extension hierarchical creation property, e.g. "[WOW:Server]ServerCollection"
        /// </summary>
        SimpleExtensionCreationProperty,
        /// <summary>
        /// schematized top level property, e.g. "Notes"
        /// </summary>
        SchematizedTopLevelProperty,
        /// <summary>
        /// schematized hierarchical property: "EmailAddressCollection/EmailAddress[1]/Address"
        /// </summary>
        SchematizedHierarchicalProperty,
        /// <summary>
        /// schematized hierarchical node: "EmailAddressCollection/EmailAddress[1]"
        /// </summary>
        SchematizedNode,
        /// <summary>
        /// schematized collection name: "EmailAddressCollection"
        /// </summary>
        SchematizedCollectionName,
    }

    internal static class PropertyNameUtil
    {
        /// <summary>
        /// Tries to parse the index out of a property name that might represent an array node.
        /// </summary>
        /// <param name="propertyName">The array node property name that contains the index to parse.</param>
        /// <returns>The zero-based parsed index if this appears to be an array node.  Otherwise returns -1.</returns>
        public static int GetIndexFromNode(string propertyName)
        {
            var pni = new PropertyNameInfo(propertyName);
            Assert.AreNotEqual(pni.Type, PropertyNameTypes.None);

            if (pni.Type != PropertyNameTypes.SchematizedNode && pni.Type != PropertyNameTypes.SimpleExtensionNode)
            {
                return -1;
            }
            Assert.IsNeitherNullNorEmpty(pni.Index);

            int i;
            // PropertyNameInfo should have blocked negative values here.
            if (!Int32.TryParse(pni.Index, out i))
            {
                return -1;
            }
            Assert.IsTrue(i > 0);

            --i;

            return i;
        }

        /// <summary>
        /// Does the given string represent a legal array-node property name?
        /// </summary>
        /// <param name="propertyName">The string to check.</param>
        /// <returns>Returns whether the string is a legal array node or level-3 property..</returns>
        public static bool IsPropertyValidNode(string propertyName)
        {
            var pni = new PropertyNameInfo(propertyName);
            Assert.AreNotEqual(pni.Type, PropertyNameTypes.None);
            return pni.Type == PropertyNameTypes.SchematizedNode || pni.Type == PropertyNameTypes.SimpleExtensionNode;
        }

        /*
        [TestMethod]
        public void ParseIndexFromNodeTest()
        {
            Assert.AreEqual(-1, PropertyNameUtil.GetIndexFromNode(string.Format(PropertyNames.NameFormattedNameFormat, 1)));
            Assert.AreEqual(0, PropertyNameUtil.GetIndexFromNode(PropertyNames.NameCollection + PropertyNames.NameArrayNode + "[1]"));
            Assert.AreEqual(4, PropertyNameUtil.GetIndexFromNode(PropertyNames.NameCollection + PropertyNames.NameArrayNode + "[5]"));
            Assert.AreEqual(-1, PropertyNameUtil.GetIndexFromNode(""));
            Assert.AreEqual(-1, PropertyNameUtil.GetIndexFromNode(PropertyNames.NameCollection + PropertyNames.NameArrayNode + "[-5]"));
            Assert.AreEqual(-1, PropertyNameUtil.GetIndexFromNode(PropertyNames.NameCollection + PropertyNames.NameArrayNode + "[0]"));
            Assert.AreEqual(-1, PropertyNameUtil.GetIndexFromNode(PropertyNames.NameCollection + PropertyNames.NameArrayNode + "[Preferred]"));
            Assert.AreEqual(Int32.MaxValue, PropertyNameUtil.GetIndexFromNode(PropertyNames.NameCollection + PropertyNames.NameArrayNode + "[" + (((long)Int32.MaxValue) + 1) + "]"));
            Assert.AreEqual(-1, PropertyNameUtil.GetIndexFromNode(PropertyNames.NameCollection + PropertyNames.NameArrayNode + "[" + (((long)Int32.MaxValue) + 2) + "]"));
        }
        */
    }


}
