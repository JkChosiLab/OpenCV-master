using ActionsModule.Attributes;
using OpenCvSharp;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace ActionsModule.Actions
{
    public enum BitWiseMode 
    {
        And,
        Or,
        Xor,
        Not
    }

    [Category("Preprocessing")]
    public class BitWiseAction : ImageAction
    {
        public BitWiseAction()
        {
            Name = "BitWise";

            Action = (src) =>
            {
                // 비활성화면 그대로 통과
                if (!IsEnabled || src == null || src.Empty())
                    return src;

                HasError = false;
                ErrorMessage = null;

                try
                {
                    Mat dst = new Mat();

                    switch (Mode)
                    {
                        case BitWiseMode.Not:
                            // NOT 은 src 하나만 사용
                            Cv2.BitwiseNot(src, dst);
                            break;

                        case BitWiseMode.And:
                        case BitWiseMode.Or:
                        case BitWiseMode.Xor:
                            {
                                if (Mask == null || Mask.Empty())
                                {
                                    HasError = true;
                                    ErrorMessage = "BitWiseAction: Mask 가 설정되어 있지 않습니다.";
                                    return src;
                                }

                                // src와 동일 크기/타입인지 확인 (간단 체크)
                                if (Mask.Size() != src.Size())
                                {
                                    HasError = true;
                                    ErrorMessage = "BitWiseAction: Mask 크기가 입력 이미지와 다릅니다.";
                                    return src;
                                }

                                Mat maskToUse = Mask;

                                // 1채널(Gray) 마스크면 BGR로 변환해서 사용
                                if (Mask.Channels() == 1 && src.Channels() == 3)
                                {
                                    maskToUse = new Mat();
                                    Cv2.CvtColor(Mask, maskToUse, ColorConversionCodes.GRAY2BGR);
                                }

                                // 필요시 다른 채널 조합도 여기서 맞춰줄 수 있음

                                switch (Mode)
                                {
                                    case BitWiseMode.And:
                                        Cv2.BitwiseAnd(src, maskToUse, dst);
                                        break;
                                    case BitWiseMode.Or:
                                        Cv2.BitwiseOr(src, maskToUse, dst);
                                        break;
                                    case BitWiseMode.Xor:
                                        Cv2.BitwiseXor(src, maskToUse, dst);
                                        break;
                                }

                                if (maskToUse != Mask)
                                    maskToUse.Dispose();
                            }
                            break;
                    }

                    // 원본 src를 더 이상 안 쓸 거면 메모리 해제
                    src.Dispose();

                    return dst;
                }
                catch (Exception ex)
                {
                    HasError = true;
                    ErrorMessage = $"BitWiseAction 오류: {ex.Message}";
                    return src;
                }
            };
        }
    

        // 외부에서 주입할 마스크 (예: 다른 액션에서 만든 도넛 마스크 등)
        // 파이프라인에서 SetProperty로 연결하거나 직접 할당해서 사용
        private Mat mask;

        /// <summary>
        /// Bitwise 연산에 사용할 마스크 이미지 (src와 동일 크기)
        /// 1채널(Gray)이어도 되고, 3채널(BGR)이어도 됨.
        /// </summary>
        public Mat Mask
        {
            get => mask;
            set => SetProperty(ref mask, value);
        }

        private BitWiseMode mode = BitWiseMode.And;

        [ImportExport]
        [Enum(typeof(BitWiseMode))]
        public BitWiseMode Mode
        {
            get => mode;
            set => SetProperty(ref mode, value);
        }

    }
}
