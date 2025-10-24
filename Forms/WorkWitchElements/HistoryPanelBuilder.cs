using Shifr.Modul.Data;
using Shifr.WorkFile;
using Shifr.WorkFile.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shifr.Forms.WorkWitchElements
{
    internal class HistoryPanelBuilder
    {
        private FlowLayoutPanel _container;
        private ShifrClient _client;
        private PasswordForm _form;
        public HistoryPanelBuilder(FlowLayoutPanel container, ShifrClient client)
        {
            _container = container;
            _client = client;
        }
        public void LoadHistory()
        {
            _container.Controls.Clear();

            var allHistory = _client.GetOperationHistory()
                .OrderByDescending(r => r.Date)
                .ToList();

            var history = new List<DataRecord>();

            foreach (var group in allHistory.GroupBy(r => Path.Combine(r.Path, r.File)))
            {
                var operations = group.OrderByDescending(r => r.Date).ToList();
                var lastOp = operations.First();

                if (lastOp.IsEncryption)
                {
                    history.AddRange(operations); // добавляем все операции этого файла
                }
            }

            // Итоговый список history уже содержит все операции, кроме файлов, которые последней операцией были расшифрованы
            history = history.OrderByDescending(r => r.Date).ToList();

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

        private Panel CreateHistoryPanel(DataRecord record)
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
            Button openFileButton = new Button
            {
                Size = new Size(32, 32),
                Location = new Point(panel.Width - 90, (panel.Height - 32) / 2),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Black,
                BackgroundImage = Properties.Resources.OpenFile,
                BackgroundImageLayout = ImageLayout.Zoom,
                Tag = record
            };
            openFileButton.FlatAppearance.BorderSize = 0;
            openFileButton.Click += (s, e) => OpenFileClick(record);
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
            panel.Controls.Add(openFileButton);

            panel.Resize += (s, e) => decryptButton.Left = panel.Width - 45;

            return panel;
        }
        private void DescryptionClick(DataRecord record)
        {
            if (_form == null || _form.IsDisposed)
            {
                _form = new PasswordForm();
                CryotionData.Path = record.Path + "\\" + record.File;
                CryotionData.cipherType = record.CipherType;
                CryotionData.Key = record.Key;
                _form.Show();
            }
            else
            {
                _form.Focus();
            }
        }
        private void OpenFileClick(DataRecord record)
        {
            string fullPath = Path.Combine(record.Path, record.File);

            if (!File.Exists(fullPath))
            {
                MessageBox.Show("Файл не найден", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = fullPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть файл: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

