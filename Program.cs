using System;
using System.Windows.Forms;

namespace SenranKaguraAspectMOD;

internal static class Program
{
  [STAThread]
  private static void Main()
  {
    try
    {
      ApplicationConfiguration.Initialize();
      Application.Run((Form) new MainForm());
    }
    catch (Exception ex)
    {
      MessageBox.Show($"The application failed to start.\n\n{ex.Message}", "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
  }
}
