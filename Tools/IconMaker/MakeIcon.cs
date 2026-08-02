using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

/*
 * MakeIcon - WCS 태스크 아이콘 생성기
 *
 *   사용법 : MakeIcon.exe <출력.ico> <텍스트> [#RRGGBB]
 *            텍스트에 '/' 를 넣으면 두 줄로 그린다.  예) "CV/1F"
 *
 *   예)  MakeIcon.exe disp.ico DISP  #FF8C1A
 *        MakeIcon.exe cv.ico   CV/1F #56BAEA
 *        MakeIcon.exe s-sc.ico S-SC  #B5E61D
 *
 *   이 프로젝트군의 아이콘 규칙
 *     - 불투명 검정 정사각 배경에 굵은 산세리프 텍스트를 꽉 채워 중앙 배치
 *     - 시스템마다 글자색을 달리해서 작업표시줄에서 색만으로 구분되게 한다
 *     - 16/24/32/48/64/128/256 프레임을 모두 32bpp DIB 로 넣는다
 *       (PNG 압축 프레임을 쓰지 않아 구형 셸/런타임에서도 안전)
 *     - 16/24 는 안티에일리어싱하면 글자가 뭉개지므로
 *       폭이 좁은 Arial Narrow Bold 를 픽셀 그리드에 맞춰 또렷하게 찍는다
 *
 *   빌드/실행은 같은 폴더의 MakeIcon.cmd 참조.
 */
class MakeIcon
{
    static readonly int[] SIZES = { 16, 24, 32, 48, 64, 128, 256 };
    static readonly Color BG = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);

    const string FACE_SMALL = "Arial Narrow";   // 16/24 용 (좁은 폭)
    const string FACE_LARGE = "Arial Black";    // 32 이상 용 (계열 표준)
    const int TINY_MAX = 24;

    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("usage: MakeIcon.exe <out.ico> <text> [#RRGGBB]");
            Console.WriteLine("       text 에 '/' 를 넣으면 두 줄로 그린다.  예) CV/1F");
            return 1;
        }

        string outPath = args[0];
        string text = args[1];
        Color fg = (args.Length >= 3) ? ParseColor(args[2]) : Color.FromArgb(0xFF, 0xFF, 0x8C, 0x1A);

        string[] lines = text.Split('/');
        if (lines.Length > 2)
        {
            Console.WriteLine("error: 줄은 최대 2줄까지만 지원한다.");
            return 1;
        }

        List<Bitmap> frames = new List<Bitmap>();
        foreach (int s in SIZES) frames.Add(Render(s, lines, fg));

        WriteIco(outPath, frames);
        foreach (Bitmap b in frames) b.Dispose();

        Console.WriteLine(string.Format("wrote {0}  ({1} bytes, {2} frames, text=\"{3}\", color=#{4:X2}{5:X2}{6:X2})",
                          outPath, new FileInfo(outPath).Length, SIZES.Length, text, fg.R, fg.G, fg.B));
        return 0;
    }

    static Color ParseColor(string s)
    {
        s = s.TrimStart('#');
        int v = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
        return Color.FromArgb(0xFF, (v >> 16) & 0xFF, (v >> 8) & 0xFF, v & 0xFF);
    }

    // ---- 렌더링 ----------------------------------------------------------
    static Bitmap Render(int size, string[] lines, Color fg)
    {
        Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            bool bTiny = (size <= TINY_MAX);
            string face = bTiny ? FACE_SMALL : FACE_LARGE;
            FontStyle style = bTiny ? FontStyle.Bold : FontStyle.Regular;

            g.Clear(BG);
            g.TextRenderingHint = bTiny ? TextRenderingHint.SingleBitPerPixelGridFit : TextRenderingHint.AntiAlias;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            float maxW = size * (bTiny ? 0.96f : 0.92f);
            float maxH = size * (bTiny ? 0.85f : 0.80f) / lines.Length;

            // 폭/높이에 맞는 최대 글꼴 크기를 찾는다
            Font font = null;
            SizeF[] measured = new SizeF[lines.Length];
            for (float pt = size; pt >= 1f; pt -= 0.25f)
            {
                Font f = new Font(face, pt, style, GraphicsUnit.Pixel);
                float wmax = 0f, hmax = 0f;
                SizeF[] m = new SizeF[lines.Length];
                for (int i = 0; i < lines.Length; i++)
                {
                    m[i] = g.MeasureString(lines[i], f, PointF.Empty, StringFormat.GenericTypographic);
                    if (m[i].Width > wmax) wmax = m[i].Width;
                    if (m[i].Height > hmax) hmax = m[i].Height;
                }
                if (wmax <= maxW && hmax <= maxH) { font = f; measured = m; break; }
                f.Dispose();
            }
            if (font == null)
            {
                font = new Font(face, Math.Max(1f, size * 0.2f), style, GraphicsUnit.Pixel);
                for (int i = 0; i < lines.Length; i++)
                    measured[i] = g.MeasureString(lines[i], font, PointF.Empty, StringFormat.GenericTypographic);
            }

            float total = 0f;
            foreach (SizeF m in measured) total += m.Height;
            float y = (size - total) / 2f;

            using (SolidBrush br = new SolidBrush(fg))
                for (int i = 0; i < lines.Length; i++)
                {
                    g.DrawString(lines[i], font, br,
                                 new PointF((size - measured[i].Width) / 2f, y),
                                 StringFormat.GenericTypographic);
                    y += measured[i].Height;
                }

            font.Dispose();
        }
        return bmp;
    }

    // ---- ICO(DIB) 인코딩 -------------------------------------------------
    static void WriteIco(string path, List<Bitmap> frames)
    {
        List<byte[]> blobs = new List<byte[]>();
        foreach (Bitmap b in frames) blobs.Add(EncodeDib(b));

        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        using (BinaryWriter w = new BinaryWriter(fs))
        {
            w.Write((ushort)0);                 // reserved
            w.Write((ushort)1);                 // type = icon
            w.Write((ushort)frames.Count);

            int offset = 6 + (16 * frames.Count);
            for (int i = 0; i < frames.Count; i++)
            {
                Bitmap b = frames[i];
                w.Write((byte)(b.Width >= 256 ? 0 : b.Width));    // 256 은 0 으로 기록
                w.Write((byte)(b.Height >= 256 ? 0 : b.Height));
                w.Write((byte)0);               // color count
                w.Write((byte)0);               // reserved
                w.Write((ushort)1);             // planes
                w.Write((ushort)32);            // bit count
                w.Write(blobs[i].Length);
                w.Write(offset);
                offset += blobs[i].Length;
            }
            foreach (byte[] blob in blobs) w.Write(blob);
        }
    }

    // BITMAPINFOHEADER + 32bpp BGRA XOR(bottom-up) + 1bpp AND mask
    static byte[] EncodeDib(Bitmap bmp)
    {
        int w = bmp.Width, h = bmp.Height;
        int xorSize = w * h * 4;
        int maskStride = ((w + 31) / 32) * 4;
        int andSize = maskStride * h;

        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(40);                       // biSize
            bw.Write(w);                        // biWidth
            bw.Write(h * 2);                    // biHeight (XOR + AND 를 합친 높이)
            bw.Write((ushort)1);                // biPlanes
            bw.Write((ushort)32);               // biBitCount
            bw.Write(0);                        // biCompression = BI_RGB
            bw.Write(xorSize + andSize);        // biSizeImage
            bw.Write(0); bw.Write(0);           // biX/YPelsPerMeter
            bw.Write(0); bw.Write(0);           // biClrUsed / biClrImportant

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                byte[] row = new byte[w * 4];
                for (int y = h - 1; y >= 0; y--)   // DIB 는 bottom-up
                {
                    IntPtr src = new IntPtr(bd.Scan0.ToInt64() + (long)y * bd.Stride);
                    System.Runtime.InteropServices.Marshal.Copy(src, row, 0, row.Length);
                    bw.Write(row);
                }
            }
            finally { bmp.UnlockBits(bd); }

            bw.Write(new byte[andSize]);        // AND 마스크 : 전부 불투명이므로 0

            bw.Flush();
            return ms.ToArray();
        }
    }
}
