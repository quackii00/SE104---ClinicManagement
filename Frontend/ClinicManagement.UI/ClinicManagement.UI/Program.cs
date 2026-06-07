using System;
using System.Windows;
using System.Collections.Generic;
using ClinicManagement.UI.Models;

namespace ClinicManagement.UI
{
    public class Program
    {
        public static void RunFrontendTests()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("======= KHỞI CHẠY KIỂM THỬ LUỒNG ĐỐI TƯỢNG OOP TỪ BACKEND =======");

            var phieuKhamCuaHuyen = new PhieuKhamBenh
            {
                MaPhieuKham = "PK-2026-001",
                NgayKham = DateTime.Today,
                TrieuChung = "Đau họng, ho khan, sốt nhẹ",
                TenLoaiBenh = "Viêm họng cấp",

                BenhNhanKham = new BenhNhan
                {
                    MaBenhNhan = "BN23521698",
                    HoTen = "Nguyễn Thị Ngọc Huyền",
                    GioiTinh = "Nữ",
                    NamSinh = 2006,
                    DiaChi = "KTX Khu A - ĐHQG TP.HCM"
                },

                ChiTietToaThuoc = new List<ChiTietToaThuoc>
                {
                    new ChiTietToaThuoc
                    {
                        SoLuong = 15,
                        DonGia = 3000,
                        Thuoc = new Thuoc { MaThuoc = "T05", TenThuoc = "Amoxicillin 500mg" },
                        CachDung = new CachDung { MaCachDung = "CD01", MoTaCachDung = "Uống ngày 2 lần sau ăn" }
                    }
                }
            };

            var hoaDon = new HoaDonThanhToan
            {
                MaHoaDon = "HD-2026-777",
                PhieuKhamBenh = phieuKhamCuaHuyen
            };

            hoaDon.TinhTienKham(30000);
            hoaDon.TinhTienThuoc();
            hoaDon.TinhTongTien();
            hoaDon.XuatHoaDon();

            Console.WriteLine("\n[KẾT QUẢ LIÊN THÔNG ĐỐI TƯỢNG]:");
            Console.WriteLine("  - Mã số sinh viên / Bệnh nhân: " + hoaDon.PhieuKhamBenh.BenhNhanKham.MaBenhNhan);
            Console.WriteLine("  - Tên bệnh nhân: " + hoaDon.PhieuKhamBenh.BenhNhanKham.HoTen);
            Console.WriteLine("  - Thuốc chỉ định: " + hoaDon.PhieuKhamBenh.ChiTietToaThuoc[0].Thuoc.TenThuoc);
            Console.WriteLine("  - Hướng dẫn dùng: " + hoaDon.PhieuKhamBenh.ChiTietToaThuoc[0].CachDung.MoTaCachDung);
            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("  + Tiền khám: " + hoaDon.TienKham.ToString("N0") + " VND");
            Console.WriteLine("  + Tiền thuốc: " + hoaDon.TienThuoc.ToString("N0") + " VND (15 viên x 3,000đ)");
            Console.WriteLine("  => TỔNG TIỀN: " + hoaDon.TongTien.ToString("N0") + " VND");
            Console.WriteLine("  + Trạng thái: " + (hoaDon.TrangThaiThanhToan ? "Đã thanh toán" : "Chưa thanh toán"));

            try
            {
                MessageBox.Show("Frontend tests finished. Close this dialog to continue.", "Tests finished", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                try { Console.ReadLine(); } catch { }
            }
        }
    }
}