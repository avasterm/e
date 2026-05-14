using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Example
{
	// Token: 0x02000009 RID: 9
	public class PlatformInvokeTest
	{
		// Token: 0x0600000E RID: 14
		[DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool CLIENT_Init(PlatformInvokeTest.fDisConnect cbDisConnect, uint dwUser);

		// Token: 0x0600000F RID: 15
		[DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void CLIENT_SetAutoReconnect(PlatformInvokeTest.fHaveReConnect cbHaveReconnt, uint dwUser);

		// Token: 0x06000010 RID: 16
		[DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
		public static extern long CLIENT_Login(string pchDVRIP, ushort wDVRPort, string pchUserName, string pchPassword, ref LPNET_DEVICEINFO lpDeviceInfo, ref long error);

		// Token: 0x06000011 RID: 17
		[DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern int CLIENT_RealPlay(long lLoginID, short nChannelID, IntPtr hWnd);

		// Token: 0x06000012 RID: 18
		[DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.StdCall)]
		public static extern int CLIENT_RealPlayEx(long lLoginID, short nChannelID, IntPtr hWnd, uint type);

		// Token: 0x06000013 RID: 19
		[DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		public static extern bool CLIENT_CapturePicture(int hPlayHandle, [MarshalAs(UnmanagedType.LPStr)] string pchPicFileName);

		// Token: 0x06000014 RID: 20
		[DllImport("dhnetsdk.dll")]
		public static extern bool CLIENT_SnapPicture(long lLoginID, SNAP_PARAMS par);

		// Token: 0x06000015 RID: 21
		[DllImport("dhnetsdk.dll")]
		public static extern void CLIENT_SetSnapRevCallBack(PlatformInvokeTest.SnapshotCallbackDelegate callback, uint user);

		// Token: 0x06000016 RID: 22
		[DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool CLIENT_GetDevConfig(long loginId, uint command, int channel, out NET_TIME buffer, out uint bufferSize, IntPtr lpBytesReturned, int waittime = 500);

		// Token: 0x06000017 RID: 23
		[DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool CLIENT_StopRealPlay(long lRealHandle);

		// Token: 0x06000018 RID: 24
		[DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern bool CLIENT_Logout(long lID);

		// Token: 0x06000019 RID: 25
		[DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern void CLIENT_Cleanup();

		// Token: 0x0600001A RID: 26
		[DllImport("dhnetsdk.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern uint CLIENT_GetLastError();

		// Token: 0x0600001B RID: 27 RVA: 0x00002768 File Offset: 0x00000968
		public static void fDisConnectMethod(int lLoginID, IntPtr pchDVRIP, long nDVRPort, uint dwUser)
		{
			Console.WriteLine("lLoginID" + lLoginID);
			Console.WriteLine("pchDVRIP" + pchDVRIP.ToString());
			Console.WriteLine("nDVRPort" + nDVRPort);
			Console.WriteLine("dwUser" + dwUser);
			Console.WriteLine("Disconnect");
		}

		// Token: 0x0600001C RID: 28 RVA: 0x000027DC File Offset: 0x000009DC
		public static void fHaveReConnectMethod(int lLoginID, IntPtr pchDVRIP, long nDVRPort, uint dwUser)
		{
			Console.WriteLine("Reconnect success");
		}

		// Token: 0x0600001D RID: 29 RVA: 0x000027EC File Offset: 0x000009EC
		public static void WorkEnd(string ip)
		{
			Console.WriteLine("[" + ip + "] ----- Work ended -----");
			PlatformInvokeTest.isCameraStop = true;
		}

		// Token: 0x0600001E RID: 30 RVA: 0x0000280C File Offset: 0x00000A0C
		public static void Main(string[] args)
		{
			string text = "";
			try
			{
				PlatformInvokeTest.WorkEndDelegate workEnd = new PlatformInvokeTest.WorkEndDelegate(PlatformInvokeTest.WorkEnd);
				PlatformInvokeTest.fDisConnect fDisConnect = new PlatformInvokeTest.fDisConnect(PlatformInvokeTest.fDisConnectMethod);
				bool flag = PlatformInvokeTest.CLIENT_Init(new PlatformInvokeTest.fDisConnect(PlatformInvokeTest.fDisConnectMethod), 0U);
				bool flag2 = !flag;
				if (flag2)
				{
					uint num = PlatformInvokeTest.CLIENT_GetLastError() - 2147483648U;
					Console.WriteLine("Client init error:" + num);
				}
				else
				{
					bool flag3 = args.Length < 1;
					if (!flag3)
					{
						text = args[0];
						ushort port = 37777;
						string[] logins = File.ReadAllText("logins.txt").Split(new string[]
						{
							"\n",
							"\r"
						}, StringSplitOptions.RemoveEmptyEntries);
						string[] passwords = File.ReadAllText("passwords.txt").Split(new string[]
						{
							"\n",
							"\r"
						}, StringSplitOptions.RemoveEmptyEntries);
						CameraSnapshot cameraSnapshot = new CameraSnapshot(text, port, logins, passwords, workEnd);
						bool flag4 = !cameraSnapshot.Start();
						if (flag4)
						{
							PlatformInvokeTest.isCameraStop = true;
						}
						do
						{
							Thread.Sleep(3000);
						}
						while (!PlatformInvokeTest.isCameraStop);
					}
				}
			}
			catch (NullReferenceException ex)
			{
				Console.WriteLine("[EXCEPTION System.NullReferenceException ] into " + text);
			}
			catch (Exception ex2)
			{
				Console.WriteLine("[EXCEPTION System.Exception] into " + text);
			}
			finally
			{
				PlatformInvokeTest.CLIENT_Cleanup();
			}
		}

		// Token: 0x0400002A RID: 42
		private static bool isCameraStop = false;

		// Token: 0x0200000B RID: 11
		// (Invoke) Token: 0x06000026 RID: 38
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate void SnapshotCallbackDelegate(long lLogin, IntPtr bufer, uint revLen, uint encodeType, uint cmdSerial, uint user);

		// Token: 0x0200000C RID: 12
		// (Invoke) Token: 0x0600002A RID: 42
		public delegate void fDisConnect(int lLoginID, IntPtr pchDVRIP, long nDVRPort, uint dwUser);

		// Token: 0x0200000D RID: 13
		// (Invoke) Token: 0x0600002E RID: 46
		public delegate void fHaveReConnect(int lLoginID, IntPtr pchDVRIP, long nDVRPort, uint dwUser);

		// Token: 0x0200000E RID: 14
		// (Invoke) Token: 0x06000032 RID: 50
		public delegate void WorkEndDelegate(string ip);
	}
}
