// Decompiled with JetBrains decompiler
// Type: Senran_Kagura_EV_BRN_Aspect_MOD.Program
// Assembly: SenranKaguraAspectMOD, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 60DBC0BF-07EE-4770-9CD6-2005A5CED825
// Assembly location: SenranKaguraAspectMOD.dll inside D:\SteamLibrary\steamapps\common\Senran Kagura Burst ReNewal\SenranKaguraAspectMOD1.02.exe)

using System;
using System.Windows.Forms;

#nullable disable
namespace Senran_Kagura_EV_BRN_Aspect_MOD;

internal static class Program
{
  [STAThread]
  private static void Main()
  {
    try
    {
      // ISSUE: reference to a compiler-generated method
      ApplicationConfiguration.Initialize();
      Application.Run((Form) new MainForm());
    }
    catch (Exception ex)
    {
      MessageBox.Show($"The application failed to start.\n\n{ex.Message}", "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
  }
}
