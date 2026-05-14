using System;
using System.Runtime.InteropServices;

namespace Example
{
	// Token: 0x02000005 RID: 5
	public struct SNAP_PARAMS
	{
		// Token: 0x04000011 RID: 17
		public uint Channel;

		// Token: 0x04000012 RID: 18
		public uint Quality;

		// Token: 0x04000013 RID: 19
		public uint ImageSize;

		// Token: 0x04000014 RID: 20
		public uint mode;

		// Token: 0x04000015 RID: 21
		public uint InterSnap;

		// Token: 0x04000016 RID: 22
		public uint CmdSerial;

		// Token: 0x04000017 RID: 23
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
		public uint[] Reserved;
	}
}
