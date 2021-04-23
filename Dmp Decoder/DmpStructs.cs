using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace Dmp_Decoder
{
    public abstract class DmpStruct
    {
        public DmpStruct(byte[] bytes) => BytesToFields(bytes);

        protected abstract void BytesToFields(byte[] bytes);

        protected ICollection<string> BytesToHexConvert(byte[] bytes, int[] bytesDelimiters = null)
        {
            List<string> hexCodes = new List<string>();

            if (bytesDelimiters is null || bytesDelimiters.Length == 0)
            {
                Array.Reverse(bytes);
                hexCodes.Add(BitConverter.ToString(bytes).Replace("-", string.Empty));
            }
            else
            {
                int currentIteration = 0;
                foreach (var delimeter in bytesDelimiters)
                {
                    byte[] iteration = new byte[delimeter];
                    Array.Copy(bytes, currentIteration, iteration, 0, delimeter);
                    Array.Reverse(iteration);
                    hexCodes.Add(BitConverter.ToString(iteration).Replace("-", string.Empty));

                    currentIteration += delimeter;
                }
            }

            return hexCodes;
        }
    }

    public class BMFH : DmpStruct
    {
        #region Properites

        public string Type { get; private set; }
        public string SizeF { get; private set; }
        public string TimeStampHi { get; private set; }
        public string TimeStampLo { get; private set; }
        public string OffBits { get; private set; }
        public static int StructSize { get; private set; } = 14;

        #endregion

        public BMFH(byte[] bytes) : base(bytes) { }

        protected override void BytesToFields(byte[] bytes)
        {
            if (bytes.Length != StructSize) throw new ArgumentException($"Invalid given bytes array length in {GetType()}! " +
                $"Length must be {StructSize}.");

            int[] FieldsSize = new int[5] { 2, 4, 2, 2, 4 };
            List<string> hexCodes = (List<string>)BytesToHexConvert(bytes, FieldsSize);

            Type = hexCodes[0];
            SizeF = hexCodes[1];
            TimeStampHi = hexCodes[2];
            TimeStampLo = hexCodes[3];
            OffBits = hexCodes[4];
        }
    }

    public class BMIH : DmpStruct
    {
        #region Properties

        public string Size { get; private set; }
        public string Width { get; private set; }
        public string Height { get; private set; }
        public string Planes { get; private set; }
        public string BitCount { get; private set; }
        public string Compression { get; private set; }
        public string SizeImage { get; private set; }
        public string XPelsPeMeter { get; private set; }
        public string YPelsPeMeter { get; private set; }
        public string ClrUsed { get; private set; }
        public string ClrImportant { get; private set; }
        public string SecBlockSet { get; private set; }
        public static int StructSize { get; private set; } = 42;

        #endregion

        public BMIH(byte[] bytes) : base(bytes) { }

        protected override void BytesToFields(byte[] bytes)
        {
            if (bytes.Length != StructSize) throw new ArgumentException($"Invalid given bytes array length in {GetType()}! " +
                $"Length must be {StructSize}.");

            int[] FieldsSize = new int[12] { 4, 4, 4, 2, 2, 4, 4, 4, 4, 4, 4, 2 };
            List<string> hexCodes = (List<string>)BytesToHexConvert(bytes, FieldsSize);

            Size = hexCodes[0]; Width = hexCodes[1]; Height = hexCodes[2]; Planes = hexCodes[3]; BitCount = hexCodes[4]; Compression = hexCodes[5]; SizeImage = hexCodes[6];
            XPelsPeMeter = hexCodes[7]; YPelsPeMeter = hexCodes[8]; ClrUsed = hexCodes[9]; ClrImportant = hexCodes[10]; SecBlockSet = hexCodes[11];
        }
    }

    public class SecBlock : DmpStruct
    {
        #region Properties

        public string Size { get; private set; }
        public string Station { get; private set; }
        public string Camera { get; private set; }
        public string Exposure { get; private set; }
        public string AllSet { get; private set; }
        public string AmpVideo { get; private set; }
        public string Focusing { get; private set; }
        public string Zoom { get; private set; }
        public string Cord { get; private set; }
        public string Data { get; private set; }
        public string Time_1 { get; private set; }
        public string Time_2 { get; private set; }
        public string Azimuth { get; private set; }
        public string Elevation { get; private set; }
        public string SEQ_ID { get; private set; }
        public string Reserve { get; private set; }
        public static int StructSize { get; private set; } = 140;

        #endregion

        public SecBlock(byte[] bytes) : base(bytes) { }

        protected override void BytesToFields(byte[] bytes)
        {
            if (bytes.Length != StructSize) throw new ArgumentException($"Invalid given bytes array length in {GetType()}! " +
                $"Length must be {StructSize}.");

            int[] FieldsSize = new int[16] { 2, 1, 1, 2, 1, 1, 2, 2, 4, 4, 4, 4, 4, 4, 4, 100 };
            List<string> hexCodes = (List<string>)BytesToHexConvert(bytes, FieldsSize);

            Size = hexCodes[0]; Station = hexCodes[1]; Camera = hexCodes[2]; Exposure = hexCodes[3]; AllSet = hexCodes[4]; AmpVideo = hexCodes[5]; Focusing = hexCodes[6];
            Zoom = hexCodes[7]; Cord = hexCodes[8]; Data = hexCodes[9]; Time_1 = hexCodes[10]; Time_2 = hexCodes[11]; Azimuth = hexCodes[12]; Elevation = hexCodes[13];
            SEQ_ID = hexCodes[14]; Reserve = hexCodes[15];
        }
    }

    public class ImageBlock : DmpStruct
    {
        private byte[] _imgCode;

        public string ImageCode { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int BitCount { get; private set; }

        public ImageBlock(byte[] bytes, int width, int height, int bitCount) : base(bytes)
        {
            _imgCode = bytes;
            Width = width;
            Height = height;
            BitCount = bitCount;
        }

        public Bitmap GetBitmap()
        {
            int[] newcode = new int[_imgCode.Length / 2];
            for (int i = 0; i < _imgCode.Length; i++)
            {
                if (i % 2 == 0)
                {
                    if ((_imgCode[i] & 1) > 0)
                    {
                        newcode[i / 2] += 256;
                    }
                    if ((_imgCode[i] & 2) > 0)
                    {
                        newcode[i / 2] += 512;
                    }
                    newcode[i / 2] += _imgCode[i + 1];
                }
            }

            Bitmap bitMap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);

            int v = 0;
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    int a = (int)Utils.MapValue(newcode[v], 0, 1023, 0, 255);
                    bitMap.SetPixel(i, j, Color.FromArgb(a, 0, 0, 0));
                    v++;
                }
            }

            return bitMap;
        }

        protected override void BytesToFields(byte[] bytes)
        {
            List<string> hex = (List<string>)BytesToHexConvert(bytes);
            ImageCode = hex[0];
        }

    }
}
