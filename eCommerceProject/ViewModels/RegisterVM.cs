using System.ComponentModel.DataAnnotations;

namespace eCommerceProject.ViewModels
{
    public class RegisterVM
    {
        [Display(Name ="Tên đăng nhập")]
        [Required(ErrorMessage ="*")]
        [MaxLength(20,ErrorMessage ="Chỉ cho phép tối đa 20 kí tự")]
        public string MaKh { get; set; }

        [Display(Name ="Mật khẩu")]
        [Required(ErrorMessage = "*")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }
        

        [Display(Name = "Họ tên")]
        [Required(ErrorMessage = "*")]
        [MaxLength(50, ErrorMessage = "Chỉ cho phép tối đa 50 kí tự")]
        public string HoTen { get; set; }

        public bool GioiTinh { get; set; } = true;

        [Display(Name ="Ngày Sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Display(Name = "Địa Chỉ")]
        [MaxLength(60, ErrorMessage = "Chỉ cho phép tối đa 60 kí tự")]
        public string DiaChi { get; set; }

        [Display(Name = "Điện Thoại ")]
        [MaxLength(24, ErrorMessage = "Chỉ cho phép tối đa 24 kí tự")]
        [RegularExpression(@"0[9875]\d{8}",ErrorMessage ="Chưa đúng định dạng")]
        public string DienThoai { get; set; }


        [EmailAddress(ErrorMessage ="Chưa đúng định dạng Email")]
        public string Email { get; set; } 

        public string? Hinh { get; set; }
    }
}
