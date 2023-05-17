using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ListView = System.Windows.Forms.ListView;
using System.Diagnostics;
using System.IO.Compression;

namespace lr27_28
{
    public partial class Form1 : Form
    {

        private string currentDirectory;

        public Form1()
        {
            InitializeComponent();

        }

        private void disks_btn_Click(object sender, EventArgs e)
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                TreeNode node = new TreeNode(drive.Name);
                node.Tag = drive.RootDirectory.FullName;
                node.Nodes.Add(new TreeNode());
                treeView1.Nodes.Add(node);
            }
        }

        private void basic_properties_btn_Click(object sender, EventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;
            if (node.Nodes.Count == 1 && node.Nodes[0].Tag == null)
            {
                node.Nodes.Clear();

                string path = node.Tag.ToString();
                DirectoryInfo directory = new DirectoryInfo(path);
                try
                {
                    foreach (DirectoryInfo subdirectory in directory.GetDirectories())
                    {
                        TreeNode subnode = new TreeNode(subdirectory.Name);
                        subnode.Tag = subdirectory.FullName;
                        subnode.Nodes.Add(new TreeNode());
                        node.Nodes.Add(subnode);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Відмовлено в доступі.");
                }

                foreach (FileInfo file in directory.GetFiles())
                {
                    TreeNode subnode = new TreeNode(file.Name);
                    subnode.Tag = file.FullName;
                    node.Nodes.Add(subnode);
                }
            }
        }

        private void main_properties_catalog_btn_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();

            TreeNode node = treeView1.SelectedNode;
            if (node.Tag != null)
            {
                currentDirectory = node.Tag.ToString();
                DirectoryInfo directory = new DirectoryInfo(currentDirectory);
                try
                {
                    foreach (DirectoryInfo subdirectory in directory.GetDirectories())
                    {
                        ListViewItem item = new ListViewItem(subdirectory.Name);
                        item.SubItems.Add("Папка");
                        item.SubItems.Add(subdirectory.LastWriteTime.ToString());
                        item.Tag = subdirectory.FullName;
                        listView1.Items.Add(item);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Відмовлено в доступі.");
                }

                foreach (FileInfo file in directory.GetFiles())
                {
                    ListViewItem item = new ListViewItem(file.Name);
                    item.SubItems.Add(file.Extension);
                    item.SubItems.Add(file.LastWriteTime.ToString());
                    item.Tag = file.FullName;
                    listView1.Items.Add(item);
                }
            }
        }

        private void main_properties_selected_file_btn_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
            {
                return;
            }
            string path = listView1.SelectedItems[0].Tag.ToString();
            FileInfo file = new FileInfo(path);
            OpenImageFile(path);
        }

        private void OpenImageFile(string fileName)
        {
            Process.Start(fileName);
        }

        private bool IsImageFile(string extension)
        {
            throw new NotImplementedException();
        }

        private void filtering_list_files_btn_Click(object sender, EventArgs e)
        {
            string filter = textBox1.Text.ToLower();

            foreach (ListViewItem item in listView1.Items)
            {
                string name = item.Text.ToLower();
                if (name.Contains(filter))
                {
                    item.ForeColor = SystemColors.WindowText;
                    item.BackColor = SystemColors.Window;
                }
                else
                {
                    item.ForeColor = SystemColors.GrayText;
                    item.BackColor = SystemColors.Control;
                }
            }
        }

        private void security_attributes_btn_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string path = listView1.SelectedItems[0].Tag.ToString();
                FileSystemSecurity security = null;
                if (Directory.Exists(path))
                {
                    DirectoryInfo directory = new DirectoryInfo(path);
                    security = directory.GetAccessControl();
                }
                else if (File.Exists(path))
                {
                    FileInfo file = new FileInfo(path);
                    security = file.GetAccessControl();
                }
                if (security != null)
                {
                    SecurityForm form = new SecurityForm();
                    form.DisplaySecurity(security);
                    form.ShowDialog();
                }
            }
        }

        private void view_text_file_btn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                OpenTextFile(openFileDialog.FileName);
            }
        }

        private void OpenTextFile(string fileName)
        {
            Process.Start(fileName);
        }

        private void view_graphic_file_btn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.bmp, *.jpg, *.jpeg, *.png)|*.bmp;*.jpg;*.jpeg;*.png";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                OpenImageFile(openFileDialog.FileName);
            }
        }

        private void creating_directory_btn_Click(object sender, EventArgs e)
        {
            string directoryPath = textBox2.Text;

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                MessageBox.Show("Каталог успішно створено.");
            }
            else
            {
                MessageBox.Show("Каталог вже існує.");
            }
        }

        private void moving_directory_btn_Click(object sender, EventArgs e)
        {
            string sourceDirectoryPath = textBox3.Text;
            string destinationDirectoryPath = textBox4.Text;

            if (Directory.Exists(sourceDirectoryPath))
            {
                Directory.Move(sourceDirectoryPath, destinationDirectoryPath);
                MessageBox.Show("Каталог успішно переміщено.");
            }
            else
            {
                MessageBox.Show("Каталог не знайдено.");
            }
        }

        private void copying_directory_btn_Click(object sender, EventArgs e)
        {
            string sourceDirectoryPath = textBox3.Text;
            string destinationDirectoryPath = textBox4.Text;

            if (Directory.Exists(sourceDirectoryPath))
            {
                Directory.CreateDirectory(destinationDirectoryPath);
                string[] files = Directory.GetFiles(sourceDirectoryPath);

                foreach (string file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string destinationFilePath = Path.Combine(destinationDirectoryPath, fileName);
                    File.Copy(file, destinationFilePath, true);
                }

                MessageBox.Show("Каталог успішно скопійовано.");
            }
            else
            {
                MessageBox.Show("Каталог не знайдено.");
            }
        }

        private void delete_directory_btn_Click(object sender, EventArgs e)
        {
            string directoryPath = textBox2.Text;

            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
                MessageBox.Show("Каталог успішно видалено.");
            }
            else
            {
                MessageBox.Show("Каталог не знайдено.");
            }
        }

        private void file_creation_btn_Click(object sender, EventArgs e)
        {
            string filePath = textBox5.Text;

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                MessageBox.Show("Файл успішно створено.");
            }
            else
            {
                MessageBox.Show("Файл вже існує.");
            }
        }

        private void moving_file_btn_Click(object sender, EventArgs e)
        {
            string sourceFilePath = textBox6.Text;
            string destinationFilePath = textBox7.Text;

            if (File.Exists(sourceFilePath))
            {
                File.Move(sourceFilePath, destinationFilePath);
                MessageBox.Show("Файл успішно переміщено.");
            }
            else
            {
                MessageBox.Show("Файл не знайдено.");
            }
        }

        private void copying_file_btn_Click(object sender, EventArgs e)
        {
            string sourceFilePath = textBox6.Text;
            string destinationFilePath = textBox7.Text;

            if (File.Exists(sourceFilePath))
            {
                File.Copy(sourceFilePath, destinationFilePath, true);
                MessageBox.Show("Файл успішно скопійовано.");
            }
            else
            {
                MessageBox.Show("Файл не знайдено.");
            }
        }

        private void file_deletion_btn_Click(object sender, EventArgs e)
        {
            string filePath = textBox5.Text;

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                MessageBox.Show("Файл успішно видалено.");
            }
            else
            {
                MessageBox.Show("Файл не знайдено.");
            }
        }

        private void reading_attribute_btn_Click(object sender, EventArgs e)
        {
            string filePath = textBox6.Text;

            if (File.Exists(filePath))
            {
                FileAttributes attributes = File.GetAttributes(filePath);

                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {

                    attributes &= ~FileAttributes.ReadOnly;
                    File.SetAttributes(filePath, attributes);
                    MessageBox.Show("Атрибут \"тільки для читання\" успішно знято з файлу.");
                }
                else
                {

                    attributes |= FileAttributes.ReadOnly;
                    File.SetAttributes(filePath, attributes);
                    MessageBox.Show("Атрибут \"тільки для читання\" успішно додано до файлу.");
                }
            }
            else
            {
                MessageBox.Show("Файл не знайдено.");
            }
        }

        private void attribute_hidden_btn_Click(object sender, EventArgs e)
        {
            string directoryPath = textBox3.Text;

            if (Directory.Exists(directoryPath))
            {
                FileAttributes attributes = File.GetAttributes(directoryPath);

                if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {

                    attributes &= ~FileAttributes.Hidden;
                    File.SetAttributes(directoryPath, attributes);
                    MessageBox.Show("Атрибут \"прихований\" успішно знято з каталогу.");
                }
                else
                {

                    attributes |= FileAttributes.Hidden;
                    File.SetAttributes(directoryPath, attributes);
                    MessageBox.Show("Атрибут \"прихований\" успішно додано до каталогу.");
                }
            }
            else
            {
                MessageBox.Show("Каталог не знайдено.");
            }
        }

        private void archive_creation_btn_Click(object sender, EventArgs e)
        {
            string sourceDirectoryPath = textBox3.Text;
            string zipFilePath = textBox8.Text;

            ZipFile.CreateFromDirectory(sourceDirectoryPath, zipFilePath);
            MessageBox.Show("Архів успішно створено.");
        }

        private void unpacking_archive_btn_Click(object sender, EventArgs e)
        {
            string zipFilePath = textBox3.Text;
            string destinationDirectoryPath = textBox8.Text;

            ZipFile.ExtractToDirectory(zipFilePath, destinationDirectoryPath);
            MessageBox.Show("Файли успішно розпаковано.");
        }
    }

    class SecurityForm
    {
        private FileSystemSecurity security;

        public SecurityForm()
        {
        }

        public SecurityForm(FileSystemSecurity security)
        {
            this.security = security;
        }

        internal void DisplaySecurity(FileSystemSecurity security)
        {
            throw new NotImplementedException();
        }

        internal void ShowDialog()
        {
            throw new NotImplementedException();
        }
    }
}
