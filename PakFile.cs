namespace BGEModTools
{
    class PakFile
    {
        public string name;
        public uint compressedSize;
        public uint uncompressedSize;
        public ulong offset;

        public PakFile(string fileName, uint size, uint uncompressedSize, ulong offset)
        {
            this.name = fileName;
            this.compressedSize = size;
            this.uncompressedSize = uncompressedSize;
            this.offset = offset;
        }
    }
}
