using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SenranKaguraAspectMOD;

public class MainForm : Form
{
  private readonly string SKEVEXEID = ".\\SKEstivalVersus.exe";
  private readonly string SKBRNEXEID = ".\\SKBurstReNewal.exe";
  private readonly string ext = ".backup";
  private const int AspectTokenA = 1071877689;
  private const int AspectTokenB = 1071768034;
  private const int PatchReadBufferSize = 262144;
  private IContainer components = null!;
  private ComboBox Box_GameSelect = null!;
  private ComboBox Box_AspectRatio = null!;
  private Button Button_Revert = null!;
  private Button Button_Apply = null!;
  private PictureBox Box_Image = null!;
  private Label Text_Instructions = null!;
  private Label Text_Output = null!;
  private TextBox Box_Log = null!;
  private CheckBox Check_DeleteBackupsOnRevert = null!;
  private Button Button_ToggleLog = null!;
  private readonly Size CollapsedWindowSize = new(524, 363);
  private readonly Size ExpandedWindowSize = new(524, 469);

  public MainForm()
  {
    this.InitializeComponent();
    this.AutoSelectDetectedGame();
    this.Append_Log("Ready.");
  }

  private void AutoSelectDetectedGame()
  {
    bool hasEstivalVersus = File.Exists(this.SKEVEXEID);
    bool hasBurstReNewal = File.Exists(this.SKBRNEXEID);
    if (hasEstivalVersus && !hasBurstReNewal)
    {
      this.Box_GameSelect.SelectedIndex = 0;
      this.Append_Log("Auto-selected SENRAN KAGURA Estival Versus based on detected executable.");
    }
    else if (!hasEstivalVersus && hasBurstReNewal)
    {
      this.Box_GameSelect.SelectedIndex = 1;
      this.Append_Log("Auto-selected SENRAN KAGURA Burst Re:Newal based on detected executable.");
    }
    else if (hasEstivalVersus && hasBurstReNewal)
      this.Append_Log("Both supported executables were detected. Select the game manually.");
    else
      this.Append_Log("No supported executable was detected. Select the game manually.");
  }

  private bool TryGetGameInfo(
    out string executablePath,
    out string backupExecutablePath,
    out string shortName,
    out string displayName)
  {
    (bool success, executablePath, backupExecutablePath, shortName, displayName) = this.Box_GameSelect.SelectedIndex switch
    {
      0 => (true, this.SKEVEXEID, this.SKEVEXEID + this.ext, "EV", "SENRAN KAGURA Estival Versus"),
      1 => (true, this.SKBRNEXEID, this.SKBRNEXEID + this.ext, "BrN", "SENRAN KAGURA Burst Re:Newal"),
      _ => (false, string.Empty, string.Empty, string.Empty, string.Empty)
    };
    return success;
  }

  private bool ConfirmAction(string title, string message)
  {
    return MessageBox.Show((IWin32Window) this, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
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

  private void Box_GameSelect_SelectedIndexChanged(object? sender, EventArgs e)
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

  private void Box_AspectRatio_SelectedIndexChanged(object? sender, EventArgs e)
  {
    this.ValidationCheck();
  }

  private void Button_Revert_Click(object? sender, EventArgs e)
  {
    if (!this.ValidateRevertPreconditions(out string path, out string n, out string displayName, out string errorMessage))
    {
      this.Update_Text_Interface(errorMessage);
      return;
    }
    if (!this.ConfirmAction("Confirm Revert", $"Revert aspect-ratio changes for {displayName}?"))
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

  private async void Button_Apply_Click(object? sender, EventArgs e)
  {
    if (!this.ValidateApplyPreconditions(out string errorMessage))
    {
      this.Update_Text_Interface(errorMessage);
      return;
    }
    if (!this.TryGetGameInfo(out string _, out string _, out string _, out string displayName))
      return;
    if (!this.ConfirmAction("Confirm Apply", $"Apply aspect-ratio changes to {displayName}?"))
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
    return i switch
    {
      0 => 1070386381,
      1 => 1070945621,
      2 => 1071874873,
      3 => 1075295270,
      4 => 1075372942,
      5 => 1075419546,
      6 => 1080266297,
      _ => 1070386381
    };
  }

  private void RestoreBackupDirectoryFiles(string backupDirectory, string destinationDirectory)
  {
    if (!Directory.Exists(destinationDirectory))
      Directory.CreateDirectory(destinationDirectory);
    foreach (string backupFile in Directory.GetFiles(backupDirectory))
    {
      string destinationFile = Path.Combine(destinationDirectory, Path.GetFileName(backupFile));
      this.Append_Log($"Restoring file: {backupFile} -> {destinationFile}");
      File.Copy(backupFile, destinationFile, true);
    }
  }

  private void DeleteFilesInDirectory(string directoryPath)
  {
    if (!Directory.Exists(directoryPath))
      return;
    foreach (string filePath in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
      this.Append_Log($"Deleting backup file: {filePath}");
    this.Append_Log($"Deleting backup directory: {directoryPath}");
    Directory.Delete(directoryPath, true);
  }

  private void Revert_Aspect_Ratios(string path, string n)
  {
    bool missingBackups = false;
    if (!File.Exists(path))
    {
      this.Append_Log($"Unable to find backup .exe file for {n}.");
      missingBackups = true;
    }
    else
    {
      string destinationExePath = path[..^this.ext.Length];
      this.Append_Log($"Restoring executable: {path} -> {destinationExePath}");
      File.Copy(path, destinationExePath, true);
    }
    if (this.Box_GameSelect.SelectedIndex == 0)
    {
      string beachBackupDirectory = ".\\GameData\\Motion\\Beach\\Backup";
      if (Directory.Exists(beachBackupDirectory))
        this.RestoreBackupDirectoryFiles(beachBackupDirectory, ".\\GameData\\Motion\\Beach");
      else
      {
        this.Append_Log($"Unable to find backup directory files for {n}: {beachBackupDirectory}");
        missingBackups = true;
      }
    }
    else if (this.Box_GameSelect.SelectedIndex == 1)
    {
      string uiBackupDirectory = ".\\GameData\\Ui\\Backup";
      if (Directory.Exists(uiBackupDirectory))
        this.RestoreBackupDirectoryFiles(uiBackupDirectory, ".\\GameData\\Ui");
      else
      {
        this.Append_Log($"Unable to find backup directory files for {n}: {uiBackupDirectory}");
        missingBackups = true;
      }
    }
    string playerBackupDirectory = ".\\GameData\\Motion\\Player\\Backup";
    if (Directory.Exists(playerBackupDirectory))
      this.RestoreBackupDirectoryFiles(playerBackupDirectory, ".\\GameData\\Motion\\Player");
    else
    {
      this.Append_Log($"Unable to find backup directory files for {n}: {playerBackupDirectory}");
      missingBackups = true;
    }
    if (this.Check_DeleteBackupsOnRevert.Checked)
    {
      if (!missingBackups)
        this.DeleteBackupFilesAfterRevert(path);
      else
        this.Append_Log("Skipping backup deletion because one or more backup locations were missing.");
    }
    this.Update_Text_Interface(missingBackups ? $"Files for {n} were reverted with warnings." : $"Files for {n} have been reverted.");
  }

  private void DeleteBackupFilesAfterRevert(string backupExecutablePath)
  {
    this.Append_Log("Delete backups after revert: enabled.");
    if (File.Exists(backupExecutablePath))
    {
      this.Append_Log($"Deleting backup file: {backupExecutablePath}");
      File.Delete(backupExecutablePath);
    }
    this.DeleteFilesInDirectory(".\\GameData\\Motion\\Player\\Backup");
    if (this.Box_GameSelect.SelectedIndex == 0)
      this.DeleteFilesInDirectory(".\\GameData\\Motion\\Beach\\Backup");
    else if (this.Box_GameSelect.SelectedIndex == 1)
      this.DeleteFilesInDirectory(".\\GameData\\Ui\\Backup");
  }

  private void ValidationCheck()
  {
    bool gameSelected = this.Box_GameSelect.SelectedItem != null && !string.IsNullOrEmpty(this.Box_GameSelect.SelectedItem.ToString());
    bool ratioSelected = this.Box_AspectRatio.SelectedItem != null && !string.IsNullOrEmpty(this.Box_AspectRatio.SelectedItem.ToString());
    this.Button_Apply.Enabled = gameSelected && ratioSelected;
    this.Button_Revert.Enabled = gameSelected;
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
    await this.Patch_SKEV_Beach_Menu(ratio);
    int creativeFileCount = Directory.GetFiles(".\\GameData\\Placement\\plbg", "*.cat").Length;
    this.Append_Log($"Patching {creativeFileCount} creative finish file(s) in .\\GameData\\Placement\\plbg");
    await this.Patch_SKEV_Creative_Finishes(ratio);
    int characterFileCount = Directory.GetFiles(".\\GameData\\Motion\\Player", "*cam.cat").Length;
    this.Append_Log($"Patching {characterFileCount} character camera file(s) in .\\GameData\\Motion\\Player");
    await this.Patch_Character_Files(ratio);
    this.Update_Text_Interface("SK Estival Versus Patched.");
  }

  private void Patch_SKEV_Application(int ratio)
  {
    int[] numArray = [2366806, 5721776, 6086588];
    if (!File.Exists(this.SKEVEXEID + this.ext))
      File.Copy(this.SKEVEXEID, this.SKEVEXEID + this.ext);
    using FileStream fileStream = new(this.SKEVEXEID, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    using BinaryWriter binaryWriter = new((Stream) fileStream);
    for (int index = 0; index < numArray.Length; ++index)
    {
      binaryWriter.BaseStream.Position = (long) numArray[index];
      binaryWriter.Write(ratio);
    }
  }

  private void EnsureBackupFile(string filePath, string backupDirectory)
  {
    string backupFile = Path.Combine(backupDirectory, Path.GetFileName(filePath));
    if (!File.Exists(backupFile))
    {
      this.Append_Log($"Creating backup: {backupFile}");
      File.Copy(filePath, backupFile, false);
    }
    else
      this.Append_Log($"Backup already exists: {backupFile}");
  }

  private void PatchBinaryFile(string filePath, string backupDirectory, int replacementValue, params int[] tokens)
  {
    this.EnsureBackupFile(filePath, backupDirectory);
    this.Append_Log($"Patching file: {filePath}");
    HashSet<int> tokenSet = [.. tokens];
    List<long> patchPositions = [];
    const int tokenSize = 4;
    int overlapSize = tokenSize - 1;
    byte[] scanBuffer = new byte[PatchReadBufferSize + overlapSize];
    using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    using BinaryWriter binaryWriter = new((Stream) fileStream);
    long scanBaseOffset = 0L;
    int carryBytes = 0;
    fileStream.Position = 0L;
    while (true)
    {
      int bytesRead = fileStream.Read(scanBuffer, carryBytes, PatchReadBufferSize);
      if (bytesRead <= 0)
        break;
      int totalBytes = carryBytes + bytesRead;
      int scanLimit = totalBytes - overlapSize;
      for (int i = 0; i < scanLimit; ++i)
      {
        int currentValue = BitConverter.ToInt32(scanBuffer, i);
        if (tokenSet.Contains(currentValue))
          patchPositions.Add(scanBaseOffset + i);
      }
      carryBytes = Math.Min(overlapSize, totalBytes);
      if (carryBytes > 0)
        Buffer.BlockCopy(scanBuffer, totalBytes - carryBytes, scanBuffer, 0, carryBytes);
      scanBaseOffset += totalBytes - carryBytes;
    }
    for (int index = 0; index < patchPositions.Count; ++index)
    {
      binaryWriter.BaseStream.Position = patchPositions[index];
      binaryWriter.Write(replacementValue);
    }
  }

  private static Task PatchFilesAsync(string[] files, Action<string> patchAction)
  {
    return Task.Run((Action) (() => Parallel.ForEach<string>(files, new ParallelOptions
    {
      MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount)
    }, patchAction)));
  }

  private async Task Patch_SKEV_Beach_Menu(int ratio)
  {
    string sourceBeachDirectory = ".\\GameData\\Motion\\Beach";
    string backupBeachDirectory = ".\\GameData\\Motion\\Beach\\Backup";
    if (!Directory.Exists(backupBeachDirectory))
      Directory.CreateDirectory(backupBeachDirectory);
    string[] files = Directory.GetFiles(sourceBeachDirectory, "*.cat");
    await MainForm.PatchFilesAsync(files, file => this.PatchBinaryFile(file, backupBeachDirectory, ratio, AspectTokenA));
  }

  private async Task Patch_SKEV_Creative_Finishes(int ratio)
  {
    string sourceFinishDirectory = ".\\GameData\\Placement\\plbg";
    string backupFinishDirectory = ".\\GameData\\Placement\\plbg\\Backup";
    if (!Directory.Exists(backupFinishDirectory))
      Directory.CreateDirectory(backupFinishDirectory);
    string[] files = Directory.GetFiles(sourceFinishDirectory, "*.cat");
    await MainForm.PatchFilesAsync(files, file => this.PatchBinaryFile(file, backupFinishDirectory, ratio, AspectTokenA, AspectTokenB));
  }

  private async Task Patch_Senran_Kagura_Burst_ReNewal_Async(int ratio)
  {
    this.Append_Log($"Patching executable: {this.SKBRNEXEID} (backup: {this.SKBRNEXEID + this.ext})");
    await Task.Run((Action) (() => this.Patch_SKBRN_Application(ratio)));
    int roomFileCount = Directory.GetFiles(".\\GameData\\Ui", "*data.cat").Length;
    this.Append_Log($"Patching {roomFileCount} room menu file(s) in .\\GameData\\Ui");
    await this.Patch_SKBRN_Room_Menus(ratio);
    int characterFileCount = Directory.GetFiles(".\\GameData\\Motion\\Player", "*cam.cat").Length;
    this.Append_Log($"Patching {characterFileCount} character camera file(s) in .\\GameData\\Motion\\Player");
    await this.Patch_Character_Files(ratio);
    this.Update_Text_Interface("SK Burst ReNewal Patched.");
  }

  private void Patch_SKBRN_Application(int ratio)
  {
    int[] numArray =
    [
      2637606,
      6980488,
      6991120,
      7471796
    ];
    if (!File.Exists(this.SKBRNEXEID + this.ext))
      File.Copy(this.SKBRNEXEID, this.SKBRNEXEID + this.ext);
    using FileStream fileStream = new(this.SKBRNEXEID, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    using BinaryWriter binaryWriter = new((Stream) fileStream);
    for (int index = 0; index < numArray.Length; ++index)
    {
      binaryWriter.BaseStream.Position = (long) numArray[index];
      binaryWriter.Write(ratio);
    }
  }

  private async Task Patch_SKBRN_Room_Menus(int ratio)
  {
    string sourceRoomDirectory = ".\\GameData\\Ui";
    string backupRoomDirectory = ".\\GameData\\Ui\\Backup";
    if (!Directory.Exists(backupRoomDirectory))
      Directory.CreateDirectory(backupRoomDirectory);
    string[] files = Directory.GetFiles(sourceRoomDirectory, "*data.cat");
    await MainForm.PatchFilesAsync(files, file => this.PatchBinaryFile(file, backupRoomDirectory, ratio, AspectTokenB));
  }

  private async Task Patch_Character_Files(int ratio)
  {
    string sourceDirectory = ".\\GameData\\Motion\\Player";
    string backupDirectory = ".\\GameData\\Motion\\Player\\Backup";
    if (!Directory.Exists(backupDirectory))
      Directory.CreateDirectory(backupDirectory);
    string[] files = Directory.GetFiles(sourceDirectory, "*cam.cat");
    await MainForm.PatchFilesAsync(files, file => this.PatchBinaryFile(file, backupDirectory, ratio, AspectTokenA, AspectTokenB));
  }

  private void Append_Log(string s)
  {
    if (this.Box_Log == null || this.Box_Log.IsDisposed)
      return;
    if (this.Box_Log.InvokeRequired)
    {
      this.Box_Log.BeginInvoke((Delegate) new Action<string>(this.Append_Log), s);
      return;
    }
    this.Box_Log.AppendText($"[{DateTime.Now:HH:mm:ss}] {s}{Environment.NewLine}");
  }

  private void Button_ToggleLog_Click(object? sender, EventArgs e)
  {
    bool showLog = !this.Box_Log.Visible;
    this.Box_Log.Visible = showLog;
    this.Button_ToggleLog.Text = showLog ? "Hide Log" : "Show Log";
    this.ClientSize = showLog ? this.ExpandedWindowSize : this.CollapsedWindowSize;
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
    ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(MainForm));
    this.Box_GameSelect = new ComboBox();
    this.Box_AspectRatio = new ComboBox();
    this.Button_Revert = new Button();
    this.Button_Apply = new Button();
    this.Box_Image = new PictureBox();
    this.Text_Instructions = new Label();
    this.Text_Output = new Label();
    this.Box_Log = new TextBox();
    this.Check_DeleteBackupsOnRevert = new CheckBox();
    this.Button_ToggleLog = new Button();
    ((ISupportInitialize) this.Box_Image).BeginInit();
    this.SuspendLayout();
    this.Box_GameSelect.FormattingEnabled = true;
    this.Box_GameSelect.Items.AddRange(new object[2]
    {
      (object) "SENRAN KAGURA Estival Versus (EV)",
      (object) "SENRAN KAGURA Burst Re:Newal (BrN)"
    });
    this.Box_GameSelect.Location = new Point(12, 226);
    this.Box_GameSelect.Name = "Box_GameSelect";
    this.Box_GameSelect.Size = new Size(500, 23);
    this.Box_GameSelect.TabIndex = 0;
    this.Box_GameSelect.Text = "Game Select";
    this.Box_GameSelect.SelectedIndexChanged += this.Box_GameSelect_SelectedIndexChanged;
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
    this.Box_AspectRatio.SelectedIndexChanged += this.Box_AspectRatio_SelectedIndexChanged;
    this.Button_Revert.Enabled = false;
    this.Button_Revert.Location = new Point(12, 285);
    this.Button_Revert.Name = "Button_Revert";
    this.Button_Revert.Size = new Size(171, 39);
    this.Button_Revert.TabIndex = 2;
    this.Button_Revert.Text = "Revert";
    this.Button_Revert.UseVisualStyleBackColor = true;
    this.Button_Revert.Click += this.Button_Revert_Click;
    this.Button_Apply.Enabled = false;
    this.Button_Apply.Location = new Point(189, 285);
    this.Button_Apply.Name = "Button_Apply";
    this.Button_Apply.Size = new Size(323, 39);
    this.Button_Apply.TabIndex = 3;
    this.Button_Apply.Text = "Apply";
    this.Button_Apply.UseVisualStyleBackColor = true;
    this.Button_Apply.Click += this.Button_Apply_Click;
    this.Box_Image.Image = (Image) SenranKaguraAspectMOD.Properties.Resources._4827930348255969816;
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
    this.Text_Output.Location = new Point(202, 259);
    this.Text_Output.Name = "Text_Output";
    this.Text_Output.Size = new Size(310, 15);
    this.Text_Output.TabIndex = 7;
    this.Text_Output.Text = "Ready.";
    this.Text_Output.TextAlign = (ContentAlignment) 4;
    this.Check_DeleteBackupsOnRevert.AutoSize = true;
    this.Check_DeleteBackupsOnRevert.Checked = true;
    this.Check_DeleteBackupsOnRevert.CheckState = CheckState.Checked;
    this.Check_DeleteBackupsOnRevert.Location = new Point(12, 331);
    this.Check_DeleteBackupsOnRevert.Name = "Check_DeleteBackupsOnRevert";
    this.Check_DeleteBackupsOnRevert.Size = new Size(194, 19);
    this.Check_DeleteBackupsOnRevert.TabIndex = 8;
    this.Check_DeleteBackupsOnRevert.Text = "Delete backups automatically";
    this.Check_DeleteBackupsOnRevert.UseVisualStyleBackColor = true;
    this.Button_ToggleLog.Location = new Point(411, 327);
    this.Button_ToggleLog.Name = "Button_ToggleLog";
    this.Button_ToggleLog.Size = new Size(101, 23);
    this.Button_ToggleLog.TabIndex = 9;
    this.Button_ToggleLog.Text = "Show Log";
    this.Button_ToggleLog.UseVisualStyleBackColor = true;
    this.Button_ToggleLog.Click += this.Button_ToggleLog_Click;
    this.Box_Log.Location = new Point(12, 356);
    this.Box_Log.Multiline = true;
    this.Box_Log.Name = "Box_Log";
    this.Box_Log.ReadOnly = true;
    this.Box_Log.ScrollBars = ScrollBars.Vertical;
    this.Box_Log.Size = new Size(500, 100);
    this.Box_Log.TabIndex = 10;
    this.Box_Log.TabStop = false;
    this.Box_Log.Visible = false;
    this.AutoScaleDimensions = new SizeF(7f, 15f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.BackgroundImageLayout = ImageLayout.None;
    this.ClientSize = new Size(524, 363);
    this.Controls.AddRange(new Control[10]
    {
      this.Button_ToggleLog,
      this.Box_Log,
      this.Check_DeleteBackupsOnRevert,
      this.Text_Output,
      this.Text_Instructions,
      this.Box_Image,
      this.Button_Apply,
      this.Button_Revert,
      this.Box_AspectRatio,
      this.Box_GameSelect
    });
    this.FormBorderStyle = FormBorderStyle.FixedSingle;
    this.Name = "MainForm";
    this.Text = "Senran Kagura EV-BrN Aspect Ratio MOD Tool - Version 1.2";
    ((ISupportInitialize) this.Box_Image).EndInit();
    this.ResumeLayout(false);
    this.PerformLayout();
  }

  }
