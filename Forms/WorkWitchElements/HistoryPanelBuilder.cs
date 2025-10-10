using Shifr.Modul.Data;
using Shifr.WorkFile;
using Shifr.WorkFile.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shifr.Forms.WorkWitchElements
{
    internal class HistoryPanelBuilder
    {
        private FlowLayoutPanel _container;
        private  ShifrClient _client;
        private PasswordForm _form;
        public HistoryPanelBuilder(FlowLayoutPanel container, ShifrClient client)
        {
            _container = container;
            _client = client;
        }
        public void LoadHistory()
        {
            _container.Controls.Clear();

            var history = _client.GetOperationHistory()
                .Where(r => r.IsEncryption)
                .OrderByDescending(r => r.Date)
                .ToList();

            if (history.Count == 0)
            {
                Label noHistory = new Label()
                {
                    Text = "Нет активных зашифрованных файлов",
                    ForeColor = Color.Gray,
                    Font = new Font("Segoe UI", 10, FontStyle.Italic),
                    AutoSize = true
                };
                _container.Controls.Add(noHistory);
                return;
            }

            foreach (var record in history)
            {
                _container.Controls.Add(CreateHistoryPanel(record));
            }
        }

        private Panel CreateHistoryPanel(IData record)
        {
            Panel panel = new Panel
            {
                Width = _container.ClientSize.Width - 25,
                Height = 70,
                BackColor = Color.Black,
                Margin = new Padding(5),
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Top
            };

            Label pathLabel = new Label
            {
                Text = $"Путь: {record.Path}",
                Location = new Point(10, 10),
                AutoSize = true,
                ForeColor = Color.Yellow,
                Font = new Font("Segoe UI", 9)
            };

            Label fileLabel = new Label
            {
                Text = $"Файл: {record.File}",
                Location = new Point(10, 28),
                AutoSize = true,
                ForeColor = Color.Yellow,
                Font = new Font("Segoe UI", 9)
            };

            Label dateLabel = new Label
            {
                Text = $"Дата: {record.Date:G}",
                Location = new Point(10, 46),
                AutoSize = true,
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.LightYellow
            };

            Button decryptButton = new Button
            {
                Size = new Size(32, 32),
                Location = new Point(panel.Width - 50, (panel.Height - 32) / 2),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Black,
                BackgroundImage = Properties.Resources.Key,
                BackgroundImageLayout = ImageLayout.Zoom,
                Tag = record.File
            };
            decryptButton.FlatAppearance.BorderSize = 0;

            decryptButton.Click += (s, e) =>
            {
                // открываем форму для ввода пароля
                using (var decryptForm = new PasswordForm())
                {
                    DescryptionClick(record);
                }
            };

            panel.Controls.Add(pathLabel);
            panel.Controls.Add(fileLabel);
            panel.Controls.Add(dateLabel);
            panel.Controls.Add(decryptButton);

            panel.Resize += (s, e) => decryptButton.Left = panel.Width - 45;

            return panel;
        }
        private void DescryptionClick(IData record)
        {
            if (_form == null || _form.IsDisposed)
            {
                _form = new PasswordForm();
                CryotionData.Path = record.Path + "\\" + record.File;
                _form.Show();
            }
            else
            {
                _form.Focus();
            }
        }
    }
}
