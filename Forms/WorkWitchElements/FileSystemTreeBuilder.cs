using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shifr.Forms.WorkWitchElements
{
    internal class FileSystemTreeBuilder
    {
        public void LoadDrives(TreeView treeView)
        {
            treeView.Nodes.Clear();

            // --- Быстрый доступ ---
            TreeNode quickAccess = new TreeNode("Быстрый доступ")
            {
                Tag = null
            };

            AddSpecialFolderNode(quickAccess, "Рабочий стол", Environment.SpecialFolder.Desktop);
            AddSpecialFolderNode(quickAccess, "Документы", Environment.SpecialFolder.MyDocuments);
            AddCustomFolderNode(quickAccess, "Загрузки", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"));
            AddSpecialFolderNode(quickAccess, "Изображения", Environment.SpecialFolder.MyPictures);
            AddSpecialFolderNode(quickAccess, "Музыка", Environment.SpecialFolder.MyMusic);
            AddSpecialFolderNode(quickAccess, "Видео", Environment.SpecialFolder.MyVideos);

            treeView.Nodes.Add(quickAccess);
            quickAccess.Expand();

            // --- Диски ---
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (!drive.IsReady)
                    continue;

                TreeNode driveNode = new TreeNode(drive.Name)
                {
                    Tag = drive.Name
                };
                driveNode.Nodes.Add("Загрузка...");
                treeView.Nodes.Add(driveNode);
            }
        }
        public void PopulateNode(TreeNode node)
        {
            if (node == null || node.Tag == null)
                return;

            string path = node.Tag.ToString();

            if (!Directory.Exists(path))
                return;

            try
            {
                node.Nodes.Clear();

                // --- Папки ---
                foreach (string dir in GetSafeDirectories(path))
                {
                    TreeNode subNode = new TreeNode(Path.GetFileName(dir))
                    {
                        Tag = dir
                    };
                    subNode.Nodes.Add("Загрузка..."); // для подгрузки подпапок
                    node.Nodes.Add(subNode);
                }

                // --- Файлы ---
                foreach (string file in GetSafeFiles(path))
                {
                    TreeNode fileNode = new TreeNode(Path.GetFileName(file))
                    {
                        Tag = file
                    };
                    node.Nodes.Add(fileNode);
                }
            }
            catch
            {
                // Игнорируем ошибки доступа
            }
        }
        private IEnumerable<string> GetSafeDirectories(string path)
        {
            try
            {
                return Directory.GetDirectories(path)
                    .Where(dir =>
                    {
                        try
                        {
                            var attr = File.GetAttributes(dir);
                            bool isSystem = attr.HasFlag(FileAttributes.System);
                            bool isHidden = attr.HasFlag(FileAttributes.Hidden);
                            return !isSystem && !isHidden;
                        }
                        catch
                        {
                            return false;
                        }
                    });
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }
        private IEnumerable<string> GetSafeFiles(string path)
        {
            try
            {
                return Directory.GetFiles(path)
                    .Where(file =>
                    {
                        try
                        {
                            var attr = File.GetAttributes(file);
                            bool isSystem = attr.HasFlag(FileAttributes.System);
                            bool isHidden = attr.HasFlag(FileAttributes.Hidden);
                            return !isSystem && !isHidden;
                        }
                        catch
                        {
                            return false;
                        }
                    });
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        private void AddSpecialFolderNode(TreeNode parent, string name, Environment.SpecialFolder folder)
        {
            string path = Environment.GetFolderPath(folder);
            if (Directory.Exists(path))
            {
                TreeNode node = new TreeNode(name)
                {
                    Tag = path
                };
                node.Nodes.Add("Загрузка...");
                parent.Nodes.Add(node);
            }
        }

        private void AddCustomFolderNode(TreeNode parent, string name, string path)
        {
            if (Directory.Exists(path))
            {
                TreeNode node = new TreeNode(name)
                {
                    Tag = path
                };
                node.Nodes.Add("Загрузка...");
                parent.Nodes.Add(node);
            }
        }
    }

}
