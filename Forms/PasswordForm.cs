using Shifr.Modul.Data;
using Shifr.WorkFile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
            string filePath = CryotionData.Path; 
            string key = CryotionData.Key;       

            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show("Файл не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }

            if(textBoxPassword.Text == key)
                try
                {
                    string content = File.ReadAllText(filePath);

                    // используем ключ из CryotionData
                    string decryptedContent = client.ProcessText(content, CryotionData.cipherType, false, key, filePath);

                    File.WriteAllText(filePath, decryptedContent);

                    MessageBox.Show("Файл успешно расшифрован", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при дешифровке: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            else
            {
                MessageBox.Show("Ключ не верный", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
