/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

// This file contains general utilities to aid in development.
// Classes here generally shouldn't be exposed publicly since
// they're not particular to any library functionality.
// Because the classes here are internal, it's likely this file
// might be included in multiple assemblies.
namespace Standard
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    internal static class Utility
    {
        private struct _EmptyEnumerable<T> : IEnumerable<T>
        {
            #region IEnumerable<T> Members

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                yield break;
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        private static readonly MD5 _md5Hash = MD5.Create();

        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file.")
        ]
        private static bool _MemCmp(IntPtr left, IntPtr right, long cb)
        {
            int offset = 0;
            
            for (; offset < (cb - sizeof(Int64)); offset += sizeof(Int64))
            {
                Int64 left64 = Marshal.ReadInt64(left, offset);
                Int64 right64 = Marshal.ReadInt64(right, offset);

                if (left64 != right64)
                {
                    return false;
                }
            }

            for (; offset < cb; offset += sizeof(byte))
            {
                byte left8 = Marshal.ReadByte(left, offset);
                byte right8 = Marshal.ReadByte(right, offset);

                if (left8 != right8)
                {
                    return false;
                }
            }

            return true;
        }

        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file.")
        ]
        private static byte[] _GetBytesFromBitmapSource(BitmapSource bmp)
        {
            int width = bmp.PixelWidth;
            int height = bmp.PixelHeight;
            int stride = width * ((bmp.Format.BitsPerPixel + 7) / 8);

            var pixels = new byte[height * stride];

            bmp.CopyPixels(pixels, stride, 0);

            return pixels;
        }

        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file.")
        ]
        private static BitmapSource _GenerateBitmapSource(ImageSource img)
        {
            var dv = new DrawingVisual();
            DrawingContext dc = dv.RenderOpen();
            dc.DrawImage(img, new Rect(0, 0, img.Width, img.Height));
            dc.Close();
            var bmp = new RenderTargetBitmap((int)img.Width, (int)img.Height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            return bmp;
        }

        public static int GET_X_LPARAM(IntPtr lParam)
        {
            return LOWORD(lParam.ToInt32());
        }

        public static int GET_Y_LPARAM(IntPtr lParam)
        {
            return HIWORD(lParam.ToInt32());
        }

        public static int HIWORD(int i)
        {
            return i >> 16;
        }

        public static int LOWORD(int i)
        {
            return i & 0xFFFF;
        }

        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file.")
        ]
        public static bool AreImageSourcesEqual(ImageSource left, ImageSource right)
        {
            if (null == left)
            {
                return right == null;
            }
            if (null == right)
            {
                return false;
            }

            BitmapSource leftBmp = _GenerateBitmapSource(left);
            BitmapSource rightBmp = _GenerateBitmapSource(right);

            byte[] leftPixels = _GetBytesFromBitmapSource(leftBmp);
            byte[] rightPixels = _GetBytesFromBitmapSource(rightBmp);

            if (leftPixels.Length != rightPixels.Length)
            {
                return false;
            }

            // pin the left buffer
            GCHandle handleLeft = GCHandle.Alloc(leftPixels, GCHandleType.Pinned);
            IntPtr ptrLeft = handleLeft.AddrOfPinnedObject();

            // pin the right buffer
            GCHandle handleRight = GCHandle.Alloc(rightPixels, GCHandleType.Pinned);
            IntPtr ptrRight = handleRight.AddrOfPinnedObject();

            bool fRet = _MemCmp(ptrLeft, ptrRight, leftPixels.Length);

            handleLeft.Free();
            handleRight.Free();

            return fRet;
        }

        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file."),
            SuppressMessage(
                "Microsoft.Security",
                "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")
        ]
        public static bool AreStreamsEqual(Stream left, Stream right)
        {
            if (null == left)
            {
                return right == null;
            }
            if (null == right)
            {
                return false;
            }

            if (!left.CanRead || !right.CanRead)
            {
                throw new NotSupportedException("The streams can't be read for comparison");
            }

            if (left.Length != right.Length)
            {
                return false;
            }

            var length = (int)left.Length;

            // seek to beginning
            left.Position = 0;
            right.Position = 0;

            // total bytes read
            int totalReadLeft = 0;
            int totalReadRight = 0;

            // bytes read on this iteration
            int cbReadLeft = 0;
            int cbReadRight = 0;

            // where to store the read data
            var leftBuffer = new byte[512];
            var rightBuffer = new byte[512];

            // pin the left buffer
            GCHandle handleLeft = GCHandle.Alloc(leftBuffer, GCHandleType.Pinned);
            IntPtr ptrLeft = handleLeft.AddrOfPinnedObject();

            // pin the right buffer
            GCHandle handleRight = GCHandle.Alloc(rightBuffer, GCHandleType.Pinned);
            IntPtr ptrRight = handleRight.AddrOfPinnedObject();

            try
            {
                while (totalReadLeft < length)
                {
                    Assert.AreEqual(totalReadLeft, totalReadRight);

                    cbReadLeft = left.Read(leftBuffer, 0, leftBuffer.Length);
                    cbReadRight = right.Read(rightBuffer, 0, rightBuffer.Length);

                    // verify the contents are an exact match
                    if (cbReadLeft != cbReadRight)
                    {
                        return false;
                    }

                    if (!_MemCmp(ptrLeft, ptrRight, cbReadLeft))
                    {
                        return false;
                    }

                    totalReadLeft += cbReadLeft;
                    totalReadRight += cbReadRight;
                }

                Assert.AreEqual(cbReadLeft, cbReadRight);
                Assert.AreEqual(totalReadLeft, totalReadRight);
                Assert.AreEqual(length, totalReadLeft);

                return true;
            }
            finally
            {
                handleLeft.Free();
                handleRight.Free();
            }
        }

        public static IEnumerable<T> GetEmptyEnumerable<T>()
        {
            return new _EmptyEnumerable<T>();
        }

        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file.")
        ]
        public static bool GuidTryParse(string guidString, out Guid guid)
        {
            Verify.IsNeitherNullNorEmpty(guidString, "guidString");

            try
            {
                guid = new Guid(guidString);
                return true;
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }
            // Doesn't seem to be a valid guid.
            guid = default(Guid);
            return false;
        }

        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file.")
        ]
        public static bool IsFlagSet(int value, int mask)
        {
            return 0 != (value & mask);
        }

        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file.")
        ]
        public static bool IsFlagSet(uint value, uint mask)
        {
            return 0 != (value & mask);
        }

        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file.")
        ]
        public static bool IsFlagSet(long value, long mask)
        {
            return 0 != (value & mask);
        }

        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file.")
        ]
        public static bool IsFlagSet(ulong value, ulong mask)
        {
            return 0 != (value & mask);
        }

        /// <summary>
        /// Simple guard against the exceptions that File.Delete throws on null and empty strings.
        /// </summary>
        /// <param name="path">The path to delete.  Unlike File.Delete, this can be null or empty.</param>
        public static void SafeDeleteFile(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                File.Delete(path);
            }
        }

        /// <summary>GDI's DeleteObject</summary>
        /// <param name="hObject">The GDI object to delete.</param>
        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file.")
        ]
        public static void SafeDeleteObject(ref IntPtr hObject)
        {
            IntPtr p = hObject;
            hObject = IntPtr.Zero;
            if (IntPtr.Zero != p)
            {
                Interop.NativeMethods.DeleteObject(p);
            }
        }

        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file.")
        ]
        public static void SafeDispose<T>(ref T disposable) where T : IDisposable
        {
            // Dispose can safely be called on an object multiple times.
            IDisposable t = disposable;
            disposable = default(T);
            if (null != t)
            {
                t.Dispose();
            }
        }

        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file."),
            SuppressMessage(
                "Microsoft.Security",
                "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")
        ]
        public static void SafeCoTaskMemFree(ref IntPtr ptr)
        {
            IntPtr p = ptr;
            ptr = IntPtr.Zero;
            if (IntPtr.Zero != p)
            {
                Marshal.FreeCoTaskMem(p);
            }
        }

        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file."),
            SuppressMessage(
                "Microsoft.Security",
                "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")
        ]
        public static void SafeFreeHGlobal(ref IntPtr hglobal)
        {
            IntPtr p = hglobal;
            hglobal = IntPtr.Zero;
            if (IntPtr.Zero != p)
            {
                Marshal.FreeHGlobal(p);
            }
        }

        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file."),
            SuppressMessage(
                "Microsoft.Security",
                "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")
        ]
        public static void SafeRelease<T>(ref T comObject) where T : class
        {
            T t = comObject;
            comObject = default(T);
            if (null != t)
            {
                Assert.IsTrue(Marshal.IsComObject(t));
                Marshal.ReleaseComObject(t);
            }
        }

        /// <summary>
        /// Utility to help classes catenate their properties for implementing ToString().
        /// </summary>
        /// <param name="source">The StringBuilder to catenate the results into.</param>
        /// <param name="propertyName">The name of the property to be catenated.</param>
        /// <param name="value">The value of the property to be catenated.</param>
        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file.")
        ]
        public static void GeneratePropertyString(StringBuilder source, string propertyName, string value)
        {
            Assert.IsNotNull(source);
            Assert.IsFalse(string.IsNullOrEmpty(propertyName));

            if (0 != source.Length)
            {
                source.Append(' ');
            }

            source.Append(propertyName);
            source.Append(": ");
            if (string.IsNullOrEmpty(value))
            {
                source.Append("<null>");
            }
            else
            {
                source.Append('\"');
                source.Append(value);
                source.Append('\"');
            }
        }

        /// <summary>
        /// Generates ToString functionality for a struct.  This is an expensive way to do it,
        /// it exists for the sake of debugging while classes are in flux.
        /// Eventually this should just be removed and the classes should
        /// do this without reflection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="object"></param>
        /// <returns></returns>
        [
            Obsolete,
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file.")
        ]
        public static string GenerateToString<T>(T @object) where T : struct
        {
            var sbRet = new StringBuilder();
            foreach (PropertyInfo property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (0 != sbRet.Length)
                {
                    sbRet.Append(", ");
                }
                Assert.AreEqual(0, property.GetIndexParameters().Length);
                object value = property.GetValue(@object, null);
                string format = null == value ? "{0}: <null>" : "{0}: \"{1}\"";
                sbRet.AppendFormat(format, property.Name, value);
            }
            return sbRet.ToString();
        }

        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file.")
        ]
        public static void CopyStream(Stream destination, Stream source)
        {
            Assert.IsNotNull(source);
            Assert.IsNotNull(destination);

            destination.Position = 0;

            // If we're copying from, say, a web stream, don't fail because of this.
            if (source.CanSeek)
            {
                source.Position = 0;

                // Consider that this could throw because 
                // the source stream doesn't know it's size...
                destination.SetLength(source.Length);
            }

            var buffer = new byte[4096];
            int cbRead;

            do
            {
                cbRead = source.Read(buffer, 0, buffer.Length);
                if (0 != cbRead)
                {
                    destination.Write(buffer, 0, cbRead);
                }
            }
            while (buffer.Length == cbRead);

            // Reset the Seek pointer before returning.
            destination.Position = 0;
        }

        [
            SuppressMessage(
                "Microsoft.Performance",
                "CA1811:AvoidUncalledPrivateCode",
                Justification = "Shared code file.")
        ]
        public static string HashStreamMD5(Stream stm)
        {
            stm.Position = 0;
            var hashBuilder = new StringBuilder();
            foreach (byte b in _md5Hash.ComputeHash(stm))
            {
                hashBuilder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            }

            return hashBuilder.ToString();
        }
    }
}
