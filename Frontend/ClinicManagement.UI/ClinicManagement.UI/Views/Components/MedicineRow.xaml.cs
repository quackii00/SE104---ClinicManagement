using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClinicManagement.UI.Views.Components
{
    /// <summary>
    /// Interaction logic for MedicineRow.xaml
    /// </summary>
    public partial class MedicineRow : UserControl
    {
        public MedicineRow()
        {
            InitializeComponent();
        }

        // Ô Số lượng chỉ nhận chữ số (QĐ2: số lượng là số nguyên dương) -> tránh lỗi binding khi gõ chữ.
        private static readonly Regex _onlyDigits = new Regex("^[0-9]+$");

        private void SoLuong_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !_onlyDigits.IsMatch(e.Text);
        }

        private void SoLuong_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (!_onlyDigits.IsMatch(text)) e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
