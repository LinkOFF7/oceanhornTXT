using System;
using System.IO;
using zlib;

namespace oceanhornTXT
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "-d")
                {
                    DecryptTXT(args[1]);
                }
                else if (args[0] == "-e")
                {
                    EncryptTXT(args[1], args[1] + ".txt");
                }
            }
            else
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("-d       Decrypt TXT file");
                Console.WriteLine("-e       Encrypt text file into game TXT");
            }
        }

        static void EncryptTXT(string inputFile, string outputFile)
        {
            var inFileName = File.ReadAllBytes(inputFile);
            if (File.Exists(outputFile)) File.Delete(outputFile);
            var outFileName = File.OpenWrite(outputFile);
            using (var writer = new BinaryWriter(outFileName))
            {
                byte[] cData = CompressZlib(inFileName);
                writer.Write(inFileName.Length);
                writer.Write(cData);
            }
        }

        static void DecryptTXT(string inputFile)
        {
            var fileName = File.OpenRead(inputFile);
            using (var reader = new BinaryReader(fileName))
            {
                var uSize = reader.ReadInt32();
                if (uSize < 0)
                {
                    reader.BaseStream.Seek(0x02, SeekOrigin.Begin); //skip first 2 bytes. Based on example i've get.
                    uSize = reader.ReadInt32();
                    var data = reader.ReadBytes((int)(fileName.Length - 4));
                    byte[] uncArray = DecompressZlib(data);
                    File.WriteAllBytes(Path.GetFileNameWithoutExtension(inputFile) + ".dec.txt", uncArray);
                }
                else
                {
                    var data = reader.ReadBytes((int)(fileName.Length - 4));
                    byte[] uncArray = DecompressZlib(data);
                    File.WriteAllBytes(Path.GetFileNameWithoutExtension(inputFile) + ".dec.txt", uncArray);
                }
            }
        }

        static byte[] CompressZlib(byte[] input)
        {
            using (var outMS = new MemoryStream())
            using (var outZStream = new ZOutputStream(outMS, 8)) //8 - compression level, similar to original size
            using (Stream inMemoryStream = new MemoryStream(input))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                byte[] output = outMS.ToArray();
                return output;
            }
        }

        static byte[] DecompressZlib(byte[] input)
        {
            using (var outMS = new MemoryStream())
            using (var outZStream = new ZOutputStream(outMS))
            using (Stream inMemoryStream = new MemoryStream(input))
            {
                CopyStream(inMemoryStream, outZStream);
                outZStream.finish();
                byte[] output = outMS.ToArray();
                return output;
            }
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }
    }
}
