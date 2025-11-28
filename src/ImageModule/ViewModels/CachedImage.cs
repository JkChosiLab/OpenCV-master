using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace ImageModule.ViewModels
{
    public class CachedImage : IDisposable
    {
        public static double PreviewSize = 80;

        private readonly Mat image;

        private CachedImage(Mat image, BitmapSource preview)
        {
            this.image = image;
            this.Preview = preview;
        }

        public static CachedImage[] FromFiles(string[] files)
        {
            //return files
            //        .Select(FromFile)
            //        .ToArray();

            if (files == null || files.Length == 0)
                return Array.Empty<CachedImage>();

            return files
                .Select(FromFile)
                .Where(x => x != null)      // null 제거
                .ToArray();
        }

        public static CachedImage FromFile(string file)
        {
            // 파일 유효성 체크
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
                return null;
            Mat image = null;
            try
            {
                // 이미지 읽기
                image = Cv2.ImRead(file, ImreadModes.Unchanged);

                // 로드 실패 or 빈 이미지 처리
                if (image == null || image.Empty() || image.Width <= 0 || image.Height <= 0)
                {
                    image?.Dispose();
                    return null;
                }

                // Resize factor 계산
                double resizeFactor = 1.0;
                if (image.Width > PreviewSize || image.Height > PreviewSize)
                {
                    resizeFactor = (image.Width > image.Height)
                        ? (PreviewSize / image.Width)
                        : (PreviewSize / image.Height);
                }

                int previewW = Math.Max((int)Math.Round(image.Width * resizeFactor), 1);
                int previewH = Math.Max((int)Math.Round(image.Height * resizeFactor), 1);

                BitmapSource preview;

                // Preview 생성
                if (resizeFactor != 1.0)
                {
                    using (var resized = image.Resize(new Size(previewW, previewH)))
                    {
                        if (resized.Empty())
                        {
                            image.Dispose();
                            return null;
                        }
                        preview = resized.ToBitmapSource();
                    }
                }
                else
                {
                    preview = image.ToBitmapSource();
                }

                // Preview 생성 OK → CachedImage 반환
                return new CachedImage(image, preview);
            }
            catch
            {
                image?.Dispose();
                return null;
            }
        }

        public BitmapSource Preview { get; }

        public void Dispose()
        {
            this.image?.Dispose();
        }

        public Mat GetCopy()
        {
           // return this.image.Clone();

            if (image == null || image.Empty() || image.Width <= 0 || image.Height <= 0)
                return null;

            try
            {
                var dst = new Mat();
                image.CopyTo(dst);
                return dst;
            }
            catch (OpenCvSharp.OpenCVException ex)
            {
                // 여기서 로그 남기고 null 반환
                System.Diagnostics.Debug.WriteLine($"GetCopy Clone 실패: {ex.Message}");
                return null;
            }
        }
    }
}
