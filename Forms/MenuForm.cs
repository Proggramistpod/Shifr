using Shifr.Forms;
using Shifr.Forms.WorkWitchElements;
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

namespace Shifr
{
    public partial class MenuForm : Form
    {
        private CriptionForm cr;
        private static ShifrClient sc = new ShifrClient();
        private HistoryPanelBuilder hs;
        public MenuForm()
        {
            InitializeComponent();
        }
        private void Menu_Load(object sender, EventArgs e)
        {
            hs = new HistoryPanelBuilder(flowLayoutPanel1, sc);
            hs.LoadHistory();
        }
        private void EncryptionButton_Click(object sender, EventArgs e)
        {
            if (cr == null || cr.IsDisposed)
            {
                cr = new CriptionForm();
                cr.Show();
            }
            else
            {
                cr.Focus(); 
            }
            hs.LoadHistory();
        }
        private void UpdateButton_Click(object sender, EventArgs e)
        {
            hs.LoadHistory();
        }
    }
}
