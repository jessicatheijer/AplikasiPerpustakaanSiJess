using System;
using System.Drawing;
using System.Windows.Forms;
using AplikasiPerpustakaanSiJess.Repositories;
using AplikasiPerpustakaanSiJess.UI;

namespace AplikasiPerpustakaanSiJess.UI.Forms
{
    public class SignupForm : Form
    {
        TextBox txtNama, txtNik, txtAlamat, txtTelp, txtUser, txtPass;
        ComboBox cbJK;
        Button btnSave, btnEye;

        public SignupForm()
        {
            Theme.ApplyForm(this);
            Text = "Sign Up Anggota - SiJess";
            Width = 520;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Theme.Bg;

            var card = new Panel
            {
                Width = 450,
                Height = 500,
                BackColor = Theme.Card,
                Left = 30,
                Top = 30,
                AutoScroll = true
            };
            Controls.Add(card);

            int y = 20;
            card.Controls.Add(new Label
            {
                Text = "Sign Up Anggota",
                Font = Theme.H1,
                Left = 18,
                Top = y,
                AutoSize = true
            });
            y += 45;

            txtNama = AddInput(card, "Nama lengkap", ref y);
            txtNik = AddInput(card, "NIK/NIS (opsional)", ref y);

            cbJK = new ComboBox
            {
                Left = 18,
                Top = y,
                Width = 400,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbJK.Items.AddRange(new object[] { "Laki-laki", "Perempuan" });
            cbJK.SelectedIndex = 0;
            card.Controls.Add(new Label
            {
                Text = "Jenis kelamin",
                Left = 18,
                Top = y - 18,
                Font = Theme.Body,
                ForeColor = Theme.Muted,
                AutoSize = true
            });
            card.Controls.Add(cbJK);
            y += 55;

            txtAlamat = AddInput(card, "Alamat", ref y);
            txtTelp = AddInput(card, "No telp", ref y);
            txtUser = AddInput(card, "Username", ref y);

            // Password
            card.Controls.Add(new Label
            {
                Text = "Password",
                Left = 18,
                Top = y - 18,
                Font = Theme.Body,
                ForeColor = Theme.Muted,
                AutoSize = true
            });

            txtPass = new TextBox
            {
                Left = 18,
                Top = y,
                Width = 360, 
                UseSystemPasswordChar = true
            };
            card.Controls.Add(txtPass);

            btnEye = new Button
            {
                Text = "👁",
                Left = 18 + 360 + 6,
                Top = y,
                Width = 34,
                Height = txtPass.Height,
                FlatStyle = FlatStyle.Flat
            };
            btnEye.FlatAppearance.BorderSize = 0;
            card.Controls.Add(btnEye);

            btnEye.Click += (s, e) =>
            {
                txtPass.UseSystemPasswordChar = !txtPass.UseSystemPasswordChar;
            };

            y += 55;

            btnSave = new Button
            {
                Text = "Create Account",
                Left = 18,
                Top = y,
                Width = 400,
                Height = 36,
                BackColor = Theme.Primary,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 0;
            card.Controls.Add(btnSave);

            btnSave.Click += (s, e) => DoSignup();
        }

        private TextBox AddInput(Panel parent, string label, ref int y)
        {
            parent.Controls.Add(new Label
            {
                Text = label,
                Left = 18,
                Top = y - 18,
                Font = Theme.Body,
                ForeColor = Theme.Muted,
                AutoSize = true
            });

            var tb = new TextBox
            {
                Left = 18,
                Top = y,
                Width = 400
            };
            parent.Controls.Add(tb);

            y += 55;
            return tb;
        }

        private void DoSignup()
        {
            if (txtNama.Text.Trim() == "" ||
                txtAlamat.Text.Trim() == "" ||
                txtTelp.Text.Trim() == "" ||
                txtUser.Text.Trim() == "" ||
                txtPass.Text == "")
            {
                MessageBox.Show("Mohon lengkapi data wajib.");
                return;
            }

            var repo = new AuthRepository();
            var res = repo.SignupAnggota(
                txtNama.Text.Trim(),
                txtNik.Text.Trim(),
                cbJK.SelectedItem.ToString(),
                txtAlamat.Text.Trim(),
                txtTelp.Text.Trim(),
                txtUser.Text.Trim(),
                txtPass.Text
            );

            MessageBox.Show(res.message);
            if (res.ok) Close();
        }
    }
}
