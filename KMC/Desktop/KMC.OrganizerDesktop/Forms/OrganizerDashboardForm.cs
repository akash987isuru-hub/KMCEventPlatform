using KMC.OrganizerDesktop.Models;
using KMC.OrganizerDesktop.Services;
using KMC.OrganizerDesktop.Session;
using System.Drawing;
using System.Linq;

namespace KMC.OrganizerDesktop.Forms
{
    public partial class OrganizerDashboardForm : Form
    {
        private readonly KmcApiClient _apiClient;

        // =========================================================
        // MAIN CONTROLS
        // =========================================================
        private Label lblWelcome = null!;
        private Label lblEmail = null!;
        private Label lblLastUpdated = null!;

        private Label lblTotalEvents = null!;
        private Label lblPublishedEvents = null!;
        private Label lblUpcomingEvents = null!;
        private Label lblTotalCapacity = null!;

        private DataGridView dgvRecentEvents = null!;
        private Label lblNoEvents = null!;

        private Button btnDashboard = null!;
        private Button btnMyEvents = null!;
        private Button btnPublishEvent = null!;
        private Button btnRefresh = null!;
        private Button btnLogout = null!;

        // =========================================================
        // COLORS
        // =========================================================
        private readonly Color BackgroundColor =
            Color.FromArgb(15, 15, 22);

        private readonly Color SidebarColor =
            Color.FromArgb(22, 22, 32);

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
        public OrganizerDashboardForm()
        {
            InitializeComponent();

            _apiClient = new KmcApiClient();

            CreateDashboardUI();
        }

        // =========================================================
        // LOAD DATA WHEN DASHBOARD OPENS
        // =========================================================
        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            await LoadDashboardDataAsync();
        }

        // =========================================================
        // CREATE DASHBOARD UI
        // =========================================================
        private void CreateDashboardUI()
        {
            // =====================================================
            // FORM
            // =====================================================
            Text = "KMC Organizer Workspace";

            StartPosition =
                FormStartPosition.CenterScreen;

            ClientSize =
                new Size(1250, 720);

            BackColor =
                BackgroundColor;

            FormBorderStyle =
                FormBorderStyle.FixedSingle;

            MaximizeBox = false;

            // =====================================================
            // SIDEBAR
            // =====================================================
            Panel sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = SidebarColor
            };

            Controls.Add(sidebar);

            // =====================================================
            // KMC LOGO
            // =====================================================
            Label lblLogo = new Label
            {
                Text = "KMC",

                TextAlign =
                    ContentAlignment.MiddleCenter,

                Size =
                    new Size(105, 50),

                ForeColor =
                    Color.White,

                BackColor =
                    PrimaryColor,

                Font = new Font(
                    "Segoe UI",
                    18,
                    FontStyle.Bold),

                Location =
                    new Point(25, 22)
            };

            sidebar.Controls.Add(lblLogo);

            // =====================================================
            // EVENT PLATFORM
            // =====================================================
            Label lblBrand = new Label
            {
                Text =
                    "EVENT PLATFORM",

                AutoSize = true,

                ForeColor =
                    Color.White,

                Font = new Font(
                    "Segoe UI",
                    12,
                    FontStyle.Bold),

                Location =
                    new Point(25, 82)
            };

            sidebar.Controls.Add(lblBrand);

            // =====================================================
            // ORGANIZER WORKSPACE
            // =====================================================
            Label lblBrandSub = new Label
            {
                Text =
                    "Organizer Workspace",

                AutoSize = true,

                ForeColor =
                    CyanColor,

                Font = new Font(
                    "Segoe UI",
                    9),

                Location =
                    new Point(26, 108)
            };

            sidebar.Controls.Add(lblBrandSub);

            // =====================================================
            // BRAND DIVIDER
            // =====================================================
            Panel brandDivider = new Panel
            {
                Size =
                    new Size(190, 1),

                Location =
                    new Point(20, 140),

                BackColor =
                    SurfaceLightColor
            };

            sidebar.Controls.Add(brandDivider);

            // =====================================================
            // USER PROFILE CARD
            // =====================================================
            Panel profileCard = new Panel
            {
                Size =
                    new Size(190, 95),

                Location =
                    new Point(20, 160),

                BackColor =
                    SurfaceColor
            };

            sidebar.Controls.Add(profileCard);

            // =====================================================
            // USER INITIALS
            // =====================================================
            Label lblInitials = new Label
            {
                Text =
                    GetInitials(
                        UserSession.FullName),

                TextAlign =
                    ContentAlignment.MiddleCenter,

                Size =
                    new Size(46, 46),

                Location =
                    new Point(14, 14),

                BackColor =
                    PrimaryColor,

                ForeColor =
                    Color.White,

                Font = new Font(
                    "Segoe UI",
                    13,
                    FontStyle.Bold)
            };

            profileCard.Controls.Add(lblInitials);

            // =====================================================
            // USER NAME
            // =====================================================
            Label lblUserName = new Label
            {
                Text =
                    string.IsNullOrWhiteSpace(
                        UserSession.FullName)
                        ? "Organizer"
                        : UserSession.FullName,

                AutoEllipsis = true,

                Size =
                    new Size(110, 22),

                ForeColor =
                    Color.White,

                Font = new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold),

                Location =
                    new Point(70, 15)
            };

            profileCard.Controls.Add(lblUserName);

            // =====================================================
            // USER ROLE
            // =====================================================
            Label lblUserRole = new Label
            {
                Text = "Organizer",

                AutoSize = true,

                ForeColor =
                    SuccessColor,

                Font = new Font(
                    "Segoe UI",
                    8),

                Location =
                    new Point(70, 42)
            };

            profileCard.Controls.Add(lblUserRole);

            // =====================================================
            // WORKSPACE LABEL
            // =====================================================
            Label lblMenu = new Label
            {
                Text = "WORKSPACE",

                AutoSize = true,

                ForeColor =
                    SecondaryTextColor,

                Font = new Font(
                    "Segoe UI",
                    8,
                    FontStyle.Bold),

                Location =
                    new Point(25, 280)
            };

            sidebar.Controls.Add(lblMenu);

            // =====================================================
            // DASHBOARD BUTTON
            // =====================================================
            btnDashboard =
                CreateSidebarButton(
                    "Dashboard",
                    315,
                    true);

            sidebar.Controls.Add(btnDashboard);

            // =====================================================
            // MY EVENTS BUTTON
            // =====================================================
            btnMyEvents =
                CreateSidebarButton(
                    "My Events",
                    370,
                    false);

            btnMyEvents.Click +=
                btnMyEvents_Click;

            sidebar.Controls.Add(btnMyEvents);

            // =====================================================
            // PUBLISH EVENT BUTTON
            // =====================================================
            btnPublishEvent =
                CreateSidebarButton(
                    "+  Publish Event",
                    425,
                    false);

            btnPublishEvent.Click +=
                btnPublishEvent_Click;

            sidebar.Controls.Add(btnPublishEvent);

            // =====================================================
            // REFRESH BUTTON
            // =====================================================
            btnRefresh =
                CreateSidebarButton(
                    "Refresh Dashboard",
                    480,
                    false);

            btnRefresh.Click +=
                btnRefresh_Click;

            sidebar.Controls.Add(btnRefresh);

            // =====================================================
            // LOGOUT BUTTON
            // =====================================================
            btnLogout = new Button
            {
                Text = "Logout",

                Size =
                    new Size(190, 45),

                Location =
                    new Point(20, 645),

                BackColor =
                    Color.FromArgb(
                        55,
                        30,
                        38),

                ForeColor =
                    DangerColor,

                FlatStyle =
                    FlatStyle.Flat,

                Font = new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold),

                Cursor =
                    Cursors.Hand
            };

            btnLogout.FlatAppearance.BorderSize = 1;

            btnLogout.FlatAppearance.BorderColor =
                Color.FromArgb(
                    100,
                    45,
                    55);

            btnLogout.MouseEnter +=
                (_, _) =>
                {
                    btnLogout.BackColor =
                        DangerColor;

                    btnLogout.ForeColor =
                        Color.White;
                };

            btnLogout.MouseLeave +=
                (_, _) =>
                {
                    btnLogout.BackColor =
                        Color.FromArgb(
                            55,
                            30,
                            38);

                    btnLogout.ForeColor =
                        DangerColor;
                };

            btnLogout.Click +=
                btnLogout_Click;

            sidebar.Controls.Add(btnLogout);

            // =====================================================
            // MAIN CONTENT
            // =====================================================
            Panel content = new Panel
            {
                Location =
                    new Point(230, 0),

                Size =
                    new Size(1020, 720),

                BackColor =
                    BackgroundColor
            };

            Controls.Add(content);

            // =====================================================
            // PAGE TITLE
            // =====================================================
            Label lblPage = new Label
            {
                Text = "Dashboard",

                AutoSize = true,

                ForeColor =
                    Color.White,

                Font = new Font(
                    "Segoe UI",
                    23,
                    FontStyle.Bold),

                Location =
                    new Point(35, 25)
            };

            content.Controls.Add(lblPage);

            Label lblDescription = new Label
            {
                Text =
                    "Manage your events and monitor your organizer activity.",

                AutoSize = true,

                ForeColor =
                    SecondaryTextColor,

                Font = new Font(
                    "Segoe UI",
                    10),

                Location =
                    new Point(38, 66)
            };

            content.Controls.Add(lblDescription);

            // =====================================================
            // LAST UPDATED
            // =====================================================
            lblLastUpdated = new Label
            {
                Text =
                    "Last updated: --",

                AutoSize = true,

                ForeColor =
                    SecondaryTextColor,

                Font = new Font(
                    "Segoe UI",
                    9),

                Location =
                    new Point(820, 40)
            };

            content.Controls.Add(lblLastUpdated);

            // =====================================================
            // WELCOME CARD
            // =====================================================
            Panel welcomeCard = new Panel
            {
                Size =
                    new Size(950, 105),

                Location =
                    new Point(35, 110),

                BackColor =
                    SurfaceColor
            };

            content.Controls.Add(welcomeCard);

            Panel welcomeAccent = new Panel
            {
                Dock =
                    DockStyle.Left,

                Width = 5,

                BackColor =
                    PrimaryColor
            };

            welcomeCard.Controls.Add(welcomeAccent);

            lblWelcome = new Label
            {
                Text =
                    $"Welcome back, {UserSession.FullName}",

                AutoSize = true,

                ForeColor =
                    Color.White,

                Font = new Font(
                    "Segoe UI",
                    18,
                    FontStyle.Bold),

                Location =
                    new Point(25, 20)
            };

            welcomeCard.Controls.Add(lblWelcome);

            lblEmail = new Label
            {
                Text =
                    UserSession.Email,

                AutoSize = true,

                ForeColor =
                    CyanColor,

                Font = new Font(
                    "Segoe UI",
                    9),

                Location =
                    new Point(28, 59)
            };

            welcomeCard.Controls.Add(lblEmail);

            // =====================================================
            // QUICK CREATE EVENT
            // =====================================================
            Button btnQuickPublish = new Button
            {
                Text =
                    "+  CREATE EVENT",

                Size =
                    new Size(160, 44),

                Location =
                    new Point(755, 30),

                BackColor =
                    PrimaryColor,

                ForeColor =
                    Color.White,

                FlatStyle =
                    FlatStyle.Flat,

                Font = new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold),

                Cursor =
                    Cursors.Hand
            };

            btnQuickPublish.FlatAppearance.BorderSize = 0;

            btnQuickPublish.Click +=
                btnPublishEvent_Click;

            welcomeCard.Controls.Add(btnQuickPublish);

            // =====================================================
            // STATISTICS CARDS
            // =====================================================
            Panel totalCard =
                CreateStatCard(
                    "TOTAL EVENTS",
                    "All your events",
                    PrimaryColor,
                    35,
                    245,
                    out lblTotalEvents);

            content.Controls.Add(totalCard);

            Panel publishedCard =
                CreateStatCard(
                    "PUBLISHED",
                    "Live on KMC",
                    SuccessColor,
                    275,
                    245,
                    out lblPublishedEvents);

            content.Controls.Add(publishedCard);

            Panel upcomingCard =
                CreateStatCard(
                    "UPCOMING",
                    "Future events",
                    CyanColor,
                    515,
                    245,
                    out lblUpcomingEvents);

            content.Controls.Add(upcomingCard);

            Panel capacityCard =
                CreateStatCard(
                    "TOTAL CAPACITY",
                    "Available seats",
                    WarningColor,
                    755,
                    245,
                    out lblTotalCapacity);

            content.Controls.Add(capacityCard);

            // =====================================================
            // RECENT EVENTS TITLE
            // =====================================================
            Label lblRecentTitle = new Label
            {
                Text =
                    "Recent Events",

                AutoSize = true,

                ForeColor =
                    Color.White,

                Font = new Font(
                    "Segoe UI",
                    15,
                    FontStyle.Bold),

                Location =
                    new Point(35, 390)
            };

            content.Controls.Add(lblRecentTitle);

            Label lblRecentSub = new Label
            {
                Text =
                    "Your latest published and updated events",

                AutoSize = true,

                ForeColor =
                    SecondaryTextColor,

                Font = new Font(
                    "Segoe UI",
                    9),

                Location =
                    new Point(37, 423)
            };

            content.Controls.Add(lblRecentSub);

            // =====================================================
            // RECENT EVENTS TABLE
            // =====================================================
            dgvRecentEvents = new DataGridView
            {
                Location =
                    new Point(35, 460),

                Size =
                    new Size(950, 215),

                BackgroundColor =
                    SurfaceColor,

                BorderStyle =
                    BorderStyle.None,

                ReadOnly = true,

                AllowUserToAddRows = false,

                AllowUserToDeleteRows = false,

                AllowUserToResizeRows = false,

                RowHeadersVisible = false,

                AutoGenerateColumns = false,

                MultiSelect = false,

                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,

                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill
            };

            dgvRecentEvents.EnableHeadersVisualStyles = false;

            dgvRecentEvents
                .ColumnHeadersDefaultCellStyle
                .BackColor =
                SurfaceLightColor;

            dgvRecentEvents
                .ColumnHeadersDefaultCellStyle
                .ForeColor =
                Color.White;

            dgvRecentEvents
                .ColumnHeadersDefaultCellStyle
                .Font =
                new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold);

            dgvRecentEvents.ColumnHeadersHeight = 38;

            dgvRecentEvents
                .DefaultCellStyle
                .BackColor =
                SurfaceColor;

            dgvRecentEvents
                .DefaultCellStyle
                .ForeColor =
                Color.White;

            dgvRecentEvents
                .DefaultCellStyle
                .SelectionBackColor =
                SurfaceLightColor;

            dgvRecentEvents
                .DefaultCellStyle
                .SelectionForeColor =
                Color.White;

            dgvRecentEvents
                .DefaultCellStyle
                .Font =
                new Font(
                    "Segoe UI",
                    9);

            dgvRecentEvents.RowTemplate.Height = 36;

            // EVENT TITLE
            dgvRecentEvents.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Event",
                    DataPropertyName = "Title",
                    FillWeight = 130
                });

            // TYPE
            dgvRecentEvents.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Type",
                    DataPropertyName = "EventType",
                    FillWeight = 65
                });

            // DATE
            dgvRecentEvents.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Date",
                    DataPropertyName = "EventDate",
                    FillWeight = 65,

                    DefaultCellStyle =
                        new DataGridViewCellStyle
                        {
                            Format =
                                "dd MMM yyyy"
                        }
                });

            // VENUE
            dgvRecentEvents.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    HeaderText = "Venue",
                    DataPropertyName = "Venue",
                    FillWeight = 100
                });

            // STATUS
            dgvRecentEvents.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "Status",
                    HeaderText = "Status",
                    DataPropertyName = "Status",
                    FillWeight = 60
                });

            dgvRecentEvents.CellFormatting +=
                dgvRecentEvents_CellFormatting;

            content.Controls.Add(dgvRecentEvents);

            // =====================================================
            // EMPTY STATE
            // =====================================================
            lblNoEvents = new Label
            {
                Text =
                    "No events yet. Create your first event!",

                AutoSize = true,

                ForeColor =
                    SecondaryTextColor,

                Font = new Font(
                    "Segoe UI",
                    11),

                Location =
                    new Point(360, 555),

                Visible = false,

                BackColor =
                    SurfaceColor
            };

            content.Controls.Add(lblNoEvents);

            lblNoEvents.BringToFront();
        }

        // =========================================================
        // LOAD DASHBOARD DATA
        // =========================================================
        private async Task LoadDashboardDataAsync()
        {
            try
            {
                btnRefresh.Enabled = false;

                btnRefresh.Text =
                    "Refreshing...";

                List<EventResponse> events =
                    await _apiClient
                        .GetMyEventsAsync();

                // TOTAL EVENTS
                lblTotalEvents.Text =
                    events.Count.ToString();

                // PUBLISHED EVENTS
                int publishedCount =
                    events.Count(
                        e => string.Equals(
                            e.Status,
                            "Published",
                            StringComparison.OrdinalIgnoreCase));

                lblPublishedEvents.Text =
                    publishedCount.ToString();

                // UPCOMING EVENTS
                int upcomingCount =
                    events.Count(
                        e =>
                            e.EventDate.Date >=
                            DateTime.Today &&

                            !string.Equals(
                                e.Status,
                                "Cancelled",
                                StringComparison.OrdinalIgnoreCase));

                lblUpcomingEvents.Text =
                    upcomingCount.ToString();

                // TOTAL CAPACITY
                int totalCapacity =
                    events.Sum(
                        e =>
                            e.TicketTiers != null &&
                            e.TicketTiers.Count > 0

                                ? e.TicketTiers.Sum(
                                    tier =>
                                        tier.Capacity)

                                : e.Capacity);

                lblTotalCapacity.Text =
                    totalCapacity.ToString("N0");

                // RECENT EVENTS
                List<EventResponse> recentEvents =
                    events
                        .OrderByDescending(
                            e => e.EventDate)
                        .Take(5)
                        .ToList();

                dgvRecentEvents.DataSource = null;

                dgvRecentEvents.DataSource =
                    recentEvents;

                bool hasEvents =
                    recentEvents.Count > 0;

                dgvRecentEvents.Visible =
                    hasEvents;

                lblNoEvents.Visible =
                    !hasEvents;

                lblLastUpdated.Text =
                    $"Last updated: {DateTime.Now:hh:mm tt}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Dashboard Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnRefresh.Enabled = true;

                    btnRefresh.Text =
                        "Refresh Dashboard";
                }
            }
        }

        // =========================================================
        // CREATE SIDEBAR BUTTON
        // =========================================================
        private Button CreateSidebarButton(
            string text,
            int y,
            bool active)
        {
            Color normalColor =
                active
                    ? PrimaryColor
                    : SidebarColor;

            Color hoverColor =
                active
                    ? Color.FromArgb(
                        125,
                        95,
                        255)
                    : SurfaceLightColor;

            Button button = new Button
            {
                Text = text,

                TextAlign =
                    ContentAlignment.MiddleLeft,

                Padding =
                    new Padding(
                        18,
                        0,
                        0,
                        0),

                Size =
                    new Size(190, 44),

                Location =
                    new Point(20, y),

                BackColor =
                    normalColor,

                ForeColor =
                    Color.White,

                FlatStyle =
                    FlatStyle.Flat,

                Font = new Font(
                    "Segoe UI",
                    10,
                    active
                        ? FontStyle.Bold
                        : FontStyle.Regular),

                Cursor =
                    Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;

            button.MouseEnter +=
                (_, _) =>
                {
                    button.BackColor =
                        hoverColor;
                };

            button.MouseLeave +=
                (_, _) =>
                {
                    button.BackColor =
                        normalColor;
                };

            return button;
        }

        // =========================================================
        // CREATE STAT CARD
        // =========================================================
        private Panel CreateStatCard(
            string title,
            string description,
            Color accentColor,
            int x,
            int y,
            out Label valueLabel)
        {
            Panel card = new Panel
            {
                Size =
                    new Size(220, 115),

                Location =
                    new Point(x, y),

                BackColor =
                    SurfaceColor
            };

            Panel accent = new Panel
            {
                Dock =
                    DockStyle.Top,

                Height = 4,

                BackColor =
                    accentColor
            };

            card.Controls.Add(accent);

            Label titleLabel = new Label
            {
                Text = title,

                AutoSize = true,

                ForeColor =
                    SecondaryTextColor,

                Font = new Font(
                    "Segoe UI",
                    8,
                    FontStyle.Bold),

                Location =
                    new Point(18, 20)
            };

            card.Controls.Add(titleLabel);

            valueLabel = new Label
            {
                Text = "0",

                AutoSize = true,

                ForeColor =
                    Color.White,

                Font = new Font(
                    "Segoe UI",
                    22,
                    FontStyle.Bold),

                Location =
                    new Point(17, 42)
            };

            card.Controls.Add(valueLabel);

            Label descriptionLabel = new Label
            {
                Text = description,

                AutoSize = true,

                ForeColor =
                    accentColor,

                Font = new Font(
                    "Segoe UI",
                    8),

                Location =
                    new Point(20, 86)
            };

            card.Controls.Add(descriptionLabel);

            return card;
        }

        // =========================================================
        // STATUS COLOR
        // =========================================================
        private void dgvRecentEvents_CellFormatting(
            object? sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0)
            {
                return;
            }

            if (dgvRecentEvents
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
                "Cancelled",
                StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.ForeColor =
                    DangerColor;
            }
            else if (status.Equals(
                "Draft",
                StringComparison.OrdinalIgnoreCase))
            {
                e.CellStyle.ForeColor =
                    WarningColor;
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
        // GET USER INITIALS
        // =========================================================
        private string GetInitials(
            string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return "OR";
            }

            string[] parts =
                fullName.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                return parts[0]
                    .Substring(0, 1)
                    .ToUpper();
            }

            return
                $"{parts[0][0]}{parts[^1][0]}"
                .ToUpper();
        }

        // =========================================================
        // MY EVENTS
        // =========================================================
        private async void btnMyEvents_Click(
            object? sender,
            EventArgs e)
        {
            using MyEventsForm myEventsForm =
                new MyEventsForm();

            myEventsForm.ShowDialog(this);

            await LoadDashboardDataAsync();
        }

        // =========================================================
        // PUBLISH EVENT
        // =========================================================
        private async void btnPublishEvent_Click(
            object? sender,
            EventArgs e)
        {
            using EventForm eventForm =
                new EventForm();

            DialogResult result =
                eventForm.ShowDialog(this);

            if (result ==
                DialogResult.OK)
            {
                await LoadDashboardDataAsync();
            }
        }

        // =========================================================
        // REFRESH
        // =========================================================
        private async void btnRefresh_Click(
            object? sender,
            EventArgs e)
        {
            await LoadDashboardDataAsync();
        }

        // =========================================================
        // LOGOUT
        // =========================================================
        private void btnLogout_Click(
            object? sender,
            EventArgs e)
        {
            DialogResult result =
                MessageBox.Show(
                    "Are you sure you want to logout?",
                    "Logout",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (result !=
                DialogResult.Yes)
            {
                return;
            }

            UserSession.Clear();

            DialogResult =
                DialogResult.OK;

            Close();
        }
    }
}