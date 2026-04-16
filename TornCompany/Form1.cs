using TornCompany.Models;
using TornCompany.Services;

namespace TornCompany;

public sealed class CompanyTypeItem
{
    public int TypeId { get; }
    public string DisplayName { get; }

    public CompanyTypeItem(int typeId, string displayName)
    {
        TypeId = typeId;
        DisplayName = displayName;
    }

    public override string ToString() => DisplayName;
}

public partial class Form1 : Form
{
    private readonly TornApiService _apiService = new();
    private readonly SettingsService _settings = new();
    private readonly AppliedService _applied = new();
    private CancellationTokenSource? _cts;
    private List<Company> _allCompanies = new();

    public Form1()
    {
        InitializeComponent();
        _settings.Load();
        _applied.Load();
        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            txtApiKey.Text = _settings.ApiKey;
        }
    }

    private void TxtApiKey_Leave(object? sender, EventArgs e)
    {
        var key = txtApiKey.Text.Trim();
        if (key != _settings.ApiKey)
        {
            _settings.ApiKey = key;
            _settings.Save();
        }
    }

    private async void BtnFetch_Click(object? sender, EventArgs e)
    {
        var apiKey = txtApiKey.Text.Trim();
        if (string.IsNullOrEmpty(apiKey))
        {
            MessageBox.Show("Please enter your API key.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Save API key on fetch
        if (apiKey != _settings.ApiKey)
        {
            _settings.ApiKey = apiKey;
            _settings.Save();
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        btnFetch.Enabled = false;
        cboCompanyType.Enabled = false;
        Cursor = Cursors.WaitCursor;
        _allCompanies.Clear();

        var selectedItem = cboCompanyType.SelectedItem as CompanyTypeItem;
        var selectedTypeId = selectedItem?.TypeId ?? 0;
        var selectedRating = cboRating.SelectedIndex + 3; // index 0 = rating 3, index 7 = rating 10

        try
        {
            if (selectedTypeId > 0)
            {
                // Fetch single type
                lblStatus.Text = $"Fetching {selectedItem!.DisplayName}...";
                var response = await _apiService.GetCompaniesByTypeAsync(
                    selectedTypeId, apiKey, _cts.Token);

                if (response?.Company != null)
                {
                    _allCompanies.AddRange(response.Company.Values);
                }
            }
            else
            {
                // Fetch all types
                progressBar.Visible = true;
                progressBar.Value = 0;

                var typeIds = CompanyTypes.Names.Keys.OrderBy(id => id).ToList();
                var totalTypes = typeIds.Count;

                for (int i = 0; i < totalTypes; i++)
                {
                    var typeId = typeIds[i];
                    var typeName = CompanyTypes.GetName(typeId);
                    lblStatus.Text = $"Fetching {typeName} ({i + 1}/{totalTypes})...";
                    progressBar.Value = (int)((i + 1) * 100.0 / totalTypes);

                    var response = await _apiService.GetCompaniesByTypeAsync(
                        typeId, apiKey, _cts.Token);

                    if (response?.Company != null)
                    {
                        _allCompanies.AddRange(response.Company.Values);
                    }
                }
            }

            var withOpenings = _allCompanies
                .Where(c => c.Openings > 0 && c.Rating == selectedRating)
                .OrderByDescending(c => c.DailyIncome)
                .ToList();

            // Fetch director profiles for companies with openings
            progressBar.Visible = true;
            progressBar.Value = 0;
            var uniqueDirectors = withOpenings.Select(c => c.Director).Distinct().ToList();

            for (int i = 0; i < uniqueDirectors.Count; i++)
            {
                var directorId = uniqueDirectors[i];
                lblStatus.Text = $"Fetching director info ({i + 1}/{uniqueDirectors.Count})...";
                progressBar.Value = (int)((i + 1) * 100.0 / uniqueDirectors.Count);

                try
                {
                    var profile = await _apiService.GetUserProfileAsync(
                        directorId, apiKey, _cts.Token);

                    if (profile is not null)
                    {
                        foreach (var company in withOpenings.Where(c => c.Director == directorId))
                        {
                            company.DirectorName = profile.Name;
                            company.DirectorLastAction = profile.LastAction?.Relative ?? "Unknown";
                            company.DirectorLastActionTimestamp = profile.LastAction?.Timestamp ?? 0;
                        }
                    }
                }
                catch (HttpRequestException)
                {
                    // Skip directors we can't fetch
                }
            }

            // Filter out companies where director inactive 3+ days or no employees hired
            var threeDaysAgo = DateTimeOffset.UtcNow.AddDays(-3).ToUnixTimeSeconds();
            withOpenings = withOpenings
                .Where(c => c.EmployeesHired > 0)
                .Where(c => c.DirectorLastActionTimestamp == 0 || c.DirectorLastActionTimestamp > threeDaysAgo)
                .ToList();

            BindGrid(withOpenings);
            lblStatus.Text = $"Companies with openings: {withOpenings.Count} (of {_allCompanies.Count} total)";
        }
        catch (OperationCanceledException)
        {
            // User cancelled — silent
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"API request failed: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unexpected error: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnFetch.Enabled = true;
            cboCompanyType.Enabled = true;
            Cursor = Cursors.Default;
            progressBar.Visible = false;
        }
    }

    private void BindGrid(List<Company> companies)
    {
        var displayData = companies.Select(c => new
        {
            c.Name,
            Type = CompanyTypes.GetName(c.CompanyType),
            c.Rating,
            c.Openings,
            Hired = c.EmployeesHired,
            Capacity = c.EmployeesCapacity,
            Director = c.DirectorName,
            LastOnline = c.DirectorLastAction,
            DaysOld = c.DaysOld,
            DailyIncome = c.DailyIncome,
            DailyCustomers = c.DailyCustomers,
            c.Id
        }).ToList();

        dgvCompanies.DataSource = displayData;

        if (dgvCompanies.Columns.Count > 0)
        {
            // Mark all bound columns read-only; only Applied checkbox is editable
            foreach (DataGridViewColumn col in dgvCompanies.Columns)
                col.ReadOnly = true;

            // Add Applied checkbox column if not present
            if (dgvCompanies.Columns["Applied"] is null)
            {
                var chkCol = new DataGridViewCheckBoxColumn
                {
                    Name = "Applied",
                    HeaderText = "Applied",
                    FillWeight = 45,
                    DisplayIndex = 0,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    ReadOnly = false
                };
                dgvCompanies.Columns.Add(chkCol);
            }
            else
            {
                dgvCompanies.Columns["Applied"]!.ReadOnly = false;
            }

            ConfigureColumn("Applied", "Applied", 45);
            ConfigureColumn("Name", "Company Name", 140);
            ConfigureColumn("Type", "Type", 110);
            ConfigureColumn("Rating", "Rating", 50, DataGridViewContentAlignment.MiddleCenter);
            ConfigureColumn("Openings", "Openings", 55, DataGridViewContentAlignment.MiddleCenter);
            ConfigureColumn("Hired", "Hired", 45, DataGridViewContentAlignment.MiddleCenter);
            ConfigureColumn("Capacity", "Capacity", 55, DataGridViewContentAlignment.MiddleCenter);
            ConfigureColumn("Director", "Director", 100);
            ConfigureColumn("LastOnline", "Last Online", 90);
            ConfigureColumn("DaysOld", "Days Old", 55, DataGridViewContentAlignment.MiddleCenter);
            ConfigureColumn("DailyIncome", "Daily Income ($)", 85,
                DataGridViewContentAlignment.MiddleRight, "N0");
            ConfigureColumn("DailyCustomers", "Daily Customers", 75,
                DataGridViewContentAlignment.MiddleCenter);
            ConfigureColumn("Id", "ID", 45);

            // Restore applied state for each row
            foreach (DataGridViewRow row in dgvCompanies.Rows)
            {
                var name = row.Cells["Name"].Value as string ?? string.Empty;
                row.Cells["Applied"].Value = _applied.IsApplied(name);
            }
        }

    }

    private void DgvCompanies_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        var column = dgvCompanies.Columns[e.ColumnIndex];
        var sortableColumns = new HashSet<string> { "Openings", "Rating", "DailyIncome", "DailyCustomers", "DaysOld", "Hired", "Capacity" };

        if (!sortableColumns.Contains(column.Name))
            return;

        var ascending = column.Tag as string != "asc";
        column.Tag = ascending ? "asc" : "desc";

        var ratingFilter = cboRating.SelectedIndex + 3;
        var withOpenings = _allCompanies
            .Where(c => c.Openings > 0 && c.Rating == ratingFilter)
            .ToList();

        withOpenings = column.Name switch
        {
            "Openings" => ascending
                ? withOpenings.OrderBy(c => c.Openings).ToList()
                : withOpenings.OrderByDescending(c => c.Openings).ToList(),
            "Rating" => ascending
                ? withOpenings.OrderBy(c => c.Rating).ToList()
                : withOpenings.OrderByDescending(c => c.Rating).ToList(),
            "DailyIncome" => ascending
                ? withOpenings.OrderBy(c => c.DailyIncome).ToList()
                : withOpenings.OrderByDescending(c => c.DailyIncome).ToList(),
            "DailyCustomers" => ascending
                ? withOpenings.OrderBy(c => c.DailyCustomers).ToList()
                : withOpenings.OrderByDescending(c => c.DailyCustomers).ToList(),
            "DaysOld" => ascending
                ? withOpenings.OrderBy(c => c.DaysOld).ToList()
                : withOpenings.OrderByDescending(c => c.DaysOld).ToList(),
            "Hired" => ascending
                ? withOpenings.OrderBy(c => c.EmployeesHired).ToList()
                : withOpenings.OrderByDescending(c => c.EmployeesHired).ToList(),
            "Capacity" => ascending
                ? withOpenings.OrderBy(c => c.EmployeesCapacity).ToList()
                : withOpenings.OrderByDescending(c => c.EmployeesCapacity).ToList(),
            _ => withOpenings
        };

        BindGrid(withOpenings);
    }

    private void DgvCompanies_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
            return;
        if (dgvCompanies.Columns[e.ColumnIndex].Name != "Applied")
            return;

        // Commit the edit so the cell value is updated, then read and persist it
        dgvCompanies.CommitEdit(DataGridViewDataErrorContexts.Commit);
        var isChecked = dgvCompanies.Rows[e.RowIndex].Cells[e.ColumnIndex].Value is true;
        var name = dgvCompanies.Rows[e.RowIndex].Cells["Name"].Value as string ?? string.Empty;
        _applied.SetApplied(name, isChecked);
    }

    private void DgvCompanies_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || dgvCompanies.Columns["Id"] is null)
            return;

        if (dgvCompanies.Columns[e.ColumnIndex].Name == "Applied")
            return;

        var id = dgvCompanies.Rows[e.RowIndex].Cells["Id"].Value;
        if (id is not null)
        {
            var url = $"https://www.torn.com/joblist.php#/p=corpinfo&ID={id}";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }

    private void ConfigureColumn(string name, string header, int fillWeight,
        DataGridViewContentAlignment? alignment = null, string? format = null)
    {
        if (dgvCompanies.Columns[name] is not { } col)
            return;

        col.HeaderText = header;
        col.FillWeight = fillWeight;

        if (alignment.HasValue)
            col.DefaultCellStyle.Alignment = alignment.Value;

        if (format is not null)
            col.DefaultCellStyle.Format = format;
    }
}
