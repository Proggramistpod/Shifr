using Shifr.Forms.WorkWitchElements;
using Shifr.WorkFile;
using System;
using System.IO;
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
            shifrClient.EncryptFile(originalPath, textBoxPassword.Text);

            TreeNode node = FindTreeNodeByPath(originalPath);
            if (node != null && node.Parent != null)
            {
                RefreshNode(node.Parent); 
            }

            MessageBox.Show("Данные зашифрованы", "ПРОЦЕСС", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    }
}
