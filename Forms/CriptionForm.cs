using Shifr.Forms.WorkWitchElements;
using Shifr.WorkFile;
using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace Shifr.Forms
{
    public partial class CriptionForm : Form
    {
        private ShifrClient shifrClient = new ShifrClient();
        private FileSystemTreeBuilder treeBuilder = new FileSystemTreeBuilder();

        public CriptionForm()
        {
            InitializeComponent();
            LoadDrives();
            InitializeComboBox();
        }

        private void InitializeComboBox()
        {
            comboBox1.SelectedIndex = 0; // Выбираем первый элемент по умолчанию
        }

        private void LoadDrives()
        {
            treeBuilder.LoadDrives(treeViewExploler);
        }

        private void treeViewExploler_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            treeBuilder.PopulateNode(e.Node);
        }

        private void treeViewExploler_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag != null)
            {
                textBoxPath.Text = e.Node.Tag.ToString();
            }
        }

        private void ButtonCryption_Click(object sender, EventArgs e)
        {
            if (!File.Exists(textBoxPath.Text))
            {
                MessageBox.Show("Файл не найден. Откройте это окно снова", "ОШИБКА", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(textBoxPassword.Text))
            {
                MessageBox.Show("Пароль не может быть пустым", "ОШИБКА", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string originalPath = textBoxPath.Text;
            string algorithmName = comboBox1.SelectedItem.ToString();
            CipherType cipherType = ConvertToCipherType(algorithmName);

            try
            {
                // Проверяем, что файл текстовый по расширению
                string extension = Path.GetExtension(originalPath).ToLower();
                string[] textExtensions = {
            ".txt", ".csv", ".xml", ".json", ".log", ".ini", ".config",
            ".html", ".htm", ".css", ".js", ".sql", ".md", ".rtf",
            ".bat", ".ps1", ".py", ".java", ".cpp", ".c", ".h", ".cs",
            ".php", ".rb", ".pl", ".sh", ".yaml", ".yml", ".properties"
        };

                if (!textExtensions.Contains(extension))
                {
                    MessageBox.Show("Можно шифровать только текстовые файлы\nПоддерживаемые форматы: " +
                                   string.Join(", ", textExtensions), "ОШИБКА",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Читаем содержимое текстового файла
                string fileContent = File.ReadAllText(originalPath);

                // Шифруем содержимое
                string encryptedContent = shifrClient.ProcessText(
                                            fileContent,
                                            cipherType,
                                            true,
                                            textBoxPassword.Text,
                                            originalPath 
                                            );

                // Перезаписываем файл зашифрованным содержимым
                File.WriteAllText(originalPath, encryptedContent);

                TreeNode node = FindTreeNodeByPath(originalPath);
                if (node != null && node.Parent != null)
                {
                    RefreshNode(node.Parent);
                }

                MessageBox.Show("Текстовый файл успешно зашифрован", "ПРОЦЕСС", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при шифровании: {ex.Message}", "ОШИБКА", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private CipherType ConvertToCipherType(string algorithmName)
        {
            switch (algorithmName)
            {
                case "Цезарь":
                    return CipherType.Caesar;
                case "Виженер":
                    return CipherType.Vigenere;
                case "Плейфер":
                    return CipherType.Playfair;
                default:
                    throw new ArgumentException($"Неизвестный алгоритм: {algorithmName}");
            }
        }

        private TreeNode FindTreeNodeByPath(string path)
        {
            foreach (TreeNode driveNode in treeViewExploler.Nodes)
            {
                TreeNode foundNode = FindNodeRecursive(driveNode, path);
                if (foundNode != null)
                    return foundNode;
            }
            return null;
        }

        private TreeNode FindNodeRecursive(TreeNode currentNode, string targetPath)
        {
            if (currentNode.Tag?.ToString() == targetPath)
                return currentNode;

            foreach (TreeNode child in currentNode.Nodes)
            {
                TreeNode result = FindNodeRecursive(child, targetPath);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void RefreshNode(TreeNode node)
        {
            string currentPath = node.Tag as string;
            if (string.IsNullOrEmpty(currentPath))
                return;

            bool wasExpanded = node.IsExpanded;
            node.Nodes.Clear();
            node.Nodes.Add("Загрузка...");
            if (wasExpanded)
            {
                node.Expand();
                treeBuilder.PopulateNode(node);
            }
        }

        private void Cription_Load(object sender, EventArgs e)
        {

        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        // Обработчик изменения выбора в ComboBox
        private void comboBoxAlgorithm_SelectedIndexChanged(object sender, EventArgs e)
        {
            string algorithm = comboBox1.SelectedItem.ToString();
            if (algorithm == "Цезарь")
            {
                textBoxPassword.Text = "Сдвиг (число)";
            }
            else
            {
                textBoxPassword.Text = "Ключ";
            }
        }
    }
}
