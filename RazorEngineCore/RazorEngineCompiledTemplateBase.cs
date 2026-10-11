using System;
using System.IO;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;


namespace RazorEngineCore {

   public abstract class RazorEngineCompiledTemplateBase {

      protected RazorEngineCompiledTemplateMeta Meta              { get; }
      protected Type                            TemplateType      { get; }
      protected bool                            IsDebuggerEnabled { get; set; }


      internal RazorEngineCompiledTemplateBase(RazorEngineCompiledTemplateMeta meta) {
         Meta = meta;

         var assemblyStream = new MemoryStream(meta.AssemblyByteCode ?? []);
         var pdbStream      = meta.PdbByteCode != null ? new MemoryStream(meta.PdbByteCode) : null;
         var assembly       = AssemblyLoadContext.Default.LoadFromStream(assemblyStream, pdbStream);

         // Exception with unqiue ID
         TemplateType = assembly.GetType
                           (meta.TemplateNamespace + ".Template")
                        ?? throw new Exception("2qdilk Failed to load template type");
      }


      public void SaveToFile(string fileName) { SaveToFileAsync(fileName).GetAwaiter().GetResult(); }


      public async Task SaveToFileAsync(string fileName) {
         await using var fileStream = new FileStream
            (
             path: fileName
             , mode: FileMode.OpenOrCreate
             , access: FileAccess.Write
             , share: FileShare.None
             , bufferSize: 4096
             , useAsync: true);

         await SaveToStreamAsync(fileStream);
      }


      public void SaveToStream(Stream      stream) { SaveToStreamAsync(stream).GetAwaiter().GetResult(); }
      public Task SaveToStreamAsync(Stream stream) { return Meta.WriteAsync(stream); }


      public void EnableDebugging(string? debuggingOutputDirectory = null) {
         if (Meta.PdbByteCode == null
             || Meta.PdbByteCode.Length == 0
             || string.IsNullOrWhiteSpace(Meta.TemplateFileName)
             || string.IsNullOrWhiteSpace(Meta.TemplateSource)) {
            
            // Exception with unqiue ID
            throw new RazorEngineException
               ("2qdill No debugging info available, compile template with builder.IncludeDebuggingInfo(); option");
         }

         // Write File as UTF8 
         File.WriteAllText
            (Path.Combine(debuggingOutputDirectory ?? ".", Meta.TemplateFileName + ".txt")
             , Meta.TemplateSource + "\r\n\r\n--------------\r\n\r\n" + Meta.GeneratedSourceCode
             , new UTF8Encoding());

         IsDebuggerEnabled = true;
      }

   }

}
