using System.Runtime.InteropServices;
using System.Text.Json;
using System.Diagnostics;

namespace HotkeyLauncher;

public partial class MainForm : Form
{
    private Dictionary<Keys, string> hotkeyActions = new();
    private NotifyIcon trayIcon = null!;
    private ContextMenuStrip trayMenu = null!;
    private Keys currentHotkey;
    private const string CONFIG_FILE = "hotkeys.json";

    public MainForm()
    {
        InitializeComponent();
        InitializeTrayIcon();
        LoadHotkeys();
        RegisterHotkeys();
        RefreshHotkeyList();
    }

    private void InitializeTrayIcon()
    {
        trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("Показать", null, (s, e) => 
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        });
        trayMenu.Items.Add("Выход", null, (s, e) => Application.Exit());

        trayIcon = new NotifyIcon()
        {
            Icon = SystemIcons.Application,
            ContextMenuStrip = trayMenu,
            Visible = true,
            Text = "Hotkey Launcher"
        };

        trayIcon.DoubleClick += (s, e) => 
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        };
    }

    private void LoadHotkeys()
    {
        try
        {
            if (File.Exists(CONFIG_FILE))
            {
                var json = File.ReadAllText(CONFIG_FILE);
                var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (config != null)
                {
                    foreach (var kvp in config)
                    {
                        if (Enum.TryParse<Keys>(kvp.Key, out Keys key))
                        {
                            hotkeyActions[key] = kvp.Value;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки конфигурации: {ex.Message}", "Ошибка", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveHotkeys()
    {
        try
        {
            var config = hotkeyActions.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value
            );
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(CONFIG_FILE, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения конфигурации: {ex.Message}", "Ошибка", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshHotkeyList()
    {
        hotkeyListView.Items.Clear();
        foreach (var kvp in hotkeyActions)
        {
            var item = new ListViewItem(GetHotkeyString(kvp.Key));
            item.SubItems.Add(kvp.Value);
            hotkeyListView.Items.Add(item);
        }
    }

    private string GetHotkeyString(Keys key)
    {
        var modifiers = new List<string>();
        if ((key & Keys.Control) != 0) modifiers.Add("Ctrl");
        if ((key & Keys.Alt) != 0) modifiers.Add("Alt");
        if ((key & Keys.Shift) != 0) modifiers.Add("Shift");
        
        var mainKey = key & Keys.KeyCode;
        modifiers.Add(mainKey.ToString());
        
        return string.Join(" + ", modifiers);
    }

    private void RegisterHotkeys()
    {
        foreach (var hotkey in hotkeyActions.Keys)
        {
            RegisterHotKey(Handle, hotkey.GetHashCode(), 
                (uint)((hotkey & Keys.Control) != 0 ? 0x0002 : 0) |
                (uint)((hotkey & Keys.Alt) != 0 ? 0x0001 : 0) |
                (uint)((hotkey & Keys.Shift) != 0 ? 0x0004 : 0),
                (uint)(hotkey & Keys.KeyCode));
        }
    }

    private void HotkeyTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        e.SuppressKeyPress = true;
        currentHotkey = e.KeyData;
        hotkeyTextBox.Text = GetHotkeyString(currentHotkey);
    }

    private void BrowseButton_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Все файлы|*.*",
            Title = "Выберите программу"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            pathTextBox.Text = dialog.FileName;
        }
    }

    private void AddButton_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(pathTextBox.Text))
        {
            MessageBox.Show("Выберите путь к программе", "Ошибка", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (currentHotkey == Keys.None)
        {
            MessageBox.Show("Нажмите комбинацию клавиш", "Ошибка", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (hotkeyActions.ContainsKey(currentHotkey))
        {
            MessageBox.Show("Эта комбинация клавиш уже используется", "Ошибка", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        hotkeyActions[currentHotkey] = pathTextBox.Text;
        RegisterHotKey(Handle, currentHotkey.GetHashCode(),
            (uint)((currentHotkey & Keys.Control) != 0 ? 0x0002 : 0) |
            (uint)((currentHotkey & Keys.Alt) != 0 ? 0x0001 : 0) |
            (uint)((currentHotkey & Keys.Shift) != 0 ? 0x0004 : 0),
            (uint)(currentHotkey & Keys.KeyCode));

        SaveHotkeys();
        RefreshHotkeyList();
        
        pathTextBox.Clear();
        hotkeyTextBox.Clear();
        currentHotkey = Keys.None;
    }

    private void RemoveButton_Click(object sender, EventArgs e)
    {
        if (hotkeyListView.SelectedItems.Count == 0) return;

        var selectedItem = hotkeyListView.SelectedItems[0];
        var hotkeyText = selectedItem.Text;
        var hotkey = hotkeyActions.Keys.FirstOrDefault(k => GetHotkeyString(k) == hotkeyText);

        if (hotkey != Keys.None)
        {
            UnregisterHotKey(Handle, hotkey.GetHashCode());
            hotkeyActions.Remove(hotkey);
            SaveHotkeys();
            RefreshHotkeyList();
        }
    }

protected override void WndProc(ref Message m)
{
    const int WM_HOTKEY = 0x0312;

    if (m.Msg == WM_HOTKEY)
    {
        int id = m.WParam.ToInt32();
        var hotkey = hotkeyActions.Keys.FirstOrDefault(k => k.GetHashCode() == id);

        if (hotkey != Keys.None && hotkeyActions.TryGetValue(hotkey, out string? action))
        {
            try
            {
                if (File.Exists(action))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = action,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show($"Файл не найден: {action}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    base.WndProc(ref m);
}


    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            this.Hide();
        }
        else
        {
            foreach (var hotkey in hotkeyActions.Keys)
            {
                UnregisterHotKey(Handle, hotkey.GetHashCode());
            }
            trayIcon.Visible = false;
        }
        base.OnFormClosing(e);
    }
}

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
} 