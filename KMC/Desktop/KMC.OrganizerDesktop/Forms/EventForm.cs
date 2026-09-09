using KMC.OrganizerDesktop.Models;
using KMC.OrganizerDesktop.Services;
using System.Drawing;
using System.Linq;

namespace KMC.OrganizerDesktop.Forms
{
    public partial class EventForm : Form
    {
        private readonly KmcApiClient _apiClient;

        // Null = Create Mode
        // Has value = Edit Mode
        private readonly EventResponse? _editingEvent;


        // =========================================================
        // CONTROLS
        // =========================================================
        private TextBox txtTitle = null!;
        private TextBox txtDescription = null!;
        private ComboBox cmbEventType = null!;

        private DateTimePicker dtpEventDate = null!;
        private DateTimePicker dtpEndDate = null!;
        private DateTimePicker dtpStartTime = null!;
        private DateTimePicker dtpEndTime = null!;

        private TextBox txtVenue = null!;
        private TextBox txtLocation = null!;

        private NumericUpDown numStandardPrice = null!;
        private NumericUpDown numStandardCapacity = null!;

        private NumericUpDown numPremiumPrice = null!;
        private NumericUpDown numPremiumCapacity = null!;

        private NumericUpDown numVipPrice = null!;
        private NumericUpDown numVipCapacity = null!;

        private Button btnPublish = null!;
        private Button btnCancel = null!;


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
        // CREATE MODE
        // =========================================================
        public EventForm()
        {
            InitializeComponent();

            _apiClient =
                new KmcApiClient();

            _editingEvent = null;

            CreateEventUI();
        }


        // =========================================================
        // EDIT MODE
        // =========================================================
        public EventForm(
            EventResponse existingEvent)
        {
            InitializeComponent();

            _apiClient =
                new KmcApiClient();

            _editingEvent =
                existingEvent;

            CreateEventUI();

            LoadExistingEventData();
        }


        // =========================================================
        // CREATE UI
        // =========================================================
        private void CreateEventUI()
        {
            Controls.Clear();


            // =====================================================
            // FORM
            // =====================================================
            Text =
                _editingEvent == null
                    ? "KMC - Publish New Event"
                    : "KMC - Update Event";

            StartPosition =
                FormStartPosition.CenterScreen;

            ClientSize =
                new Size(1180, 760);

            MinimumSize =
                new Size(1196, 799);

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
            // HEADER TITLE
            // =====================================================
            Label lblPageTitle =
                new Label
                {
                    Text =
                        _editingEvent == null
                            ? "PUBLISH NEW EVENT"
                            : "UPDATE EVENT",

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
                        new Point(35, 18)
                };

            header.Controls.Add(
                lblPageTitle);


            Label lblSubtitle =
                new Label
                {
                    Text =
                        _editingEvent == null
                            ? "Create and publish a new event to the KMC Event Platform."
                            : "Edit your event details and save the latest changes.",

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
            // STATUS BADGE
            // =====================================================
            string currentStatus =
                _editingEvent?.Status
                ?? "Published";

            Label lblStatus =
                new Label
                {
                    Text =
                        $"  {currentStatus.ToUpper()}  ",

                    AutoSize =
                        true,

                    ForeColor =
                        GetStatusColor(
                            currentStatus),

                    BackColor =
                        SurfaceLightColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            9,
                            FontStyle.Bold),

                    Padding =
                        new Padding(
                            6,
                            5,
                            6,
                            5),

                    Location =
                        new Point(1000, 33)
                };

            header.Controls.Add(
                lblStatus);


            // =====================================================
            // EVENT DETAILS CARD
            // =====================================================
            Panel eventDetailsCard =
                CreateSectionCard(
                    30,
                    130,
                    545,
                    330);


            Controls.Add(
                eventDetailsCard);


            AddSectionTitle(
                eventDetailsCard,
                "EVENT DETAILS",
                "Basic information about your event");


            // =====================================================
            // EVENT TITLE
            // =====================================================
            AddFieldLabel(
                eventDetailsCard,
                "Event Title",
                25,
                85);


            txtTitle =
                CreateTextBox(
                    25,
                    110,
                    495);


            txtTitle.PlaceholderText =
                "Enter event title";


            eventDetailsCard.Controls.Add(
                txtTitle);


            // =====================================================
            // EVENT TYPE
            // =====================================================
            AddFieldLabel(
                eventDetailsCard,
                "Event Type",
                25,
                160);


            cmbEventType =
                new ComboBox
                {
                    Location =
                        new Point(25, 185),

                    Size =
                        new Size(495, 34),

                    DropDownStyle =
                        ComboBoxStyle.DropDownList,

                    FlatStyle =
                        FlatStyle.Flat,

                    BackColor =
                        SurfaceLightColor,

                    ForeColor =
                        Color.White,

                    Font =
                        new Font(
                            "Segoe UI",
                            10)
                };


            cmbEventType.Items.AddRange(
                new object[]
                {
                    "Cultural",
                    "Music",
                    "Food",
                    "Sports",
                    "Education",
                    "Community",
                    "Festival",
                    "Other"
                });


            cmbEventType.SelectedIndex =
                0;


            eventDetailsCard.Controls.Add(
                cmbEventType);


            // =====================================================
            // DESCRIPTION
            // =====================================================
            AddFieldLabel(
                eventDetailsCard,
                "Description",
                25,
                235);


            txtDescription =
                new TextBox
                {
                    Location =
                        new Point(25, 260),

                    Size =
                        new Size(495, 50),

                    Multiline =
                        true,

                    ScrollBars =
                        ScrollBars.Vertical,

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
                        "Write a short description about the event..."
                };


            eventDetailsCard.Controls.Add(
                txtDescription);


            // =====================================================
            // SCHEDULE & LOCATION CARD
            // =====================================================
            Panel scheduleCard =
                CreateSectionCard(
                    605,
                    130,
                    545,
                    330);


            Controls.Add(
                scheduleCard);


            AddSectionTitle(
                scheduleCard,
                "SCHEDULE & LOCATION",
                "Choose when and where the event will take place");


            // =====================================================
            // START DATE
            // =====================================================
            AddFieldLabel(
                scheduleCard,
                "Start Date",
                25,
                85);


            dtpEventDate =
                CreateDatePicker(
                    25,
                    110,
                    235);


            scheduleCard.Controls.Add(
                dtpEventDate);


            // =====================================================
            // END DATE
            // =====================================================
            AddFieldLabel(
                scheduleCard,
                "End Date",
                285,
                85);


            dtpEndDate =
                CreateDatePicker(
                    285,
                    110,
                    235);


            scheduleCard.Controls.Add(
                dtpEndDate);


            // =====================================================
            // START TIME
            // =====================================================
            AddFieldLabel(
                scheduleCard,
                "Start Time",
                25,
                160);


            dtpStartTime =
                CreateTimePicker(
                    25,
                    185,
                    235);


            scheduleCard.Controls.Add(
                dtpStartTime);


            // =====================================================
            // END TIME
            // =====================================================
            AddFieldLabel(
                scheduleCard,
                "End Time",
                285,
                160);


            dtpEndTime =
                CreateTimePicker(
                    285,
                    185,
                    235);


            scheduleCard.Controls.Add(
                dtpEndTime);


            // =====================================================
            // VENUE
            // =====================================================
            AddFieldLabel(
                scheduleCard,
                "Venue",
                25,
                235);


            txtVenue =
                CreateTextBox(
                    25,
                    260,
                    235);


            txtVenue.PlaceholderText =
                "e.g. Kandy City Centre";


            scheduleCard.Controls.Add(
                txtVenue);


            // =====================================================
            // LOCATION
            // =====================================================
            AddFieldLabel(
                scheduleCard,
                "Location",
                285,
                235);


            txtLocation =
                CreateTextBox(
                    285,
                    260,
                    235);


            txtLocation.PlaceholderText =
                "e.g. Kandy";


            scheduleCard.Controls.Add(
                txtLocation);


            // =====================================================
            // TICKET PRICING CARD
            // =====================================================
            Panel ticketCard =
                CreateSectionCard(
                    30,
                    485,
                    1120,
                    180);


            Controls.Add(
                ticketCard);


            AddSectionTitle(
                ticketCard,
                "TICKET PRICING",
                "Configure price and capacity for each ticket category");


            // =====================================================
            // STANDARD CARD
            // =====================================================
            Panel standardCard =
                CreateTicketCard(
                    "STANDARD",
                    PrimaryColor,
                    25,
                    75);


            ticketCard.Controls.Add(
                standardCard);


            AddTicketInputs(
                standardCard,
                out numStandardPrice,
                out numStandardCapacity);


            // =====================================================
            // PREMIUM CARD
            // =====================================================
            Panel premiumCard =
                CreateTicketCard(
                    "PREMIUM",
                    CyanColor,
                    390,
                    75);


            ticketCard.Controls.Add(
                premiumCard);


            AddTicketInputs(
                premiumCard,
                out numPremiumPrice,
                out numPremiumCapacity);


            // =====================================================
            // VIP CARD
            // =====================================================
            Panel vipCard =
                CreateTicketCard(
                    "VIP",
                    WarningColor,
                    755,
                    75);


            ticketCard.Controls.Add(
                vipCard);


            AddTicketInputs(
                vipCard,
                out numVipPrice,
                out numVipCapacity);


            // =====================================================
            // FOOTER ACTION AREA
            // =====================================================
            Panel footer =
                new Panel
                {
                    Location =
                        new Point(30, 685),

                    Size =
                        new Size(1120, 55),

                    BackColor =
                        BackgroundColor
                };


            Controls.Add(
                footer);


            // =====================================================
            // INFO TEXT
            // =====================================================
            Label lblInfo =
                new Label
                {
                    Text =
                        _editingEvent == null
                            ? "The event will be published to the KMC Event Platform."
                            : "Changes will be applied to your existing KMC event.",

                    AutoSize =
                        true,

                    ForeColor =
                        SecondaryTextColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            9),

                    Location =
                        new Point(0, 18)
                };


            footer.Controls.Add(
                lblInfo);


            // =====================================================
            // CANCEL BUTTON
            // =====================================================
            btnCancel =
                new Button
                {
                    Text =
                        "CANCEL",

                    Size =
                        new Size(130, 44),

                    Location =
                        new Point(680, 5),

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


            btnCancel.FlatAppearance.BorderSize =
                0;


            btnCancel.MouseEnter +=
                (_, _) =>
                {
                    btnCancel.BackColor =
                        Color.FromArgb(
                            55,
                            55,
                            72);
                };


            btnCancel.MouseLeave +=
                (_, _) =>
                {
                    btnCancel.BackColor =
                        SurfaceLightColor;
                };


            btnCancel.Click +=
                btnCancel_Click;


            footer.Controls.Add(
                btnCancel);


            // =====================================================
            // PUBLISH / UPDATE BUTTON
            // =====================================================
            btnPublish =
                new Button
                {
                    Text =
                        _editingEvent == null
                            ? "PUBLISH EVENT"
                            : "UPDATE EVENT",

                    Size =
                        new Size(285, 44),

                    Location =
                        new Point(835, 5),

                    BackColor =
                        PrimaryColor,

                    ForeColor =
                        Color.White,

                    FlatStyle =
                        FlatStyle.Flat,

                    Font =
                        new Font(
                            "Segoe UI",
                            10,
                            FontStyle.Bold),

                    Cursor =
                        Cursors.Hand
                };


            btnPublish.FlatAppearance.BorderSize =
                0;


            btnPublish.MouseEnter +=
                (_, _) =>
                {
                    btnPublish.BackColor =
                        Color.FromArgb(
                            125,
                            95,
                            255);
                };


            btnPublish.MouseLeave +=
                (_, _) =>
                {
                    btnPublish.BackColor =
                        PrimaryColor;
                };


            btnPublish.Click +=
                btnPublish_Click;


            footer.Controls.Add(
                btnPublish);


            AcceptButton =
                btnPublish;


            txtTitle.Focus();
        }


        // =========================================================
        // SECTION CARD
        // =========================================================
        private Panel CreateSectionCard(
            int x,
            int y,
            int width,
            int height)
        {
            return new Panel
            {
                Location =
                    new Point(x, y),

                Size =
                    new Size(
                        width,
                        height),

                BackColor =
                    SurfaceColor
            };
        }


        // =========================================================
        // SECTION TITLE
        // =========================================================
        private void AddSectionTitle(
            Control parent,
            string title,
            string subtitle)
        {
            Panel accent =
                new Panel
                {
                    Size =
                        new Size(4, 45),

                    Location =
                        new Point(20, 20),

                    BackColor =
                        PrimaryColor
                };


            parent.Controls.Add(
                accent);


            Label lblTitle =
                new Label
                {
                    Text =
                        title,

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
                        new Point(35, 17)
                };


            parent.Controls.Add(
                lblTitle);


            Label lblSubtitle =
                new Label
                {
                    Text =
                        subtitle,

                    AutoSize =
                        true,

                    ForeColor =
                        SecondaryTextColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            8),

                    Location =
                        new Point(36, 45)
                };


            parent.Controls.Add(
                lblSubtitle);
        }


        // =========================================================
        // FIELD LABEL
        // =========================================================
        private void AddFieldLabel(
            Control parent,
            string text,
            int x,
            int y)
        {
            Label label =
                new Label
                {
                    Text =
                        text,

                    AutoSize =
                        true,

                    ForeColor =
                        Color.White,

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


            parent.Controls.Add(
                label);
        }


        // =========================================================
        // TEXT BOX
        // =========================================================
        private TextBox CreateTextBox(
            int x,
            int y,
            int width)
        {
            return new TextBox
            {
                Location =
                    new Point(
                        x,
                        y),

                Size =
                    new Size(
                        width,
                        34),

                Font =
                    new Font(
                        "Segoe UI",
                        10),

                BackColor =
                    SurfaceLightColor,

                ForeColor =
                    Color.White,

                BorderStyle =
                    BorderStyle.FixedSingle
            };
        }


        // =========================================================
        // DATE PICKER
        // =========================================================
        private DateTimePicker CreateDatePicker(
            int x,
            int y,
            int width)
        {
            return new DateTimePicker
            {
                Location =
                    new Point(
                        x,
                        y),

                Size =
                    new Size(
                        width,
                        34),

                Format =
                    DateTimePickerFormat.Custom,

                CustomFormat =
                    "dd MMM yyyy",

                CalendarMonthBackground =
                    SurfaceColor,

                CalendarForeColor =
                    Color.White,

                Font =
                    new Font(
                        "Segoe UI",
                        10)
            };
        }


        // =========================================================
        // TIME PICKER
        // =========================================================
        private DateTimePicker CreateTimePicker(
            int x,
            int y,
            int width)
        {
            return new DateTimePicker
            {
                Location =
                    new Point(
                        x,
                        y),

                Size =
                    new Size(
                        width,
                        34),

                Format =
                    DateTimePickerFormat.Custom,

                CustomFormat =
                    "hh:mm tt",

                ShowUpDown =
                    true,

                Font =
                    new Font(
                        "Segoe UI",
                        10)
            };
        }


        // =========================================================
        // TICKET CARD
        // =========================================================
        private Panel CreateTicketCard(
            string title,
            Color accentColor,
            int x,
            int y)
        {
            Panel card =
                new Panel
                {
                    Location =
                        new Point(
                            x,
                            y),

                    Size =
                        new Size(
                            340,
                            85),

                    BackColor =
                        SurfaceLightColor
                };


            Panel accent =
                new Panel
                {
                    Dock =
                        DockStyle.Left,

                    Width =
                        4,

                    BackColor =
                        accentColor
                };


            card.Controls.Add(
                accent);


            Label lblTitle =
                new Label
                {
                    Text =
                        title,

                    AutoSize =
                        true,

                    ForeColor =
                        accentColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            9,
                            FontStyle.Bold),

                    Location =
                        new Point(
                            18,
                            9)
                };


            card.Controls.Add(
                lblTitle);


            return card;
        }


        // =========================================================
        // TICKET INPUTS
        // =========================================================
        private void AddTicketInputs(
            Panel card,
            out NumericUpDown priceControl,
            out NumericUpDown capacityControl)
        {
            Label lblPrice =
                new Label
                {
                    Text =
                        "Price (LKR)",

                    AutoSize =
                        true,

                    ForeColor =
                        SecondaryTextColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            7),

                    Location =
                        new Point(
                            18,
                            35)
                };


            card.Controls.Add(
                lblPrice);


            priceControl =
                CreatePriceNumeric(
                    18,
                    53);


            card.Controls.Add(
                priceControl);


            Label lblCapacity =
                new Label
                {
                    Text =
                        "Capacity",

                    AutoSize =
                        true,

                    ForeColor =
                        SecondaryTextColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            7),

                    Location =
                        new Point(
                            180,
                            35)
                };


            card.Controls.Add(
                lblCapacity);


            capacityControl =
                CreateCapacityNumeric(
                    180,
                    53);


            card.Controls.Add(
                capacityControl);
        }


        // =========================================================
        // PRICE INPUT
        // =========================================================
        private NumericUpDown CreatePriceNumeric(
            int x,
            int y)
        {
            return new NumericUpDown
            {
                Location =
                    new Point(
                        x,
                        y),

                Size =
                    new Size(
                        140,
                        27),

                DecimalPlaces =
                    2,

                Minimum =
                    0,

                Maximum =
                    1000000,

                ThousandsSeparator =
                    true,

                BackColor =
                    SurfaceColor,

                ForeColor =
                    Color.White,

                Font =
                    new Font(
                        "Segoe UI",
                        9)
            };
        }


        // =========================================================
        // CAPACITY INPUT
        // =========================================================
        private NumericUpDown CreateCapacityNumeric(
            int x,
            int y)
        {
            return new NumericUpDown
            {
                Location =
                    new Point(
                        x,
                        y),

                Size =
                    new Size(
                        140,
                        27),

                Minimum =
                    0,

                Maximum =
                    100000,

                BackColor =
                    SurfaceColor,

                ForeColor =
                    Color.White,

                Font =
                    new Font(
                        "Segoe UI",
                        9)
            };
        }


        // =========================================================
        // STATUS COLOR
        // =========================================================
        private Color GetStatusColor(
            string status)
        {
            if (status.Equals(
                "Published",
                StringComparison.OrdinalIgnoreCase))
            {
                return SuccessColor;
            }


            if (status.Equals(
                "Cancelled",
                StringComparison.OrdinalIgnoreCase))
            {
                return DangerColor;
            }


            if (status.Equals(
                "Draft",
                StringComparison.OrdinalIgnoreCase))
            {
                return WarningColor;
            }


            return CyanColor;
        }


        // =========================================================
        // LOAD EXISTING EVENT INTO EDIT FORM
        // =========================================================
        private void LoadExistingEventData()
        {
            if (_editingEvent == null)
            {
                return;
            }


            txtTitle.Text =
                _editingEvent.Title;


            txtDescription.Text =
                _editingEvent.Description
                ?? string.Empty;


            // =====================================================
            // EVENT TYPE
            // =====================================================
            string? matchingType =
                cmbEventType.Items
                    .Cast<object>()
                    .Select(
                        item =>
                            item.ToString())
                    .FirstOrDefault(
                        item =>
                            string.Equals(
                                item,
                                _editingEvent.EventType,
                                StringComparison.OrdinalIgnoreCase));


            if (matchingType != null)
            {
                cmbEventType.SelectedItem =
                    matchingType;
            }
            else
            {
                cmbEventType.Items.Add(
                    _editingEvent.EventType);

                cmbEventType.SelectedItem =
                    _editingEvent.EventType;
            }


            // =====================================================
            // DATE / TIME
            // =====================================================
            dtpEventDate.Value =
                _editingEvent.EventDate;


            dtpEndDate.Value =
                _editingEvent.EndDate;


            dtpStartTime.Value =
                DateTime.Today.Add(
                    _editingEvent.StartTime);


            dtpEndTime.Value =
                DateTime.Today.Add(
                    _editingEvent.EndTime);


            // =====================================================
            // VENUE / LOCATION
            // =====================================================
            txtVenue.Text =
                _editingEvent.Venue;


            txtLocation.Text =
                _editingEvent.Location;


            // =====================================================
            // STANDARD
            // =====================================================
            TicketTierResponse? standard =
                FindTicketTier(
                    "Standard");


            if (standard != null)
            {
                SetNumericValue(
                    numStandardPrice,
                    standard.Price);


                SetNumericValue(
                    numStandardCapacity,
                    standard.Capacity);
            }


            // =====================================================
            // PREMIUM
            // =====================================================
            TicketTierResponse? premium =
                FindTicketTier(
                    "Premium");


            if (premium != null)
            {
                SetNumericValue(
                    numPremiumPrice,
                    premium.Price);


                SetNumericValue(
                    numPremiumCapacity,
                    premium.Capacity);
            }


            // =====================================================
            // VIP
            // =====================================================
            TicketTierResponse? vip =
                FindTicketTier(
                    "VIP");


            if (vip != null)
            {
                SetNumericValue(
                    numVipPrice,
                    vip.Price);


                SetNumericValue(
                    numVipCapacity,
                    vip.Capacity);
            }
        }


        // =========================================================
        // PUBLISH / UPDATE EVENT
        // =========================================================
        private async void btnPublish_Click(
            object? sender,
            EventArgs e)
        {
            string title =
                txtTitle.Text.Trim();


            string description =
                txtDescription.Text.Trim();


            string eventType =
                cmbEventType.Text.Trim();


            string venue =
                txtVenue.Text.Trim();


            string location =
                txtLocation.Text.Trim();


            // =====================================================
            // REQUIRED VALIDATION
            // =====================================================
            if (string.IsNullOrWhiteSpace(
                    title)
                ||
                string.IsNullOrWhiteSpace(
                    eventType)
                ||
                string.IsNullOrWhiteSpace(
                    venue)
                ||
                string.IsNullOrWhiteSpace(
                    location))
            {
                MessageBox.Show(
                    "Please fill in all required event details.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);


                return;
            }


            // =====================================================
            // DATE VALIDATION
            // =====================================================
            if (dtpEndDate.Value.Date <
                dtpEventDate.Value.Date)
            {
                MessageBox.Show(
                    "End date cannot be before the start date.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);


                return;
            }


            // =====================================================
            // SAME-DAY TIME VALIDATION
            // =====================================================
            if (dtpEventDate.Value.Date ==
                    dtpEndDate.Value.Date
                &&
                dtpEndTime.Value.TimeOfDay <=
                    dtpStartTime.Value.TimeOfDay)
            {
                MessageBox.Show(
                    "End time must be later than start time when the event starts and ends on the same day.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);


                return;
            }


            // =====================================================
            // CAPACITY VALIDATION
            // =====================================================
            if (numStandardCapacity.Value <= 0
                ||
                numPremiumCapacity.Value <= 0
                ||
                numVipCapacity.Value <= 0)
            {
                MessageBox.Show(
                    "Each ticket category must have a capacity greater than zero.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);


                return;
            }


            try
            {
                btnPublish.Enabled =
                    false;


                btnCancel.Enabled =
                    false;


                btnPublish.Text =
                    _editingEvent == null
                        ? "PUBLISHING..."
                        : "UPDATING...";


                // =================================================
                // REQUEST
                // =================================================
                CreateEventRequest request =
                    new CreateEventRequest
                    {
                        Title =
                            title,

                        Description =
                            description,

                        EventType =
                            eventType,

                        EventDate =
                            dtpEventDate.Value.Date,

                        EndDate =
                            dtpEndDate.Value.Date,

                        StartTime =
                            dtpStartTime.Value.TimeOfDay,

                        EndTime =
                            dtpEndTime.Value.TimeOfDay,

                        Venue =
                            venue,

                        Location =
                            location,

                        Status =
                            _editingEvent?.Status
                            ?? "Published",

                        TicketTiers =
                            new List<TicketTierRequest>
                            {
                                new TicketTierRequest
                                {
                                    Id =
                                        GetExistingTicketTierId(
                                            "Standard"),

                                    Name =
                                        "Standard",

                                    Price =
                                        numStandardPrice.Value,

                                    Capacity =
                                        (int)
                                        numStandardCapacity.Value,

                                    SortOrder =
                                        1
                                },


                                new TicketTierRequest
                                {
                                    Id =
                                        GetExistingTicketTierId(
                                            "Premium"),

                                    Name =
                                        "Premium",

                                    Price =
                                        numPremiumPrice.Value,

                                    Capacity =
                                        (int)
                                        numPremiumCapacity.Value,

                                    SortOrder =
                                        2
                                },


                                new TicketTierRequest
                                {
                                    Id =
                                        GetExistingTicketTierId(
                                            "VIP"),

                                    Name =
                                        "VIP",

                                    Price =
                                        numVipPrice.Value,

                                    Capacity =
                                        (int)
                                        numVipCapacity.Value,

                                    SortOrder =
                                        3
                                }
                            }
                    };


                // =================================================
                // CREATE OR UPDATE
                // =================================================
                EventResponse? result;


                if (_editingEvent == null)
                {
                    result =
                        await _apiClient
                            .CreateEventAsync(
                                request);
                }
                else
                {
                    result =
                        await _apiClient
                            .UpdateEventAsync(
                                _editingEvent.Id,
                                request);
                }


                // =================================================
                // SUCCESS
                // =================================================
                if (result != null)
                {
                    string message =
                        _editingEvent == null
                            ? $"Event \"{title}\" was published successfully."
                            : $"Event \"{title}\" was updated successfully.";


                    string messageTitle =
                        _editingEvent == null
                            ? "Event Published"
                            : "Event Updated";


                    MessageBox.Show(
                        message,
                        messageTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);


                    DialogResult =
                        DialogResult.OK;


                    Close();
                }
            }
            catch (HttpRequestException)
            {
                MessageBox.Show(
                    "Unable to connect to KMC API.\n\nPlease make sure KMC.Api is running.",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,

                    _editingEvent == null
                        ? "Publish Event Failed"
                        : "Update Event Failed",

                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnPublish.Enabled =
                        true;


                    btnCancel.Enabled =
                        true;


                    btnPublish.Text =
                        _editingEvent == null
                            ? "PUBLISH EVENT"
                            : "UPDATE EVENT";
                }
            }
        }


        // =========================================================
        // CANCEL
        // =========================================================
        private void btnCancel_Click(
            object? sender,
            EventArgs e)
        {
            DialogResult result =
                MessageBox.Show(
                    _editingEvent == null
                        ? "Cancel creating this event?"
                        : "Discard your unsaved changes?",
                    "Cancel",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);


            if (result ==
                DialogResult.Yes)
            {
                Close();
            }
        }


        // =========================================================
        // FIND TICKET TIER
        // =========================================================
        private TicketTierResponse? FindTicketTier(
            string tierName)
        {
            if (_editingEvent == null
                ||
                _editingEvent.TicketTiers == null)
            {
                return null;
            }


            return _editingEvent
                .TicketTiers
                .FirstOrDefault(
                    tier =>
                        string.Equals(
                            tier.Name,
                            tierName,
                            StringComparison.OrdinalIgnoreCase));
        }


        // =========================================================
        // GET EXISTING TICKET ID
        // =========================================================
        private int GetExistingTicketTierId(
            string tierName)
        {
            TicketTierResponse? tier =
                FindTicketTier(
                    tierName);


            return tier?.Id ?? 0;
        }


        // =========================================================
        // SET NUMERIC VALUE SAFELY
        // =========================================================
        private void SetNumericValue(
            NumericUpDown control,
            decimal value)
        {
            if (value <
                control.Minimum)
            {
                control.Value =
                    control.Minimum;

                return;
            }


            if (value >
                control.Maximum)
            {
                control.Value =
                    control.Maximum;

                return;
            }


            control.Value =
                value;
        }
    }
}