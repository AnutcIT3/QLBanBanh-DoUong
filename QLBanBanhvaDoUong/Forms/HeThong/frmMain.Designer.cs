namespace QLBanBanhvaDoUong.Fomrs.HeThong
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.banHangToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sanPhamToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.banhNgotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.banhManToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.traToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cafeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.khachHangToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hoaDonToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nhanVienToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.thongKeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblXinChao = new System.Windows.Forms.Label();
            this.btnDangXuat = new System.Windows.Forms.Button();
            this.grbSanPham = new System.Windows.Forms.GroupBox();
            this.picTop2 = new System.Windows.Forms.PictureBox();
            this.picTop1 = new System.Windows.Forms.PictureBox();
            this.dgvSanPhamNoiBat = new System.Windows.Forms.DataGridView();
            this.lblDoanhThuNgay = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.grbSanPham.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTop2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picTop1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSanPhamNoiBat)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.banHangToolStripMenuItem,
            this.sanPhamToolStripMenuItem,
            this.khachHangToolStripMenuItem,
            this.hoaDonToolStripMenuItem,
            this.nhanVienToolStripMenuItem,
            this.thongKeToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // banHangToolStripMenuItem
            // 
            this.banHangToolStripMenuItem.Name = "banHangToolStripMenuItem";
            this.banHangToolStripMenuItem.Size = new System.Drawing.Size(85, 24);
            this.banHangToolStripMenuItem.Text = "Bán hàng";
            this.banHangToolStripMenuItem.Click += new System.EventHandler(this.banHangToolStripMenuItem_Click);
            // 
            // sanPhamToolStripMenuItem
            // 
            this.sanPhamToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.banhNgotToolStripMenuItem,
            this.banhManToolStripMenuItem,
            this.traToolStripMenuItem,
            this.cafeToolStripMenuItem,
            this.AllToolStripMenuItem});
            this.sanPhamToolStripMenuItem.Name = "sanPhamToolStripMenuItem";
            this.sanPhamToolStripMenuItem.Size = new System.Drawing.Size(89, 24);
            this.sanPhamToolStripMenuItem.Text = "Sản phẩm";
            this.sanPhamToolStripMenuItem.DoubleClick += new System.EventHandler(this.sanPhamToolStripMenuItem_Click);
            // 
            // banhNgotToolStripMenuItem
            // 
            this.banhNgotToolStripMenuItem.Name = "banhNgotToolStripMenuItem";
            this.banhNgotToolStripMenuItem.Size = new System.Drawing.Size(200, 26);
            this.banhNgotToolStripMenuItem.Text = "Bánh ngọt";
            this.banhNgotToolStripMenuItem.Click += new System.EventHandler(this.banhNgotToolStripMenuItem_Click);
            // 
            // banhManToolStripMenuItem
            // 
            this.banhManToolStripMenuItem.Name = "banhManToolStripMenuItem";
            this.banhManToolStripMenuItem.Size = new System.Drawing.Size(200, 26);
            this.banhManToolStripMenuItem.Text = "Bánh mặn";
            this.banhManToolStripMenuItem.Click += new System.EventHandler(this.banhManToolStripMenuItem_Click);
            // 
            // traToolStripMenuItem
            // 
            this.traToolStripMenuItem.Name = "traToolStripMenuItem";
            this.traToolStripMenuItem.Size = new System.Drawing.Size(200, 26);
            this.traToolStripMenuItem.Text = "Trà";
            this.traToolStripMenuItem.Click += new System.EventHandler(this.traToolStripMenuItem_Click);
            // 
            // cafeToolStripMenuItem
            // 
            this.cafeToolStripMenuItem.Name = "cafeToolStripMenuItem";
            this.cafeToolStripMenuItem.Size = new System.Drawing.Size(200, 26);
            this.cafeToolStripMenuItem.Text = "Cafe";
            this.cafeToolStripMenuItem.Click += new System.EventHandler(this.cafeToolStripMenuItem_Click);
            // 
            // AllToolStripMenuItem
            // 
            this.AllToolStripMenuItem.Name = "AllToolStripMenuItem";
            this.AllToolStripMenuItem.Size = new System.Drawing.Size(200, 26);
            this.AllToolStripMenuItem.Text = "Tất cả sản phẩm";
            this.AllToolStripMenuItem.Click += new System.EventHandler(this.AllToolStripMenuItem_Click);
            // 
            // khachHangToolStripMenuItem
            // 
            this.khachHangToolStripMenuItem.Name = "khachHangToolStripMenuItem";
            this.khachHangToolStripMenuItem.Size = new System.Drawing.Size(100, 24);
            this.khachHangToolStripMenuItem.Text = "Khách hàng";
            this.khachHangToolStripMenuItem.Click += new System.EventHandler(this.khachHangToolStripMenuItem_Click);
            // 
            // hoaDonToolStripMenuItem
            // 
            this.hoaDonToolStripMenuItem.Name = "hoaDonToolStripMenuItem";
            this.hoaDonToolStripMenuItem.Size = new System.Drawing.Size(81, 24);
            this.hoaDonToolStripMenuItem.Text = "Hóa đơn";
            this.hoaDonToolStripMenuItem.Click += new System.EventHandler(this.hoaDonToolStripMenuItem_Click);
            // 
            // nhanVienToolStripMenuItem
            // 
            this.nhanVienToolStripMenuItem.Name = "nhanVienToolStripMenuItem";
            this.nhanVienToolStripMenuItem.Size = new System.Drawing.Size(89, 24);
            this.nhanVienToolStripMenuItem.Text = "Nhân viên";
            this.nhanVienToolStripMenuItem.Click += new System.EventHandler(this.nhanVienToolStripMenuItem_Click);
            // 
            // thongKeToolStripMenuItem
            // 
            this.thongKeToolStripMenuItem.Name = "thongKeToolStripMenuItem";
            this.thongKeToolStripMenuItem.Size = new System.Drawing.Size(84, 24);
            this.thongKeToolStripMenuItem.Text = "Thống kê";
            this.thongKeToolStripMenuItem.Click += new System.EventHandler(this.thongKeToolStripMenuItem_Click);
            // 
            // lblXinChao
            // 
            this.lblXinChao.AutoSize = true;
            this.lblXinChao.Location = new System.Drawing.Point(12, 41);
            this.lblXinChao.Name = "lblXinChao";
            this.lblXinChao.Size = new System.Drawing.Size(58, 16);
            this.lblXinChao.TabIndex = 1;
            this.lblXinChao.Text = "Xin chào";
            // 
            // btnDangXuat
            // 
            this.btnDangXuat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDangXuat.AutoSize = true;
            this.btnDangXuat.Location = new System.Drawing.Point(682, 396);
            this.btnDangXuat.Name = "btnDangXuat";
            this.btnDangXuat.Size = new System.Drawing.Size(106, 42);
            this.btnDangXuat.TabIndex = 2;
            this.btnDangXuat.Text = "Đăng xuất";
            this.btnDangXuat.UseVisualStyleBackColor = true;
            this.btnDangXuat.Click += new System.EventHandler(this.btnDangXuat_Click);
            // 
            // grbSanPham
            // 
            this.grbSanPham.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grbSanPham.AutoSize = true;
            this.grbSanPham.Controls.Add(this.picTop2);
            this.grbSanPham.Controls.Add(this.picTop1);
            this.grbSanPham.Controls.Add(this.dgvSanPhamNoiBat);
            this.grbSanPham.Location = new System.Drawing.Point(12, 71);
            this.grbSanPham.Name = "grbSanPham";
            this.grbSanPham.Size = new System.Drawing.Size(782, 322);
            this.grbSanPham.TabIndex = 3;
            this.grbSanPham.TabStop = false;
            this.grbSanPham.Text = "Sản phẩm nổi bật";
            // 
            // picTop2
            // 
            this.picTop2.Location = new System.Drawing.Point(443, 180);
            this.picTop2.Name = "picTop2";
            this.picTop2.Size = new System.Drawing.Size(333, 121);
            this.picTop2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picTop2.TabIndex = 3;
            this.picTop2.TabStop = false;
            // 
            // picTop1
            // 
            this.picTop1.Location = new System.Drawing.Point(443, 21);
            this.picTop1.Name = "picTop1";
            this.picTop1.Size = new System.Drawing.Size(333, 130);
            this.picTop1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picTop1.TabIndex = 2;
            this.picTop1.TabStop = false;
            // 
            // dgvSanPhamNoiBat
            // 
            this.dgvSanPhamNoiBat.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSanPhamNoiBat.Location = new System.Drawing.Point(6, 38);
            this.dgvSanPhamNoiBat.Name = "dgvSanPhamNoiBat";
            this.dgvSanPhamNoiBat.RowHeadersWidth = 51;
            this.dgvSanPhamNoiBat.RowTemplate.Height = 24;
            this.dgvSanPhamNoiBat.Size = new System.Drawing.Size(422, 256);
            this.dgvSanPhamNoiBat.TabIndex = 1;
            // 
            // lblDoanhThuNgay
            // 
            this.lblDoanhThuNgay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblDoanhThuNgay.AutoSize = true;
            this.lblDoanhThuNgay.Location = new System.Drawing.Point(562, 41);
            this.lblDoanhThuNgay.Name = "lblDoanhThuNgay";
            this.lblDoanhThuNgay.Size = new System.Drawing.Size(73, 16);
            this.lblDoanhThuNgay.TabIndex = 4;
            this.lblDoanhThuNgay.Text = "Doanh thu: ";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Moccasin;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lblDoanhThuNgay);
            this.Controls.Add(this.grbSanPham);
            this.Controls.Add(this.btnDangXuat);
            this.Controls.Add(this.lblXinChao);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMain";
            this.Text = "Hệ thống quản lý";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.grbSanPham.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picTop2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picTop1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSanPhamNoiBat)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem banHangToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sanPhamToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem khachHangToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hoaDonToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nhanVienToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem thongKeToolStripMenuItem;
        private System.Windows.Forms.Label lblXinChao;
        private System.Windows.Forms.Button btnDangXuat;
        private System.Windows.Forms.GroupBox grbSanPham;
        private System.Windows.Forms.Label lblDoanhThuNgay;
        private System.Windows.Forms.ToolStripMenuItem banhNgotToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem banhManToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem traToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cafeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AllToolStripMenuItem;
        private System.Windows.Forms.DataGridView dgvSanPhamNoiBat;
        private System.Windows.Forms.PictureBox picTop2;
        private System.Windows.Forms.PictureBox picTop1;
    }
}

