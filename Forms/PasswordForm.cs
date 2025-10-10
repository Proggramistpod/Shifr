using Shifr.Modul.Data;
using Shifr.WorkFile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shifr.Forms
{
    public partial class PasswordForm : Form
    {
        ShifrClient client = new ShifrClient();
        public PasswordForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (client.DecryptFile(CryotionData.Path, textBoxPassword.Text))
            {
                MessageBox.Show("Расшифрвоано", "УСПЕШНО", MessageBoxButtons.OK);
                this.Close();
            }
        }
    }
}
