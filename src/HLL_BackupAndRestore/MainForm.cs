using System;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace HLL_BackupAndRestore
{
    public partial class MainForm : Form
    {
        // Base: EXE folder
        private readonly string _appBase = AppContext.BaseDirectory;
        private readonly string _datePattern = @"^\d{4}-\d{2}-\d{2}(\-\d+)?$"; // Folder-Name/Format: yyyy-MM-dd, optional -2

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadBackups();
        }


        #region BACKUP

        private void btnBackup_Click(object sender, EventArgs e)
        {
            RunBackup();
        }

        private void RunBackup()
        {
            try
            {
                ClearLog();
                Log("Start Backup ...");

                var toBackup = CollectSources();
                if (toBackup.Count == 0)
                {
                    Log("No relevant files found.");
                    return;
                }
                foreach (var f in toBackup) Log("- found: " + f);

                var destFolder = GetUniqueDatedFolder();
                Directory.CreateDirectory(destFolder);
                var filesDir = Path.Combine(destFolder, "files");
                Directory.CreateDirectory(filesDir);

                var items = new List<BackupItem>();
                int idx = 0;

                foreach (var src in toBackup)
                {
                    if (!File.Exists(src)) continue;

                    var backupName = string.Format("{0:D4}_{1}", idx, Path.GetFileName(src));
                    var backupPath = Path.Combine(filesDir, backupName);
                    var targetDir = Path.GetDirectoryName(backupPath);
                    if (!string.IsNullOrEmpty(targetDir)) Directory.CreateDirectory(targetDir);
                    File.Copy(src, backupPath, true);

                    var fi = new FileInfo(src);
                    var bi = new BackupItem();
                    bi.SourcePath = src;
                    bi.BackupFileName = backupName;
                    bi.Size = fi.Length;
                    bi.LastWriteTimeUtc = fi.LastWriteTimeUtc;

                    items.Add(bi);
                    Log("copied: " + src + "  ->  " + backupName);
                    idx++;
                }

                var manifest = new BackupManifest();
                manifest.CreatedUtc = DateTime.UtcNow;
                manifest.Items = items;

                var manifestPath = Path.Combine(destFolder, "manifest.json");
                File.WriteAllText(manifestPath, JsonConvert.SerializeObject(manifest, Formatting.Indented));

                Log("");
                Log("Backup completed: " + destFolder);

                // Rebind Backup-List
                LoadBackupData();
            }
            catch (Exception ex)
            {
                Log("ERROR: " + ex.Message);
            }
        }

        private List<string> CollectSources()
        {
            var list = new List<string>();

            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var hllRoot = Path.Combine(localAppData, "HLL");
            if (Directory.Exists(hllRoot))
            {
                foreach (var f in Directory.EnumerateFiles(hllRoot, "*.*", SearchOption.AllDirectories))
                    list.Add(f);
            }

            foreach (var sd in GuessSteamUserdataRoots())
            {
                try
                {
                    foreach (var steamIdDir in Directory.GetDirectories(sd))
                    {
                        var remote = Path.Combine(steamIdDir, "686810", "remote");
                        if (Directory.Exists(remote))
                        {
                            foreach (var f in Directory.EnumerateFiles(remote, "*.*", SearchOption.AllDirectories))
                                list.Add(f);
                        }
                    }
                }
                catch { }
            }

            return list.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        private IEnumerable<string> GuessSteamUserdataRoots()
        {
            var roots = new List<string>();

            var pf86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            if (!string.IsNullOrEmpty(pf86))
                roots.Add(Path.Combine(pf86, "Steam", "userdata"));

            var pf = Environment.GetEnvironmentVariable("ProgramW6432");
            if (!string.IsNullOrEmpty(pf))
                roots.Add(Path.Combine(pf, "Steam", "userdata"));

            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            roots.Add(Path.Combine(home, "Steam", "userdata"));

            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var existing = new List<string>();
            foreach (var r in roots)
            {
                if (!string.IsNullOrEmpty(r) && Directory.Exists(r) && !unique.Contains(r))
                {
                    existing.Add(r);
                    unique.Add(r);
                }
            }
            return existing;
        }

        private string GetUniqueDatedFolder()
        {
            var baseName = DateTime.Now.ToString("yyyy-MM-dd");
            var candidate = Path.Combine(_appBase, baseName);
            if (!Directory.Exists(candidate)) return candidate;

            int n = 2;
            while (true)
            {
                var c = Path.Combine(_appBase, baseName + "-" + n);
                if (!Directory.Exists(c)) return c;
                n++;
            }
        }

        #endregion


        #region RESTORE

        void LoadBackups()
        {
            //List-Settings
            listBackups.View = View.Details;
            listBackups.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            listBackups.FullRowSelect = true;
            listBackups.GridLines = true;

            // Add Columns to List
            listBackups.Columns.Add("#", 50);
            listBackups.Columns.Add("Backup", 150);
            listBackups.Columns.Add(" ", 50, HorizontalAlignment.Center);

            // Fill List with Data
            LoadBackupData();

            // List-Events
            listBackups.OwnerDraw = true;
            listBackups.DrawColumnHeader += listView1_DrawColumnHeader;
            listBackups.DrawItem += listView1_DrawItem;
            listBackups.DrawSubItem += listView1_DrawSubItem;
        }

        private void LoadBackupData()
        {
            listBackups.Items.Clear();

            int count = 0;
            var rx = new Regex(_datePattern);
            var dirs = Directory.EnumerateDirectories(_appBase)
                                .Select(Path.GetFileName)
                                .Where(n => !string.IsNullOrEmpty(n) && rx.IsMatch(n))
                                .OrderByDescending(n => n)
                                .ToList();

            foreach (var d in dirs)
            {
                count += 1;
                var listItem = new ListViewItem(count.ToString());
                listItem.SubItems.Add(d);
                listItem.SubItems.Add("⋯"); // 3-Dots-Icon

                listBackups.Items.Add(listItem);
            }
        }

        // Column-Headers
        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        // Columns
        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }

        // Action-Column
        private const int ActionColIndex = 2;
        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            if (e.ColumnIndex != ActionColIndex)
            {
                e.DrawDefault = true;
                return;
            }

            var backColor = (e.Item.Selected && listBackups.Focused)
                ? SystemColors.Highlight
                : e.SubItem.BackColor;

            using (var b = new SolidBrush(backColor))
                e.Graphics.FillRectangle(b, e.Bounds);

            var foreColor = (e.Item.Selected && listBackups.Focused)
                ? SystemColors.HighlightText
                : e.SubItem.ForeColor;

            TextRenderer.DrawText(
                e.Graphics,
                "⋯",
                new Font(e.SubItem.Font.FontFamily, e.SubItem.Font.Size + 5, FontStyle.Bold),
                e.Bounds,
                foreColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );

            if (e.Item.Selected && listBackups.Focused)
                e.DrawFocusRectangle(e.Bounds);
        }

        


        #region Action

        private void listBackups_MouseClick(object sender, MouseEventArgs e)
        {
            var info = listBackups.HitTest(e.Location);
            if (info.Item != null && info.SubItem != null)
            {
                int colIndex = info.Item.SubItems.IndexOf(info.SubItem);
                if (colIndex == ActionColIndex) // Spalte "⋮"
                {
                    // Menü anzeigen
                    ContextMenuStrip menu = new ContextMenuStrip();
                    menu.Items.Add("Restore", null, (s, ev) => RunRestore(info.Item.SubItems[1].Text));
                    menu.Items.Add("Open", null, (s, ev) => OpenFolder(info.Item.SubItems[1].Text));
                    menu.Items.Add("Delete", null, (s, ev) => DeleteFolder(info.Item.SubItems[1].Text));
                    menu.Show(listBackups, e.Location);
                }
            }
        }

        private void RunRestore(string backupName)
        {
            try
            {
                var folder = Path.Combine(_appBase, backupName);
                var manifestPath = Path.Combine(folder, "manifest.json");
                var filesPath = Path.Combine(folder, "files");

                if (!File.Exists(manifestPath))
                {
                    MessageBox.Show("manifest.json is missing in this backup.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!Directory.Exists(filesPath))
                {
                    MessageBox.Show("files subfolder is missing in this backup.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var manifestJson = File.ReadAllText(manifestPath);
                var manifest = JsonConvert.DeserializeObject<BackupManifest>(manifestJson);
                if (manifest == null || manifest.Items == null || manifest.Items.Count == 0)
                {
                    MessageBox.Show("This backup contains no files.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var errors = new List<string>();
                int restored = 0;

                foreach (var it in manifest.Items)
                {
                    try
                    {
                        var src = Path.Combine(filesPath, it.BackupFileName);
                        if (!File.Exists(src)) continue;

                        var target = it.SourcePath;
                        var targetDir = Path.GetDirectoryName(target);
                        if (string.IsNullOrEmpty(targetDir)) continue;

                        Directory.CreateDirectory(targetDir);
                        File.Copy(src, target, true);
                        restored++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add(it.SourcePath + " → " + ex.Message);
                    }
                }

                if (errors.Count > 0)
                {
                    var head = "Done with errors.\nSuccessful: " + restored + "\nError:\n- ";
                    var body = string.Join("\n- ", errors.Count > 10 ? errors.GetRange(0, 10) : errors);
                    var tail = errors.Count > 10 ? "\n(+ " + (errors.Count - 10) + " more …)" : "";
                    MessageBox.Show(head + body + tail, "Restore completed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Restore successful. Files restored: " + restored + ".", "Restore", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR during restore: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenFolder(string backupName)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = backupName;
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        private void DeleteFolder(string backupName)
        {
            var res = MessageBox.Show(
                "Really delete this backup?\n" + backupName,
                "Confirm delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (res == DialogResult.Yes)
            {
                try
                {
                    Directory.Delete(backupName, true);
                    LoadBackupData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not delete backup: " + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #endregion


        #region HELPERS

        private void ClearLog() { txtLog.Text = string.Empty; }
        private void Log(string msg) { txtLog.AppendText(msg + Environment.NewLine); }

        // DTOs
        public class BackupManifest
        {
            public DateTime CreatedUtc { get; set; }
            public List<BackupItem> Items { get; set; }
        }

        public class BackupItem
        {
            public string SourcePath { get; set; }
            public string BackupFileName { get; set; }
            public long Size { get; set; }
            public DateTime LastWriteTimeUtc { get; set; }
        }

        #endregion

    }
}
