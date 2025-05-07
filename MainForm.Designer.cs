namespace HotkeyLauncher;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private ListView hotkeyListView;
    private Button addButton;
    private Button removeButton;
    private Button browseButton;
    private TextBox pathTextBox;
    private Label pathLabel;
    private Label hotkeyLabel;
    private TextBox hotkeyTextBox;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(600, 450);
        this.Text = "Hotkey Launcher";
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.MaximizeBox = true;
        this.StartPosition = FormStartPosition.CenterScreen;

        // Инициализация ListView
        hotkeyListView = new ListView();
        hotkeyListView.Location = new Point(12, 12);
        hotkeyListView.Size = new Size(576, 350);
        hotkeyListView.View = View.Details;
        hotkeyListView.FullRowSelect = true;
        hotkeyListView.Columns.Add("Горячая клавиша", 200);
        hotkeyListView.Columns.Add("Путь", 376);

        // Инициализация элементов управления
        pathLabel = new Label();
        pathLabel.Text = "Путь:";
        pathLabel.Location = new Point(12, 380);
        pathLabel.AutoSize = true;

        pathTextBox = new TextBox();
        pathTextBox.Location = new Point(50, 377);
        pathTextBox.Size = new Size(400, 23);

        browseButton = new Button();
        browseButton.Text = "Обзор...";
        browseButton.Location = new Point(456, 376);
        browseButton.Size = new Size(75, 23);
        browseButton.Click += BrowseButton_Click;

        hotkeyLabel = new Label();
        hotkeyLabel.Text = "Горячая клавиша:";
        hotkeyLabel.Location = new Point(12, 410);
        hotkeyLabel.AutoSize = true;

        hotkeyTextBox = new TextBox();
        hotkeyTextBox.Location = new Point(140, 407);
        hotkeyTextBox.Size = new Size(200, 23);
        hotkeyTextBox.ReadOnly = true;
        hotkeyTextBox.KeyDown += HotkeyTextBox_KeyDown;

        addButton = new Button();
        addButton.Text = "Добавить";
        addButton.Location = new Point(456, 406);
        addButton.Size = new Size(75, 23);
        addButton.Click += AddButton_Click;

        removeButton = new Button();
        removeButton.Text = "Удалить";
        removeButton.Location = new Point(537, 406);
        removeButton.Size = new Size(75, 23);
        removeButton.Click += RemoveButton_Click;

        // Добавление элементов на форму
        this.Controls.AddRange(new Control[] {
            hotkeyListView,
            pathLabel,
            pathTextBox,
            browseButton,
            hotkeyLabel,
            hotkeyTextBox,
            addButton,
            removeButton
        });
    }
} 