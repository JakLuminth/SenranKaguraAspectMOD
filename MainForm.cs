// Decompiled with JetBrains decompiler
// Type: Senran_Kagura_EV_BRN_Aspect_MOD.MainForm
// Assembly: SenranKaguraAspectMOD, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 60DBC0BF-07EE-4770-9CD6-2005A5CED825
// Assembly location: SenranKaguraAspectMOD.dll inside D:\SteamLibrary\steamapps\common\Senran Kagura Burst ReNewal\SenranKaguraAspectMOD1.02.exe)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Forms;

#nullable enable
namespace Senran_Kagura_EV_BRN_Aspect_MOD;

public class MainForm : Form
{
  private readonly string SKEVEXEID = ".\\SKEstivalVersus.exe";
  private readonly string SKBRNEXEID = ".\\SKBurstReNewal.exe";
  private readonly string ext = ".backup";
  private 
  #nullable disable
  IContainer components;
  private ComboBox Box_GameSelect;
  private ComboBox Box_AspectRatio;
  private Button Button_Revert;
  private Button Button_Apply;
  private PictureBox Box_Image;
  private Label Text_Instructions;
  private Label Texet;
  private Label Text_Output;
  private TextBox Box_Log;

  public MainForm()
  {
    this.InitializeComponent();
    this.Append_Log("Ready.");
  }

  private bool TryGetGameInfo(
    out string executablePath,
    out string backupExecutablePath,
    out string shortName,
    out string displayName)
  {
    executablePath = string.Empty;
    backupExecutablePath = string.Empty;
    shortName = string.Empty;
    displayName = string.Empty;
    switch (this.Box_GameSelect.SelectedIndex)
    {
      case 0:
        executablePath = this.SKEVEXEID;
        backupExecutablePath = this.SKEVEXEID + this.ext;
        shortName = "EV";
        displayName = "SENRAN KAGURA Estival Versus";
        return true;
      case 1:
        executablePath = this.SKBRNEXEID;
        backupExecutablePath = this.SKBRNEXEID + this.ext;
        shortName = "BrN";
        displayName = "SENRAN KAGURA Burst Re:Newal";
        return true;
      default:
        return false;
    }
  }

  private static bool ConfirmAction(string title, string message)
  {
    return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
  }

  private bool ValidateApplyPreconditions(out string errorMessage)
  {
    errorMessage = string.Empty;
    if (!this.TryGetGameInfo(out string executablePath, out string _, out string _, out string _))
    {
      errorMessage = "Select a game before applying changes.";
      return false;
    }
    if (this.Box_AspectRatio.SelectedIndex < 0)
    {
      errorMessage = "Select an aspect ratio before applying changes.";
      return false;
    }
    if (!File.Exists(executablePath))
    {
      errorMessage = $"Unable to find executable: {executablePath}";
      return false;
    }
    if (!Directory.Exists(".\\GameData\\Motion\\Player"))
    {
      errorMessage = "Required directory not found: .\\GameData\\Motion\\Player";
      return false;
    }
    if (this.Box_GameSelect.SelectedIndex == 0)
    {
      if (!Directory.Exists(".\\GameData\\Motion\\Beach"))
      {
        errorMessage = "Required directory not found: .\\GameData\\Motion\\Beach";
        return false;
      }
      if (!Directory.Exists(".\\GameData\\Placement\\plbg"))
      {
        errorMessage = "Required directory not found: .\\GameData\\Placement\\plbg";
        return false;
      }
    }
    else if (this.Box_GameSelect.SelectedIndex == 1 && !Directory.Exists(".\\GameData\\Ui"))
    {
      errorMessage = "Required directory not found: .\\GameData\\Ui";
      return false;
    }
    return true;
  }

  private bool ValidateRevertPreconditions(out string backupPath, out string shortName, out string displayName, out string errorMessage)
  {
    backupPath = string.Empty;
    shortName = string.Empty;
    displayName = string.Empty;
    errorMessage = string.Empty;
    if (!this.TryGetGameInfo(out string _, out backupPath, out shortName, out displayName))
    {
      errorMessage = "Select a game before reverting changes.";
      return false;
    }
    if (!File.Exists(backupPath))
    {
      errorMessage = $"Unable to find backup .exe file for {shortName}.";
      return false;
    }
    return true;
  }

  private void Box_GameSelect_SelectedIndexChanged(
  #nullable enable
  object sender, EventArgs e)
  {
    this.ValidationCheck();
    if (this.Box_GameSelect.SelectedItem != null && !string.IsNullOrEmpty(this.Box_GameSelect.SelectedItem.ToString()))
    {
      this.Box_AspectRatio.Enabled = true;
      this.Button_Revert.Enabled = true;
    }
    else
    {
      this.Box_AspectRatio.Enabled = false;
      this.Button_Revert.Enabled = false;
    }
  }

  private void Box_AspectRatio_SelectedIndexChanged(object sender, EventArgs e)
  {
    this.ValidationCheck();
  }

  private void Button_Revert_Click(object sender, EventArgs e)
  {
    if (!this.ValidateRevertPreconditions(out string path, out string n, out string displayName, out string errorMessage))
    {
      this.Update_Text_Interface(errorMessage);
      return;
    }
    if (!MainForm.ConfirmAction("Confirm Revert", $"Revert aspect-ratio changes for {displayName}?"))
    {
      this.Update_Text_Interface("Revert cancelled.");
      return;
    }
    this.Button_Apply.Enabled = false;
    this.Button_Revert.Enabled = false;
    try
    {
      this.Update_Text_Interface("...Reverting");
      this.Revert_Aspect_Ratios(path, n);
    }
    catch (Exception ex)
    {
      this.Update_Text_Interface($"Revert failed: {ex.Message}");
    }
    finally
    {
      this.ValidationCheck();
    }
  }

  private async void Button_Apply_Click(object sender, EventArgs e)
  {
    if (!this.ValidateApplyPreconditions(out string errorMessage))
    {
      this.Update_Text_Interface(errorMessage);
      return;
    }
    if (!this.TryGetGameInfo(out string _, out string _, out string _, out string displayName))
      return;
    if (!MainForm.ConfirmAction("Confirm Apply", $"Apply aspect-ratio changes to {displayName}?"))
    {
      this.Update_Text_Interface("Apply cancelled.");
      return;
    }
    this.Button_Apply.Enabled = false;
    this.Button_Revert.Enabled = false;
    try
    {
      this.Update_Text_Interface("...Working");
      await this.Update_Aspect_Ratios(MainForm.Retrieve_Aspect_Int(this.Box_AspectRatio.SelectedIndex));
    }
    catch (Exception ex)
    {
      this.Update_Text_Interface($"Apply failed: {ex.Message}");
    }
    finally
    {
      this.ValidationCheck();
    }
  }

  private static int Retrieve_Aspect_Int(int i)
  {
    int num;
    switch (i)
    {
      case 0:
        num = 1070386381;
        break;
      case 1:
        num = 1070945621;
        break;
      case 2:
        num = 1071874873;
        break;
      case 3:
        num = 1075295270;
        break;
      case 4:
        num = 1075372942;
        break;
      case 5:
        num = 1075419546;
        break;
      case 6:
        num = 1080266297;
        break;
      default:
        num = 1070386381;
        break;
    }
    return num;
  }

  private void Revert_Aspect_Ratios(string path, string n)
  {
    if (!File.Exists(path))
    {
      this.Update_Text_Interface($"Unable to find backup .exe file for {n}.");
      return;
    }
    string destinationExePath = path.Substring(0, path.Length - this.ext.Length);
    this.Append_Log($"Restoring executable: {path} -> {destinationExePath}");
    File.Copy(path, destinationExePath, true);
    if (this.Box_GameSelect.SelectedIndex == 0)
    {
      string beachDirectory = ".\\GameData\\Motion\\Beach";
      string beachBackupDirectory = ".\\GameData\\Motion\\Beach\\Backup";
      if (Directory.Exists(beachBackupDirectory))
      {
        if (!Directory.Exists(beachDirectory))
          Directory.CreateDirectory(beachDirectory);
        foreach (string backupFile in Directory.GetFiles(beachBackupDirectory))
        {
          string destinationFile = Path.Combine(beachDirectory, Path.GetFileName(backupFile));
          this.Append_Log($"Restoring file: {backupFile} -> {destinationFile}");
          File.Copy(backupFile, destinationFile, true);
        }
      }
    }
    string playerDirectory = ".\\GameData\\Motion\\Player";
    string playerBackupDirectory = ".\\GameData\\Motion\\Player\\Backup";
    if (!Directory.Exists(playerBackupDirectory))
    {
      this.Update_Text_Interface($"Unable to find backup directory files for {n}.");
      return;
    }
    if (!Directory.Exists(playerDirectory))
      Directory.CreateDirectory(playerDirectory);
    foreach (string backupFile in Directory.GetFiles(playerBackupDirectory))
    {
      string destinationFile = Path.Combine(playerDirectory, Path.GetFileName(backupFile));
      this.Append_Log($"Restoring file: {backupFile} -> {destinationFile}");
      File.Copy(backupFile, destinationFile, true);
    }
    this.Update_Text_Interface($"Files for {n} have been reverted.");
  }

  private void ValidationCheck()
  {
    bool flag1 = false;
    bool flag2 = false;
    if (this.Box_GameSelect.SelectedItem != null && !string.IsNullOrEmpty(this.Box_GameSelect.SelectedItem.ToString()))
      flag1 = true;
    if (this.Box_AspectRatio.SelectedItem != null && !string.IsNullOrEmpty(this.Box_AspectRatio.SelectedItem.ToString()))
      flag2 = true;
    this.Button_Apply.Enabled = flag1 & flag2;
  }

  private async Task Update_Aspect_Ratios(int a)
  {
    if (this.Box_GameSelect.SelectedIndex == 0)
    {
      await this.Patch_Senran_Kagura_Estival_Versus_Async(a);
    }
    else
    {
      if (this.Box_GameSelect.SelectedIndex != 1)
        return;
      await this.Patch_Senran_Kagura_Burst_ReNewal_Async(a);
    }
  }

  private async Task Patch_Senran_Kagura_Estival_Versus_Async(int ratio)
  {
    this.Append_Log($"Patching executable: {this.SKEVEXEID} (backup: {this.SKEVEXEID + this.ext})");
    await Task.Run((Action) (() => this.Patch_SKEV_Application(ratio)));
    int beachFileCount = Directory.GetFiles(".\\GameData\\Motion\\Beach", "*.cat").Length;
    this.Append_Log($"Patching {beachFileCount} beach scene file(s) in .\\GameData\\Motion\\Beach");
    await Task.Run((Func<Task>) (() => MainForm.Patch_SKEV_Beach_Menu(ratio)));
    int creativeFileCount = Directory.GetFiles(".\\GameData\\Placement\\plbg", "*.cat").Length;
    this.Append_Log($"Patching {creativeFileCount} creative finish file(s) in .\\GameData\\Placement\\plbg");
    await Task.Run((Func<Task>) (() => MainForm.Patch_SKEV_Creative_Finishes(ratio)));
    int characterFileCount = Directory.GetFiles(".\\GameData\\Motion\\Player", "*cam.cat").Length;
    this.Append_Log($"Patching {characterFileCount} character camera file(s) in .\\GameData\\Motion\\Player");
    await Task.Run((Func<Task>) (() => MainForm.Patch_Character_Files(ratio)));
    this.Update_Text_Interface("SK Estival Versus Patched.");
  }

  private void Patch_SKEV_Application(int ratio)
  {
    int[] numArray = new int[3]{ 2366806, 5721776, 6086588 };
    if (!File.Exists(this.SKEVEXEID + this.ext))
      File.Copy(this.SKEVEXEID, this.SKEVEXEID + this.ext);
    using (FileStream fileStream = new FileStream(this.SKEVEXEID, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
    using (BinaryWriter binaryWriter = new BinaryWriter((Stream) fileStream))
    {
      for (int index = 0; index < numArray.Length; ++index)
      {
        binaryWriter.BaseStream.Position = (long) numArray[index];
        binaryWriter.Write(ratio);
      }
    }
  }

  private static async Task Patch_SKEV_Beach_Menu(int ratio)
  {
    List<Task<int>> taskList = new List<Task<int>>();
    string SourceBeachDirectory = ".\\GameData\\Motion\\Beach";
    string BackupBeachDirectory = ".\\GameData\\Motion\\Beach\\Backup";
    if (!Directory.Exists(BackupBeachDirectory))
      Directory.CreateDirectory(BackupBeachDirectory);
    foreach (string file in Directory.GetFiles(SourceBeachDirectory))
    {
      string filename = file;
      if (filename.EndsWith(".cat"))
        taskList.Add(Task.Run<int>((Func<int>) (() => MainForm.HexEditBeachScenes(filename, SourceBeachDirectory, BackupBeachDirectory, ratio))));
    }
    int[] numArray = await Task.WhenAll<int>((IEnumerable<Task<int>>) taskList);
  }

  private static async Task Patch_SKEV_Creative_Finishes(int ratio)
  {
    List<Task<int>> taskList = new List<Task<int>>();
    string SourceFinishDirectory = ".\\GameData\\Placement\\plbg";
    string BackupFinishDirectory = ".\\GameData\\Placement\\plbg\\Backup";
    if (!Directory.Exists(BackupFinishDirectory))
      Directory.CreateDirectory(BackupFinishDirectory);
    foreach (string file in Directory.GetFiles(SourceFinishDirectory))
    {
      string filename = file;
      if (filename.EndsWith(".cat"))
        taskList.Add(Task.Run<int>((Func<int>) (() => MainForm.HexEditCreativeScenes(filename, SourceFinishDirectory, BackupFinishDirectory, ratio))));
    }
    int[] numArray = await Task.WhenAll<int>((IEnumerable<Task<int>>) taskList);
  }

  private static int HexEditCreativeScenes(string n, string sD, string bD, int a)
  {
    string backupFile = Path.Combine(bD, Path.GetFileName(n));
    if (!File.Exists(backupFile))
      File.Copy(n, backupFile, false);
    using (FileStream fileStream = new FileStream(n, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
    using (BinaryReader binaryReader = new BinaryReader((Stream) fileStream))
    using (BinaryWriter binaryWriter = new BinaryWriter((Stream) fileStream))
    {
      for (int index = 0; (long) index < ((Stream) fileStream).Length - 10L; ++index)
      {
        ((Stream) fileStream).Seek((long) index, SeekOrigin.Begin);
        if (binaryReader.ReadInt32() == 1071877689)
        {
          binaryWriter.BaseStream.Position = (long) index;
          binaryWriter.Write(a);
        }
        if (binaryReader.ReadInt32() == 1071768034)
        {
          binaryWriter.BaseStream.Position = (long) (index + 4);
          binaryWriter.Write(a);
        }
      }
    }
    return 0;
  }

  private static int HexEditBeachScenes(string n, string sD, string bD, int a)
  {
    string backupFile = Path.Combine(bD, Path.GetFileName(n));
    if (!File.Exists(backupFile))
      File.Copy(n, backupFile, false);
    using (FileStream fileStream = new FileStream(n, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
    using (BinaryReader binaryReader = new BinaryReader((Stream) fileStream))
    using (BinaryWriter binaryWriter = new BinaryWriter((Stream) fileStream))
    {
      for (int index = 0; (long) index < ((Stream) fileStream).Length - 10L; ++index)
      {
        ((Stream) fileStream).Seek((long) index, SeekOrigin.Begin);
        if (binaryReader.ReadInt32() == 1071877689)
        {
          binaryWriter.BaseStream.Position = (long) index;
          binaryWriter.Write(a);
        }
      }
    }
    return 0;
  }

  private async Task Patch_Senran_Kagura_Burst_ReNewal_Async(int ratio)
  {
    this.Append_Log($"Patching executable: {this.SKBRNEXEID} (backup: {this.SKBRNEXEID + this.ext})");
    await Task.Run((Action) (() => this.Patch_SKBRN_Application(ratio)));
    int roomFileCount = Directory.GetFiles(".\\GameData\\Ui", "*data.cat").Length;
    this.Append_Log($"Patching {roomFileCount} room menu file(s) in .\\GameData\\Ui");
    await Task.Run((Func<Task>) (() => MainForm.Patch_SKBRN_Room_Menus(ratio)));
    int characterFileCount = Directory.GetFiles(".\\GameData\\Motion\\Player", "*cam.cat").Length;
    this.Append_Log($"Patching {characterFileCount} character camera file(s) in .\\GameData\\Motion\\Player");
    await Task.Run((Func<Task>) (() => MainForm.Patch_Character_Files(ratio)));
    this.Update_Text_Interface("SK Burst ReNewal Patched.");
  }

  private void Patch_SKBRN_Application(int ratio)
  {
    int[] numArray = new int[4]
    {
      2637606,
      6980488,
      6991120,
      7471796
    };
    if (!File.Exists(this.SKBRNEXEID + this.ext))
      File.Copy(this.SKBRNEXEID, this.SKBRNEXEID + this.ext);
    using (FileStream fileStream = new FileStream(this.SKBRNEXEID, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
    using (BinaryWriter binaryWriter = new BinaryWriter((Stream) fileStream))
    {
      for (int index = 0; index < numArray.Length; ++index)
      {
        binaryWriter.BaseStream.Position = (long) numArray[index];
        binaryWriter.Write(ratio);
      }
    }
  }

  private static async Task Patch_SKBRN_Room_Menus(int ratio)
  {
    List<Task<int>> taskList = new List<Task<int>>();
    string SourceRoomDirectory = ".\\GameData\\Ui";
    string BackupRoomDirectory = ".\\GameData\\Ui\\Backup";
    if (!Directory.Exists(BackupRoomDirectory))
      Directory.CreateDirectory(BackupRoomDirectory);
    foreach (string file in Directory.GetFiles(SourceRoomDirectory))
    {
      string filename = file;
      if (filename.EndsWith("data.cat"))
        taskList.Add(Task.Run<int>((Func<int>) (() => MainForm.HexEditRoomFiles(filename, SourceRoomDirectory, BackupRoomDirectory, ratio))));
    }
    int[] numArray = await Task.WhenAll<int>((IEnumerable<Task<int>>) taskList);
  }

  private static async Task Patch_Character_Files(int ratio)
  {
    List<Task<int>> taskList = new List<Task<int>>();
    string SourceDirectory = ".\\GameData\\Motion\\Player";
    string BackupDirectory = ".\\GameData\\Motion\\Player\\Backup";
    if (!Directory.Exists(BackupDirectory))
      Directory.CreateDirectory(BackupDirectory);
    foreach (string file in Directory.GetFiles(SourceDirectory))
    {
      string filename = file;
      if (filename.EndsWith("cam.cat"))
        taskList.Add(Task.Run<int>((Func<int>) (() => MainForm.HexEditCharacterFile(filename, SourceDirectory, BackupDirectory, ratio))));
    }
    int[] numArray = await Task.WhenAll<int>((IEnumerable<Task<int>>) taskList);
  }

  private static int HexEditRoomFiles(string n, string sD, string bD, int a)
  {
    string backupFile = Path.Combine(bD, Path.GetFileName(n));
    if (!File.Exists(backupFile))
      File.Copy(n, backupFile, false);
    using (FileStream fileStream = new FileStream(n, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
    using (BinaryReader binaryReader = new BinaryReader((Stream) fileStream))
    using (BinaryWriter binaryWriter = new BinaryWriter((Stream) fileStream))
    {
      for (int index = 0; (long) index < ((Stream) fileStream).Length - 10L; ++index)
      {
        ((Stream) fileStream).Seek((long) index, SeekOrigin.Begin);
        if (binaryReader.ReadInt32() == 1071768034)
        {
          binaryWriter.BaseStream.Position = (long) (index + 4);
          binaryWriter.Write(a);
        }
      }
    }
    return 0;
  }

  private static int HexEditCharacterFile(string n, string sD, string bD, int a)
  {
    string backupFile = Path.Combine(bD, Path.GetFileName(n));
    if (!File.Exists(backupFile))
      File.Copy(n, backupFile, false);
    using (FileStream fileStream = new FileStream(n, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
    using (BinaryReader binaryReader = new BinaryReader((Stream) fileStream))
    using (BinaryWriter binaryWriter = new BinaryWriter((Stream) fileStream))
    {
      for (int index = 0; (long) index < ((Stream) fileStream).Length - 10L; ++index)
      {
        ((Stream) fileStream).Seek((long) index, SeekOrigin.Begin);
        if (binaryReader.ReadInt32() == 1071877689)
        {
          binaryWriter.BaseStream.Position = (long) index;
          binaryWriter.Write(a);
        }
        if (binaryReader.ReadInt32() == 1071768034)
        {
          binaryWriter.BaseStream.Position = (long) (index + 4);
          binaryWriter.Write(a);
        }
      }
    }
    return 0;
  }

  private void Append_Log(string s)
  {
    if (this.Box_Log == null)
      return;
    this.Box_Log.AppendText($"[{DateTime.Now:HH:mm:ss}] {s}{Environment.NewLine}");
  }

  private void Update_Text_Interface(string s)
  {
    this.Text_Output.Text = s;
    this.Append_Log(s);
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      ((IDisposable) this.components).Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (MainForm));
    this.Box_GameSelect = new ComboBox();
    this.Box_AspectRatio = new ComboBox();
    this.Button_Revert = new Button();
    this.Button_Apply = new Button();
    this.Box_Image = new PictureBox();
    this.Text_Instructions = new Label();
    this.Texet = new Label();
    this.Text_Output = new Label();
    this.Box_Log = new TextBox();
    ((ISupportInitialize) this.Box_Image).BeginInit();
    this.SuspendLayout();
    this.Box_GameSelect.FormattingEnabled = true;
    this.Box_GameSelect.Items.AddRange(new object[2]
    {
      (object) "SENRAN KAGURA Estival Versues (EV)",
      (object) "SENRAN KAGURA Burst Re:Newal (BrN)"
    });
    this.Box_GameSelect.Location = new Point(12, 226);
    this.Box_GameSelect.Name = "Box_GameSelect";
    this.Box_GameSelect.Size = new Size(500, 23);
    this.Box_GameSelect.TabIndex = 0;
    this.Box_GameSelect.Text = "Game Select";
    this.Box_GameSelect.SelectedIndexChanged += new EventHandler(this.Box_GameSelect_SelectedIndexChanged);
    this.Box_AspectRatio.Enabled = false;
    this.Box_AspectRatio.FormattingEnabled = true;
    this.Box_AspectRatio.Items.AddRange(new object[7]
    {
      (object) "16:10",
      (object) "15:9",
      (object) "16:9",
      (object) "21:9 (2560 by 1080)",
      (object) "21:9 (3440 by 1440)",
      (object) "21:9 (3840 by 1600)",
      (object) "32:9"
    });
    this.Box_AspectRatio.Location = new Point(12, 256 /*0x0100*/);
    this.Box_AspectRatio.Name = "Box_AspectRatio";
    this.Box_AspectRatio.Size = new Size(171, 23);
    this.Box_AspectRatio.TabIndex = 1;
    this.Box_AspectRatio.Text = "Aspect Ratio";
    this.Box_AspectRatio.SelectedIndexChanged += new EventHandler(this.Box_AspectRatio_SelectedIndexChanged);
    this.Button_Revert.Enabled = false;
    this.Button_Revert.Location = new Point(12, 285);
    this.Button_Revert.Name = "Button_Revert";
    this.Button_Revert.Size = new Size(171, 39);
    this.Button_Revert.TabIndex = 2;
    this.Button_Revert.Text = "Revert";
    this.Button_Revert.UseVisualStyleBackColor = true;
    this.Button_Revert.Click += new EventHandler(this.Button_Revert_Click);
    this.Button_Apply.Enabled = false;
    this.Button_Apply.Location = new Point(189, 285);
    this.Button_Apply.Name = "Button_Apply";
    this.Button_Apply.Size = new Size(323, 39);
    this.Button_Apply.TabIndex = 3;
    this.Button_Apply.Text = "Apply";
    this.Button_Apply.UseVisualStyleBackColor = true;
    this.Button_Apply.Click += new EventHandler(this.Button_Apply_Click);
    this.Box_Image.Image = (Image) Senran_Kagura_EV_BRN_Aspect_MOD.Properties.Resources._4827930348255969816;
    this.Box_Image.Location = new Point(12, 12);
    this.Box_Image.Name = "Box_Image";
    this.Box_Image.Size = new Size(500, 140);
    this.Box_Image.SizeMode = PictureBoxSizeMode.CenterImage;
    this.Box_Image.TabIndex = 4;
    this.Box_Image.TabStop = false;
    this.Text_Instructions.AutoSize = true;
    this.Text_Instructions.Location = new Point(13, 158);
    this.Text_Instructions.Name = "Text_Instructions";
    this.Text_Instructions.Size = new Size(454, 60);
    this.Text_Instructions.TabIndex = 5;
    this.Text_Instructions.Text = ((ResourceManager) componentResourceManager).GetString("Text_Instructions.Text");
    this.Texet.AutoSize = true;
    this.Texet.Location = new Point(474, 234);
    this.Texet.Name = "Texet";
    this.Texet.Size = new Size(0, 15);
    this.Texet.TabIndex = 6;
    this.Texet.TextAlign = (ContentAlignment) 4;
    this.Text_Output.Location = new Point(202, 259);
    this.Text_Output.Name = "Text_Output";
    this.Text_Output.Size = new Size(310, 15);
    this.Text_Output.TabIndex = 7;
    this.Text_Output.Text = "Ready.";
    this.Text_Output.TextAlign = (ContentAlignment) 4;
    this.Box_Log.Location = new Point(12, 336);
    this.Box_Log.Multiline = true;
    this.Box_Log.Name = "Box_Log";
    this.Box_Log.ReadOnly = true;
    this.Box_Log.ScrollBars = ScrollBars.Vertical;
    this.Box_Log.Size = new Size(500, 120);
    this.Box_Log.TabIndex = 8;
    this.Box_Log.TabStop = false;
    this.AutoScaleDimensions = new SizeF(7f, 15f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.BackgroundImageLayout = ImageLayout.None;
    this.ClientSize = new Size(524, 469);
    this.Controls.Add((Control) this.Box_Log);
    this.Controls.Add((Control) this.Text_Output);
    this.Controls.Add((Control) this.Texet);
    this.Controls.Add((Control) this.Text_Instructions);
    this.Controls.Add((Control) this.Box_Image);
    this.Controls.Add((Control) this.Button_Apply);
    this.Controls.Add((Control) this.Button_Revert);
    this.Controls.Add((Control) this.Box_AspectRatio);
    this.Controls.Add((Control) this.Box_GameSelect);
    this.FormBorderStyle = FormBorderStyle.FixedSingle;
    this.Name = nameof (MainForm);
    this.Text = "Senran Kagura EV-BrN Aspect Ratio MOD Tool - Version 1.1";
    ((ISupportInitialize) this.Box_Image).EndInit();
    this.ResumeLayout(false);
    this.PerformLayout();
  }

  private delegate void UpdateUI(string msg);
}
