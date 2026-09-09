using KMC.OrganizerDesktop.Models;
using KMC.OrganizerDesktop.Services;
using System.Drawing;
using System.Linq;

namespace KMC.OrganizerDesktop.Forms
{
    public partial class MyEventsForm : Form
    {
        private readonly KmcApiClient _apiClient;

        private List<EventResponse> _allEvents = new();

        private DataGridView dgvEvents = null!;

        private TextBox txtSearch = null!;

        private ComboBox cmbTypeFilter = null!;
        private ComboBox cmbStatusFilter = null!;

        private Label lblCount = null!;
        private Label lblEmpty = null!;

        private Button btnRefresh = null!;
        private Button btnBack = null!;


        // =========================================================
        // COLORS
        // =========================================================
        private readonly Color BackgroundColor =
            Color.FromArgb(15, 15, 22);

        private readonly Color SurfaceColor =
            Color.FromArgb(28, 28, 40);

        private readonly Color SurfaceLightColor =
            Color.FromArgb(38, 38, 52);

        private readonly Color PrimaryColor =
            Color.FromArgb(109, 76, 255);

        private readonly Color CyanColor =
            Color.FromArgb(50, 200, 255);

        private readonly Color SuccessColor =
            Color.FromArgb(65, 210, 130);

        private readonly Color WarningColor =
            Color.FromArgb(255, 195, 70);

        private readonly Color DangerColor =
            Color.FromArgb(220, 65, 80);

        private readonly Color SecondaryTextColor =
            Color.FromArgb(160, 160, 178);


        // =========================================================
        // CONSTRUCTOR
        // =========================================================
        public MyEventsForm()
        {
            InitializeComponent();

            _apiClient =
                new KmcApiClient();

            CreateUI();
        }


        // =========================================================
        // LOAD WHEN FORM OPENS
        // =========================================================
        protected override async void OnShown(
            EventArgs e)
        {
            base.OnShown(e);

            await LoadMyEventsAsync();
        }


        // =========================================================
        // CREATE UI
        // =========================================================
        private void CreateUI()
        {
            Controls.Clear();


            // =====================================================
            // FORM
            // =====================================================
            Text =
                "KMC - My Events";

            StartPosition =
                FormStartPosition.CenterScreen;

            ClientSize =
                new Size(1200, 700);

            BackColor =
                BackgroundColor;

            FormBorderStyle =
                FormBorderStyle.FixedSingle;

            MaximizeBox =
                false;


            // =====================================================
            // HEADER
            // =====================================================
            Panel header =
                new Panel
                {
                    Dock =
                        DockStyle.Top,

                    Height =
                        105,

                    BackColor =
                        SurfaceColor
                };

            Controls.Add(header);


            // Purple accent
            Panel headerAccent =
                new Panel
                {
                    Dock =
                        DockStyle.Bottom,

                    Height =
                        3,

                    BackColor =
                        PrimaryColor
                };

            header.Controls.Add(
                headerAccent);


            // =====================================================
            // TITLE
            // =====================================================
            Label lblTitle =
                new Label
                {
                    Text =
                        "MY EVENTS",

                    AutoSize =
                        true,

                    ForeColor =
                        Color.White,

                    Font =
                        new Font(
                            "Segoe UI",
                            22,
                            FontStyle.Bold),

                    Location =
                        new Point(35, 20)
                };

            header.Controls.Add(
                lblTitle);


            Label lblSubtitle =
                new Label
                {
                    Text =
                        "Search, update and manage your published events.",

                    AutoSize =
                        true,

                    ForeColor =
                        SecondaryTextColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            9),

                    Location =
                        new Point(38, 62)
                };

            header.Controls.Add(
                lblSubtitle);


            // =====================================================
            // BACK BUTTON
            // =====================================================
            btnBack =
                new Button
                {
                    Text =
                        "←  BACK",

                    Size =
                        new Size(110, 42),

                    Location =
                        new Point(930, 30),

                    BackColor =
                        SurfaceLightColor,

                    ForeColor =
                        Color.White,

                    FlatStyle =
                        FlatStyle.Flat,

                    Font =
                        new Font(
                            "Segoe UI",
                            9,
                            FontStyle.Bold),

                    Cursor =
                        Cursors.Hand
                };

            btnBack.FlatAppearance.BorderSize =
                0;

            btnBack.Click +=
                btnBack_Click;

            header.Controls.Add(
                btnBack);


            // =====================================================
            // REFRESH BUTTON
            // =====================================================
            btnRefresh =
                new Button
                {
                    Text =
                        "↻  REFRESH",

                    Size =
                        new Size(120, 42),

                    Location =
                        new Point(1055, 30),

                    BackColor =
                        PrimaryColor,

                    ForeColor =
                        Color.White,

                    FlatStyle =
                        FlatStyle.Flat,

                    Font =
                        new Font(
                            "Segoe UI",
                            9,
                            FontStyle.Bold),

                    Cursor =
                        Cursors.Hand
                };

            btnRefresh.FlatAppearance.BorderSize =
                0;


            btnRefresh.MouseEnter +=
                (_, _) =>
                {
                    btnRefresh.BackColor =
                        Color.FromArgb(
                            125,
                            95,
                            255);
                };


            btnRefresh.MouseLeave +=
                (_, _) =>
                {
                    btnRefresh.BackColor =
                        PrimaryColor;
                };


            btnRefresh.Click +=
                btnRefresh_Click;

            header.Controls.Add(
                btnRefresh);


            // =====================================================
            // FILTER CARD
            // =====================================================
            Panel filterCard =
                new Panel
                {
                    Location =
                        new Point(30, 130),

                    Size =
                        new Size(1140, 105),

                    BackColor =
                        SurfaceColor
                };

            Controls.Add(
                filterCard);


            // =====================================================
            // SEARCH
            // =====================================================
            Label lblSearch =
                CreateFilterLabel(
                    "Search Events",
                    25,
                    18);

            filterCard.Controls.Add(
                lblSearch);


            txtSearch =
                new TextBox
                {
                    Size =
                        new Size(390, 34),

                    Location =
                        new Point(25, 48),

                    Font =
                        new Font(
                            "Segoe UI",
                            10),

                    BackColor =
                        SurfaceLightColor,

                    ForeColor =
                        Color.White,

                    BorderStyle =
                        BorderStyle.FixedSingle,

                    PlaceholderText =
                        "Search by title, venue or location..."
                };

            txtSearch.TextChanged +=
                FilterChanged;

            filterCard.Controls.Add(
                txtSearch);


            // =====================================================
            // TYPE FILTER
            // =====================================================
            Label lblType =
                CreateFilterLabel(
                    "Event Type",
                    445,
                    18);

            filterCard.Controls.Add(
                lblType);


            cmbTypeFilter =
                new ComboBox
                {
                    Size =
                        new Size(210, 34),

                    Location =
                        new Point(445, 48),

                    DropDownStyle =
                        ComboBoxStyle.DropDownList,

                    BackColor =
                        SurfaceLightColor,

                    ForeColor =
                        Color.White,

                    FlatStyle =
                        FlatStyle.Flat,

                    Font =
                        new Font(
                            "Segoe UI",
                            10)
                };


            cmbTypeFilter.Items.AddRange(
                new object[]
                {
                    "All Types",
                    "Cultural",
                    "Music",
                    "Food",
                    "Sports",
                    "Education",
                    "Community",
                    "Festival",
                    "Other"
                });


            cmbTypeFilter.SelectedIndex =
                0;

            cmbTypeFilter.SelectedIndexChanged +=
                FilterChanged;

            filterCard.Controls.Add(
                cmbTypeFilter);


            // =====================================================
            // STATUS FILTER
            // =====================================================
            Label lblStatus =
                CreateFilterLabel(
                    "Status",
                    685,
                    18);

            filterCard.Controls.Add(
                lblStatus);


            cmbStatusFilter =
                new ComboBox
                {
                    Size =
                        new Size(200, 34),

                    Location =
                        new Point(685, 48),

                    DropDownStyle =
                        ComboBoxStyle.DropDownList,

                    BackColor =
                        SurfaceLightColor,

                    ForeColor =
                        Color.White,

                    FlatStyle =
                        FlatStyle.Flat,

                    Font =
                        new Font(
                            "Segoe UI",
                            10)
                };


            cmbStatusFilter.Items.AddRange(
                new object[]
                {
                    "All Status",
                    "Published",
                    "Draft",
                    "Cancelled",
                    "Completed"
                });


            cmbStatusFilter.SelectedIndex =
                0;

            cmbStatusFilter.SelectedIndexChanged +=
                FilterChanged;

            filterCard.Controls.Add(
                cmbStatusFilter);


            // =====================================================
            // RESULT COUNT
            // =====================================================
            lblCount =
                new Label
                {
                    Text =
                        "0 events",

                    TextAlign =
                        ContentAlignment.MiddleCenter,

                    Size =
                        new Size(200, 55),

                    Location =
                        new Point(915, 30),

                    ForeColor =
                        CyanColor,

                    BackColor =
                        SurfaceLightColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            11,
                            FontStyle.Bold)
                };

            filterCard.Controls.Add(
                lblCount);


            // =====================================================
            // TABLE CARD
            // =====================================================
            Panel tableCard =
                new Panel
                {
                    Location =
                        new Point(30, 260),

                    Size =
                        new Size(1140, 400),

                    BackColor =
                        SurfaceColor
                };

            Controls.Add(
                tableCard);


            Label lblTableTitle =
                new Label
                {
                    Text =
                        "Event List",

                    AutoSize =
                        true,

                    ForeColor =
                        Color.White,

                    Font =
                        new Font(
                            "Segoe UI",
                            13,
                            FontStyle.Bold),

                    Location =
                        new Point(20, 15)
                };

            tableCard.Controls.Add(
                lblTableTitle);


            // =====================================================
            // GRID
            // =====================================================
            dgvEvents =
                new DataGridView
                {
                    Location =
                        new Point(20, 50),

                    Size =
                        new Size(1100, 325),

                    BackgroundColor =
                        SurfaceColor,

                    BorderStyle =
                        BorderStyle.None,

                    ReadOnly =
                        true,

                    AllowUserToAddRows =
                        false,

                    AllowUserToDeleteRows =
                        false,

                    AllowUserToResizeRows =
                        false,

                    RowHeadersVisible =
                        false,

                    AutoGenerateColumns =
                        false,

                    MultiSelect =
                        false,

                    SelectionMode =
                        DataGridViewSelectionMode
                            .FullRowSelect,

                    AutoSizeColumnsMode =
                        DataGridViewAutoSizeColumnsMode
                            .Fill
                };


            // =====================================================
            // GRID HEADER STYLE
            // =====================================================
            dgvEvents.EnableHeadersVisualStyles =
                false;

            dgvEvents
                .ColumnHeadersDefaultCellStyle
                .BackColor =
                SurfaceLightColor;

            dgvEvents
                .ColumnHeadersDefaultCellStyle
                .ForeColor =
                Color.White;

            dgvEvents
                .ColumnHeadersDefaultCellStyle
                .SelectionBackColor =
                SurfaceLightColor;

            dgvEvents
                .ColumnHeadersDefaultCellStyle
                .Font =
                new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold);

            dgvEvents.ColumnHeadersHeight =
                42;


            // =====================================================
            // GRID ROW STYLE
            // =====================================================
            dgvEvents
                .DefaultCellStyle
                .BackColor =
                SurfaceColor;

            dgvEvents
                .DefaultCellStyle
                .ForeColor =
                Color.White;

            dgvEvents
                .DefaultCellStyle
                .SelectionBackColor =
                Color.FromArgb(
                    45,
                    45,
                    62);

            dgvEvents
                .DefaultCellStyle
                .SelectionForeColor =
                Color.White;

            dgvEvents
                .DefaultCellStyle
                .Font =
                new Font(
                    "Segoe UI",
                    9);

            dgvEvents
                .DefaultCellStyle
                .Padding =
                new Padding(
                    5,
                    0,
                    5,
                    0);

            dgvEvents.RowTemplate.Height =
                42;

            dgvEvents.GridColor =
                Color.FromArgb(
                    45,
                    45,
                    60);


            // =====================================================
            // ID
            // =====================================================
            dgvEvents.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    HeaderText =
                        "ID",

                    DataPropertyName =
                        "Id",

                    FillWeight =
                        30
                });


            // =====================================================
            // TITLE
            // =====================================================
            dgvEvents.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    HeaderText =
                        "Event",

                    DataPropertyName =
                        "Title",

                    FillWeight =
                        120
                });


            // =====================================================
            // TYPE
            // =====================================================
            dgvEvents.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    HeaderText =
                        "Type",

                    DataPropertyName =
                        "EventType",

                    FillWeight =
                        65
                });


            // =====================================================
            // DATE
            // =====================================================
            dgvEvents.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    HeaderText =
                        "Date",

                    DataPropertyName =
                        "EventDate",

                    FillWeight =
                        65,

                    DefaultCellStyle =
                        new DataGridViewCellStyle
                        {
                            Format =
                                "dd MMM yyyy"
                        }
                });


            // =====================================================
            // VENUE
            // =====================================================
            dgvEvents.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    HeaderText =
                        "Venue",

                    DataPropertyName =
                        "Venue",

                    FillWeight =
                        90
                });


            // =====================================================
            // LOCATION
            // =====================================================
            dgvEvents.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    HeaderText =
                        "Location",

                    DataPropertyName =
                        "Location",

                    FillWeight =
                        75
                });


            // =====================================================
            // STATUS
            // =====================================================
            dgvEvents.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name =
                        "Status",

                    HeaderText =
                        "Status",

                    DataPropertyName =
                        "Status",

                    FillWeight =
                        60
                });


            // =====================================================
            // EDIT
            // =====================================================
            DataGridViewButtonColumn editColumn =
                new DataGridViewButtonColumn
                {
                    Name =
                        "EditEvent",

                    HeaderText =
                        "Edit",

                    Text =
                        "EDIT",

                    UseColumnTextForButtonValue =
                        true,

                    FillWeight =
                        52,

                    FlatStyle =
                        FlatStyle.Flat
                };


            editColumn
                .DefaultCellStyle
                .BackColor =
                PrimaryColor;

            editColumn
                .DefaultCellStyle
                .ForeColor =
                Color.White;

            editColumn
                .DefaultCellStyle
                .SelectionBackColor =
                Color.FromArgb(
                    125,
                    95,
                    255);

            editColumn
                .DefaultCellStyle
                .SelectionForeColor =
                Color.White;


            dgvEvents.Columns.Add(
                editColumn);


            // =====================================================
            // DELETE
            // =====================================================
            DataGridViewButtonColumn deleteColumn =
                new DataGridViewButtonColumn
                {
                    Name =
                        "DeleteEvent",

                    HeaderText =
                        "Delete",

                    Text =
                        "DELETE",

                    UseColumnTextForButtonValue =
                        true,

                    FillWeight =
                        58,

                    FlatStyle =
                        FlatStyle.Flat
                };


            deleteColumn
                .DefaultCellStyle
                .BackColor =
                Color.FromArgb(
                    75,
                    30,
                    40);

            deleteColumn
                .DefaultCellStyle
                .ForeColor =
                DangerColor;

            deleteColumn
                .DefaultCellStyle
                .SelectionBackColor =
                DangerColor;

            deleteColumn
                .DefaultCellStyle
                .SelectionForeColor =
                Color.White;


            dgvEvents.Columns.Add(
                deleteColumn);


            dgvEvents.CellContentClick +=
                dgvEvents_CellContentClick;

            dgvEvents.CellFormatting +=
                dgvEvents_CellFormatting;


            tableCard.Controls.Add(
                dgvEvents);


            // =====================================================
            // EMPTY STATE
            // =====================================================
            lblEmpty =
                new Label
                {
                    Text =
                        "No events found.\nTry changing your search or filters.",

                    TextAlign =
                        ContentAlignment.MiddleCenter,

                    Size =
                        new Size(400, 80),

                    Location =
                        new Point(370, 175),

                    ForeColor =
                        SecondaryTextColor,

                    BackColor =
                        SurfaceColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            11),

                    Visible =
                        false
                };

            tableCard.Controls.Add(
                lblEmpty);

            lblEmpty.BringToFront();
        }


        // =========================================================
        // CREATE FILTER LABEL
        // =========================================================
        private Label CreateFilterLabel(
            string text,
            int x,
            int y)
        {
            return new Label
            {
                Text =
                    text,

                AutoSize =
                    true,

                ForeColor =
                    SecondaryTextColor,

                Font =
                    new Font(
                        "Segoe UI",
                        8,
                        FontStyle.Bold),

                Location =
                    new Point(
                        x,
                        y)
            };
        }


        // =========================================================
        // LOAD EVENTS FROM API
        // =========================================================
        private async Task LoadMyEventsAsync()
        {
            try
            {
                btnRefresh.Enabled =
                    false;

                btnRefresh.Text =
                    "LOADING...";


                _allEvents =
                    await _apiClient
                        .GetMyEventsAsync();


                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Unable to Load Events",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);


                lblCount.Text =
                    "Unable to load";
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnRefresh.Enabled =
                        true;

                    btnRefresh.Text =
                        "↻  REFRESH";
                }
            }
        }


        // =========================================================
        // APPLY SEARCH + FILTERS
        // =========================================================
        private void ApplyFilters()
        {
            if (dgvEvents == null)
            {
                return;
            }


            IEnumerable<EventResponse> filtered =
                _allEvents;


            // =====================================================
            // SEARCH
            // =====================================================
            string searchText =
                txtSearch.Text
                    .Trim();


            if (!string.IsNullOrWhiteSpace(
                searchText))
            {
                filtered =
                    filtered.Where(
                        e =>
                            ContainsIgnoreCase(
                                e.Title,
                                searchText)

                            ||

                            ContainsIgnoreCase(
                                e.Venue,
                                searchText)

                            ||

                            ContainsIgnoreCase(
                                e.Location,
                                searchText)

                            ||

                            ContainsIgnoreCase(
                                e.EventType,
                                searchText));
            }


            // =====================================================
            // TYPE FILTER
            // =====================================================
            string selectedType =
                cmbTypeFilter
                    .SelectedItem?
                    .ToString()
                ?? "All Types";


            if (selectedType !=
                "All Types")
            {
                filtered =
                    filtered.Where(
                        e =>
                            string.Equals(
                                e.EventType,
                                selectedType,
                                StringComparison
                                    .OrdinalIgnoreCase));
            }


            // =====================================================
            // STATUS FILTER
            // =====================================================
            string selectedStatus =
                cmbStatusFilter
                    .SelectedItem?
                    .ToString()
                ?? "All Status";


            if (selectedStatus !=
                "All Status")
            {
                filtered =
                    filtered.Where(
                        e =>
                            string.Equals(
                                e.Status,
                                selectedStatus,
                                StringComparison
                                    .OrdinalIgnoreCase));
            }


            List<EventResponse> result =
                filtered
                    .OrderByDescending(
                        e => e.EventDate)
                    .ToList();


            dgvEvents.DataSource =
                null;

            dgvEvents.DataSource =
                result;


            lblCount.Text =
                result.Count == 1
                    ? "1 event"
                    : $"{result.Count} events";


            bool hasEvents =
                result.Count > 0;


            dgvEvents.Visible =
                hasEvents;

            lblEmpty.Visible =
                !hasEvents;
        }


        // =========================================================
        // CASE INSENSITIVE SEARCH
        // =========================================================
        private bool ContainsIgnoreCase(
            string? source,
            string search)
        {
            return !string.IsNullOrWhiteSpace(
                       source)
                   &&
                   source.Contains(
                       search,
                       StringComparison
                           .OrdinalIgnoreCase);
        }


        // =========================================================
        // FILTER CHANGED
        // =========================================================
        private void FilterChanged(
            object? sender,
            EventArgs e)
        {
            ApplyFilters();
        }


        // =========================================================
        // GRID STATUS COLOR
        // =========================================================
        private void dgvEvents_CellFormatting(
            object? sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0)
            {
                return;
            }


            if (dgvEvents
                    .Columns[e.ColumnIndex]
                    .Name != "Status")
            {
                return;
            }


            string status =
                e.Value?.ToString()
                ?? string.Empty;


            if (status.Equals(
                "Published",
                StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.ForeColor =
                    SuccessColor;
            }
            else if (status.Equals(
                "Draft",
                StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.ForeColor =
                    WarningColor;
            }
            else if (status.Equals(
                "Cancelled",
                StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.ForeColor =
                    DangerColor;
            }
            else
            {
                e.CellStyle.ForeColor =
                    CyanColor;
            }


            e.CellStyle.Font =
                new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold);
        }


        // =========================================================
        // REFRESH
        // =========================================================
        private async void btnRefresh_Click(
            object? sender,
            EventArgs e)
        {
            await LoadMyEventsAsync();
        }


        // =========================================================
        // BACK
        // =========================================================
        private void btnBack_Click(
            object? sender,
            EventArgs e)
        {
            Close();
        }


        // =========================================================
        // EDIT / DELETE
        // =========================================================
        private async void dgvEvents_CellContentClick(
            object? sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 ||
                e.ColumnIndex < 0)
            {
                return;
            }


            EventResponse? selectedEvent =
                dgvEvents
                    .Rows[e.RowIndex]
                    .DataBoundItem
                    as EventResponse;


            if (selectedEvent == null)
            {
                return;
            }


            string columnName =
                dgvEvents
                    .Columns[e.ColumnIndex]
                    .Name;


            // =====================================================
            // EDIT
            // =====================================================
            if (columnName ==
                "EditEvent")
            {
                using EventForm eventForm =
                    new EventForm(
                        selectedEvent);


                DialogResult result =
                    eventForm.ShowDialog(
                        this);


                if (result ==
                    DialogResult.OK)
                {
                    await LoadMyEventsAsync();
                }


                return;
            }


            // =====================================================
            // DELETE
            // =====================================================
            if (columnName ==
                "DeleteEvent")
            {
                DialogResult confirmation =
                    MessageBox.Show(
                        $"Are you sure you want to delete \"{selectedEvent.Title}\"?\n\nThis action cannot be undone.",
                        "Delete Event",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);


                if (confirmation !=
                    DialogResult.Yes)
                {
                    return;
                }


                try
                {
                    dgvEvents.Enabled =
                        false;


                    await _apiClient
                        .DeleteEventAsync(
                            selectedEvent.Id);


                    MessageBox.Show(
                        $"Event \"{selectedEvent.Title}\" deleted successfully.",
                        "Event Deleted",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);


                    await LoadMyEventsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        ex.Message,
                        "Delete Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                finally
                {
                    if (!IsDisposed)
                    {
                        dgvEvents.Enabled =
                            true;
                    }
                }
            }
        }
    }
}