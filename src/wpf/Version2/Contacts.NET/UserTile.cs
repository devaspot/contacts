namespace Microsoft.Communications.Contacts
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Windows;
    using Standard;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using Microsoft.ContactsBridge.Interop;
    
    /// <summary>
    /// Utilities for rendering a Contact's User Tile image in different ways.
    /// </summary>
    public class UserTile : INotifyPropertyChanged
    {
        private struct FrameMetric
        {
            public FrameMetric(int width, int height, Rect boundingFrame, Uri path)
                : this()
            {
                Width = width;
                Height = height;
                BoundingFrame = boundingFrame;
                Path = path;
            }

            public readonly int Width;
            public readonly int Height;
            public readonly Rect BoundingFrame;
            public readonly Uri Path;
        }

        #region Fields
        private static readonly List<FrameMetric> _FrameMetrics;
        private static readonly Uri _emptyUserTileUri;

        private readonly ImageSource _imageSource;
        private ImageSource _imageComposited;
        private ImageSource _overlay;
        #endregion

        //
        // WATCHOUT!
        // This is really weird... the "pack://" URI syntax is actually an invalid URL.
        // Uri's constructor throws a UriFormatException *sometimes* when passed one of
        // those strings.  It apparently depends on whether some specific WPF Dll has
        // been loaded.
        // The only writeup of this behavior I've seen is here:
        // http://chaodaimos.blogspot.com/2007/01/latest-greatest-wpf-horror-story.html
        // The way to get around this is get the pack:// syntax registered by invoking
        // the System.Windows.Application class.  Don't need to do anything here, just
        // need to get it in memory.
        //

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static UserTile()
        {
            // Code herein relies on this being in largest to smallest order.
            _FrameMetrics = new List<FrameMetric>
            {
                new FrameMetric(256, 256, new Rect(30, 28, 190, 190), ContactUtil.GetResourceUri("UserTileFrame256.png")),
                new FrameMetric(128, 128, new Rect(14, 13,  95,  97), ContactUtil.GetResourceUri("UserTileFrame128.png")),
                new FrameMetric( 96,  96, new Rect(11, 12,  68,  69), ContactUtil.GetResourceUri("UserTileFrame96.png")),
                new FrameMetric( 64,  64, new Rect( 8,  8,  44,  44), ContactUtil.GetResourceUri("UserTileFrame64.png")),
                new FrameMetric( 32,  32, new Rect( 5,  5,  20,  20), ContactUtil.GetResourceUri("UserTileFrame32.png")),
                new FrameMetric( 16,  16, new Rect( 3,  3,  10,  10), ContactUtil.GetResourceUri("UserTileFrame16.png"))
            };

            _emptyUserTileUri = ContactUtil.GetResourceUri("UserTileEmpty.bmp");

        }

        private void _NotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Assert.IsNotNull(sender);
            Assert.IsNotNull(e);

            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(sender, e);
            }
        }

        internal UserTile(Photo photo)
        {
            _imageSource = _GetImageFromPhoto(photo);
            _UpdateImage();
        }

        /// <summary>
        /// The image associated with the user tile.
        /// </summary>
        public ImageSource Image
        {
            get
            {
                return _imageComposited;
            }
            private set
            {
                _imageComposited = value;
                _NotifyPropertyChanged(this, new PropertyChangedEventArgs("Image"));
            }
        }

        public ImageSource Overlay
        {
            get
            {
                return _overlay;
            }
            set
            {
                _overlay = value;

                _NotifyPropertyChanged(this, new PropertyChangedEventArgs("Overlay"));
                _UpdateImage();
            }
        }

        private void _UpdateImage()
        {
            Image = _GetFramedImage(_imageSource, _overlay, 96);
        }

        private static FrameMetric _GetMetric(int dimensions)
        {
            if (dimensions <= 0)
            {
                throw new ArgumentOutOfRangeException("dimensions", "The requested dimensions of the frame must be positive.");
            }

            // Only ever scale down the image.
            // Find the smallest frame that matches and then scale to fit.

            // Initially set it to the largest path.
            Assert.AreNotEqual(0, _FrameMetrics.Count);
            FrameMetric frame = _FrameMetrics[0];
            foreach (FrameMetric metric in _FrameMetrics)
            {
                // If the current metric is at least as large as the requested frame we can use it.
                if (metric.Width < dimensions)
                {
                    break;
                }
                frame = metric;
            }

            return frame;
        }

        private static ImageSource _GetImageFromPhoto(Photo photo)
        {
            Stream stm = photo.ResolveToStream();

            if (null == stm)
            {
                // No Value and no useful Url.
                // Just return null and let the caller deal with it.
                return null;
            }

            var image = new BitmapImage();
            image.BeginInit();
            
            // CONSIDER: Don't need to load all of a giant image...
            //    (even though we already have the stream in memory)
            //
            // To keep aspect ratio don't set both width and height.
            // This doesn't guarantee a square, this is facebook behavior.
            //image.DecodePixelWidth = (int)size.Width;

            stm.Position = 0;
            image.StreamSource = stm;
            image.EndInit();

            return image;
        }

        private static Rect _GetScaledRect(Rect frame, double actualWidth, double actualHeight)
        {
            bool tooWide = frame.Width < actualWidth;
            bool tooTall = frame.Height < actualHeight;

            if (!tooTall && !tooWide)
            {
                // No scaling needed, just use the given frame.
                return frame;
            }

            if (tooTall && tooWide)
            {
                // Too big on both ends.  Determine which one we want to bound by.
                if (actualHeight * frame.Width > actualWidth * frame.Height)
                {
                    tooWide = false;
                }
                else
                {
                    tooTall = false;
                }
            }

            double scaledWidth;
            double scaledHeight;

            if (!tooTall)
            {
                scaledWidth = frame.Width;
                scaledHeight = actualHeight * frame.Width / actualWidth;
            }
            else
            {
                Assert.IsFalse(tooWide);
                scaledWidth = actualWidth * frame.Height / actualHeight;
                scaledHeight = frame.Height;
            }

            // If the picture didn't stand up to scaling then just stretch it.
            // Avoid funky edge-cases.
            if (0 >= scaledHeight || 0 >= scaledWidth)
            {
                scaledWidth = frame.Width;
                scaledHeight = frame.Height;
            }

            // Scaling the image.  Need to adjust the offsets of the rectangle also.
            Assert.IsTrue(scaledWidth <= frame.Width);
            double scaledX = frame.X + ((frame.Width - scaledWidth) / 2);

            Assert.IsTrue(scaledHeight <= frame.Height);
            double scaledY = frame.Y + ((frame.Height - scaledHeight) / 2);

            return new Rect(scaledX, scaledY, scaledWidth, scaledHeight);
        }

        private static ImageSource _GetFramedImage(ImageSource usertile, ImageSource overlay, int dimensions)
        {
            FrameMetric metric = _GetMetric(dimensions);
            var drawingGroup = new DrawingGroup();

            var frameDrawing = new ImageDrawing
            {
                Rect = new Rect(0, 0, metric.Width, metric.Height),
                ImageSource = new BitmapImage(metric.Path)
            };

            var tileDrawing = new ImageDrawing();
            if (null == usertile)
            {
                usertile = new BitmapImage(_emptyUserTileUri);
            }

            tileDrawing.Rect = _GetScaledRect(metric.BoundingFrame, usertile.Width, usertile.Height);
            tileDrawing.ImageSource = usertile;

            ImageDrawing overlayDrawing = null;
            if (null != overlay)
            {
                overlayDrawing = new ImageDrawing
                {
                    Rect = (metric.Width > 32
                        ? new Rect(0, metric.Height*2/3, metric.Width*1/3, metric.Height*1/3)
                        : new Rect(0, metric.Height*1/2, metric.Width*1/2, metric.Height*1/2)),
                    ImageSource = overlay
                };
            }


            // The frame has an alpha overlay in the middle.
            // Blit it second to get the bevel effect.
            drawingGroup.Children.Add(tileDrawing);
            drawingGroup.Children.Add(frameDrawing);
            if (null != overlayDrawing)
            {
                drawingGroup.Children.Add(overlayDrawing);
            }

            var compositeImage = new DrawingImage(drawingGroup);
            compositeImage.Freeze();
            return compositeImage;
        }

        public static ImageSource GetFramedPhoto(Photo photo, int dimensions)
        {
            return _GetFramedImage(_GetImageFromPhoto(photo), null, dimensions);
        }

        public static ImageSource GetEmptyFrame(int dimensions)
        {
            FrameMetric frame = _GetMetric(dimensions);

            var image = new BitmapImage();
            image.BeginInit();
            // Even though this should be square, don't set both width and height
            //    to ensure that the image's aspect ratio is respected.
            // Don't scale up the image even if the caller wants something larger.
            Assert.AreEqual(frame.Width, frame.Height);
            image.DecodePixelWidth = Math.Min(dimensions, frame.Width);
            image.UriSource = frame.Path;
            image.EndInit();

            return image;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
