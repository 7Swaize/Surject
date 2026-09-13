using System.CodeDom.Compiler;
using System.IO;

public static class IndentedTextWriterExtensions {
    extension(IndentedTextWriter writer) {
        public void WriteMultiline(string text) {
            using StringReader reader = new(text); 
            
            while (reader.ReadLine() is { } line) {
                writer.WriteLine(line);
            }
        }
    }
}