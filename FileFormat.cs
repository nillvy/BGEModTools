namespace BGEModTools
{
    class FileFormat
    {
        public byte[] header;
        public string extension;

        public FileFormat(string extension, byte[] header)
        {
            this.header = header;
            this.extension = extension;
        }
    }
}
