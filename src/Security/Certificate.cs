// 
// Certificate.cs: Implements the managed SecCertificate wrapper.
//
// Authors: 
//	Miguel de Icaza
//  Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2010 Novell, Inc
// Copyright 2012 Xamarin Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using MonoMac.ObjCRuntime;
using MonoMac.CoreFoundation;
using MonoMac.Foundation;

namespace MonoMac.Security {

	public class SecCertificate : INativeObject, IDisposable {
		internal IntPtr handle;
		
		// invoked by marshallers
		internal SecCertificate (IntPtr handle)
			: this (handle, false)
		{
		}
		
		[Preserve (Conditional = true)]
		internal SecCertificate (IntPtr handle, bool owns)
		{
			if (handle == IntPtr.Zero)
				throw new Exception ("Invalid handle");

			this.handle = handle;
			if (!owns)
				CFType.Retain (handle);
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static uint SecCertificateGetTypeID ();

		public static uint TypeID {
			get { return SecCertificateGetTypeID (); }
		}
			
		[DllImport (Constants.SecurityLibrary)]
		extern static IntPtr SecCertificateCreateWithData (IntPtr allocator, IntPtr cfData);

		public SecCertificate (NSData data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			Initialize (data);
		}

		public SecCertificate (byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			using (NSData cert = NSData.FromArray (data)) {
				Initialize (cert);
			}
		}

		public SecCertificate (X509Certificate certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			using (NSData cert = NSData.FromArray (certificate.GetRawCertData ())) {
				Initialize (cert);
			}
		}

		public SecCertificate (X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException ("certificate");

			using (NSData cert = NSData.FromArray (certificate.RawData)) {
				Initialize (cert);
			}
		}

		void Initialize (NSData data)
		{
			handle = SecCertificateCreateWithData (IntPtr.Zero, data.Handle);
			if (handle == IntPtr.Zero)
				throw new ArgumentException ("Not a valid DER-encoded X.509 certificate");
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static IntPtr SecCertificateCopySubjectSummary (IntPtr cert);

		public string SubjectSummary {
			get {
				if (handle == IntPtr.Zero)
					throw new ObjectDisposedException ("SecCertificate");
				
				IntPtr cfstr = SecCertificateCopySubjectSummary (handle);
				string ret = CFString.FetchString (cfstr);
				CFType.Release (cfstr);
				return ret;
			}
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static IntPtr SecCertificateCopyData (IntPtr cert);

		public NSData DerData {
			get {
				if (handle == IntPtr.Zero)
					throw new ObjectDisposedException ("SecCertificate");

				IntPtr data = SecCertificateCopyData (handle);
				if (data == IntPtr.Zero)
					throw new ArgumentException ("Not a valid certificate");
				return new NSData (data);
			}
		}
		
		~SecCertificate ()
		{
			Dispose (false);
		}

		public IntPtr Handle {
			get {
				return handle;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero){
				CFType.Release (handle);
				handle = IntPtr.Zero;
			}
		}
	}

	public class SecIdentity : CFType {
		// invoked by marshallers
		internal SecIdentity (IntPtr handle)
			: this (handle, false)
		{
		}
		
		[Preserve (Conditional = true)]
		internal SecIdentity (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static uint SecIdentityGetTypeID ();

		public static uint TypeID {
			get { return SecIdentityGetTypeID (); }
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static IntPtr SecIdentityCopyCertificate (IntPtr handle, out IntPtr cert);

		public SecCertificate Certificate {
			get {
				ThrowIfDisposed ();
				IntPtr cert;
				SecIdentityCopyCertificate (Handle, out cert);
				return new SecCertificate (Handle, true);
			}
		}

		~SecIdentity ()
		{
			Dispose (false);
		}
	}

	public class SecKey : CFType {
		// invoked by marshallers
		public SecKey (IntPtr handle)
			: this (handle, false)
		{
		}
		
		[Preserve (Conditional = true)]
		public SecKey (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static uint SecKeyGetTypeID ();

		public static uint TypeID {
			get { return SecKeyGetTypeID (); }
		}
		
		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode SecKeyGeneratePair (IntPtr dictHandle, out IntPtr pubKey, out IntPtr privKey);

		// TODO: pull all the TypeRefs needed for the NSDictionary
		
		public static SecStatusCode GenerateKeyPair (NSDictionary parameters, out SecKey publicKey, out SecKey privateKey)
		{
			if (parameters == null)
				throw new ArgumentNullException ("parameters");

			IntPtr pub, priv;
			
			var res = SecKeyGeneratePair (parameters.Handle, out pub, out priv);
			if (res == SecStatusCode.Success){
				publicKey = new SecKey (pub, true);
				privateKey = new SecKey (priv, true);
			} else
				publicKey = privateKey = null;
			return res;
		}
			
		[DllImport (Constants.SecurityLibrary)]
		extern static IntPtr SecKeyGetBlockSize (IntPtr handle);

		long BlockSize {
			get {
				ThrowIfDisposed ();
				return (long) SecKeyGetBlockSize (Handle);
			}
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode SecKeyRawSign (IntPtr handle, SecPadding padding, IntPtr dataToSign, IntPtr dataToSignLen, IntPtr sig, IntPtr sigLen);

		SecStatusCode RawSign (SecPadding padding, IntPtr dataToSign, int dataToSignLen, out byte [] result)
		{
			ThrowIfDisposed ();
			result = new byte [(int) SecKeyGetBlockSize (Handle)];
			unsafe {
				fixed (byte *p = &result [0])
					return SecKeyRawSign (Handle, padding, dataToSign, (IntPtr) dataToSignLen, (IntPtr) p, (IntPtr)result.Length);
			}
		}

		SecStatusCode RawSign (SecPadding padding, byte [] dataToSign, out byte [] result)
		{
			ThrowIfDisposed ();
			if (dataToSign == null)
				throw new ArgumentNullException ("dataToSign");

			result = new byte [(int) SecKeyGetBlockSize (Handle)];
			unsafe {
				fixed (byte *bp = &dataToSign [0]){
					fixed (byte *p = &result [0])
						return SecKeyRawSign (Handle, padding, (IntPtr) bp, (IntPtr)dataToSign.Length, (IntPtr) p, (IntPtr) result.Length);
				}
			}
		}
		
		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode SecKeyRawVerify (IntPtr handle, SecPadding padding, IntPtr signedData, IntPtr signedLen, IntPtr sign, IntPtr signLen);
		public unsafe SecStatusCode RawVerify (SecPadding padding, IntPtr signedData, int signedDataLen, IntPtr signature, int signatureLen)
		{
			ThrowIfDisposed ();
			return SecKeyRawVerify (Handle, padding, signedData, (IntPtr) signedDataLen, signature, (IntPtr) signatureLen);
		}

		public SecStatusCode RawVerify (SecPadding padding, byte [] signedData, byte [] signature)
		{
			ThrowIfDisposed ();
			if (signature == null)
				throw new ArgumentNullException ("signature");
			if (signedData == null)
				throw new ArgumentNullException ("signedData");
			unsafe {
				fixed (byte *sp = &signature[0]){
					fixed (byte *dp = &signedData [0]){
						return SecKeyRawVerify (Handle, padding, (IntPtr) dp, (IntPtr) signedData.Length, (IntPtr) sp, (IntPtr) signature.Length);
					}
				}
			}
		}
		
		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode SecKeyEncrypt (IntPtr handle, SecPadding padding, IntPtr plainText, IntPtr playLen, IntPtr cipherText, IntPtr cipherLen);
		public unsafe SecStatusCode Encrypt (SecPadding padding, IntPtr plainText, int playLen, IntPtr cipherText, int cipherLen)
		{
			ThrowIfDisposed ();
			return SecKeyEncrypt (Handle, padding, plainText, (IntPtr) playLen, cipherText, (IntPtr) cipherLen);
		}

		public SecStatusCode Encrypt (SecPadding padding, byte [] plainText, byte [] cipherText)
		{
			ThrowIfDisposed ();
			if (cipherText == null)
				throw new ArgumentNullException ("cipherText");
			if (plainText == null)
				throw new ArgumentNullException ("plainText");
			unsafe {
				fixed (byte *cp = &cipherText[0]){
					fixed (byte *pp = &plainText [0]){
						return SecKeyEncrypt (Handle, padding, (IntPtr) pp, (IntPtr) plainText.Length, (IntPtr) cp, (IntPtr) cipherText.Length);
					}
				}
			}
		}

		[DllImport (Constants.SecurityLibrary)]
		extern static SecStatusCode SecKeyDecrypt (IntPtr handle, SecPadding padding, IntPtr cipherText, IntPtr cipherLen, IntPtr plainText, IntPtr playLen);
		
		public unsafe SecStatusCode Decrypt (SecPadding padding, IntPtr cipherText, int cipherLen, IntPtr plainText, int playLen)
		{
			ThrowIfDisposed ();
			return SecKeyDecrypt (Handle, padding, cipherText, (IntPtr) cipherLen, plainText, (IntPtr) playLen);
		}

		public SecStatusCode Decrypt (SecPadding padding, byte [] cipherText, byte [] plainText)
		{
			ThrowIfDisposed ();
			if (cipherText == null)
				throw new ArgumentNullException ("cipherText");
			if (plainText == null)
				throw new ArgumentNullException ("plainText");
			unsafe {
				fixed (byte *cp = &cipherText[0]){
					fixed (byte *pp = &plainText [0]){
						return SecKeyDecrypt (Handle, padding, (IntPtr) cp, (IntPtr) cipherText.Length, (IntPtr) pp, (IntPtr) plainText.Length);
					}
				}
			}
		}
		
		~SecKey ()
		{
			Dispose (false);
		}
	}
	
}