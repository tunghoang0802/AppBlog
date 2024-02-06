using System.Net;
using System.Text.RegularExpressions;

namespace AppBlog.Helpers
{
    public static class Utilities
    {
        public static int Page_Size = 20;

        public static async Task<long> GetFileSizeAsync(string url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            if (response.Content.Headers.ContentLength.HasValue)
            {
                return response.Content.Headers.ContentLength.Value;
            }
            else
            {
                return -1;
                throw new Exception("Content-Length header not present in the response.");
            }
        }
        public static async Task<string> UploadFile(IFormFile file, string sDirectory, string? newname = null)
        {
            try
            {
                if (newname == null) newname = file.FileName;
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", sDirectory, newname);
                string path2 = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", sDirectory);
                if (!System.IO.Directory.Exists(path2))
                {
                    System.IO.Directory.CreateDirectory(path2);
                }

                var supportedTypes = new[] { "jpg", "jpeg", "png", "gif" };
                var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);

                if (!supportedTypes.Contains(fileExt.ToLower())) // Khác với các tệp đã định nghĩa
                    return "";

                else
                {
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    return newname;
                }
            }
            catch
            {
                return "";
            }

        }

        public static string SEOUrl(string? url)
        {
            if (url == null) return "";
            // Chuyển đổi url thành chữ thường
            url = url.ToLower();

            // Thay thế các ký tự tiếng Việt bằng ký tự không dấu tương ứng
            url = Regex.Replace(url, @"[áàảãạăắằẳẵặâấầẩẫậ]", "a");
            url = Regex.Replace(url, @"[éèẻẽẹêếềểễệ]", "e");
            url = Regex.Replace(url, @"[óòỏõọôốồổỗộơớờởỡợ]", "o");
            url = Regex.Replace(url, @"[íìỉĩị]", "i");
            url = Regex.Replace(url, @"[ýỳỷỹỵ]", "y");
            url = Regex.Replace(url, @"[úùủũụưứừửữự]", "u");
            url = Regex.Replace(url, @"[đ]", "d");

            // Loại bỏ các ký tự không phải là chữ và số
            url = Regex.Replace(url.Trim(), @"[^0-9a-z\s]", "").Trim();

            // Thay thế các khoảng trắng bằng dấu gạch ngang
            url = Regex.Replace(url.Trim(), @"\s+", "-");
            url = Regex.Replace(url, @"\s", "-");

            // Loại bỏ các dấu gạch ngang kép
            while (true)
            {
                if (url.IndexOf("--") != -1)
                {
                    url = url.Replace("--", "-");
                }
                else
                {
                    break;
                }
            }

            // Trả về url đã được tối ưu
            return url;
        }

    }
}

