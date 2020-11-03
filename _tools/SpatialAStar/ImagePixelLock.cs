/*
The MIT License

Copyright (c) 2010 Christoph Husse

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace SettlersEngine
{
    public unsafe class ImagePixelLock : System.Runtime.ConstrainedExecution.CriticalFinalizerObject, IDisposable
    {
        private static byte[] buffer = new byte[1024];
        private static byte[] tmpBuffer = new byte[1024];
        private Bitmap bitmap;
        private BitmapData data;
        public Boolean IsCopy { get; private set; }
        public Int64 Checksum { get; private set; }
        public int* Pixels { get; private set; }

        public int Width { get { return bitmap.Width; } }
        public int Height { get { return bitmap.Height; } }

        public ImagePixelLock(Bitmap inSource)
            : this(inSource, new System.Drawing.Rectangle(0, 0, inSource.Width, inSource.Height), false)
        {
        }

        public ImagePixelLock(Bitmap inSource, Boolean inCreateCopy)
            : this(inSource, new System.Drawing.Rectangle(0, 0, inSource.Width, inSource.Height), inCreateCopy)
        {
        }

        public ImagePixelLock(Bitmap inSource, System.Drawing.Rectangle inLockRegion)
            : this(inSource, inLockRegion, false)
        {
        }

        public ImagePixelLock(Bitmap inSource, System.Drawing.Rectangle inLockRegion, Boolean inCreateCopy)
        {
            if (inSource.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                throw new ArgumentException("Given bitmap has an unsupported pixel format.");

            IsCopy = inCreateCopy;

            if (inCreateCopy)
                bitmap = (Bitmap)inSource.Clone();
            else
                bitmap = inSource;

            data = bitmap.LockBits(inLockRegion, ImageLockMode.ReadWrite, inSource.PixelFormat);
            Pixels = (int*)data.Scan0.ToPointer();

            // compute checksum from pixeldata
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            int* ptr = (int*)data.Scan0.ToPointer();

            for (int i = 0, byteCount = Width * Height * 4; i < byteCount; i += buffer.Length)
            {
                int count = Math.Min(buffer.Length, byteCount - i);

                System.Runtime.InteropServices.Marshal.Copy((IntPtr)ptr, buffer, 0, count);
                md5.TransformBlock(buffer, 0, count, tmpBuffer, 0);

                ptr += count / 4;
            }

            md5.TransformFinalBlock(new byte[0], 0, 0);

            byte[] checksum = md5.Hash;

            for (int i = 0; i < 8; i++)
            {
                Checksum |= (((Int64)checksum[i]) << (i * 8));
            }
        }

        public override int GetHashCode()
        {
            return unchecked((Int32)Checksum);
        }

        ~ImagePixelLock()
        {
            Dispose();
        }

        public void Dispose()
        {
            try
            {
                if ((data != null) && (bitmap != null))
                    bitmap.UnlockBits(data);

                if (IsCopy && (bitmap != null))
                    bitmap.Dispose();
            }
            catch
            {
                // bitmap might be already disposed even if we got a valid pixellock
            }

            data = null;
            bitmap = null;
            Pixels = (int*)0;
        }
    }

}
