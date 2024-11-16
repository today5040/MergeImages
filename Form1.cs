using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MergeImages
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnChooseFolder_Click(object sender, EventArgs e)
        {
            // 创建 FolderBrowserDialog 对象
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                // 设置初始目录
                folderBrowserDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                // 显示对话框
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    // 获取用户选择的路径
                    txtFolder.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }
        private async void btnMerge_Click(object sender, EventArgs e)
        {
            var folder = txtFolder.Text;
            if (string.IsNullOrWhiteSpace(folder))
            {
                MessageBox.Show("请先选择文件夹");
                return;
            }
            txtResult.AppendText("所选文件夹" + folder + ";开始检测文件...\r\n");
            var files = Directory.GetFiles(folder);
            if (files.Length == 0)
            {
                txtResult.AppendText("文件夹" + folder + "未发现文件，已停止处理。\r\n");
                return;
            }
            txtResult.AppendText("文件夹" + folder + "检测到" + files.Length + "个文件，禁止文件夹中存放非图片文件，开始处理。\r\n");
            try
            {
                // 使用 Task 在后台线程执行处理
                await Task.Run(() =>
                {
                    string[] imageFiles = files.Where(c => IsImageFile(c)).ToArray();
                    if (imageFiles.Length == 0)
                    {
                        // 更新 UI（使用 Invoke）
                        Invoke(new Action(() =>
                        {
                            txtResult.AppendText("文件夹中未发现有效图片文件;\r\n");
                        }));
                    }

                    Bitmap[] bitmaps = new Bitmap[imageFiles.Length];
                    for (int i = 0; i < imageFiles.Length; i++)
                    {
                        bitmaps[i] = new Bitmap(imageFiles[i]);
                    }

                    Bitmap output = new Bitmap(bitmaps[0].Width, bitmaps.Select(temp => temp.Height).Sum());
                    int yy = 0;

                    for (int i = 0; i < bitmaps.Length; i++)
                    {
                        for (int y = 0; y < bitmaps[i].Height; y++)
                        {
                            for (int x = 0; x < bitmaps[i].Width; x++)
                            {
                                var c = bitmaps[i].GetPixel(x, y);
                                output.SetPixel(x, yy, c);
                            }
                            yy++;
                        }
                        // 更新 UI（使用 Invoke）
                        Invoke(new Action(() =>
                        {
                            txtResult.AppendText("正在处理第" + (i + 1) + "张图片;\r\n");
                        }));
                    }

                    string path = Path.Combine(folder, "合并后图片.jpg");
                    output.Save(path, ImageFormat.Jpeg);

                    // 释放资源
                    foreach (var bmp in bitmaps)
                    {
                        bmp.Dispose();
                    }
                    output.Dispose();

                    // 更新 UI（使用 Invoke）
                    Invoke(new Action(() =>
                    {
                        txtResult.AppendText("处理完成；保存路径：" + path + "；\r\n");
                    }));
                });
            }
            catch (Exception ex)
            {
                txtResult.AppendText("处理时，发生错误，" + ex.Message + ";\r\n");
            }
        }
        bool IsImageFile(string filePath)
        {
            // 常见图片扩展名
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp" };
            string fileExtension = Path.GetExtension(filePath)?.ToLower();

            return imageExtensions.Contains(fileExtension);
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}
