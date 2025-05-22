using System;
using System.Collections.Generic;
using APIClothesEcommerceShop.Models;
using Microsoft.EntityFrameworkCore;

namespace APIClothesEcommerceShop.Data;

public partial class EcommerceShopContext : DbContext
{
    public EcommerceShopContext()
    {
    }

    public EcommerceShopContext(DbContextOptions<EcommerceShopContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Chitietcombohoadon> Chitietcombohoadons { get; set; }

    public virtual DbSet<Chitietdanhmuc> Chitietdanhmucs { get; set; }

    public virtual DbSet<Chitietsanpham> Chitietsanphams { get; set; }

    public virtual DbSet<Chucvu> Chucvus { get; set; }

    public virtual DbSet<Combo> Combos { get; set; }

    public virtual DbSet<Cthoadon> Cthoadons { get; set; }

    public virtual DbSet<Danhmuccha> Danhmucchas { get; set; }

    public virtual DbSet<Danhmuccon> Danhmuccons { get; set; }

    public virtual DbSet<Giohang> Giohangs { get; set; }

    public virtual DbSet<Giohangctcombo> Giohangctcombos { get; set; }

    public virtual DbSet<Hinhanh> Hinhanhs { get; set; }

    public virtual DbSet<Hoadon> Hoadons { get; set; }

    public virtual DbSet<Khachhang> Khachhangs { get; set; }

    public virtual DbSet<Macoupon> Macoupons { get; set; }

    public virtual DbSet<Nhanvien> Nhanviens { get; set; }

    public virtual DbSet<Refreshtoken> Refreshtokens { get; set; }

    public virtual DbSet<Sanpham> Sanphams { get; set; }

    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chitietcombohoadon>(entity =>
        {
            entity.HasKey(e => new { e.MaHd, e.MaCtsp, e.MaCombo }).HasName("PK__CHITIETC__9F02B7B6FFDE6FBA");

            entity.ToTable("CHITIETCOMBOHOADON");

            entity.Property(e => e.MaHd).HasColumnName("MaHD");
            entity.Property(e => e.MaCtsp).HasColumnName("MaCTSp");

            entity.HasOne(d => d.MaComboNavigation).WithMany(p => p.Chitietcombohoadons)
                .HasForeignKey(d => d.MaCombo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHITIETCO__MaCom__6754599E");

            entity.HasOne(d => d.MaCtspNavigation).WithMany(p => p.Chitietcombohoadons)
                .HasForeignKey(d => d.MaCtsp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHITIETCO__MaCTS__66603565");

            entity.HasOne(d => d.MaHdNavigation).WithMany(p => p.Chitietcombohoadons)
                .HasForeignKey(d => d.MaHd)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHITIETCOM__MaHD__656C112C");
        });

        modelBuilder.Entity<Chitietdanhmuc>(entity =>
        {
            entity.HasKey(e => new { e.MaDanhMucCha, e.MaDanhMucCon, e.MaSp }).HasName("PK__CHITIETD__9C4F44F4B8B620B8");

            entity.ToTable("CHITIETDANHMUC");

            entity.Property(e => e.MaSp).HasColumnName("MaSP");

            entity.HasOne(d => d.MaDanhMucChaNavigation).WithMany(p => p.Chitietdanhmucs)
                .HasForeignKey(d => d.MaDanhMucCha)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHITIETDA__MaDan__52593CB8");

            entity.HasOne(d => d.MaDanhMucConNavigation).WithMany(p => p.Chitietdanhmucs)
                .HasForeignKey(d => d.MaDanhMucCon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHITIETDA__MaDan__534D60F1");

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.Chitietdanhmucs)
                .HasForeignKey(d => d.MaSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHITIETDAN__MaSP__5441852A");
        });

        modelBuilder.Entity<Chitietsanpham>(entity =>
        {
            entity.HasKey(e => e.MaCtsp).HasName("PK__CHITIETS__1E4FCECD5A7581C1");

            entity.ToTable("CHITIETSANPHAM");

            entity.Property(e => e.MaCtsp).HasColumnName("MaCTSP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.KichThuoc).HasMaxLength(30);
            entity.Property(e => e.MaSp).HasColumnName("MaSP");
            entity.Property(e => e.MauSac).HasMaxLength(30);

            entity.HasOne(d => d.MaSpNavigation).WithMany(p => p.Chitietsanphams)
                .HasForeignKey(d => d.MaSp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHITIETSAN__MaSP__4F7CD00D");
        });

        modelBuilder.Entity<Chucvu>(entity =>
        {
            entity.HasKey(e => e.MaChucVu).HasName("PK__CHUCVU__D4639533D95BEECA");

            entity.ToTable("CHUCVU");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Luong).HasColumnType("decimal(11, 2)");
            entity.Property(e => e.TenChucVu).HasMaxLength(40);
        });

        modelBuilder.Entity<Combo>(entity =>
        {
            entity.HasKey(e => e.MaCombo).HasName("PK__COMBO__C3EDBC788ACDE30D");

            entity.ToTable("COMBO");

            entity.Property(e => e.Hinh).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MoTa).HasColumnType("nvarchar(max)");
            entity.Property(e => e.TenCombo).HasMaxLength(100);
        });

        modelBuilder.Entity<Cthoadon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CTHOADON__3214EC272F449754");

            entity.ToTable("CTHOADON");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MaCtsp).HasColumnName("MaCTSP");
            entity.Property(e => e.MaHd).HasColumnName("MaHD");

            entity.HasOne(d => d.MaComboNavigation).WithMany(p => p.Cthoadons)
                .HasForeignKey(d => d.MaCombo)
                .HasConstraintName("FK__CTHOADON__MaComb__6C190EBB");

            entity.HasOne(d => d.MaCtspNavigation).WithMany(p => p.Cthoadons)
                .HasForeignKey(d => d.MaCtsp)
                .HasConstraintName("FK__CTHOADON__MaCTSP__6B24EA82");

            entity.HasOne(d => d.MaHdNavigation).WithMany(p => p.Cthoadons)
                .HasForeignKey(d => d.MaHd)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CTHOADON__MaHD__6A30C649");
        });

        modelBuilder.Entity<Danhmuccha>(entity =>
        {
            entity.HasKey(e => e.MaDanhMucCha).HasName("PK__DANHMUCC__6719FD9E5876B596");

            entity.ToTable("DANHMUCCHA");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TenDanhMucCha).HasMaxLength(40);
        });

        modelBuilder.Entity<Danhmuccon>(entity =>
        {
            entity.HasKey(e => e.MaDanhMucCon).HasName("PK__DANHMUCC__6719C62F0C90C224");

            entity.ToTable("DANHMUCCON");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TenDanhMucCon).HasMaxLength(40);
        });

        modelBuilder.Entity<Giohang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GIOHANG__3214EC27D6272CAB");

            entity.ToTable("GIOHANG");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.MaCtsp).HasColumnName("MaCTSP");
            entity.Property(e => e.MaKh).HasColumnName("MaKH");

            entity.HasOne(d => d.MaComboNavigation).WithMany(p => p.Giohangs)
                .HasForeignKey(d => d.MaCombo)
                .HasConstraintName("FK__GIOHANG__MaCombo__72C60C4A");

            entity.HasOne(d => d.MaCtspNavigation).WithMany(p => p.Giohangs)
                .HasForeignKey(d => d.MaCtsp)
                .HasConstraintName("FK__GIOHANG__MaCTSP__71D1E811");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.Giohangs)
                .HasForeignKey(d => d.MaKh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GIOHANG__MaKH__70DDC3D8");
        });

        modelBuilder.Entity<Giohangctcombo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GIOHANGC__3214EC276D2330CA");

            entity.ToTable("GIOHANGCTCOMBO");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.MaCtsp).HasColumnName("MaCTSP");

            entity.HasOne(d => d.MaCtspNavigation).WithMany(p => p.Giohangctcombos)
                .HasForeignKey(d => d.MaCtsp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GIOHANGCT__MaCTS__75A278F5");

            entity.HasOne(d => d.MaGioHangNavigation).WithMany(p => p.Giohangctcombos)
                .HasForeignKey(d => d.MaGioHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__GIOHANGCT__MaGio__76969D2E");
        });

        modelBuilder.Entity<Hinhanh>(entity =>
        {
            entity.HasKey(e => e.MaHinhAnh).HasName("PK__HINHANH__A9C37A9B52310EEB");

            entity.ToTable("HINHANH");

            entity.Property(e => e.MaCtsp).HasColumnName("MaCTSP");
            entity.Property(e => e.TenHinhAnh).HasColumnType("text");

            entity.HasOne(d => d.MaCtspNavigation).WithMany(p => p.Hinhanhs)
                .HasForeignKey(d => d.MaCtsp)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HINHANH__MaCTSP__571DF1D5");
        });

        modelBuilder.Entity<Hoadon>(entity =>
        {
            entity.HasKey(e => e.MaHd).HasName("PK__HOADON__2725A6E074C6EE92");

            entity.ToTable("HOADON");

            entity.Property(e => e.MaHd).HasColumnName("MaHD");
            entity.Property(e => e.BatDauGiao).HasColumnType("datetime");
            entity.Property(e => e.DiaChiNhanHang).HasMaxLength(200);
            entity.Property(e => e.HinhThucTt)
                .HasMaxLength(50)
                .HasColumnName("HinhThucTT");
            entity.Property(e => e.HoTen).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LyDoHuy).HasMaxLength(500);
            entity.Property(e => e.MaCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MaKh).HasColumnName("MaKH");
            entity.Property(e => e.MaNv).HasColumnName("MaNV");
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.NgayNhan).HasColumnType("datetime");
            entity.Property(e => e.NgayTao).HasColumnType("datetime");
            entity.Property(e => e.NgayThanhToan).HasColumnType("datetime");
            entity.Property(e => e.PhiVanChuyen).HasColumnType("decimal(11, 2)");
            entity.Property(e => e.Sdt)
                .HasMaxLength(11)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SDT");
            entity.Property(e => e.TienGoc).HasColumnType("decimal(11, 2)");
            entity.Property(e => e.TinhTrang).HasMaxLength(50);

            entity.HasOne(d => d.MaCodeNavigation).WithMany(p => p.Hoadons)
                .HasForeignKey(d => d.MaCode)
                .HasConstraintName("FK__HOADON__MaCode__628FA481");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.Hoadons)
                .HasForeignKey(d => d.MaKh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HOADON__MaKH__60A75C0F");

            entity.HasOne(d => d.MaNvNavigation).WithMany(p => p.Hoadons)
                .HasForeignKey(d => d.MaNv)
                .HasConstraintName("FK__HOADON__MaNV__619B8048");
        });

        modelBuilder.Entity<Khachhang>(entity =>
        {
            entity.HasKey(e => e.MaKh).HasName("PK__KHACHHAN__2725CF1E44D7EB2D");

            entity.ToTable("KHACHHANG");

            entity.Property(e => e.MaKh).HasColumnName("MaKH");
            entity.Property(e => e.Cccd)
                .HasMaxLength(12)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("CCCD");
            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.GioiTinh).HasMaxLength(5);
            entity.Property(e => e.HinhDaiDien).HasColumnType("text");
            entity.Property(e => e.HoTen).HasMaxLength(40);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.NgayTao).HasColumnType("datetime");
            entity.Property(e => e.Sdt)
                .HasMaxLength(11)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SDT");
            entity.Property(e => e.TenTaiKhoan)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TinhTrang)
                .HasMaxLength(25)
                .HasDefaultValue("Đang hoạt động");
        });

        modelBuilder.Entity<Macoupon>(entity =>
        {
            entity.HasKey(e => e.MaCode).HasName("PK__MACOUPON__152C7C5D253AC471");

            entity.ToTable("MACOUPON");

            entity.Property(e => e.MaCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MoTa)
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.NgayBatDau).HasColumnType("datetime");
            entity.Property(e => e.NgayKetThuc).HasColumnType("datetime");
            entity.Property(e => e.PhanTramGiam).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<Nhanvien>(entity =>
        {
            entity.HasKey(e => e.MaNv).HasName("PK__NHANVIEN__2725D70A03818D9F");

            entity.ToTable("NHANVIEN");

            entity.Property(e => e.MaNv).HasColumnName("MaNV");
            entity.Property(e => e.Cccd)
                .HasMaxLength(12)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("CCCD");
            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.Email)
                .HasMaxLength(40)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.GioiTinh).HasMaxLength(5);
            entity.Property(e => e.HoTen).HasMaxLength(40);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Sdt)
                .HasMaxLength(11)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SDT");
            entity.Property(e => e.TenTaiKhoan)
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.TinhTrang)
                .HasMaxLength(25)
                .HasDefaultValue("Đang hoạt động");

            entity.HasOne(d => d.MaChucVuNavigation).WithMany(p => p.Nhanviens)
                .HasForeignKey(d => d.MaChucVu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__NHANVIEN__MaChuc__4222D4EF");
        });

        modelBuilder.Entity<Refreshtoken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__REFRESHT__3214EC27840D2D6F");

            entity.ToTable("REFRESHTOKEN");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.ExpiredAt).HasColumnType("datetime");
            entity.Property(e => e.IssuedAt).HasColumnType("datetime");
            entity.Property(e => e.Token).HasMaxLength(500);
        });

        modelBuilder.Entity<Sanpham>(entity =>
        {
            entity.HasKey(e => e.MaSp).HasName("PK__SANPHAM__2725081C19DDE914");

            entity.ToTable("SANPHAM");

            entity.Property(e => e.MaSp).HasColumnName("MaSP");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MoTa).HasColumnType("nvarchar(max)");
            entity.Property(e => e.TenSanPham).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
