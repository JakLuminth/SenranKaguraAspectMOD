// Decompiled with JetBrains decompiler
// Type: Senran_Kagura_EV_BRN_Aspect_MOD.Properties.Resources
// Assembly: SenranKaguraAspectMOD, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 60DBC0BF-07EE-4770-9CD6-2005A5CED825
// Assembly location: SenranKaguraAspectMOD.dll inside D:\SteamLibrary\steamapps\common\Senran Kagura Burst ReNewal\SenranKaguraAspectMOD1.02.exe)

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;

#nullable disable
namespace Senran_Kagura_EV_BRN_Aspect_MOD.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
  private const string BannerImageResourceName = "Senran_Kagura_EV_BRN_Aspect_MOD.Assets.4827930348255969816.png";
  private static ResourceManager resourceMan;
  private static CultureInfo resourceCulture;
  private static Bitmap bannerImage;

  internal Resources()
  {
  }

  [EditorBrowsable]
  internal static ResourceManager ResourceManager
  {
    get
    {
      if (Senran_Kagura_EV_BRN_Aspect_MOD.Properties.Resources.resourceMan == null)
        Senran_Kagura_EV_BRN_Aspect_MOD.Properties.Resources.resourceMan = new ResourceManager("Senran_Kagura_EV_BRN_Aspect_MOD.Properties.Resources", typeof (Senran_Kagura_EV_BRN_Aspect_MOD.Properties.Resources).Assembly);
      return Senran_Kagura_EV_BRN_Aspect_MOD.Properties.Resources.resourceMan;
    }
  }

  [EditorBrowsable]
  internal static CultureInfo Culture
  {
    get => Senran_Kagura_EV_BRN_Aspect_MOD.Properties.Resources.resourceCulture;
    set => Senran_Kagura_EV_BRN_Aspect_MOD.Properties.Resources.resourceCulture = value;
  }

  internal static Bitmap _4827930348255969816
  {
    get
    {
      if (Senran_Kagura_EV_BRN_Aspect_MOD.Properties.Resources.bannerImage == null)
      {
        Stream stream = typeof (Senran_Kagura_EV_BRN_Aspect_MOD.Properties.Resources).Assembly.GetManifestResourceStream(Senran_Kagura_EV_BRN_Aspect_MOD.Properties.Resources.BannerImageResourceName);
        if (stream == null)
          throw new MissingManifestResourceException("Unable to load embedded banner image resource.");
        using (stream)
        {
          using (Bitmap bitmap = new Bitmap(stream))
            Senran_Kagura_EV_BRN_Aspect_MOD.Properties.Resources.bannerImage = new Bitmap(bitmap);
        }
      }
      return Senran_Kagura_EV_BRN_Aspect_MOD.Properties.Resources.bannerImage;
    }
  }
}
