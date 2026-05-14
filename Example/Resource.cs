using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Example
{
	// Token: 0x0200000A RID: 10
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resource
	{
		// Token: 0x06000021 RID: 33 RVA: 0x000029C4 File Offset: 0x00000BC4
		internal Resource()
		{
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000022 RID: 34 RVA: 0x000029D0 File Offset: 0x00000BD0
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				bool flag = Resource.resourceMan == null;
				if (flag)
				{
					ResourceManager resourceManager = new ResourceManager("Example.Resource", typeof(Resource).Assembly);
					Resource.resourceMan = resourceManager;
				}
				return Resource.resourceMan;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000023 RID: 35 RVA: 0x00002A18 File Offset: 0x00000C18
		// (set) Token: 0x06000024 RID: 36 RVA: 0x00002A2F File Offset: 0x00000C2F
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resource.resourceCulture;
			}
			set
			{
				Resource.resourceCulture = value;
			}
		}

		// Token: 0x0400002B RID: 43
		private static ResourceManager resourceMan;

		// Token: 0x0400002C RID: 44
		private static CultureInfo resourceCulture;
	}
}
