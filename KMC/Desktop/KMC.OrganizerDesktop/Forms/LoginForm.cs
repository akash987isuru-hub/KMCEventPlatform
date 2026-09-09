using KMC.OrganizerDesktop.Models;
using KMC.OrganizerDesktop.Services;
using KMC.OrganizerDesktop.Session;
using System.Drawing;

namespace KMC.OrganizerDesktop.Forms
{
    public partial class LoginForm : Form
    {
        private readonly KmcApiClient _apiClient;

        private TextBox txtEmail = null!;
        private TextBox txtPassword = null!;

        private Button btnLogin = null!;

        private LinkLabel lnkRegister = null!;

        private CheckBox chkShowPassword = null!;


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

        private readonly Color SecondaryTextColor =
            Color.FromArgb(160, 160, 178);


        // =========================================================
        // CONSTRUCTOR
        // =========================================================
        public LoginForm()
        {
            InitializeComponent();

            _apiClient =
                new KmcApiClient();

            CreateLoginUI();
        }


        // =========================================================
        // CREATE LOGIN UI
        // =========================================================
        private void CreateLoginUI()
        {
            // Remove anything accidentally created in Designer
            Controls.Clear();


            // =====================================================
            // FORM
            // =====================================================
            Text =
                "KMC Organizer Login";

            StartPosition =
                FormStartPosition.CenterScreen;

            ClientSize =
                new Size(920, 560);

            BackColor =
                BackgroundColor;

            FormBorderStyle =
                FormBorderStyle.FixedSingle;

            MaximizeBox =
                false;


            // =====================================================
            // LEFT BRAND PANEL
            // =====================================================
            Panel brandPanel =
                new Panel
                {
                    Location =
                        new Point(0, 0),

                    Size =
                        new Size(390, 560),

                    BackColor =
                        SidebarColor
                };

            Controls.Add(
                brandPanel);


            // =====================================================
            // KMC LOGO
            // =====================================================
            Label lblLogo =
                new Label
                {
                    Text =
                        "KMC",

                    TextAlign =
                        ContentAlignment.MiddleCenter,

                    Size =
                        new Size(110, 55),

                    Location =
                        new Point(45, 55),

                    BackColor =
                        PrimaryColor,

                    ForeColor =
                        Color.White,

                    Font =
                        new Font(
                            "Segoe UI",
                            20,
                            FontStyle.Bold)
                };

            brandPanel.Controls.Add(
                lblLogo);


            // =====================================================
            // BRAND TITLE
            // =====================================================
            Label lblBrandTitle =
                new Label
                {
                    Text =
                        "EVENT PLATFORM",

                    AutoSize =
                        true,

                    ForeColor =
                        Color.White,

                    Font =
                        new Font(
                            "Segoe UI",
                            21,
                            FontStyle.Bold),

                    Location =
                        new Point(45, 145)
                };

            brandPanel.Controls.Add(
                lblBrandTitle);


            Label lblWorkspace =
                new Label
                {
                    Text =
                        "Organizer Workspace",

                    AutoSize =
                        true,

                    ForeColor =
                        CyanColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            11,
                            FontStyle.Bold),

                    Location =
                        new Point(48, 190)
                };

            brandPanel.Controls.Add(
                lblWorkspace);


            // =====================================================
            // BRAND DESCRIPTION
            // =====================================================
            Label lblDescription =
                new Label
                {
                    Text =
                        "Create, publish and manage your events\n" +
                        "through the KMC Event Platform.",

                    AutoSize =
                        true,

                    ForeColor =
                        SecondaryTextColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            10),

                    Location =
                        new Point(48, 245)
                };

            brandPanel.Controls.Add(
                lblDescription);


            // =====================================================
            // FEATURES
            // =====================================================
            Label lblFeature1 =
                CreateFeatureLabel(
                    "✓  Publish new events",
                    315);

            brandPanel.Controls.Add(
                lblFeature1);


            Label lblFeature2 =
                CreateFeatureLabel(
                    "✓  Update your events",
                    350);

            brandPanel.Controls.Add(
                lblFeature2);


            Label lblFeature3 =
                CreateFeatureLabel(
                    "✓  Manage published events",
                    385);

            brandPanel.Controls.Add(
                lblFeature3);


            Label lblFooter =
                new Label
                {
                    Text =
                        "Kandy Municipal Council",

                    AutoSize =
                        true,

                    ForeColor =
                        Color.FromArgb(
                            110,
                            110,
                            130),

                    Font =
                        new Font(
                            "Segoe UI",
                            8),

                    Location =
                        new Point(48, 505)
                };

            brandPanel.Controls.Add(
                lblFooter);


            // =====================================================
            // RIGHT LOGIN AREA
            // =====================================================
            Panel loginArea =
                new Panel
                {
                    Location =
                        new Point(390, 0),

                    Size =
                        new Size(530, 560),

                    BackColor =
                        BackgroundColor
                };

            Controls.Add(
                loginArea);


            // =====================================================
            // LOGIN CARD
            // =====================================================
            Panel loginCard =
                new Panel
                {
                    Size =
                        new Size(410, 445),

                    Location =
                        new Point(60, 55),

                    BackColor =
                        SurfaceColor
                };

            loginArea.Controls.Add(
                loginCard);


            // Purple accent
            Panel accent =
                new Panel
                {
                    Dock =
                        DockStyle.Top,

                    Height =
                        4,

                    BackColor =
                        PrimaryColor
                };

            loginCard.Controls.Add(
                accent);


            // =====================================================
            // LOGIN TITLE
            // =====================================================
            Label lblLoginTitle =
                new Label
                {
                    Text =
                        "Welcome Back",

                    AutoSize =
                        true,

                    ForeColor =
                        Color.White,

                    Font =
                        new Font(
                            "Segoe UI",
                            23,
                            FontStyle.Bold),

                    Location =
                        new Point(38, 38)
                };

            loginCard.Controls.Add(
                lblLoginTitle);


            Label lblLoginSubtitle =
                new Label
                {
                    Text =
                        "Sign in to your organizer account",

                    AutoSize =
                        true,

                    ForeColor =
                        SecondaryTextColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            10),

                    Location =
                        new Point(40, 82)
                };

            loginCard.Controls.Add(
                lblLoginSubtitle);


            // =====================================================
            // EMAIL
            // =====================================================
            Label lblEmail =
                new Label
                {
                    Text =
                        "Email Address",

                    AutoSize =
                        true,

                    ForeColor =
                        Color.White,

                    Font =
                        new Font(
                            "Segoe UI",
                            9,
                            FontStyle.Bold),

                    Location =
                        new Point(40, 135)
                };

            loginCard.Controls.Add(
                lblEmail);


            txtEmail =
                new TextBox
                {
                    Name =
                        "txtEmail",

                    Size =
                        new Size(330, 35),

                    Location =
                        new Point(40, 162),

                    Font =
                        new Font(
                            "Segoe UI",
                            11),

                    BackColor =
                        SurfaceLightColor,

                    ForeColor =
                        Color.White,

                    BorderStyle =
                        BorderStyle.FixedSingle
                };

            loginCard.Controls.Add(
                txtEmail);


            // =====================================================
            // PASSWORD
            // =====================================================
            Label lblPassword =
                new Label
                {
                    Text =
                        "Password",

                    AutoSize =
                        true,

                    ForeColor =
                        Color.White,

                    Font =
                        new Font(
                            "Segoe UI",
                            9,
                            FontStyle.Bold),

                    Location =
                        new Point(40, 220)
                };

            loginCard.Controls.Add(
                lblPassword);


            txtPassword =
                new TextBox
                {
                    Name =
                        "txtPassword",

                    Size =
                        new Size(330, 35),

                    Location =
                        new Point(40, 247),

                    Font =
                        new Font(
                            "Segoe UI",
                            11),

                    BackColor =
                        SurfaceLightColor,

                    ForeColor =
                        Color.White,

                    BorderStyle =
                        BorderStyle.FixedSingle,

                    UseSystemPasswordChar =
                        true
                };

            loginCard.Controls.Add(
                txtPassword);


            // =====================================================
            // SHOW PASSWORD
            // =====================================================
            chkShowPassword =
                new CheckBox
                {
                    Text =
                        "Show password",

                    AutoSize =
                        true,

                    ForeColor =
                        SecondaryTextColor,

                    BackColor =
                        SurfaceColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            8),

                    Location =
                        new Point(40, 292),

                    Cursor =
                        Cursors.Hand
                };

            chkShowPassword.CheckedChanged +=
                chkShowPassword_CheckedChanged;

            loginCard.Controls.Add(
                chkShowPassword);


            // =====================================================
            // LOGIN BUTTON
            // =====================================================
            btnLogin =
                new Button
                {
                    Name =
                        "btnLogin",

                    Text =
                        "SIGN IN",

                    Size =
                        new Size(330, 46),

                    Location =
                        new Point(40, 330),

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

            btnLogin.FlatAppearance.BorderSize =
                0;

            btnLogin.MouseEnter +=
                (_, _) =>
                {
                    btnLogin.BackColor =
                        Color.FromArgb(
                            125,
                            95,
                            255);
                };

            btnLogin.MouseLeave +=
                (_, _) =>
                {
                    btnLogin.BackColor =
                        PrimaryColor;
                };

            btnLogin.Click +=
                btnLogin_Click;

            loginCard.Controls.Add(
                btnLogin);


            // =====================================================
            // REGISTER LINK
            // =====================================================
            Label lblNoAccount =
                new Label
                {
                    Text =
                        "Don't have an organizer account?",

                    AutoSize =
                        true,

                    ForeColor =
                        SecondaryTextColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            8),

                    Location =
                        new Point(40, 397)
                };

            loginCard.Controls.Add(
                lblNoAccount);


            lnkRegister =
                new LinkLabel
                {
                    Name =
                        "lnkRegister",

                    Text =
                        "Create Account",

                    AutoSize =
                        true,

                    LinkColor =
                        CyanColor,

                    ActiveLinkColor =
                        Color.White,

                    VisitedLinkColor =
                        CyanColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            8,
                            FontStyle.Bold),

                    Location =
                        new Point(250, 397),

                    Cursor =
                        Cursors.Hand
                };

            lnkRegister.LinkClicked +=
                lnkRegister_LinkClicked;

            loginCard.Controls.Add(
                lnkRegister);


            AcceptButton =
                btnLogin;

            txtEmail.Focus();
        }


        // =========================================================
        // FEATURE LABEL
        // =========================================================
        private Label CreateFeatureLabel(
            string text,
            int y)
        {
            return new Label
            {
                Text =
                    text,

                AutoSize =
                    true,

                ForeColor =
                    SuccessColor,

                Font =
                    new Font(
                        "Segoe UI",
                        9),

                Location =
                    new Point(48, y)
            };
        }


        // =========================================================
        // SHOW / HIDE PASSWORD
        // =========================================================
        private void chkShowPassword_CheckedChanged(
            object? sender,
            EventArgs e)
        {
            txtPassword.UseSystemPasswordChar =
                !chkShowPassword.Checked;
        }


        // =========================================================
        // LOGIN
        // =========================================================
        private async void btnLogin_Click(
            object? sender,
            EventArgs e)
        {
            string email =
                txtEmail.Text.Trim();

            string password =
                txtPassword.Text;


            // =====================================================
            // VALIDATION
            // =====================================================
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Please enter your email and password.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }


            try
            {
                btnLogin.Enabled =
                    false;

                btnLogin.Text =
                    "SIGNING IN...";


                LoginRequest request =
                    new LoginRequest
                    {
                        Email =
                            email,

                        Password =
                            password
                    };


                var result =
                    await _apiClient
                        .LoginAsync(request);


                if (result == null)
                {
                    MessageBox.Show(
                        "Login failed.",
                        "Login Failed",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }


                // =================================================
                // ORGANIZER ROLE ONLY
                // =================================================
                if (!string.Equals(
                    result.Role,
                    "Organizer",
                    StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show(
                        "Only organizer accounts can access this application.",
                        "Access Denied",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }


                // =================================================
                // SAVE SESSION
                // =================================================
                UserSession.UserId =
                    result.UserId;

                UserSession.FullName =
                    result.FullName;

                UserSession.Email =
                    result.Email;

                UserSession.Role =
                    result.Role;

                UserSession.Token =
                    result.Token;

                UserSession.TokenExpiration =
                    result.TokenExpiration;


                // =================================================
                // OPEN DASHBOARD
                // =================================================
                Hide();

                try
                {
                    using OrganizerDashboardForm dashboard =
                        new OrganizerDashboardForm();

                    dashboard.ShowDialog();
                }
                finally
                {
                    Show();

                    Activate();

                    txtPassword.Clear();

                    chkShowPassword.Checked =
                        false;

                    txtEmail.Focus();
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
                    "Login Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnLogin.Enabled =
                        true;

                    btnLogin.Text =
                        "SIGN IN";
                }
            }
        }


        // =========================================================
        // OPEN REGISTER FORM
        // =========================================================
        private void lnkRegister_LinkClicked(
            object? sender,
            LinkLabelLinkClickedEventArgs e)
        {
            Hide();

            try
            {
                using RegisterForm registerForm =
                    new RegisterForm();

                registerForm.ShowDialog();
            }
            finally
            {
                Show();

                Activate();

                txtEmail.Focus();
            }
        }
    }
}