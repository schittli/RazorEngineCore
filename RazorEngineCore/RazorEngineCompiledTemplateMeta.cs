using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RazorEngineCore
{
    public class RazorEngineCompiledTemplateMeta
    {
        public byte[]? AssemblyByteCode { get; set; }

        public byte[]? PdbByteCode { get; set; }

        public string? GeneratedSourceCode { get; set; }

        public required string TemplateNamespace { get; set; }

        public string? TemplateSource { get; set; }

        public string? TemplateFileName { get; set; }

        public async Task WriteAsync(Stream stream)
        {
            await stream.WriteLong(10001);

            await WriteBufferAsync(stream, AssemblyByteCode);
            await WriteBufferAsync(stream, PdbByteCode);
            await WriteStringAsync(stream, GeneratedSourceCode);
            await WriteStringAsync(stream, TemplateSource);
            await WriteStringAsync(stream, TemplateNamespace);
            await WriteStringAsync(stream, TemplateFileName);
        }

        public static async Task<RazorEngineCompiledTemplateMeta> ReadAsync(Stream stream)
        {
            long version = await stream.ReadLong();

            if (version == 10001)
            {
                return await LoadVersion1(stream);
            }

            // Exception with unqiue ID
            throw new RazorEngineException("2qdiln Unable to load template: wrong version");
        }

        private static async Task<RazorEngineCompiledTemplateMeta> LoadVersion1(Stream stream)
        {
            return new RazorEngineCompiledTemplateMeta()
            {
                AssemblyByteCode = await ReadBufferAsync(stream),
                PdbByteCode = await ReadBufferAsync(stream),
                GeneratedSourceCode = await ReadStringAsync(stream),
                TemplateSource = await ReadStringAsync(stream),
                TemplateNamespace = await ReadStringAsync(stream) ?? "TemplateNamespace",
                TemplateFileName = await ReadStringAsync(stream),
            };
        }

        private Task WriteStringAsync(Stream stream, string? value)
        {
            var buffer = value == null ? null : Encoding.UTF8.GetBytes(value);
            return WriteBufferAsync(stream, buffer);
        }

        private async Task WriteBufferAsync(Stream stream, byte[]? buffer)
        {
            if (buffer == null)
            {
                await stream.WriteLong(0);
                return;
            }

            await stream.WriteLong(buffer.Length);
            await stream.WriteAsync(buffer);
        }

        private static async Task<string?> ReadStringAsync(Stream stream)
        {
            var buffer = await ReadBufferAsync(stream);
            return buffer == null ? null : Encoding.UTF8.GetString(buffer);
        }

        private static async Task<byte[]?> ReadBufferAsync(Stream stream)
        {
            long length = await stream.ReadLong();

            if (length == 0)
            {
                return null;
            }

            byte[] buffer = new byte[length];
            _ = await stream.ReadAsync(buffer);
            return buffer;
        }
    }
}
