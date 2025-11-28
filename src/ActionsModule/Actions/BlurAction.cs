using ActionsModule.Attributes;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionsModule.Actions
{
    public enum BlurTypes
    {
        Normal,
        Gaussian,
        Median,
        Bilateral,
        Filter2D   // ★ 추가됨
    }
    public enum KernelMode
    {
        Sharpen,        // 샤프닝
        Edge,           // 엣지 검출
        Emboss,         // 엠보싱
        Custom          // 커스텀 (확장용)
    }

    [Category("Preprocessing")]
    public class BlurAction : ImageAction
    {
        int size = 3;
        [Slider(2,40)]
        public int Size
        {
            get { return size; }
            set { SetProperty(ref size, value); }
        }

        BorderTypes bTypes = BorderTypes.Default;

        [ImportExport]
        [Enum(typeof(BorderTypes))]
        public BorderTypes BorderType
        {
            get { return bTypes; }
            set { SetProperty(ref bTypes, value); }
        }
        BlurTypes blurTypes = BlurTypes.Normal;

        [ImportExport]
        [Enum(typeof(BlurTypes))]
        public BlurTypes BlurTypes
        {
            get { return blurTypes; }
            set { SetProperty(ref blurTypes, value); }
        }
        // ★ Filter2D 전용 옵션들 추가
        int filter2dKernelSize = 3;
        [ImportExport]
        [Slider(3, 15, 2)]
        public int Filter2DKernelSize
        {
            get { return filter2dKernelSize; }
            set { SetProperty(ref filter2dKernelSize, value); }
        }
        KernelMode kernelMode = KernelMode.Sharpen;
        [ImportExport]
        [Enum(typeof(KernelMode))]
        public KernelMode KernelMode
        {
            get { return kernelMode; }
            set { SetProperty(ref kernelMode, value); }
        }
        public BlurAction()
        {
            this.Name = "Blur";
            this.Action = (m) =>
            {
                var blur = new Mat();
                if (BlurTypes == BlurTypes.Normal)
                {
                    blur = m.Blur(new Size(size, size), null, BorderType);
                }
                else if (BlurTypes == BlurTypes.Gaussian)
                {
                    blur = m.GaussianBlur(new Size(size, size), 0, 0, BorderType);
                }
                else if (BlurTypes == BlurTypes.Median)
                {
                    blur = m.MedianBlur(size);
                }
                else if (BlurTypes == BlurTypes.Bilateral)
                {
                    blur = m.BilateralFilter(size, size * 2, size / 2, BorderType);
                }
                else if (BlurTypes == BlurTypes.Filter2D)
                {
                    Mat kernel = CreateKernel(KernelMode, Filter2DKernelSize);

                    blur = new Mat();
                    Cv2.Filter2D(src: m, dst: blur, ddepth: -1, kernel: kernel);

                    kernel.Dispose();
                }
                else
                    blur = m.Blur(new Size(size, size), null, BorderType);
                m.Dispose();
                return blur;
            };
        }
        private Mat CreateKernel(KernelMode mode, int ksize)
        {
            if (ksize % 2 == 0)
                ksize += 1;  // 홀수 강제

            Mat kernel = new Mat(ksize, ksize, MatType.CV_32F);
            kernel.SetTo(0);
            const int k = 3;

            switch (mode)
            {
                case KernelMode.Sharpen:
                    // 샤프닝 커널
                    kernel = new Mat(k, k, MatType.CV_32F, new float[]
                    {
                 0, -1,  0,
                -1,  5, -1,
                 0, -1,  0
                    });
                    break;

                case KernelMode.Edge:
                    // 엣지 검출 커널
                    kernel = new Mat(k, k, MatType.CV_32F, new float[]
                    {
                -1, -1, -1,
                -1,  8, -1,
                -1, -1, -1
                    });
                    break;

                case KernelMode.Emboss:
                    // 엠보싱 커널
                    kernel = new Mat(k, k, MatType.CV_32F, new float[]
                    {
                -2, -1, 0,
                -1,  1, 1,
                 0,  1, 2
                    });
                    break;

                case KernelMode.Custom:
                default:
                    // 임시: 모두 1로 채운 평균 필터처럼 사용 (원하면 나중에 직접 값 넣는 UI 만들 수 있음)
                    kernel = new Mat(ksize, ksize, MatType.CV_32F, 1.0f);
                    break;
            }

            return kernel;
        }
    }
}
