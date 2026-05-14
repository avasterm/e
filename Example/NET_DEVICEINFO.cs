using System;
using System.Runtime.InteropServices;

namespace Example
{
	// Token: 0x02000007 RID: 7
	[StructLayout(LayoutKind.Sequential)]
	public class NET_DEVICEINFO
	{
		// Token: 0x0400001E RID: 30
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
		public byte[] sSerialNumber;

		// Token: 0x0400001F RID: 31
		public byte byAlarmInPortNum;

		// Token: 0x04000020 RID: 32
		public byte byAlarmOutPortNum;

		// Token: 0x04000021 RID: 33
		public byte byDiskNum;

		// Token: 0x04000022 RID: 34
		public byte byDVRType;

		// Token: 0x04000023 RID: 35
		public byte byChanNum;
	}
}
