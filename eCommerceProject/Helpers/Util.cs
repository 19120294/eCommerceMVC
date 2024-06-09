using System.Text;

namespace eCommerceProject.Helpers
{
    public class Util
    {

        public static string UploadHinh(IFormFile Hinh,string folder)
        {
            try
            {
                var pathFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", folder, Hinh.FileName);
                using (var myfile = new FileStream(pathFile, FileMode.CreateNew))
                {
                    Hinh.CopyTo(myfile);
                }
                return Hinh.FileName;
            }catch (Exception ex)
            {
                return string.Empty;
            }
           
        }
        public static string GenerateRandomKey(int length=5)
        {
            var pattern = @"qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM!@#$%^&*";
            var sb = new StringBuilder();
            var rd = new Random();
            for(int i = 0; i < length; i++)
            {
                sb.Append(pattern[rd.Next(0,pattern.Length)]);
            }
            return sb.ToString();
        }
    }
}
