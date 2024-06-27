using K4os.Compression.LZ4;
using System.Text;

namespace BGEModTools
{
    internal partial class Program
    {
        static void Main(string[] args)
        {
            var input = "";
            var minSize = 0;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-i" && args[i + 1] != null)
                {
                    if (File.Exists(args[i + 1]))
                    {
                        input = args[i + 1];
                    }
                }

                if (args[i] == "-minsize" && args[i + 1] != null)
                {
                    if (int.TryParse(args[i + 1], out _))
                    {
                        minSize = int.Parse(args[i + 1]);
                    }
                }
            }

            if(input == "")
            {
                Console.WriteLine("ERROR: PAK not found");
                return;
            }

            FileFormat[] formats = { new FileFormat(".png"     , [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
                                     new FileFormat(".gao"     , [0x2E, 0x67, 0x61, 0x6F]),
                                     new FileFormat(".fontdesc", [0x46, 0x4F, 0x4E, 0x54, 0x44, 0x45, 0x53, 0x43]),
                                     new FileFormat(".fnt"     , [0x69, 0x6E, 0x66, 0x6F, 0x20, 0x66, 0x61, 0x63, 0x65]),
                                     new FileFormat(".wem"     , [0x52, 0x49, 0x46, 0x46])};

            var fs = File.OpenRead(input);

            var br = new BinaryReader(fs);
            br.ReadBytes(16);

            var fileNumber = br.ReadUInt32();
            var indexSize = br.ReadUInt32();

            br.BaseStream.Position = fs.Length - indexSize;

            var pakFiles = new PakFile[fileNumber];

            for (int i = 0; i < fileNumber; i++)
            {
                var hasName = br.ReadByte();

                string fileName = "";

                if (hasName == 0)
                {
                    var nameLength = br.ReadInt32();
                    fileName = Encoding.UTF8.GetString(br.ReadBytes(nameLength));
                }
                else if (hasName == 1)
                {
                    fileName = br.ReadUInt32().ToString("X4") + ".bin"; // Hash?
                }
                else
                {
                    throw new Exception("ERROR: Unknown value when reading file index!");
                }

                var uncompressedFileSize = br.ReadUInt32();
                var compressedFileSize = br.ReadUInt32();
                br.ReadUInt64(); // Unknown data
                var fileOffset = br.ReadUInt64() + 24UL;

                pakFiles[i] = new PakFile(fileName, compressedFileSize, uncompressedFileSize, fileOffset);
            }

            foreach (var pakFile in pakFiles)
            {
                if (pakFile.uncompressedSize < minSize)
                {
                    continue;
                }

                br.BaseStream.Position = (long)pakFile.offset;

                var uncompressedBytes = new byte[pakFile.uncompressedSize];

                if (pakFile.compressedSize == 0 && pakFile.uncompressedSize > 0)
                {
                    uncompressedBytes = br.ReadBytes((int)pakFile.uncompressedSize);
                }
                else
                {
                    var compressedBytes = br.ReadBytes((int)pakFile.compressedSize);
                    var uncompressedSize = LZ4Codec.Decode(compressedBytes, uncompressedBytes);

                    if (uncompressedSize != uncompressedBytes.Length)
                    {
                        Console.WriteLine($"ERROR : Failed decompressing [{pakFile.name}]");
                        continue;
                    }
                }

                var extension = Path.GetExtension(pakFile.name);

                if (extension == ".bin")
                {
                    var identified = false;

                    foreach (var format in formats)
                    {
                        if (uncompressedBytes.Length >= format.header.Length && uncompressedBytes.Take(format.header.Length).SequenceEqual(format.header))
                        {
                            pakFile.name = Path.ChangeExtension(pakFile.name, format.extension);
                            extension = format.extension;
                            identified = true;
                        }
                    }

                    if (!identified)
                    {
                        // File could not be identified. Safe as BIN for investigation
                        pakFile.name = Path.ChangeExtension(pakFile.name, ".bin");
                    }
                }

                var path = $"Output/{extension.Replace(".", "")}/{pakFile.name}";

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    Console.WriteLine($"Unpacking [{pakFile.name}]");

                    if (!File.Exists(path))
                    {
                        File.WriteAllBytes(path, uncompressedBytes);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            br.Close();
            Console.WriteLine("Complete");
        }
    }
}
