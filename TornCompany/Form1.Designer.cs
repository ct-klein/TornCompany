using TornCompany.Models;

namespace TornCompany;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _apiService?.Dispose();
            _cts?.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        // Top Panel - Row 1: API Key
        var topPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            Padding = new Padding(10, 8, 10, 4)
        };

        // Row 1 panel (API Key)
        var row1 = new Panel
        {
            Dock = DockStyle.Top,
            Height = 32
        };

        var lblApiKey = new Label
        {
            Text = "API Key:",
            AutoSize = true,
            Dock = DockStyle.Left,
            TextAlign = ContentAlignment.MiddleLeft
        };

        txtApiKey = new TextBox
        {
            Dock = DockStyle.Left,
            Width = 350,
            UseSystemPasswordChar = true
        };
        txtApiKey.Leave += TxtApiKey_Leave;

        row1.Controls.Add(txtApiKey);
        row1.Controls.Add(lblApiKey);

        // Row 2 panel (Company Type + Fetch)
        var row2 = new Panel
        {
            Dock = DockStyle.Top,
            Height = 34
        };

        var lblType = new Label
        {
            Text = "Company Type:",
            AutoSize = true,
            Dock = DockStyle.Left,
            TextAlign = ContentAlignment.MiddleLeft
        };

        cboCompanyType = new ComboBox
        {
            Dock = DockStyle.Left,
            Width = 250,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        // Populate dropdown: "All Types" + each company type sorted by name
        cboCompanyType.Items.Add(new CompanyTypeItem(0, "All Types"));
        foreach (var kvp in CompanyTypes.Names.OrderBy(k => k.Value))
        {
            cboCompanyType.Items.Add(new CompanyTypeItem(kvp.Key, kvp.Value));
        }
        cboCompanyType.SelectedIndex = 0;

        var lblRating = new Label
        {
            Text = "Rating:",
            AutoSize = true,
            Dock = DockStyle.Left,
            TextAlign = ContentAlignment.MiddleLeft
        };

        cboRating = new ComboBox
        {
            Dock = DockStyle.Left,
            Width = 80,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        for (int r = 3; r <= 10; r++)
            cboRating.Items.Add(r.ToString());
        cboRating.SelectedIndex = 0;

        btnFetch = new Button
        {
            Text = "Fetch Companies",
            Dock = DockStyle.Left,
            Width = 130
        };
        btnFetch.Click += BtnFetch_Click;

        progressBar = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Style = ProgressBarStyle.Continuous,
            Minimum = 0,
            Maximum = 100,
            Visible = false
        };

        row2.Controls.Add(progressBar);
        row2.Controls.Add(btnFetch);
        row2.Controls.Add(cboRating);
        row2.Controls.Add(lblRating);
        row2.Controls.Add(cboCompanyType);
        row2.Controls.Add(lblType);

        topPanel.Controls.Add(row2);
        topPanel.Controls.Add(row1);

        // DataGridView
        dgvCompanies = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToResizeRows = false
        };
        dgvCompanies.ColumnHeaderMouseClick += DgvCompanies_ColumnHeaderMouseClick;
        dgvCompanies.CellDoubleClick += DgvCompanies_CellDoubleClick;
        dgvCompanies.CellContentClick += DgvCompanies_CellContentClick;

        // Bottom Panel
        var bottomPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 35,
            Padding = new Padding(10, 5, 10, 5)
        };

        lblStatus = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "Select a company type and click Fetch Companies."
        };

        bottomPanel.Controls.Add(lblStatus);

        // Form
        Controls.Add(dgvCompanies);
        Controls.Add(topPanel);
        Controls.Add(bottomPanel);

        Text = "Torn Company Viewer";
        ClientSize = new Size(950, 600);
        MinimumSize = new Size(700, 450);
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Font;
    }

    private TextBox txtApiKey;
    private ComboBox cboCompanyType;
    private ComboBox cboRating;
    private Button btnFetch;
    private ProgressBar progressBar;
    private DataGridView dgvCompanies;
    private Label lblStatus;
}
