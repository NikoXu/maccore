// 
// CVPixelBuffer.cs: Implements the managed CVPixelBuffer
//
// Authors: Mono Team
//     
// Copyright 2010 Novell, Inc
//
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using MonoMac.CoreFoundation;
using MonoMac.ObjCRuntime;
using MonoMac.Foundation;

namespace MonoMac.CoreVideo {

	[Since (4,0)]
	public class CVPixelBuffer : CVBuffer, INativeObject, IDisposable {
		public static readonly NSString PixelFormatTypeKey;
		public static readonly NSString MemoryAllocatorKey;
		public static readonly NSString WidthKey;
		public static readonly NSString HeightKey;
		public static readonly NSString ExtendedPixelsLeftKey;
		public static readonly NSString ExtendedPixelsTopKey;
		public static readonly NSString ExtendedPixelsRightKey;
		public static readonly NSString ExtendedPixelsBottomKey;
		public static readonly NSString BytesPerRowAlignmentKey;
		public static readonly NSString CGBitmapContextCompatibilityKey;
		public static readonly NSString CGImageCompatibilityKey;
		public static readonly NSString OpenGLCompatibilityKey;
		public static readonly NSString IOSurfacePropertiesKey;
		public static readonly NSString PlaneAlignmentKey;


		static CVPixelBuffer ()
		{
			var handle = Dlfcn.dlopen (Constants.CoreVideoLibrary, 0);
			if (handle == IntPtr.Zero)
				return;
			try {
				PixelFormatTypeKey = Dlfcn.GetStringConstant (handle, "kCVPixelBufferPixelFormatTypeKey");
				MemoryAllocatorKey = Dlfcn.GetStringConstant (handle, "kCVPixelBufferMemoryAllocatorKey");
				WidthKey = Dlfcn.GetStringConstant (handle, "kCVPixelBufferWidthKey");
				HeightKey = Dlfcn.GetStringConstant (handle, "kCVPixelBufferHeightKey");
				ExtendedPixelsLeftKey = Dlfcn.GetStringConstant (handle, "kCVPixelBufferExtendedPixelsLeftKey");
				ExtendedPixelsTopKey = Dlfcn.GetStringConstant (handle, "kCVPixelBufferExtendedPixelsTopKey");
				ExtendedPixelsRightKey = Dlfcn.GetStringConstant (handle, "kCVPixelBufferExtendedPixelsRightKey");
				ExtendedPixelsBottomKey = Dlfcn.GetStringConstant (handle, "kCVPixelBufferExtendedPixelsBottomKey");
				BytesPerRowAlignmentKey = Dlfcn.GetStringConstant (handle, "kCVPixelBufferBytesPerRowAlignmentKey");
				CGBitmapContextCompatibilityKey = Dlfcn.GetStringConstant (handle, "kCVPixelBufferCGBitmapContextCompatibilityKey");
				CGImageCompatibilityKey = Dlfcn.GetStringConstant (handle, "kCVPixelBufferCGImageCompatibilityKey");
				OpenGLCompatibilityKey = Dlfcn.GetStringConstant (handle, "kCVPixelBufferOpenGLCompatibilityKey");
				IOSurfacePropertiesKey = Dlfcn.GetStringConstant (handle, "kCVPixelBufferIOSurfacePropertiesKey");
				PlaneAlignmentKey = Dlfcn.GetStringConstant (handle, "kCVPixelBufferPlaneAlignmentKey");
			}
			finally {
				Dlfcn.dlclose (handle);
			}
		}

		IntPtr handle;

		internal CVPixelBuffer (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				throw new Exception ("Invalid parameters to context creation");

			CVBufferRetain (handle);
			this.handle = handle;
		}

		[Preserve (Conditional=true)]
		internal CVPixelBuffer (IntPtr handle, bool owns)
		{
			if (!owns)
				CVBufferRetain (handle);

			this.handle = handle;
		}

		~CVPixelBuffer ()
		{
			Dispose (false);
		}
		
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public IntPtr Handle {
			get { return handle; }
		}
	
		[DllImport (Constants.CoreVideoLibrary)]
		extern static void CVBufferRelease (IntPtr handle);
		
		[DllImport (Constants.CoreVideoLibrary)]
		extern static void CVBufferRetain (IntPtr handle);
		
		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero){
				CVBufferRelease (handle);
				handle = IntPtr.Zero;
			}
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static CVReturn CVPixelBufferCreate (IntPtr allocator, IntPtr width, IntPtr height, CVPixelFormatType pixelFormatType, IntPtr pixelBufferAttributes, IntPtr pixelBufferOut);
		public CVPixelBuffer (int width, int height, CVPixelFormatType pixelFormatType, NSDictionary pixelBufferAttributes)
		{
			IntPtr pixelBufferOut = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (IntPtr)));
			CVReturn ret = CVPixelBufferCreate (IntPtr.Zero, (IntPtr) width, (IntPtr) height, pixelFormatType, pixelBufferAttributes.Handle, pixelBufferOut);

			if (ret != CVReturn.Success) {
				Marshal.FreeHGlobal (pixelBufferOut);
				throw new Exception ("CVPixelBufferCreate returned: " + ret);
			}

			this.handle = Marshal.ReadIntPtr (pixelBufferOut);
			Marshal.FreeHGlobal (pixelBufferOut);
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static CVReturn CVPixelBufferCreateResolvedAttributesDictionary (IntPtr allocator, IntPtr attributes, IntPtr resolvedDictionaryOut);
		public NSDictionary GetAttributes (NSDictionary [] attributes)
		{
			IntPtr resolvedDictionaryOut = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (IntPtr)));
			NSArray attributeArray = NSArray.FromNSObjects (attributes);
			CVReturn ret = CVPixelBufferCreateResolvedAttributesDictionary (IntPtr.Zero, attributeArray.Handle, resolvedDictionaryOut);

			if (ret != CVReturn.Success) {
				Marshal.FreeHGlobal (resolvedDictionaryOut);
				throw new Exception ("CVPixelBufferCreate returned: " + ret);
			}
			
			NSDictionary dictionary = (NSDictionary) Runtime.GetNSObject (Marshal.ReadIntPtr (resolvedDictionaryOut));
			Marshal.FreeHGlobal (resolvedDictionaryOut);

			return dictionary;
		}

		// TODO: CVPixelBufferCreateWithBytes
		// TODO: CVPixelBufferCreateWithPlanarBytes
		// TODO: CVPixelBufferGetExtendedPixels
		// TODO: CVPixelBufferGetTypeID

		[DllImport (Constants.CoreVideoLibrary)]
		extern static CVReturn CVPixelBufferFillExtendedPixels (IntPtr pixelBuffer);
		public CVReturn FillExtendedPixels ()
		{
			return CVPixelBufferFillExtendedPixels (handle);
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static IntPtr CVPixelBufferGetBaseAddress (IntPtr pixelBuffer);
		public IntPtr BaseAddress {
			get {
				return CVPixelBufferGetBaseAddress (handle);
			}
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static IntPtr CVPixelBufferGetBytesPerRow (IntPtr pixelBuffer);
		public int BytesPerRow {
			get {
				return (int) CVPixelBufferGetBytesPerRow (handle);
			}
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static IntPtr CVPixelBufferGetDataSize (IntPtr pixelBuffer);
		public int DataSize {
			get {
				return (int) CVPixelBufferGetDataSize (handle);
			}
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static IntPtr CVPixelBufferGetHeight (IntPtr pixelBuffer);
		public int Height {
			get {
				return (int) CVPixelBufferGetHeight (handle);
			}
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static IntPtr CVPixelBufferGetWidth (IntPtr pixelBuffer);
		public int Width {
			get {
				return (int) CVPixelBufferGetWidth (handle);
			}
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static IntPtr CVPixelBufferGetPlaneCount (IntPtr pixelBuffer);
		public int PlaneCount {
			get {
				return (int) CVPixelBufferGetPlaneCount (handle);
			}
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static bool CVPixelBufferIsPlanar (IntPtr pixelBuffer);
		public bool IsPlanar {
			get {
				return CVPixelBufferIsPlanar (handle);
			}
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static CVPixelFormatType CVPixelBufferGetPixelFormatType (IntPtr pixelBuffer);
		public CVPixelFormatType PixelFormatType {
			get {
				return CVPixelBufferGetPixelFormatType (handle);
			}
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static IntPtr CVPixelBufferGetBaseAddressOfPlane (IntPtr pixelBuffer, IntPtr planeIndex);
		public IntPtr GetBaseAddress (int planeIndex) {
			return CVPixelBufferGetBaseAddressOfPlane (handle, (IntPtr) planeIndex);
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static IntPtr CVPixelBufferGetBytesPerRowOfPlane (IntPtr pixelBuffer, IntPtr planeIndex);
		public int GetBytesPerRowOfPlane (int planeIndex) {
			return (int) CVPixelBufferGetBytesPerRowOfPlane (handle, (IntPtr) planeIndex);
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static IntPtr CVPixelBufferGetHeightOfPlane (IntPtr pixelBuffer, IntPtr planeIndex);
		public int GetHeightOfPlane (int planeIndex) {
			return (int) CVPixelBufferGetHeightOfPlane (handle, (IntPtr) planeIndex);
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static IntPtr CVPixelBufferGetWidthtOfPlane (IntPtr pixelBuffer, IntPtr planeIndex);
		public int GetWidthtOfPlane (int planeIndex) {
			return (int) CVPixelBufferGetWidthtOfPlane (handle, (IntPtr) planeIndex);
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static CVReturn CVPixelBufferLockBaseAddress (IntPtr pixelBuffer, CVOptionFlags lockFlags);
		public CVReturn Lock (CVOptionFlags lockFlags) {
			return CVPixelBufferLockBaseAddress (handle, lockFlags);
		}

		[DllImport (Constants.CoreVideoLibrary)]
		extern static CVReturn CVPixelBufferUnlockBaseAddress (IntPtr pixelBuffer, CVOptionFlags unlockFlags);
		public CVReturn Unlock (CVOptionFlags unlockFlags) {
			return CVPixelBufferUnlockBaseAddress (handle, unlockFlags);
		}
	}
}
