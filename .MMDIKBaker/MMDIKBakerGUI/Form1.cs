using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using MikuMikuDance.Model;
using MikuMikuDance.Model.Ver1;
using MikuMikuDance.Motion;
using MikuMikuDance.Motion.Motion2;
using MMDIKBakerLibrary;

namespace MMDIKBakerGUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void startBake_Click(object sender, EventArgs e)
        {
            // 入力チェック
            if (!File.Exists(pmdFileName.Text))
            {
                MessageBox.Show(
                    "PMDファイルが見つかりません！:\n" + pmdFileName.Text,
                    "PMD読み込みエラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(vmdFileName.Text))
            {
                MessageBox.Show(
                    "VMDファイルが見つかりません！:\n" + vmdFileName.Text,
                    "VMD読み込みエラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // saveVMDの上書き確認
            if (File.Exists(saveVmdName.Text))
            {
                var result = MessageBox.Show(
                    "VMDファイルが存在します。\n" + saveVmdName.Text + "\n上書きしてよろしいですか？",
                    "上書き確認",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if(result != System.Windows.Forms.DialogResult.Yes)
                    return;

                // bakeでエラー出るっぽいので削除しておく
                File.Delete(saveVmdName.Text);
            }

            // Bake!
            try
            {
                MMDModel1 model = (MMDModel1)ModelManager.Read(pmdFileName.Text, MikuMikuDance.Model.CoordinateType.RightHandedCoordinate);
                MMDMotion2 motion = (MMDMotion2)MotionManager.Read(vmdFileName.Text, MikuMikuDance.Motion.CoordinateType.RightHandedCoordinate);
                motion = IKBaker.bake(motion, model);
                MotionManager.Write(saveVmdName.Text, motion);

                MessageBox.Show(
                    "Bake完了",
                    "Baked!!");
            }
            catch
            {
                MessageBox.Show(
                    "Bake中にエラーが発生しました",
                    "未知のエラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
        }

        private void pmdRef_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "PMDファイル|*.pmd";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pmdFileName.Text = dialog.FileName;
            }
        }

        private void vmdRef_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "VMDファイル|*.vmd";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                vmdFileName.Text = dialog.FileName;

                var f = new FileInfo(vmdFileName.Text);
                saveVmdName.Text = Path.Combine(f.DirectoryName, f.Name.Replace(f.Extension, "_baked") + f.Extension);
            }
        }

        private void saveRef_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "VMDファイル|*.vmd";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                saveVmdName.Text = dialog.FileName;
            }
        }
    }
}
