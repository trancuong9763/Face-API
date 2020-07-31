﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Face;
using Emgu.CV.CvEnum;
using System.IO;
using System.Threading;
using System.Diagnostics;
using BUS;
using DTO;

namespace FaceAPI
{
    public partial class DanhSachSV : Form
    {
        private Capture quayVideo = null;
        static readonly CascadeClassifier cascadeClassifier = new CascadeClassifier("haarcascade_frontalface_alt.xml");
        Mat frame = new Mat();
        private Image<Bgr, Byte> currentFrame = null;
        private bool addface = false;
        private bool facederection = false;
        int dem = 1;
        public DanhSachSV()
        {
            InitializeComponent();
            ChonLop();
            
        }

      
        protected void LoadDSSV()
        {

            dgvDSSV.DataSource = SinhVienBUS.LayDSSV();

        }
        protected void ChonLop()
        {
            SinhVienDTO sv = new SinhVienDTO();
            cboTim.DataSource = SinhVienBUS.ChonLop(sv);

            cboTim.DisplayMember = "MaLop";
            cboTim.ValueMember = "MaLop";
        }
        protected void XoaForm()
        {
            
            txtMSSV.Text = "";
            txtHoten.Text ="";
            txtLop.Text = "" ;
        }
        protected void GiaoDienThem(bool gd)
        {
            txtMSSV.Enabled = gd;
            txtHoten.Enabled = gd;
            txtLop.Enabled = gd;
            btnThem.Enabled = gd;
            
        }
        private void btnThem_Click(object sender, EventArgs e)
        {
            
            SinhVienDTO sv = new SinhVienDTO();
            sv.Ma_SV = txtMSSV.Text;
            sv.Ten_SV = txtHoten.Text;
            sv.Ma_Lop = txtLop.Text; ;
            
            if (txtMSSV.Text == "" || txtHoten.Text == "" || txtLop.Text == "")
            {
                MessageBox.Show("Thông tin không được để trống");
            }

            
            else if (r.IsMatch(txtHoten.Text) || r.IsMatch(txtLop.Text) || r.IsMatch(txtMSSV.Text) )
            {
                MessageBox.Show("Thông tin không hợp lệ");
            }
            else
            {
                
                if(dem == 1)
                {
                    if (SinhVienBUS.ThemSV(sv))
                    {
                        addface = true;
                        MessageBox.Show("Bạn Hãy Thêm Vào 5 Khuôn Mặt");
                        MessageBox.Show("Thêm Khuông Mặt Thứ: " + dem + " Thành Công");
                        LoadDSSV();
                        txtHoten.Enabled = false;
                        txtLop.Enabled = false;
                        txtMSSV.Enabled = false;
                        dem++;
                    }
                    else
                    {
                        MessageBox.Show("Sinh viên đã tồn tại");
                    }             
                }
                else if (dem > 1 && dem<=5)
                {
                    addface = true;
                    MessageBox.Show("Thêm Khuông Mặt Thứ: " + dem + " Thành Công");
                    dem++;
                    if(dem==6)
                    {
                        MessageBox.Show("Thêm sinh viên thành công");
                        txtHoten.Enabled = true;
                        txtLop.Enabled = true;
                        txtMSSV.Enabled = true;
                        txtHoten.Text = "";
                        txtLop.Text = "";
                        txtMSSV.Text = "";
                        dem = 1;
                    }
                    
                }
                else
                {

                    MessageBox.Show("Thêm sinh viên không thành công");

                   
                }
            }
                
        }
           
           
           
        

        private void StartFrame(object sender, EventArgs e)
        {

            quayVideo.Retrieve(frame, 0);
            currentFrame = frame.ToImage<Bgr, Byte>().Resize(320,240, Inter.Cubic);
       
            Mat grayImage = new Mat();
            CvInvoke.CvtColor(currentFrame, grayImage, ColorConversion.Bgr2Gray);
            CvInvoke.EqualizeHist(grayImage, grayImage);
            Rectangle[] faces = cascadeClassifier.DetectMultiScale(grayImage, 1.2, 10, new Size(20,20));


                if (faces.Length > 0)
                {
                    foreach (var face in faces)
                    {
                        // vẽ hình vuông vào mặt
                        CvInvoke.Rectangle(currentFrame, face, new Bgr(Color.Red).MCvScalar, 2);

                        //add khuôn mặt : resualtFace
                        Image<Gray, Byte> resualtFace = currentFrame.Convert<Gray, Byte>();
                        resualtFace.ROI = face;
                        picBox2.SizeMode = PictureBoxSizeMode.StretchImage;// Chỉnh size cho imagebox;
                        picBox2.Image = resualtFace.Bitmap;

                        if (addface)
                        {
                            string path = Directory.GetCurrentDirectory() + @"\TrainedImages";//Tạo thư mục Trained trong debug
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);
                            Task.Factory.StartNew(() =>
                            {
                                
                                    resualtFace.Resize(100, 100, Inter.Cubic).Save(path + @"\" + System.Text.RegularExpressions.Regex.Replace(txtHoten.Text.Trim(), @"[\s+]", "") + "_" + txtMSSV.Text.Trim() + "_" + txtLop.Text.Trim() + "_" + dem + ".bmp");
                                    Thread.Sleep(1000);
                      
                            });
                        }
                        addface = false;
                        if (btnThem.InvokeRequired)
                        {
                            btnThem.Invoke(new ThreadStart(delegate {
                                btnThem.Enabled = true;
                            }));
                        }
                    }  
                }
            picBox.Image = currentFrame.Bitmap;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            quayVideo = new Capture();
            quayVideo.ImageGrabbed += StartFrame;
            quayVideo.Start();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            SinhVienDTO sv = new SinhVienDTO();
            sv.Ma_SV = txtMSSV.Text;
            DialogResult dialogResult = MessageBox.Show("Bạn có chắc là muốn xóa không ?", "Thông báo", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                if (SinhVienBUS.XoaSV(sv))
                {
                    for (int i = 1; i <= 5; i++)
                    {

                        string path = Directory.GetCurrentDirectory() + @"\TrainedImages";
                        string[] files = Directory.GetFiles(path, txtHoten.Text + "_" + txtMSSV.Text + "_" + txtLop.Text + "_" + i + "*.bmp", SearchOption.AllDirectories);
                        foreach (var file in files)
                        {
                            File.Delete(file);
                           
                        }
                    }
                    MessageBox.Show("Xóa thành công");
                    XoaForm();
                    LoadDSSV();

                }
                else
                {
                    MessageBox.Show("Xóa Thất bại");
                }
            }
            else if (dialogResult == DialogResult.No)
            {

            }
        }

        private void dgvDSSV_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1 && e.ColumnIndex > -1 && dgvDSSV.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
            {
                
               
                dgvDSSV.CurrentRow.Selected = true;
                txtMSSV.Text = dgvDSSV.Rows[e.RowIndex].Cells["Ma_SV"].FormattedValue.ToString();
                txtHoten.Text = dgvDSSV.Rows[e.RowIndex].Cells["Ten_SV"].FormattedValue.ToString();
                txtLop.Text = dgvDSSV.Rows[e.RowIndex].Cells["MaLop"].FormattedValue.ToString();
            }
            
        }

        

        private void btnStop_Click(object sender, EventArgs e)
        {
            if(quayVideo!=null)
            {
                quayVideo.Stop();
            }
           
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SinhVienDTO sv = new SinhVienDTO();
            sv.Ma_Lop = cboTim.Text.ToString();
            
                dgvDSSV.DataSource = SinhVienBUS.LayDSLop(sv.Ma_Lop);
            
           
        }

       

        private void btnTim_Click(object sender, EventArgs e)
        {
            SinhVienDTO sv = new SinhVienDTO();
            sv.Ma_SV = txtTim.Text.ToString();
            if (txtTim.Text=="")
            {
                MessageBox.Show("Bạn chưa nhập thông tin cần tìm");
            }


           
            else

                dgvDSSV.DataSource = SinhVienBUS.TimKiemMaSV(sv.Ma_SV);

        }
        System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex(@"[~`!@#$%^&*()+=|\\{}':;.,<>/?[\]""_-]");
       

        private void txtLop_KeyPress(object sender, KeyPressEventArgs e)
        {
            
            e.KeyChar = char.Parse(e.KeyChar.ToString().ToUpper());
            

              if (e.Handled = (e.KeyChar == (char)Keys.Space))
            {

            }

            else
            {
                e.Handled = false;
            }

        }

        private void txtMSSV_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
       (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // ko cho phep nhap dau .
            else if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') == -1))
            {
                e.Handled = true;
            }
            else if (e.Handled = (e.KeyChar == (char)Keys.Space))
            {

            }

            else
            {
                e.Handled = false;
            }
        }

        private void txtTim_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
      (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // ko cho phep nhap dau .
            else if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') == -1))
            {
                e.Handled = true;
            }
            else if (e.Handled = (e.KeyChar == (char)Keys.Space))
            {

            }

            else
            {
                e.Handled = false;
            }
        }

        private void cboTim_TextChanged(object sender, EventArgs e)
        {
            if (cboTim.SelectedIndex < 0)
            {
                cboTim.Text = "";
            }
            else
            {
                cboTim.Text = cboTim.SelectedText;
            }
        }

        private void DanhSachSV_Load(object sender, EventArgs e)
        {
            LoadDSSV();

        }

        private void btnCapNhat_Click(object sender, EventArgs e)
        {
            addface = true;
            SinhVienDTO sv = new SinhVienDTO();
            sv.Ma_SV = txtMSSV.Text;
            sv.TrangThai = true;
            if (dem == 1)
            {
             
                    addface = true;
                    MessageBox.Show("Bạn Hãy Thêm Vào 5 Khuôn Mặt");
                    MessageBox.Show("Thêm Khuông Mặt Thứ: " + dem + " Thành Công");
                    sv.TrangThai = true;
                     SinhVienBUS.KiemTraTrangThai(sv);
                    LoadDSSV();
                    txtHoten.Enabled = false;
                    txtLop.Enabled = false;
                    txtMSSV.Enabled = false;
                    dem++;
               
                
            }
            else if (dem > 1 && dem <= 5)
            {
                addface = true;
                MessageBox.Show("Thêm Khuông Mặt Thứ: " + dem + " Thành Công");
                dem++;
                if (dem == 6)
                {
                    MessageBox.Show("Thêm sinh viên thành công");
                    txtHoten.Enabled = true;
                    txtLop.Enabled = true;
                    txtMSSV.Enabled = true;
                    txtHoten.Text = "";
                    txtLop.Text = "";
                    txtMSSV.Text = "";
                    dem = 1;
                }

            }
            else
            {
                MessageBox.Show("Thêm sinh viên không thành công");

            }
        }
    }
}
