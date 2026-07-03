#nullable disable
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Runtime.CompilerServices;

namespace SenranKaguraAspectMOD.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
  private const string BannerImageResourceName = "SenranKaguraAspectMOD.Assets.4827930348255969816.png";
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
      if (SenranKaguraAspectMOD.Properties.Resources.resourceMan == null)
        SenranKaguraAspectMOD.Properties.Resources.resourceMan = new ResourceManager("SenranKaguraAspectMOD.Properties.Resources", typeof (SenranKaguraAspectMOD.Properties.Resources).Assembly);
      return SenranKaguraAspectMOD.Properties.Resources.resourceMan;
    }
  }

  [EditorBrowsable]
  internal static CultureInfo Culture
  {
    get => SenranKaguraAspectMOD.Properties.Resources.resourceCulture;
    set => SenranKaguraAspectMOD.Properties.Resources.resourceCulture = value;
  }

  internal static Bitmap _4827930348255969816
  {
    get
    {
      if (SenranKaguraAspectMOD.Properties.Resources.bannerImage == null)
      {
        Stream stream = typeof (SenranKaguraAspectMOD.Properties.Resources).Assembly.GetManifestResourceStream(SenranKaguraAspectMOD.Properties.Resources.BannerImageResourceName);
        if (stream == null)
          throw new MissingManifestResourceException("Unable to load embedded banner image resource.");
        using (stream)
        {
          using (Bitmap bitmap = new Bitmap(stream))
            SenranKaguraAspectMOD.Properties.Resources.bannerImage = new Bitmap(bitmap);
        }
      }
      return SenranKaguraAspectMOD.Properties.Resources.bannerImage;
    }
  }
}
