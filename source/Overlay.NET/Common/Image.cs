using System;
using System.IO;

using SharpDX.WIC;
using SharpDX.Direct2D1;

using Bitmap = SharpDX.Direct2D1.Bitmap;
using PixelFormat = SharpDX.WIC.PixelFormat;
using System.Drawing;
using System.Text;
using System.Collections.Generic;

namespace Overlay.NET.Common
{
	/// <summary>
	/// Represents an Image which can be drawn using a Graphics surface.
	/// </summary>
	public class Image : IDisposable
	{
		internal static readonly ImagingFactory ImageFactory = new ImagingFactory();

		/// <summary>
		/// The SharpDX Bitmap
		/// </summary>
		public Bitmap Bitmap;

		/// <summary>
		/// Gets the width of this Image
		/// </summary>
		public float Width => Bitmap.PixelSize.Width;

		/// <summary>
		/// Gets the height of this Image
		/// </summary>
		public float Height => Bitmap.PixelSize.Height;

		private Image()
		{
		}

		/// <summary>
		/// Initializes a new Image for the given device by using a byte[].
		/// </summary>
		/// <param name="device">The Graphics device.</param>
		/// <param name="bytes">A byte[] containing image data.</param>
		public Image(RenderTarget device, byte[] bytes)
			=> Bitmap = LoadBitmapFromMemory(device, bytes);

		/// <summary>
		/// Initializes a new Image for the given device by using a file on disk.
		/// </summary>
		/// <param name="device">The Graphics device.</param>
		/// <param name="path">The path to an image file on disk.</param>
		public Image(RenderTarget device, string path)
			=> Bitmap = LoadBitmapFromFile(device, path);

		///// <summary>
		///// Initializes a new Image for the given device by using a byte[].
		///// </summary>
		///// <param name="device">The Graphics device.</param>
		///// <param name="bytes">A byte[] containing image data.</param>
		//public Image(Graphics device, byte[] bytes) : this(device, bytes)
		//{
		//}

		///// <summary>
		///// Initializes a new Image for the given device by using a file on disk.
		///// </summary>
		///// <param name="device">The Graphics device.</param>
		///// <param name="path">The path to an image file on disk.</param>
		//public Image(Graphics device, string path) : this(device.GetRenderTarget(), path)
		//{
		//}

		/// <summary>
		/// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
		/// </summary>
		~Image() => Dispose(false);

		/// <summary>
		/// Returns a value indicating whether this instance and a specified <see cref="T:System.Object" /> represent the same type and value.
		/// </summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> is a Image and equal to this instance; otherwise, <see langword="false" />.</returns>
		public override bool Equals(object obj)
		{
			if (obj is Image image)
			{
				return image.Bitmap.NativePointer == Bitmap.NativePointer;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Returns a value indicating whether two specified instances of Image represent the same value.
		/// </summary>
		/// <param name="value">An object to compare to this instance.</param>
		/// <returns><see langword="true" /> if <paramref name="value" /> is equal to this instance; otherwise, <see langword="false" />.</returns>
		public bool Equals(Image value)
		{
			return value != null
				&& value.Bitmap.NativePointer == Bitmap.NativePointer;
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode()
		{
			return OverrideHelper.HashCodes(
				Bitmap.NativePointer.GetHashCode());
		}

		/// <summary>
		/// Converts this Image instance to a human-readable string.
		/// </summary>
		/// <returns>A string representation of this Image.</returns>
		public override string ToString()
		{
			return OverrideHelper.ToString(
				"Image", "Bitmap",
				"Width", Width.ToString(),
				"Height", Height.ToString(),
				"PixelFormat", Bitmap.PixelFormat.Format.ToString());
		}

		#region IDisposable Support
		private bool disposedValue = false;

		/// <summary>
		/// Releases all resources used by this Image.
		/// </summary>
		/// <param name="disposing">A Boolean value indicating whether this is called from the destructor.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				Bitmap?.Dispose();

				disposedValue = true;
			}
		}

		/// <summary>
		/// Releases all resources used by this Image.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		/// <summary>
		/// Converts an Image to a SharpDX Bitmap.
		/// </summary>
		/// <param name="image">The Image object.</param>
		public static implicit operator Bitmap(Image image)
		{
			if (image == null) throw new ArgumentNullException(nameof(image));

			return image.Bitmap;
		}

		/// <summary>
		/// Returns a value indicating whether two specified instances of Image represent the same value.
		/// </summary>
		/// <param name="left">The first object to compare.</param>
		/// <param name="right">The second object to compare.</param>
		/// <returns> <see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, <see langword="false" />.</returns>
		public static bool Equals(Image left, Image right)
		{
			return left?.Equals(right) == true;
		}

		private static Bitmap LoadBitmapFromMemory(RenderTarget device, byte[] bytes)
		{
			if (device == null) throw new ArgumentNullException(nameof(device));
			if (bytes == null) throw new ArgumentNullException(nameof(bytes));
			if (bytes.Length == 0) throw new ArgumentOutOfRangeException(nameof(bytes));

			Bitmap bmp = null;
			MemoryStream stream = null;
			BitmapDecoder decoder = null;
			BitmapFrameDecode frame = null;
			FormatConverter converter = null;

			try
			{
				stream = new MemoryStream(bytes);
				decoder = new BitmapDecoder(ImageFactory, stream, DecodeOptions.CacheOnDemand);

				bmp = ImageDecoder.Decode(device, decoder);

				decoder.Dispose();
				stream.Dispose();

				return bmp;
			}
			catch
			{
				if (converter?.IsDisposed == false) converter.Dispose();
				if (frame?.IsDisposed == false) frame.Dispose();
				if (decoder?.IsDisposed == false) decoder.Dispose();
				if (stream != null) TryCatch(() => stream.Dispose());
				if (bmp?.IsDisposed == false) bmp.Dispose();

				throw;
			}
		}

		private static Bitmap LoadBitmapFromFile(RenderTarget device, string path) => LoadBitmapFromMemory(device, File.ReadAllBytes(path));

		private static void TryCatch(Action action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			try
			{
				action();
			}
			catch { }
		}
	}

	internal static class OverrideHelper
	{
		public static int HashCodes(params int[] hashCodes)
		{
			if (hashCodes == null) throw new ArgumentNullException(nameof(hashCodes));
			if (hashCodes.Length == 0) throw new ArgumentOutOfRangeException(nameof(hashCodes));

			unchecked
			{
				int hash = 17;

				foreach (int code in hashCodes)
				{
					hash = (hash * 23) + code;
				}

				return hash;
			}
		}

		public static string ToString(params string[] strings)
		{
			if (strings == null) throw new ArgumentNullException(nameof(strings));
			if (strings.Length == 0 || strings.Length % 2 != 0) throw new ArgumentOutOfRangeException(nameof(strings));

			StringBuilder sb = new StringBuilder(16);

			sb.Append("{ ");

			for (int i = 0; i < strings.Length - 1; i += 2)
			{
				string name = strings[i];
				string value = strings[i + 1];

				if (name == null)
				{
					if (value == null)
					{
						sb.Append("null");
					}
					else
					{
						sb.Append(value);
					}
				}
				else if (value == null)
				{
					sb.Append(name).Append(": null");
				}
				else
				{
					sb.Append(name).Append(": ").Append(value);
				}

				sb.Append(", ");
			}

			sb.Length -= 2;

			sb.Append(" }");

			return sb.ToString();
		}
	}

	internal static class ImageDecoder
	{
		private static readonly Guid[] _floatingPointFormats = new Guid[]
		{
			PixelFormat.Format128bppRGBAFloat,
			PixelFormat.Format128bppRGBAFixedPoint,
			PixelFormat.Format128bppPRGBAFloat,
			PixelFormat.Format128bppRGBFloat,
			PixelFormat.Format128bppRGBFixedPoint,
			PixelFormat.Format96bppRGBFixedPoint,
			PixelFormat.Format96bppRGBFloat,
			PixelFormat.Format64bppBGRAFixedPoint,
			PixelFormat.Format64bppRGBAFixedPoint,
			PixelFormat.Format64bppRGBFixedPoint,
			PixelFormat.Format48bppRGBFixedPoint,
			PixelFormat.Format48bppBGRFixedPoint,
			PixelFormat.Format32bppGrayFixedPoint,
			PixelFormat.Format32bppGrayFloat,
			PixelFormat.Format16bppGrayFixedPoint
		};

		// PixelFormat sorted in a best compatibility and best color accuracy order
		private static readonly Guid[] _standardPixelFormats = new Guid[]
		{
			PixelFormat.Format144bpp8ChannelsAlpha,
			PixelFormat.Format128bpp8Channels,
			PixelFormat.Format128bpp7ChannelsAlpha,
			PixelFormat.Format112bpp7Channels,
			PixelFormat.Format112bpp6ChannelsAlpha,
			PixelFormat.Format96bpp6Channels,
			PixelFormat.Format96bpp5ChannelsAlpha,
			PixelFormat.Format80bpp5Channels,
			PixelFormat.Format80bppCMYKAlpha,
			PixelFormat.Format80bpp4ChannelsAlpha,
			PixelFormat.Format72bpp8ChannelsAlpha,
			PixelFormat.Format64bppBGRA,
			PixelFormat.Format64bppRGBA,
			PixelFormat.Format64bppPBGRA,
			PixelFormat.Format64bppPRGBA,
			PixelFormat.Format64bpp8Channels,
			PixelFormat.Format64bpp4Channels,
			PixelFormat.Format64bppRGBAHalf,
			PixelFormat.Format64bppPRGBAHalf,
			PixelFormat.Format64bpp7ChannelsAlpha,
			PixelFormat.Format64bpp3ChannelsAlpha,
			PixelFormat.Format64bppRGB,
			PixelFormat.Format64bppCMYK,
			PixelFormat.Format64bppRGBHalf,
			PixelFormat.Format56bpp7Channels,
			PixelFormat.Format56bpp6ChannelsAlpha,
			PixelFormat.Format48bpp6Channels,
			PixelFormat.Format48bppRGB,
			PixelFormat.Format48bppBGR,
			PixelFormat.Format48bpp3Channels,
			PixelFormat.Format48bppRGBHalf,
			PixelFormat.Format48bpp5ChannelsAlpha,
			PixelFormat.Format40bpp5Channels,
			PixelFormat.Format40bppCMYKAlpha,
			PixelFormat.Format40bpp4ChannelsAlpha,
			PixelFormat.Format32bppBGRA,
			PixelFormat.Format32bppRGBA,
			PixelFormat.Format32bppPBGRA,
			PixelFormat.Format32bppPRGBA,
			PixelFormat.Format32bppRGBA1010102,
			PixelFormat.Format32bppRGBA1010102XR,
			PixelFormat.Format32bppCMYK,
			PixelFormat.Format32bpp4Channels,
			PixelFormat.Format32bpp3ChannelsAlpha,
			PixelFormat.Format32bppBGR,
			PixelFormat.Format32bppRGB,
			PixelFormat.Format32bppRGBE,
			PixelFormat.Format32bppBGR101010,
			PixelFormat.Format24bppBGR,
			PixelFormat.Format24bppRGB,
			PixelFormat.Format24bpp3Channels,
			PixelFormat.Format16bppBGR555,
			PixelFormat.Format16bppBGR565,
			PixelFormat.Format16bppBGRA5551,
			PixelFormat.Format16bppGray,
			PixelFormat.Format16bppGrayHalf,
			PixelFormat.Format16bppCbCr,
			PixelFormat.Format16bppYQuantizedDctCoefficients,
			PixelFormat.Format16bppCbQuantizedDctCoefficients,
			PixelFormat.Format16bppCrQuantizedDctCoefficients,
			PixelFormat.Format8bppIndexed,
			PixelFormat.Format8bppAlpha,
			PixelFormat.Format8bppY,
			PixelFormat.Format8bppCb,
			PixelFormat.Format8bppCr,
			PixelFormat.Format8bppGray
		};

		private static readonly Guid[] _uncommonFormats = new Guid[]
		{
			PixelFormat.Format4bppIndexed,
			PixelFormat.Format2bppIndexed,
			PixelFormat.Format1bppIndexed,
			PixelFormat.Format4bppGray,
			PixelFormat.Format2bppGray,
			PixelFormat.FormatDontCare,
			PixelFormat.FormatBlackWhite
		};

		private static IEnumerable<Guid> _pixelFormatEnumerator
		{
			get
			{
				foreach (var format in _standardPixelFormats)
				{
					yield return format;
				}

				foreach (var format in _floatingPointFormats)
				{
					yield return format;
				}

				foreach (var format in _uncommonFormats)
				{
					yield return format;
				}
			}
		}

		private static void TryCatch(Action action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			try
			{
				action();
			}
			catch { }
		}

		public static Bitmap Decode(RenderTarget device, BitmapDecoder decoder)
		{
			var frame = decoder.GetFrame(0);
			var converter = new FormatConverter(Image.ImageFactory);

			foreach (var format in _pixelFormatEnumerator)
			{
				try
				{
					converter.Initialize(frame, format);

					var bmp = Bitmap.FromWicBitmap(device, converter);

					TryCatch(() => converter.Dispose());
					TryCatch(() => frame.Dispose());

					return bmp;
				}
				catch
				{
					TryCatch(() => converter.Dispose());
					converter = new FormatConverter(Image.ImageFactory);
				}
			}

			TryCatch(() => converter.Dispose());
			TryCatch(() => frame.Dispose());

			throw new Exception("Unsupported Image Format!");
		}
	}

	internal static class ThrowHelper
	{
		public static InvalidOperationException DeviceNotInitialized()
		{
			return new InvalidOperationException("The DirectX device is not initialized");
		}

		public static InvalidOperationException UseBeginScene()
		{
			return new InvalidOperationException("Use BeginScene before drawing anything");
		}
	}
}
