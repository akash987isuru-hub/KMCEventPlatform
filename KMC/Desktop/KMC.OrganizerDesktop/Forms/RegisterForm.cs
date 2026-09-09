using KMC.OrganizerDesktop.Models;
using KMC.OrganizerDesktop.Services;
using System.Drawing;

namespace KMC.OrganizerDesktop.Forms
{
    public partial class RegisterForm : Form
    {
        private readonly KmcApiClient _apiClient;

        private TextBox txtFullName = null!;
        private TextBox txtEmail = null!;
        private TextBox txtPassword = null!;
        private TextBox txtConfirmPassword = null!;

        private CheckBox chkShowPassword = null!;

        private Button btnRegister = null!;
        private LinkLabel lnkLogin = null!;


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
        public RegisterForm()
        {
            InitializeComponent();

            _apiClient =
                new KmcApiClient();

            CreateRegisterUI();
        }


        // =========================================================
        // CREATE REGISTER UI
        // =========================================================
        private void CreateRegisterUI()
        {
            // Remove old designer controls if there are any
            Controls.Clear();


            // =====================================================
            // FORM
            // =====================================================
            Text =
                "KMC Organizer Registration";

            StartPosition =
                FormStartPosition.CenterScreen;

            ClientSize =
                new Size(980, 650);

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
                        new Size(390, 650),

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
            // EVENT PLATFORM
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


            // =====================================================
            // ORGANIZER WORKSPACE
            // =====================================================
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
            // DESCRIPTION
            // =====================================================
            Label lblDescription =
                new Label
                {
                    Text =
                        "Join the KMC Event Platform and start\n" +
                        "publishing and managing your events.",

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
                    "✓  Create your organizer account",
                    320);

            brandPanel.Controls.Add(
                lblFeature1);


            Label lblFeature2 =
                CreateFeatureLabel(
                    "✓  Publish events to KMC",
                    355);

            brandPanel.Controls.Add(
                lblFeature2);


            Label lblFeature3 =
                CreateFeatureLabel(
                    "✓  Update and delete your events",
                    390);

            brandPanel.Controls.Add(
                lblFeature3);


            Label lblFeature4 =
                CreateFeatureLabel(
                    "✓  Manage events from one workspace",
                    425);

            brandPanel.Controls.Add(
                lblFeature4);


            // =====================================================
            // FOOTER
            // =====================================================
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
                        new Point(48, 590)
                };

            brandPanel.Controls.Add(
                lblFooter);


            // =====================================================
            // RIGHT AREA
            // =====================================================
            Panel registerArea =
                new Panel
                {
                    Location =
                        new Point(390, 0),

                    Size =
                        new Size(590, 650),

                    BackColor =
                        BackgroundColor
                };

            Controls.Add(
                registerArea);


            // =====================================================
            // REGISTER CARD
            // =====================================================
            Panel registerCard =
                new Panel
                {
                    Size =
                        new Size(460, 565),

                    Location =
                        new Point(65, 42),

                    BackColor =
                        SurfaceColor
                };

            registerArea.Controls.Add(
                registerCard);


            // =====================================================
            // PURPLE TOP ACCENT
            // =====================================================
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

            registerCard.Controls.Add(
                accent);


            // =====================================================
            // TITLE
            // =====================================================
            Label lblRegisterTitle =
                new Label
                {
                    Text =
                        "Create Account",

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
                        new Point(45, 30)
                };

            registerCard.Controls.Add(
                lblRegisterTitle);


            Label lblRegisterSubtitle =
                new Label
                {
                    Text =
                        "Register as an event organizer",

                    AutoSize =
                        true,

                    ForeColor =
                        SecondaryTextColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            10),

                    Location =
                        new Point(47, 72)
                };

            registerCard.Controls.Add(
                lblRegisterSubtitle);


            // =====================================================
            // FULL NAME
            // =====================================================
            Label lblFullName =
                CreateFieldLabel(
                    "Full Name",
                    45,
                    120);

            registerCard.Controls.Add(
                lblFullName);


            txtFullName =
                CreateInputBox(
                    45,
                    145);

            txtFullName.Name =
                "txtFullName";

            registerCard.Controls.Add(
                txtFullName);


            // =====================================================
            // EMAIL
            // =====================================================
            Label lblEmail =
                CreateFieldLabel(
                    "Email Address",
                    45,
                    195);

            registerCard.Controls.Add(
                lblEmail);


            txtEmail =
                CreateInputBox(
                    45,
                    220);

            txtEmail.Name =
                "txtEmail";

            registerCard.Controls.Add(
                txtEmail);


            // =====================================================
            // PASSWORD
            // =====================================================
            Label lblPassword =
                CreateFieldLabel(
                    "Password",
                    45,
                    270);

            registerCard.Controls.Add(
                lblPassword);


            txtPassword =
                CreateInputBox(
                    45,
                    295);

            txtPassword.Name =
                "txtPassword";

            txtPassword.UseSystemPasswordChar =
                true;

            registerCard.Controls.Add(
                txtPassword);


            // =====================================================
            // CONFIRM PASSWORD
            // =====================================================
            Label lblConfirmPassword =
                CreateFieldLabel(
                    "Confirm Password",
                    45,
                    345);

            registerCard.Controls.Add(
                lblConfirmPassword);


            txtConfirmPassword =
                CreateInputBox(
                    45,
                    370);

            txtConfirmPassword.Name =
                "txtConfirmPassword";

            txtConfirmPassword.UseSystemPasswordChar =
                true;

            registerCard.Controls.Add(
                txtConfirmPassword);


            // =====================================================
            // SHOW PASSWORD
            // =====================================================
            chkShowPassword =
                new CheckBox
                {
                    Text =
                        "Show passwords",

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
                        new Point(45, 410),

                    Cursor =
                        Cursors.Hand
                };

            chkShowPassword.CheckedChanged +=
                chkShowPassword_CheckedChanged;

            registerCard.Controls.Add(
                chkShowPassword);


            // =====================================================
            // CREATE ACCOUNT BUTTON
            // =====================================================
            btnRegister =
                new Button
                {
                    Name =
                        "btnRegister",

                    Text =
                        "CREATE ACCOUNT",

                    Size =
                        new Size(370, 46),

                    Location =
                        new Point(45, 445),

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

            btnRegister.FlatAppearance.BorderSize =
                0;


            // Hover effect
            btnRegister.MouseEnter +=
                (_, _) =>
                {
                    btnRegister.BackColor =
                        Color.FromArgb(
                            125,
                            95,
                            255);
                };


            btnRegister.MouseLeave +=
                (_, _) =>
                {
                    btnRegister.BackColor =
                        PrimaryColor;
                };


            btnRegister.Click +=
                btnRegister_Click;

            registerCard.Controls.Add(
                btnRegister);


            // =====================================================
            // LOGIN LINK
            // =====================================================
            Label lblHaveAccount =
                new Label
                {
                    Text =
                        "Already have an organizer account?",

                    AutoSize =
                        true,

                    ForeColor =
                        SecondaryTextColor,

                    Font =
                        new Font(
                            "Segoe UI",
                            8),

                    Location =
                        new Point(45, 515)
                };

            registerCard.Controls.Add(
                lblHaveAccount);


            lnkLogin =
                new LinkLabel
                {
                    Name =
                        "lnkLogin",

                    Text =
                        "Sign In",

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
                        new Point(270, 515),

                    Cursor =
                        Cursors.Hand
                };

            lnkLogin.LinkClicked +=
                lnkLogin_LinkClicked;

            registerCard.Controls.Add(
                lnkLogin);


            AcceptButton =
                btnRegister;

            txtFullName.Focus();
        }


        // =========================================================
        // CREATE FIELD LABEL
        // =========================================================
        private Label CreateFieldLabel(
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
                    Color.White,

                Font =
                    new Font(
                        "Segoe UI",
                        9,
                        FontStyle.Bold),

                Location =
                    new Point(
                        x,
                        y)
            };
        }


        // =========================================================
        // CREATE INPUT BOX
        // =========================================================
        private TextBox CreateInputBox(
            int x,
            int y)
        {
            return new TextBox
            {
                Size =
                    new Size(
                        370,
                        35),

                Location =
                    new Point(
                        x,
                        y),

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
        }


        // =========================================================
        // CREATE FEATURE LABEL
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
                    new Point(
                        48,
                        y)
            };
        }


        // =========================================================
        // SHOW / HIDE PASSWORDS
        // =========================================================
        private void chkShowPassword_CheckedChanged(
            object? sender,
            EventArgs e)
        {
            bool hidePasswords =
                !chkShowPassword.Checked;


            txtPassword.UseSystemPasswordChar =
                hidePasswords;

            txtConfirmPassword.UseSystemPasswordChar =
                hidePasswords;
        }


        // =========================================================
        // REGISTER
        // =========================================================
        private async void btnRegister_Click(
            object? sender,
            EventArgs e)
        {
            string fullName =
                txtFullName.Text.Trim();

            string email =
                txtEmail.Text.Trim();

            string password =
                txtPassword.Text;

            string confirmPassword =
                txtConfirmPassword.Text;


            // =====================================================
            // EMPTY VALIDATION
            // =====================================================
            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show(
                    "Please fill in all fields.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }


            // =====================================================
            // EMAIL VALIDATION
            // =====================================================
            if (!email.Contains("@") ||
                !email.Contains("."))
            {
                MessageBox.Show(
                    "Please enter a valid email address.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtEmail.Focus();

                return;
            }


            // =====================================================
            // PASSWORD MATCH
            // =====================================================
            if (password !=
                confirmPassword)
            {
                MessageBox.Show(
                    "Passwords do not match.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtConfirmPassword.Clear();

                txtConfirmPassword.Focus();

                return;
            }


            // =====================================================
            // PASSWORD LENGTH
            // =====================================================
            if (password.Length < 8)
            {
                MessageBox.Show(
                    "Password must contain at least 8 characters.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtPassword.Focus();

                return;
            }


            // =====================================================
            // PASSWORD STRENGTH
            // =====================================================
            if (!password.Any(char.IsUpper) ||
                !password.Any(char.IsLower) ||
                !password.Any(char.IsDigit))
            {
                MessageBox.Show(
                    "Password must contain at least one uppercase letter, one lowercase letter and one number.",
                    "Validation Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtPassword.Focus();

                return;
            }


            try
            {
                btnRegister.Enabled =
                    false;

                btnRegister.Text =
                    "CREATING ACCOUNT...";


                RegisterRequest request =
                    new RegisterRequest
                    {
                        FullName =
                            fullName,

                        Email =
                            email,

                        Password =
                            password,

                        Role =
                            "Organizer"
                    };


                var result =
                    await _apiClient
                        .RegisterAsync(
                            request);


                if (result != null)
                {
                    MessageBox.Show(
                        "Organizer account created successfully.\n\nPlease sign in to continue.",
                        "Registration Successful",
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
                    "Registration Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnRegister.Enabled =
                        true;

                    btnRegister.Text =
                        "CREATE ACCOUNT";
                }
            }
        }


        // =========================================================
        // BACK TO LOGIN
        // =========================================================
        private void lnkLogin_LinkClicked(
            object? sender,
            LinkLabelLinkClickedEventArgs e)
        {
            Close();
        }
    }
}