using System;
using System.Runtime.InteropServices;

namespace Example
{
	// Token: 0x02000006 RID: 6
	public struct LPNET_DEVICEINFO
	{
		// Token: 0x04000018 RID: 24
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
		public byte[] sSerialNumber;

		// Token: 0x04000019 RID: 25
		public byte byAlarmInPortNum;

		// Token: 0x0400001A RID: 26
		public byte byAlarmOutPortNum;

		// Token: 0x0400001B RID: 27
		public byte byDiskNum;

		// Token: 0x0400001C RID: 28
		public byte byDVRType;

		// Token: 0x0400001D RID: 29
		public byte byChanNum;
	}
}
