using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SenranKaguraAspectMOD;

internal record GameConfig(
  string ExecutablePath,
  string ShortName,
  string DisplayName,
  int[] ExecutableOffsets,
  string[] RequiredDataDirectories
);

public class MainForm : Form
{
  private readonly string ext = ".backup";
  private const int AspectTokenA = 1071877689;
  private const int AspectTokenB = 1071768034;
  private const int PatchReadBufferSize = 262144;

  // Game data directory constants
  private const string GameDataMotionPlayerPath = ".\\GameData\\Motion\\Player";
  private const string GameDataMotionBeachPath = ".\\GameData\\Motion\\Beach";
  private const string GameDataUiPath = ".\\GameData\\Ui";
  private const string GameDataPlacementPlbgPath = ".\\GameData\\Placement\\plbg";
  private const string BackupDirectoryName = "Backup";
  private const string UiFilePattern = "*data.cat";
  private const string CharacterFilePattern = "*cam.cat";
  private const string BeachFilePattern = "*.cat";
  private const string FinishFilePattern = "*.cat";

  private readonly GameConfig[] Games =
  [
    new(
      ExecutablePath: ".\\SKEstivalVersus.exe",
      ShortName: "EV",
      DisplayName: "SENRAN KAGURA Estival Versus",
      ExecutableOffsets: [2366806, 5721776, 6086588],
      RequiredDataDirectories: [GameDataMotionPlayerPath, GameDataMotionBeachPath, GameDataPlacementPlbgPath]
    ),
    new(
      ExecutablePath: ".\\SKBurstReNewal.exe",
      ShortName: "BrN",
      DisplayName: "SENRAN KAGURA Burst Re:Newal",
      ExecutableOffsets: [2637606, 6980488, 6991120, 7471796],
      RequiredDataDirectories: [GameDataMotionPlayerPath, GameDataUiPath]
    ),
    new(
      ExecutablePath: ".\\SKPeachBeachSplash.exe",
      ShortName: "PBS",
      DisplayName: "SENRAN KAGURA Peach Beach Splash",
      ExecutableOffsets: [3802054, 7856864],
      RequiredDataDirectories: [GameDataMotionPlayerPath]
    ),
    new(
      ExecutablePath: ".\\SKReflexions.exe",
      ShortName: "Reflexions",
      DisplayName: "SENRAN KAGURA Reflexions",
      ExecutableOffsets: [1868058, 4636040],
      RequiredDataDirectories: [GameDataMotionPlayerPath]
    ),
    new(
      ExecutablePath: ".\\SKPeachBall.exe",
      ShortName: "PB",
      DisplayName: "SENRAN KAGURA Peach Ball",
      ExecutableOffsets: [2515162, 4371312, 4379424],
      RequiredDataDirectories: [GameDataMotionPlayerPath]
    )
  ];

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
    this.AppendLog("Ready.");
  }

  /// <summary>Automatically selects a game if exactly one supported executable is detected.</summary>
  private void AutoSelectDetectedGame()
  {
    var detectedGames = this.Games
      .Select((game, index) => (index, game))
      .Where(g => File.Exists(g.game.ExecutablePath))
      .ToList();

    if (detectedGames.Count == 1)
    {
      this.Box_GameSelect.SelectedIndex = detectedGames[0].index;
      this.AppendLog($"Auto-selected {detectedGames[0].game.DisplayName} based on detected executable.");
    }
    else if (detectedGames.Count > 1)
      this.AppendLog("Multiple supported executables were detected. Select the game manually.");
    else
      this.AppendLog("No supported executable was detected. Select the game manually.");
  }

  private GameConfig? TryGetGameConfig(out string errorMessage)
  {
    errorMessage = string.Empty;
    if (this.Box_GameSelect.SelectedIndex < 0 || this.Box_GameSelect.SelectedIndex >= this.Games.Length)
    {
      errorMessage = "Select a game before applying changes.";
      return null;
    }
    return this.Games[this.Box_GameSelect.SelectedIndex];
  }

  private bool TryGetGameInfo(
    out string executablePath,
    out string backupExecutablePath,
    out string shortName,
    out string displayName)
  {
    var game = this.TryGetGameConfig(out _);
    if (game == null)
    {
      executablePath = string.Empty;
      backupExecutablePath = string.Empty;
      shortName = string.Empty;
      displayName = string.Empty;
      return false;
    }
    executablePath = game.ExecutablePath;
    backupExecutablePath = game.ExecutablePath + this.ext;
    shortName = game.ShortName;
    displayName = game.DisplayName;
    return true;
  }

  private bool ConfirmAction(string title, string message)
  {
    return MessageBox.Show((IWin32Window) this, message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
  }

  private bool ValidateApplyPreconditions(out string errorMessage)
  {
    errorMessage = string.Empty;
    var game = this.TryGetGameConfig(out errorMessage);
    if (game == null)
      return false;

    if (this.Box_AspectRatio.SelectedIndex < 0)
    {
      errorMessage = "Select an aspect ratio before applying changes.";
      return false;
    }

    if (!File.Exists(game.ExecutablePath))
    {
      errorMessage = $"Unable to find executable: {game.ExecutablePath}";
      return false;
    }

    foreach (string dir in game.RequiredDataDirectories)
    {
      if (!Directory.Exists(dir))
      {
        errorMessage = $"Required directory not found: {dir}";
        return false;
      }
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

  private void BoxGameSelectSelectedIndexChanged(object? sender, EventArgs e)
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

  private void BoxAspectRatioSelectedIndexChanged(object? sender, EventArgs e)
  {
    this.ValidationCheck();
  }

  private void ButtonRevertClick(object? sender, EventArgs e)
  {
    if (!this.ValidateRevertPreconditions(out string path, out string n, out string displayName, out string errorMessage))
    {
      this.UpdateTextInterface(errorMessage);
      return;
    }
    if (!this.ConfirmAction("Confirm Revert", $"Revert aspect-ratio changes for {displayName}?"))
    {
      this.UpdateTextInterface("Revert cancelled.");
      return;
    }
    this.Button_Apply.Enabled = false;
    this.Button_Revert.Enabled = false;
    try
    {
      this.UpdateTextInterface("...Reverting");
      this.RevertAspectRatios(path, n);
    }
    catch (Exception ex)
    {
      this.UpdateTextInterface($"Revert failed: {ex.Message}");
    }
    finally
    {
      this.ValidationCheck();
    }
  }

  private async void ButtonApplyClick(object? sender, EventArgs e)
  {
    if (!this.ValidateApplyPreconditions(out string errorMessage))
    {
      this.UpdateTextInterface(errorMessage);
      return;
    }
    if (!this.TryGetGameInfo(out string _, out string _, out string _, out string displayName))
      return;
    if (!this.ConfirmAction("Confirm Apply", $"Apply aspect-ratio changes to {displayName}?"))
    {
      this.UpdateTextInterface("Apply cancelled.");
      return;
    }
    this.Button_Apply.Enabled = false;
    this.Button_Revert.Enabled = false;
    try
    {
      this.UpdateTextInterface("...Working");
      await this.UpdateAspectRatios(MainForm.RetrieveAspectInt(this.Box_AspectRatio.SelectedIndex));
    }
    catch (Exception ex)
    {
      this.UpdateTextInterface($"Apply failed: {ex.Message}");
    }
    finally
    {
      this.ValidationCheck();
    }
  }

  /// <summary>
  /// Maps an aspect ratio selection index to its corresponding binary replacement value (IEEE 754 single-precision float representation).
  /// </summary>
  /// <param name="i">The aspect ratio index from the UI ComboBox selection:
  /// 0 = 1.60:1 (16:10)
  /// 1 = 1.67:1 (15:9)
  /// 2 = 1.78:1 (16:9)
  /// 3 = 2.37:1 (21:9, 2560 by 1080)
  /// 4 = 2.39:1 (21:9, 3440 by 1440)
  /// 5 = 2.40:1 (21:9, 3840 by 1600)
  /// 6 = 3.56:1 (32:9)
  /// </param>
  /// <returns>The IEEE 754 encoded single-precision float value used in binary patches for the selected aspect ratio.</returns>
  private static int RetrieveAspectInt(int i)
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
    foreach (string backupFile in Directory.GetFiles(backupDirectory, "*", SearchOption.AllDirectories))
    {
      string relativeBackupPath = Path.GetRelativePath(backupDirectory, backupFile);
      string destinationFile = Path.Combine(destinationDirectory, relativeBackupPath);
      string? destinationFileDirectory = Path.GetDirectoryName(destinationFile);
      if (!string.IsNullOrEmpty(destinationFileDirectory) && !Directory.Exists(destinationFileDirectory))
        Directory.CreateDirectory(destinationFileDirectory);
      this.AppendLog($"Restoring file: {backupFile} -> {destinationFile}");
      File.Copy(backupFile, destinationFile, true);
    }
  }

  private void DeleteFilesInDirectory(string directoryPath)
  {
    if (!Directory.Exists(directoryPath))
      return;
    foreach (string filePath in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
      this.AppendLog($"Deleting backup file: {filePath}");
    this.AppendLog($"Deleting backup directory: {directoryPath}");
    Directory.Delete(directoryPath, true);
  }

  /// <summary>Reverts aspect ratio changes by restoring backup files for the selected game.</summary>
  /// <param name="path">Path to the backup executable file.</param>
  /// <param name="n">Short name of the game (for logging purposes).</param>
  private void RevertAspectRatios(string path, string n)
  {
    var game = this.TryGetGameConfig(out _);
    if (game == null)
      return;

    var failedBackupLocations = new List<string>();

    if (!File.Exists(path))
    {
      this.AppendLog($"Unable to find backup .exe file for {n}.");
      failedBackupLocations.Add($"Executable backup: {path}");
    }
    else
    {
      string destinationExePath = path[..^this.ext.Length];
      this.AppendLog($"Restoring executable: {path} -> {destinationExePath}");
      File.Copy(path, destinationExePath, true);
    }

    foreach (string dataDir in game.RequiredDataDirectories)
    {
      string backupDir = Path.Combine(dataDir, BackupDirectoryName);
      if (Directory.Exists(backupDir))
        this.RestoreBackupDirectoryFiles(backupDir, dataDir);
      else
      {
        this.AppendLog($"Unable to find backup directory files for {n}: {backupDir}");
        failedBackupLocations.Add(backupDir);
      }
    }

    if (this.Check_DeleteBackupsOnRevert.Checked)
    {
      if (failedBackupLocations.Count == 0)
        this.DeleteBackupFilesAfterRevert(path);
      else
        this.AppendLog("Skipping backup deletion because one or more backup locations were missing.");
    }

    if (failedBackupLocations.Count > 0)
    {
      string failureDetails = string.Join(", ", failedBackupLocations);
      this.UpdateTextInterface($"Files for {n} reverted with {failedBackupLocations.Count} warning(s): {failureDetails}");
    }
    else
    {
      this.UpdateTextInterface($"Files for {n} have been reverted.");
    }
  }

  private void DeleteBackupFilesAfterRevert(string backupExecutablePath)
  {
    var game = this.TryGetGameConfig(out _);
    if (game == null)
      return;

    this.AppendLog("Delete backups after revert: enabled.");
    if (File.Exists(backupExecutablePath))
    {
      this.AppendLog($"Deleting backup file: {backupExecutablePath}");
      File.Delete(backupExecutablePath);
    }

      foreach (string dataDir in game.RequiredDataDirectories)
      {
        string backupDir = Path.Combine(dataDir, BackupDirectoryName);
        this.DeleteFilesInDirectory(backupDir);
      }
    }

  private void ValidationCheck()
  {
    bool gameSelected = this.Box_GameSelect.SelectedItem != null && !string.IsNullOrEmpty(this.Box_GameSelect.SelectedItem.ToString());
    bool ratioSelected = this.Box_AspectRatio.SelectedItem != null && !string.IsNullOrEmpty(this.Box_AspectRatio.SelectedItem.ToString());
    this.Button_Apply.Enabled = gameSelected && ratioSelected;
    this.Button_Revert.Enabled = gameSelected;
  }

  /// <summary>Applies aspect ratio patches to the selected game's executable and data files.</summary>
  /// <param name="ratio">The replacement aspect ratio value to patch into the game files.</param>
  private async Task UpdateAspectRatios(int ratio)
  {
    var game = this.TryGetGameConfig(out _);
    if (game == null)
      return;

    this.AppendLog($"Patching executable: {game.ExecutablePath} (backup: {game.ExecutablePath + this.ext})");
    await Task.Run((Action) (() => this.PatchGameExecutable(game, ratio)));

    if (game.RequiredDataDirectories.Contains(GameDataMotionBeachPath))
    {
      int beachFileCount = Directory.GetFiles(GameDataMotionBeachPath, BeachFilePattern).Length;
      this.AppendLog($"Patching {beachFileCount} beach scene file(s) in {GameDataMotionBeachPath}");
      await this.PatchSKEVBeachMenu(ratio);
    }

    if (game.RequiredDataDirectories.Contains(GameDataPlacementPlbgPath))
    {
      int creativeFileCount = Directory.GetFiles(GameDataPlacementPlbgPath, FinishFilePattern).Length;
      this.AppendLog($"Patching {creativeFileCount} creative finish file(s) in {GameDataPlacementPlbgPath}");
      await this.PatchSKEVCreativeFinishes(ratio);
    }

    if (game.RequiredDataDirectories.Contains(GameDataUiPath))
    {
      string[] uiFiles = MainForm.GetUiPatchFiles();
      int uiFileCount = uiFiles.Length;
      this.AppendLog($"Patching {uiFileCount} UI file(s) in {GameDataUiPath}");
      await this.PatchUi(ratio);
    }

    if (game.RequiredDataDirectories.Contains(GameDataMotionPlayerPath))
    {
      int characterFileCount = Directory.GetFiles(GameDataMotionPlayerPath, CharacterFilePattern).Length;
      this.AppendLog($"Patching {characterFileCount} character camera file(s) in {GameDataMotionPlayerPath}");
      await this.PatchCharacterFiles(ratio);
    }

    this.UpdateTextInterface($"{game.DisplayName} Patched.");
  }

  private void EnsureBackupFile(string filePath, string backupDirectory)
  {
    string backupFile = Path.Combine(backupDirectory, Path.GetFileName(filePath));
    string? backupFileDirectory = Path.GetDirectoryName(backupFile);
    if (!string.IsNullOrEmpty(backupFileDirectory) && !Directory.Exists(backupFileDirectory))
      Directory.CreateDirectory(backupFileDirectory);
    if (!File.Exists(backupFile))
    {
      this.AppendLog($"Creating backup: {backupFile}");
      File.Copy(filePath, backupFile, false);
    }
    else
      this.AppendLog($"Backup already exists: {backupFile}");
  }

  private void EnsureBackupFileWithRelativePath(
    string filePath,
    string sourceRootDirectory,
    string backupDirectory)
  {
    string relativeFilePath = Path.GetRelativePath(sourceRootDirectory, filePath);
    string backupFile = Path.Combine(backupDirectory, relativeFilePath);
    string? backupFileDirectory = Path.GetDirectoryName(backupFile);
    if (!string.IsNullOrEmpty(backupFileDirectory) && !Directory.Exists(backupFileDirectory))
      Directory.CreateDirectory(backupFileDirectory);
    if (!File.Exists(backupFile))
    {
      this.AppendLog($"Creating backup: {backupFile}");
      File.Copy(filePath, backupFile, false);
    }
    else
      this.AppendLog($"Backup already exists: {backupFile}");
  }

  private void PatchBinaryFile(string filePath, string backupDirectory, int replacementValue, params int[] tokens)
  {
    this.EnsureBackupFile(filePath, backupDirectory);
    this.PatchBinaryFileInternal(filePath, replacementValue, tokens);
  }

  private void PatchBinaryFileWithRelativeBackup(
    string filePath,
    string sourceRootDirectory,
    string backupDirectory,
    int replacementValue,
    params int[] tokens)
  {
    this.EnsureBackupFileWithRelativePath(filePath, sourceRootDirectory, backupDirectory);
    this.PatchBinaryFileInternal(filePath, replacementValue, tokens);
  }

  /// <summary>
  /// Scans a binary file for specific token values and replaces them with a replacement value.
  /// Uses a sliding window algorithm to efficiently search through large files.
  /// </summary>
  /// <param name="filePath">Path to the binary file to patch.</param>
  /// <param name="replacementValue">The value to write at each matched token position.</param>
  /// <param name="tokens">Token values to search for in the file.</param>
  private void PatchBinaryFileInternal(string filePath, int replacementValue, params int[] tokens)
  {
    this.AppendLog($"Patching file: {filePath}");
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

    private static bool IsPathWithinDirectory(string filePath, string directoryPath)
  {
    string fullFilePath = Path.GetFullPath(filePath);
    string fullDirectoryPath = Path.GetFullPath(directoryPath);
    if (!fullDirectoryPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
      fullDirectoryPath += Path.DirectorySeparatorChar;
    return fullFilePath.StartsWith(fullDirectoryPath, StringComparison.OrdinalIgnoreCase);
  }

  private static string[] GetUiPatchFiles()
  {
    string backupUiDirectory = Path.Combine(GameDataUiPath, BackupDirectoryName);
    return [..Directory.GetFiles(GameDataUiPath, UiFilePattern, SearchOption.AllDirectories).Where(file => !MainForm.IsPathWithinDirectory(file, backupUiDirectory))];
  }

  private static Task PatchFilesAsync(string[] files, Action<string> patchAction)
  {
    return Task.Run((Action) (() => Parallel.ForEach<string>(files, new ParallelOptions
    {
      MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount)
    }, patchAction)));
  }

  private async Task PatchSKEVBeachMenu(int ratio)
  {
    string backupBeachDirectory = Path.Combine(GameDataMotionBeachPath, BackupDirectoryName);
    if (!Directory.Exists(backupBeachDirectory))
      Directory.CreateDirectory(backupBeachDirectory);
    string[] files = Directory.GetFiles(GameDataMotionBeachPath, BeachFilePattern);
    await MainForm.PatchFilesAsync(files, file => this.PatchBinaryFile(file, backupBeachDirectory, ratio, AspectTokenA));
  }

  private async Task PatchSKEVCreativeFinishes(int ratio)
  {
    string backupFinishDirectory = Path.Combine(GameDataPlacementPlbgPath, BackupDirectoryName);
    if (!Directory.Exists(backupFinishDirectory))
      Directory.CreateDirectory(backupFinishDirectory);
    string[] files = Directory.GetFiles(GameDataPlacementPlbgPath, FinishFilePattern);
    await MainForm.PatchFilesAsync(files, file => this.PatchBinaryFile(file, backupFinishDirectory, ratio, AspectTokenA, AspectTokenB));
  }

  private void PatchGameExecutable(GameConfig game, int ratio)
  {
    if (!File.Exists(game.ExecutablePath + this.ext))
      File.Copy(game.ExecutablePath, game.ExecutablePath + this.ext);
    using FileStream fileStream = new(game.ExecutablePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    using BinaryWriter binaryWriter = new((Stream) fileStream);
    foreach (int offset in game.ExecutableOffsets)
    {
      binaryWriter.BaseStream.Position = (long) offset;
      binaryWriter.Write(ratio);
    }
  }

  private async Task PatchUi(int ratio)
  {
    string backupUiDirectory = Path.Combine(GameDataUiPath, BackupDirectoryName);
    if (!Directory.Exists(backupUiDirectory))
      Directory.CreateDirectory(backupUiDirectory);
    string[] files = MainForm.GetUiPatchFiles();
    await MainForm.PatchFilesAsync(files, file => this.PatchBinaryFileWithRelativeBackup(file, GameDataUiPath, backupUiDirectory, ratio, AspectTokenB));
  }

  private async Task PatchCharacterFiles(int ratio)
  {
    string backupDirectory = Path.Combine(GameDataMotionPlayerPath, BackupDirectoryName);
    if (!Directory.Exists(backupDirectory))
      Directory.CreateDirectory(backupDirectory);
    string[] files = Directory.GetFiles(GameDataMotionPlayerPath, CharacterFilePattern);
    await MainForm.PatchFilesAsync(files, file => this.PatchBinaryFile(file, backupDirectory, ratio, AspectTokenA, AspectTokenB));
  }

  private void AppendLog(string s)
  {
    if (this.Box_Log == null || this.Box_Log.IsDisposed)
      return;
    if (this.Box_Log.InvokeRequired)
    {
      this.Box_Log.BeginInvoke((Delegate) new Action<string>(this.AppendLog), s);
      return;
    }
    this.Box_Log.AppendText($"[{DateTime.Now:HH:mm:ss}] {s}{Environment.NewLine}");
  }

  private void ButtonToggleLogClick(object? sender, EventArgs e)
  {
    bool showLog = !this.Box_Log.Visible;
    this.Box_Log.Visible = showLog;
    this.Button_ToggleLog.Text = showLog ? "Hide Log" : "Show Log";
    this.ClientSize = showLog ? this.ExpandedWindowSize : this.CollapsedWindowSize;
  }

  private void UpdateTextInterface(string s)
  {
    if (this.Text_Output == null || this.Text_Output.IsDisposed)
      return;
    if (this.Text_Output.InvokeRequired)
    {
      this.Text_Output.BeginInvoke((Delegate) new Action<string>(this.UpdateTextInterface), s);
      return;
    }
    this.Text_Output.Text = s;
    this.AppendLog(s);
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
    this.Box_GameSelect.Items.AddRange(new object[5]
    {
      (object) "SENRAN KAGURA Estival Versus (EV)",
      (object) "SENRAN KAGURA Burst Re:Newal (BrN)",
      (object) "SENRAN KAGURA Peach Beach Splash (PBS)",
      (object) "SENRAN KAGURA Reflexions",
      (object) "SENRAN KAGURA Peach Ball"
    });
    this.Box_GameSelect.Location = new Point(12, 226);
    this.Box_GameSelect.Name = "Box_GameSelect";
    this.Box_GameSelect.Size = new Size(500, 23);
    this.Box_GameSelect.TabIndex = 0;
    this.Box_GameSelect.Text = "Game Select";
    this.Box_GameSelect.SelectedIndexChanged += this.BoxGameSelectSelectedIndexChanged;
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
    this.Box_AspectRatio.SelectedIndexChanged += this.BoxAspectRatioSelectedIndexChanged;
    this.Button_Revert.Enabled = false;
    this.Button_Revert.Location = new Point(12, 285);
    this.Button_Revert.Name = "Button_Revert";
    this.Button_Revert.Size = new Size(171, 39);
    this.Button_Revert.TabIndex = 2;
    this.Button_Revert.Text = "Revert";
    this.Button_Revert.UseVisualStyleBackColor = true;
    this.Button_Revert.Click += this.ButtonRevertClick;
    this.Button_Apply.Enabled = false;
    this.Button_Apply.Location = new Point(189, 285);
    this.Button_Apply.Name = "Button_Apply";
    this.Button_Apply.Size = new Size(323, 39);
    this.Button_Apply.TabIndex = 3;
    this.Button_Apply.Text = "Apply";
    this.Button_Apply.UseVisualStyleBackColor = true;
    this.Button_Apply.Click += this.ButtonApplyClick;
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
    this.Button_ToggleLog.Click += this.ButtonToggleLogClick;
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
    this.Text = "Senran Kagura Aspect Ratio MOD Tool - Version 2";
    ((ISupportInitialize) this.Box_Image).EndInit();
    this.ResumeLayout(false);
    this.PerformLayout();
  }

  }

