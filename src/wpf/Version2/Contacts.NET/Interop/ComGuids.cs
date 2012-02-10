/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

namespace Microsoft.ContactsBridge.Interop
{
	public static class IIDGuid
    {
        public const string IConnectionPointContainer   = "B196B284-BAB4-101A-B69C-00AA00341D07";
        public const string IEnumConnectionPoints       = "B196B285-BAB4-101A-B69C-00AA00341D07";
        public const string IConnectionPoint            = "B196B286-BAB4-101A-B69C-00AA00341D07";
        public const string IEnumConnections            = "B196B287-BAB4-101A-B69C-00AA00341D07";
        public const string IPropertyNotifySink         = "9BFBBC02-EFF1-101A-84ED-00AA00341D07";
        public const string IPersist                    = "0000010C-0000-0000-C000-000000000046";
        public const string IPersistFile                = "0000010B-0000-0000-C000-000000000046";
        public const string IPersistStream              = "00000109-0000-0000-C000-000000000046";
        public const string IPersistStreamInit          = "7FD52380-4E07-101B-AE2D-08002B2EC713";

		public const string IContactManager = "ad553d98-deb1-474a-8e17-fc0c2075b738";
			//"D89D1A00-ECA1-438F-86F3-EF1536C2973F";
        // ad553d98-deb1-474a-8e17-fc0c2075b738

        public const string IContactCollection          = "00AB7D7B-A67A-483B-9FE2-FC9D5812FEE5";
		// b6afa338-d779-11d9-8bde-f66bad1e3f3a

        public const string IContactProperties          = "F829D953-DBDB-4B49-B183-ACF2226F1E15";
        // 70dd27dd-5cbd-46e8-bef0-23b6b346288f

        public const string IContactPropertyCollection  = "FEA51CC1-A2B4-4116-AF58-FCB91E0651D8";
        public const string IContact                    = "C7FC7C32-D26B-48E0-B0EB-0C17AE79AEB0";
    }

    internal static class CLSIDGuid
    {
        public const string Contact                     = "61B68808-8EEE-4FD1-ACB8-3D804C8DB056";
        public const string ContactManager              = "7165C8AB-AF88-42BD-86FD-5310B4285A02";
    }
}
