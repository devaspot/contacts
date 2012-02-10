using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

using Microsoft.SyncCenter;
using System.Runtime.InteropServices.ComTypes;
using System.Collections;

namespace Synrc
{
	public class HandlerEnumerator : IEnumString
	{
		private int returnedItemCount = 0;

		IList<HANDLERINFO> handlers = new List<HANDLERINFO>();

		public HandlerEnumerator(IList<HANDLERINFO> hands)
		{
			handlers = hands;
		}

		#region IEnumString Members

		public void Clone(out IEnumString ppenum)
		{
			ppenum = null;
		}

		public int Next(int celt, string[] rgelt, IntPtr pceltFetched)
		{
			if (returnedItemCount >= handlers.Count)
			{
				Marshal.WriteInt32(pceltFetched, 0);
				//pceltFetched = 0;
				return 1;  // S_FALSE
			}
			else
			{
				int added = 0;
				ArrayList strings = new ArrayList();
				for (int i = returnedItemCount; i < returnedItemCount + celt && i < handlers.Count; i++)
				{
					strings.Add(handlers[i].HandlerName);
					added++;
				}

				returnedItemCount += added;
				Marshal.WriteInt32(pceltFetched, added);
				//pceltFetched = added;
			}

			return 0;  // S_OK

		}

		public void Reset()
		{
			returnedItemCount = 0;
		}

		public int Skip(int celt)
		{
			returnedItemCount += celt;
			return 0;
		}

		#endregion
	}

	public class SYNCDEVICEINFO
	{
		public int Partnership;
		public string HandlerName;
		public string Name;
		public SYNCDEVICEINFO(string name, string handler, int partnership)
		{
			Partnership = partnership;
			Name = name;
			HandlerName = handler;
		}
	};

	public class HANDLERINFO
	{
		public string HandlerName;
		public Guid Guid;
		public HANDLERINFO(string name, Guid guid)
		{
			HandlerName = name;
			Guid = guid;
		}
	};

	[
	ComVisible(true),
	Guid("B5210630-2C33-419c-AF34-2E2F26CDA8B0"),
	ClassInterface(ClassInterfaceType.None)
	]
	public class SynrcSyncMgrHandlerCollection : ISyncMgrHandlerCollection
	{
		// SynrcSyncMgrHandlerCollection.SyncHandlerCollectionId
		public static Guid SyncHandlerCollectionId
		{
			get
			{
				GuidAttribute guidAttribute = (GuidAttribute)Attribute.GetCustomAttribute(typeof(SynrcSyncMgrHandlerCollection), typeof(GuidAttribute));
				return new Guid(guidAttribute.Value);
			}
		}

		IList<HANDLERINFO> Handlers = new List<HANDLERINFO>();

		public SynrcSyncMgrHandlerCollection()
		{
			Handlers.Add(new HANDLERINFO(SynrcSyncMgrHandler.SyncHandlerName, SynrcSyncMgrHandler.SyncHandlerId));
		}

		#region ISyncMgrHandlerCollection Members

		public int GetHandlerEnumerator(out System.Runtime.InteropServices.ComTypes.IEnumString ppenum)
		{
			HandlerEnumerator hadnlers = new HandlerEnumerator(Handlers);
			ppenum = hadnlers;
			return 0;
		}

		public int BindToHandler(string pszHandlerID, ref Guid riid, out IntPtr ppv)
		{
			if (pszHandlerID.Equals(SynrcSyncMgrHandler.SyncHandlerName))
			{
				SynrcSyncMgrHandler handler = new SynrcSyncMgrHandler();
				bool isCom = Marshal.IsComObject(handler);
				IntPtr ptr = Marshal.GetComInterfaceForObject(handler, typeof(ISyncMgrHandler));
				ppv = ptr;
				return 0;
			}
			else
			{
				ppv = IntPtr.Zero;
				return 1;
			}
			
		}

		#endregion
	}

}
