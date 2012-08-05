using System;
using System.Security.Cryptography;
using System.Text;

namespace HashGenerator
{
	public partial class hashgen
	{
		static string Hash (string input, string type)
		{
			byte[] bytes = Encoding.UTF8.GetBytes (input);
			string output = null;
			
			switch (type) {
			case "MD5":
				byte[] a0 = MD5.Create ().ComputeHash (bytes);
				StringBuilder sb = new StringBuilder ();
				
				for (int i = 0; i < a0.Length; i++) {
					sb.Append (a0 [i].ToString ("X2").ToLower ());
				}
				output = sb.ToString ();
				break;
				
			case "SHA128":
				byte[] a1 = SHA1.Create ().ComputeHash (bytes);
				
				for (int j = 0; j < a1.Length; j++) {
					byte b0 = a1 [j];
					output = output + ((b0.ToString ("X").ToLower ().Length != 1) ? "" : "0") + b0.ToString ("X").ToLower ();
				}
				break;
				
			case "SHA256":
				byte[] a2 = SHA256.Create ().ComputeHash (bytes);
				
				for (int k = 0; k < a2.Length; k++) {
					byte b1 = a2 [k];
					output += string.Format ("{0:x2}", b1);
				}
				break;
				
			case "SHA512":
				byte[] a3 = SHA512.Create ().ComputeHash (bytes);
				
				for (int l = 0; l < a3.Length; l++) {
					byte b2 = a3 [l];
					output += string.Format ("{0:x2}", b2);
				}
				break;
				
			case "CRC32":
				char[] a4 = input.ToCharArray ();
				for (int m = 0; m < a4.Length; m++) {
					if (a4 [m] <= '\u007f') {
						a4 [m] = char.ToLowerInvariant (a4 [m]);
					}
				}
				
				string s = new string (a4);
				uint n = 4294967295u;
				byte[] a5 = Encoding.UTF8.GetBytes (s);
				for(int y = 0; y < a5.Length; y++) {
					byte b3 = a5[y];
					n ^= (uint)((uint)b3 << 24);
					for(int y0 = 0; y0 < 8; y0++) {
						if((Convert.ToUInt32 (n) & 2147483648u) == 2147483648u)	{
							n = ( n << 1 ^ 79764919u);
						} 
						else {
							n <<= 1;
						}
					}
				}
				output = string.Format ("{0:x8}", n);
				break;					
				
			default:
				break;
			}
			
			return output;
		}
	}
}

