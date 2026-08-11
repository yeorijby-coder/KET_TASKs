// ---------------------------------------------------------------------------
//  MakeIoIcon
//
//  IO_SCH 계열 아이콘(파란 둥근 사각 + 흰 글자) 생성기.
//
//    MakeIoIcon <출력.ico> <윗줄> [아랫줄]
//
//  예)  MakeIoIcon IO.ico  I/O            통합판 (층 표시 없음)
//       MakeIoIcon IO.ico  I/O  1F        층별 판
//
//  MakeIcon 은 검정 정사각 + 색 글자(CV / SC / DISP)를 그린다.
//  IO_SCH 는 예전부터 파란 둥근 사각이라 그림체가 다르다.
//  기존 아이콘에서 잰 값을 그대로 쓴다.
//    모서리 반경  크기의 15%
//    테두리       크기의 1.6% (최소 1px), #78A0DC
//    바탕         위 #2B57AB -> 아래 #142F69 세로 그라데이션
//    글자         흰색, 아랫줄이 있으면 노란색(#FFD200)
//
//  출력 형식은 MakeIcon 과 같다. 16/24/32/48/64/128/256, 전부 32bpp DIB.
//  (PNG 프레임을 못 읽는 셸이 있어 DIB 로만 넣는다)
// ---------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

class MakeIoIcon
{
    static readonly int[] SIZES = { 16, 24, 32, 48, 64, 128, 256 };

    static readonly Color TOP    = Color.FromArgb(0xFF, 0x2B, 0x57, 0xAB);
    static readonly Color BOTTOM = Color.FromArgb(0xFF, 0x14, 0x2F, 0x69);
    static readonly Color EDGE   = Color.FromArgb(0xFF, 0x78, 0xA0, 0xDC);
    static readonly Color FG1    = Color.White;
    static readonly Color FG2    = Color.FromArgb(0xFF, 0xFF, 0xD2, 0x00);

    const int TINY_MAX = 24;
    const string FACE_SMALL = "Arial Narrow";
    const string FACE_LARGE = "Arial";

    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: MakeIoIcon <out.ico> <line1> [line2]");
            return 1;
        }

        string outPath = args[0];
        string[] lines = (args.Length >= 3 && args[2].Length > 0)
                       ? new string[] { args[1], args[2] }
                       : new string[] { args[1] };

        List<Bitmap> frames = new List<Bitmap>();
        foreach (int s in SIZES) frames.Add(Render(s, lines));

        string dir = Path.GetDirectoryName(Path.GetFullPath(outPath));
        if (dir.Length > 0 && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

        WriteIco(outPath, frames);
        foreach (Bitmap b in frames) b.Dispose();

        Console.WriteLine("만듦 : {0}  [{1}]", outPath, string.Join(" / ", lines));
        return 0;
    }

    // ---- 렌더링 ----------------------------------------------------------
    static Bitmap Render(int size, string[] lines)
    {
        Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            bool bTiny = (size <= TINY_MAX);

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = bTiny ? TextRenderingHint.SingleBitPerPixelGridFit
                                        : TextRenderingHint.AntiAlias;

            float pen = Math.Max(1f, size * 0.016f);
            float r = size * 0.15f;
            RectangleF box = new RectangleF(pen / 2f, pen / 2f, size - pen, size - pen);

            using (GraphicsPath path = Round(box, r))
            {
                using (LinearGradientBrush br = new LinearGradientBrush(
                           new RectangleF(0, -1, size, size + 2), TOP, BOTTOM, 90f))
                    g.FillPath(br, path);

                using (Pen p = new Pen(EDGE, pen))
                    g.DrawPath(p, path);
            }

            // 글자 : 폭/높이에 맞는 최대 글꼴 크기를 찾는다
            string face = bTiny ? FACE_SMALL : FACE_LARGE;
            float maxW = size * 0.72f;
            float maxH = size * (lines.Length == 1 ? 0.52f : 0.34f);

            Font font = null;
            SizeF[] m = new SizeF[lines.Length];
            for (float pt = size; pt >= 1f; pt -= 0.25f)
            {
                Font f = new Font(face, pt, FontStyle.Bold, GraphicsUnit.Pixel);
                float wmax = 0f, hmax = 0f;
                SizeF[] t = new SizeF[lines.Length];
                for (int i = 0; i < lines.Length; i++)
                {
                    t[i] = g.MeasureString(lines[i], f, PointF.Empty, StringFormat.GenericTypographic);
                    if (t[i].Width > wmax) wmax = t[i].Width;
                    if (t[i].Height > hmax) hmax = t[i].Height;
                }
                if (wmax <= maxW && hmax <= maxH) { font = f; m = t; break; }
                f.Dispose();
            }
            if (font == null)
            {
                font = new Font(face, Math.Max(1f, size * 0.2f), FontStyle.Bold, GraphicsUnit.Pixel);
                for (int i = 0; i < lines.Length; i++)
                    m[i] = g.MeasureString(lines[i], font, PointF.Empty, StringFormat.GenericTypographic);
            }

            float total = 0f;
            foreach (SizeF s2 in m) total += s2.Height;
            float y = (size - total) / 2f;

            for (int i = 0; i < lines.Length; i++)
            {
                using (SolidBrush br = new SolidBrush(i == 0 ? FG1 : FG2))
                    g.DrawString(lines[i], font, br,
                                 new PointF((size - m[i].Width) / 2f, y),
                                 StringFormat.GenericTypographic);
                y += m[i].Height;
            }

            font.Dispose();
        }
        return bmp;
    }

    static GraphicsPath Round(RectangleF b, float r)
    {
        float d = r * 2f;
        GraphicsPath p = new GraphicsPath();
        if (d <= 0f) { p.AddRectangle(b); return p; }

        p.AddArc(b.Left, b.Top, d, d, 180, 90);
        p.AddArc(b.Right - d, b.Top, d, d, 270, 90);
        p.AddArc(b.Right - d, b.Bottom - d, d, d, 0, 90);
        p.AddArc(b.Left, b.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }

    // ---- ICO(DIB) 인코딩 ------------------------------------------------
    //      MakeIcon.cs 와 같은 형식이다.
    static void WriteIco(string path, List<Bitmap> frames)
    {
        List<byte[]> blobs = new List<byte[]>();
        foreach (Bitmap b in frames) blobs.Add(EncodeDib(b));

        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        using (BinaryWriter w = new BinaryWriter(fs))
        {
            w.Write((ushort)0);
            w.Write((ushort)1);
            w.Write((ushort)frames.Count);

            int offset = 6 + (16 * frames.Count);
            for (int i = 0; i < frames.Count; i++)
            {
                Bitmap b = frames[i];
                w.Write((byte)(b.Width >= 256 ? 0 : b.Width));
                w.Write((byte)(b.Height >= 256 ? 0 : b.Height));
                w.Write((byte)0);
                w.Write((byte)0);
                w.Write((ushort)1);
                w.Write((ushort)32);
                w.Write(blobs[i].Length);
                w.Write(offset);
                offset += blobs[i].Length;
            }
            foreach (byte[] blob in blobs) w.Write(blob);
        }
    }

    static byte[] EncodeDib(Bitmap bmp)
    {
        int w = bmp.Width, h = bmp.Height;
        int xorSize = w * h * 4;
        int maskStride = ((w + 31) / 32) * 4;
        int andSize = maskStride * h;

        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(40);
            bw.Write(w);
            bw.Write(h * 2);
            bw.Write((ushort)1);
            bw.Write((ushort)32);
            bw.Write(0);
            bw.Write(xorSize + andSize);
            bw.Write(0); bw.Write(0);
            bw.Write(0); bw.Write(0);

            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                byte[] row = new byte[w * 4];
                for (int y = h - 1; y >= 0; y--)
                {
                    IntPtr src = new IntPtr(bd.Scan0.ToInt64() + (long)y * bd.Stride);
                    System.Runtime.InteropServices.Marshal.Copy(src, row, 0, row.Length);
                    bw.Write(row);
                }
            }
            finally { bmp.UnlockBits(bd); }

            bw.Write(new byte[andSize]);
            bw.Flush();
            return ms.ToArray();
        }
    }
}
