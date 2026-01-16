namespace Marblin.Core.Constants
{
    public static class FileConstants
    {
        public static readonly Dictionary<string, List<byte[]>> FileSignatures = new Dictionary<string, List<byte[]>>
        {
            { ".jpeg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
            { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } }
        };
    }
}
