using System;

namespace ClinicManagement.UI.Models
{
    public class BenhNhan
    {
        public string MaBenhNhan { get; set; }
        public string HoTen { get; set; }
        public string GioiTinh { get; set; }
        public int NamSinh { get; set; }
        public string DiaChi { get; set; }

        public bool KiemTraThongTinHopLe()
        {
            if (string.IsNullOrWhiteSpace(HoTen) || string.IsNullOrWhiteSpace(GioiTinh) || string.IsNullOrWhiteSpace(DiaChi))
            {
                return false;
            }

            if (NamSinh > DateTime.Now.Year || NamSinh < 1900)
            {
                return false;
            }

            return true;
        }
    }
}