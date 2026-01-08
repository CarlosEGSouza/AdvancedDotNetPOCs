using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace AdvancedDotNetPOCs.SpanAndMemory
{
    /// <summary>
    /// POC sobre Span&lt;T&gt; e Memory&lt;T&gt;
    /// 
    /// CONCEITOS:
    /// - Span&lt;T&gt;: Representa uma região contígua de memória (ref struct, stack-only)
    /// - Memory&lt;T&gt;: Versão "managed" do Span, pode ser armazenado em heap
    /// - ReadOnlySpan&lt;T&gt; e ReadOnlyMemory&lt;T&gt;: Versões somente leitura
    /// - Não há alocação de heap - trabalha diretamente com memória existente
    /// - Zero-copy: Trabalha com "views" da memória, sem copiar dados
    /// 
    /// BENEFÍCIOS:
    /// - Performance: Elimina alocações desnecessárias
    /// - Zero-copy operations: Trabalha com slices de memória
    /// - Type-safe: Segurança de tipos em operações de memória
    /// - Interoperabilidade: Facilita trabalhar com código nativo
    /// - Redução de GC pressure
    /// </summary>
    public class SpanAndMemoryExamples
    {
        #region Exemplo 1: Básico - Span vs Array
        
        /// <summary>
        /// Comparação básica entre trabalhar com arrays e Span
        /// </summary>
        public static void BasicSpanVsArray()
        {
            Console.WriteLine("--- Exemplo 1: Span vs Array ---");
            
            // Abordagem tradicional com array
            int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            
            // Pegar elementos 3-7 (índices 2-6)
            // Abordagem tradicional: cria novo array
            int[] subset = new int[5];
            Array.Copy(numbers, 2, subset, 0, 5);
            Console.WriteLine($"Array tradicional (cópia): {string.Join(", ", subset)}");
            
            // Com Span: sem alocação, apenas uma "view"
            Span<int> span = numbers;
            Span<int> spanSubset = span.Slice(2, 5);
            Console.WriteLine($"Span (zero-copy): {string.Join(", ", spanSubset.ToArray())}");
            
            // Modificar através do span afeta o array original
            spanSubset[0] = 999;
            Console.WriteLine($"Array original após modificar span: {string.Join(", ", numbers)}");
            
            // Span também pode ser criado de stackalloc
            Span<int> stackSpan = stackalloc int[5] { 1, 2, 3, 4, 5 };
            Console.WriteLine($"Span de stack: {string.Join(", ", stackSpan.ToArray())}");
        }
        
        #endregion
        
        #region Exemplo 2: Performance - String Operations
        
        /// <summary>
        /// Demonstra ganhos de performance em operações com strings
        /// </summary>
        public static void StringPerformanceComparison()
        {
            Console.WriteLine("\n--- Exemplo 2: Performance com Strings ---");
            
            const string longText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                                   "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
            const int iterations = 100_000;
            
            // Método 1: Substring tradicional (aloca nova string)
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                string sub = longText.Substring(6, 10);
                _ = sub.Length;
            }
            sw.Stop();
            Console.WriteLine($"Substring (tradicional): {sw.ElapsedMilliseconds}ms");
            
            // Método 2: ReadOnlySpan (zero allocation)
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                ReadOnlySpan<char> span = longText.AsSpan(6, 10);
                _ = span.Length;
            }
            sw.Stop();
            Console.WriteLine($"AsSpan (zero-copy): {sw.ElapsedMilliseconds}ms");
            
            // Demonstração de parsing
            Console.WriteLine("\nParsing com Span:");
            string dateString = "2026-01-08";
            ReadOnlySpan<char> dateSpan = dateString;
            
            var year = int.Parse(dateSpan.Slice(0, 4));
            var month = int.Parse(dateSpan.Slice(5, 2));
            var day = int.Parse(dateSpan.Slice(8, 2));
            
            Console.WriteLine($"Parsed: Year={year}, Month={month}, Day={day}");
        }
        
        #endregion
        
        #region Exemplo 3: Span para Parsing de CSV
        
        /// <summary>
        /// Parsing eficiente de CSV usando Span
        /// Evita múltiplas alocações de strings
        /// </summary>
        public static void CsvParsingWithSpan()
        {
            Console.WriteLine("\n--- Exemplo 3: CSV Parsing com Span ---");
            
            const string csvLine = "João,25,joao@example.com,Brasil,São Paulo";
            
            // Parsing tradicional (múltiplas alocações)
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 100_000; i++)
            {
                var parts = csvLine.Split(',');
                _ = parts[0];
                _ = int.Parse(parts[1]);
                _ = parts[2];
            }
            sw.Stop();
            Console.WriteLine($"Split tradicional: {sw.ElapsedMilliseconds}ms");
            
            // Parsing com Span (zero allocation até converter para string)
            sw.Restart();
            for (int i = 0; i < 100_000; i++)
            {
                ReadOnlySpan<char> line = csvLine;
                ParseCsvLine(line, out var name, out var age, out var email);
                _ = name;
                _ = age;
                _ = email;
            }
            sw.Stop();
            Console.WriteLine($"Span parsing: {sw.ElapsedMilliseconds}ms");
            
            // Demonstração do resultado
            ParseCsvLine(csvLine, out var n, out var a, out var e);
            Console.WriteLine($"Resultado: Name={n}, Age={a}, Email={e}");
        }
        
        private static void ParseCsvLine(
            ReadOnlySpan<char> line,
            out ReadOnlySpan<char> name,
            out int age,
            out ReadOnlySpan<char> email)
        {
            int firstComma = line.IndexOf(',');
            name = line.Slice(0, firstComma);
            
            line = line.Slice(firstComma + 1);
            int secondComma = line.IndexOf(',');
            age = int.Parse(line.Slice(0, secondComma));
            
            line = line.Slice(secondComma + 1);
            int thirdComma = line.IndexOf(',');
            email = line.Slice(0, thirdComma);
        }
        
        #endregion
        
        #region Exemplo 4: Memory<T> para Operações Assíncronas
        
        /// <summary>
        /// Memory&lt;T&gt; pode ser usado em operações assíncronas (diferente de Span)
        /// </summary>
        public static async Task MemoryAsyncOperations()
        {
            Console.WriteLine("\n--- Exemplo 4: Memory<T> com Async ---");
            
            // Span não pode ser usado em async porque é ref struct (stack-only)
            // Memory é a alternativa para cenários async
            
            byte[] buffer = new byte[1024];
            Memory<byte> memory = buffer;
            
            // Simula operação async de I/O
            await FillBufferAsync(memory);
            
            Console.WriteLine($"Primeiros 10 bytes: {string.Join(", ", memory.Slice(0, 10).ToArray())}");
            
            // ReadOnlyMemory para passar dados que não devem ser modificados
            ReadOnlyMemory<byte> readOnlyMemory = memory;
            await ProcessDataAsync(readOnlyMemory);
        }
        
        private static async Task FillBufferAsync(Memory<byte> buffer)
        {
            await Task.Delay(10); // Simula operação async
            
            // IMPORTANTE: Após await, precisamos usar buffer.Span localmente
            // (não pode ser declarado antes do await porque Span é ref struct)
            // Solução: usar buffer.Span somente após o await, sem armazenar em variável
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer.Span[i] = (byte)(i % 256);
            }
        }
        
        private static async Task ProcessDataAsync(ReadOnlyMemory<byte> data)
        {
            await Task.Delay(10);
            Console.WriteLine($"Processando {data.Length} bytes (somente leitura)");
        }
        
        #endregion
        
        #region Exemplo 5: ArrayPool com Span - Zero Allocation
        
        /// <summary>
        /// Combina ArrayPool com Span para operações de alta performance sem alocações
        /// </summary>
        public static void ArrayPoolWithSpan()
        {
            Console.WriteLine("\n--- Exemplo 5: ArrayPool com Span ---");
            
            const int bufferSize = 1024;
            const int iterations = 10_000;
            
            // Abordagem 1: Alocação tradicional
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                byte[] buffer = new byte[bufferSize];
                ProcessBuffer(buffer);
            }
            sw.Stop();
            Console.WriteLine($"Alocação tradicional: {sw.ElapsedMilliseconds}ms");
            
            // Abordagem 2: ArrayPool (reutilização)
            sw.Restart();
            var pool = ArrayPool<byte>.Shared;
            for (int i = 0; i < iterations; i++)
            {
                byte[] buffer = pool.Rent(bufferSize);
                try
                {
                    Span<byte> span = buffer.AsSpan(0, bufferSize);
                    ProcessBufferSpan(span);
                }
                finally
                {
                    pool.Return(buffer);
                }
            }
            sw.Stop();
            Console.WriteLine($"ArrayPool com Span: {sw.ElapsedMilliseconds}ms");
            
            Console.WriteLine("ArrayPool reduz drasticamente GC pressure!");
        }
        
        private static void ProcessBuffer(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(i % 256);
            }
        }
        
        private static void ProcessBufferSpan(Span<byte> buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(i % 256);
            }
        }
        
        #endregion
        
        #region Exemplo 6: Interop - Trabalhando com Unmanaged Memory
        
        /// <summary>
        /// Span facilita interoperabilidade com código nativo/unmanaged
        /// </summary>
        public static unsafe void UnmanagedMemoryExample()
        {
            Console.WriteLine("\n--- Exemplo 6: Interop com Unmanaged Memory ---");
            
            const int size = 10;
            
            // Alocar memória nativa
            IntPtr ptr = Marshal.AllocHGlobal(size * sizeof(int));
            
            try
            {
                // Criar Span a partir de ponteiro nativo
                Span<int> nativeSpan = new Span<int>(ptr.ToPointer(), size);
                
                // Trabalhar com a memória nativa usando Span
                for (int i = 0; i < nativeSpan.Length; i++)
                {
                    nativeSpan[i] = i * 10;
                }
                
                Console.WriteLine($"Dados em memória nativa: {string.Join(", ", nativeSpan.ToArray())}");
                
                // Copiar para managed array
                int[] managedArray = nativeSpan.ToArray();
                Console.WriteLine($"Copiado para managed: {string.Join(", ", managedArray)}");
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        
        #endregion
        
        #region Exemplo 7: MemoryMarshal - Advanced Scenarios
        
        /// <summary>
        /// MemoryMarshal para cenários avançados de manipulação de memória
        /// </summary>
        public static void MemoryMarshalExample()
        {
            Console.WriteLine("\n--- Exemplo 7: MemoryMarshal Advanced ---");
            
            // Cast entre tipos (reinterpretação de bytes)
            byte[] bytes = { 1, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0 };
            Span<byte> byteSpan = bytes;
            
            // Reinterpretar bytes como ints
            Span<int> intSpan = MemoryMarshal.Cast<byte, int>(byteSpan);
            Console.WriteLine($"Bytes como ints: {string.Join(", ", intSpan.ToArray())}");
            
            // Modificar através da view de ints
            intSpan[0] = 100;
            Console.WriteLine($"Bytes após modificar int: {string.Join(", ", bytes.Take(4))}");
            
            // Obter referência direta
            ref int firstInt = ref MemoryMarshal.GetReference(intSpan);
            firstInt = 999;
            Console.WriteLine($"Após modificar referência: {intSpan[0]}");
            
            // Trabalhar com structs
            var person = new PersonStruct { Id = 1, Age = 25 };
            Span<PersonStruct> personSpan = MemoryMarshal.CreateSpan(ref person, 1);
            Console.WriteLine($"Person: Id={personSpan[0].Id}, Age={personSpan[0].Age}");
        }
        
        private struct PersonStruct
        {
            public int Id;
            public int Age;
        }
        
        #endregion
        
        #region Exemplo 8: StringBuilder com Span
        
        /// <summary>
        /// Operações eficientes com StringBuilder usando Span
        /// </summary>
        public static void StringBuilderWithSpan()
        {
            Console.WriteLine("\n--- Exemplo 8: StringBuilder com Span ---");
            
            var sb = new StringBuilder(100);
            sb.Append("Hello");
            
            // Obter Span do buffer interno do StringBuilder
            Span<char> span = stackalloc char[5];
            sb.CopyTo(0, span, span.Length);
            
            Console.WriteLine($"Copiado do StringBuilder: {span.ToString()}");
            
            // Operações de formatação eficientes
            Span<char> buffer = stackalloc char[50];
            
            // Formatar diretamente no buffer
            if (TryFormatPerson("João", 25, buffer, out int charsWritten))
            {
                var result = buffer.Slice(0, charsWritten);
                Console.WriteLine($"Formatado: {result.ToString()}");
            }
        }
        
        private static bool TryFormatPerson(string name, int age, Span<char> destination, out int charsWritten)
        {
            ReadOnlySpan<char> template = "Name: ";
            charsWritten = 0;
            
            if (destination.Length < template.Length + name.Length + 10)
                return false;
            
            template.CopyTo(destination);
            charsWritten += template.Length;
            
            name.AsSpan().CopyTo(destination.Slice(charsWritten));
            charsWritten += name.Length;
            
            ReadOnlySpan<char> ageLabel = ", Age: ";
            ageLabel.CopyTo(destination.Slice(charsWritten));
            charsWritten += ageLabel.Length;
            
            age.TryFormat(destination.Slice(charsWritten), out int ageChars);
            charsWritten += ageChars;
            
            return true;
        }
        
        #endregion
        
        #region Exemplo 9: Span para Algoritmos
        
        /// <summary>
        /// Implementação eficiente de algoritmos usando Span
        /// </summary>
        public static void AlgorithmsWithSpan()
        {
            Console.WriteLine("\n--- Exemplo 9: Algoritmos com Span ---");
            
            int[] numbers = { 5, 2, 8, 1, 9, 3, 7, 4, 6 };
            
            Console.WriteLine($"Original: {string.Join(", ", numbers)}");
            
            // Reverse in-place usando Span
            Span<int> span = numbers;
            ReverseSpan(span);
            Console.WriteLine($"Reversed: {string.Join(", ", numbers)}");
            
            // Reverse de volta
            span.Reverse();
            Console.WriteLine($"Reversed (built-in): {string.Join(", ", numbers)}");
            
            // Encontrar mediana sem alocar novo array
            int median = FindMedian(span);
            Console.WriteLine($"Mediana: {median}");
            
            // Trabalhar com slices
            var firstHalf = span.Slice(0, span.Length / 2);
            var secondHalf = span.Slice(span.Length / 2);
            Console.WriteLine($"Primeira metade: {string.Join(", ", firstHalf.ToArray())}");
            Console.WriteLine($"Segunda metade: {string.Join(", ", secondHalf.ToArray())}");
        }
        
        private static void ReverseSpan<T>(Span<T> span)
        {
            for (int i = 0; i < span.Length / 2; i++)
            {
                int oppositeIndex = span.Length - 1 - i;
                T temp = span[i];
                span[i] = span[oppositeIndex];
                span[oppositeIndex] = temp;
            }
        }
        
        private static int FindMedian(Span<int> numbers)
        {
            // Para simplicidade, cria cópia para ordenar
            Span<int> sorted = stackalloc int[numbers.Length];
            numbers.CopyTo(sorted);
            sorted.Sort();
            
            return sorted[sorted.Length / 2];
        }
        
        #endregion
        
        #region Exemplo 10: Caso de Uso Real - Image Processing
        
        /// <summary>
        /// Processamento de imagem eficiente usando Span
        /// Evita múltiplas alocações ao trabalhar com pixels
        /// </summary>
        public static void ImageProcessingExample()
        {
            Console.WriteLine("\n--- Exemplo 10: Image Processing com Span ---");
            
            // Simula uma imagem RGB 10x10
            const int width = 10;
            const int height = 10;
            const int bytesPerPixel = 3; // RGB
            
            byte[] imageData = new byte[width * height * bytesPerPixel];
            
            // Preencher com dados simulados
            for (int i = 0; i < imageData.Length; i++)
            {
                imageData[i] = (byte)(i % 256);
            }
            
            Span<byte> imageSpan = imageData;
            
            // Converter para grayscale sem alocar novo array
            ConvertToGrayscale(imageSpan, width, height);
            
            Console.WriteLine($"Primeiro pixel (R,G,B): {imageSpan[0]}, {imageSpan[1]}, {imageSpan[2]}");
            
            // Aplicar filtro em uma região específica
            var topLeftQuadrant = imageSpan.Slice(0, (width * height * bytesPerPixel) / 4);
            ApplyBrightness(topLeftQuadrant, 1.5f);
            
            Console.WriteLine("Processamento de imagem concluído sem alocações extras!");
        }
        
        private static void ConvertToGrayscale(Span<byte> imageData, int width, int height)
        {
            for (int i = 0; i < imageData.Length; i += 3)
            {
                byte r = imageData[i];
                byte g = imageData[i + 1];
                byte b = imageData[i + 2];
                
                // Fórmula padrão de grayscale
                byte gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
                
                imageData[i] = gray;
                imageData[i + 1] = gray;
                imageData[i + 2] = gray;
            }
        }
        
        private static void ApplyBrightness(Span<byte> imageData, float factor)
        {
            for (int i = 0; i < imageData.Length; i++)
            {
                int newValue = (int)(imageData[i] * factor);
                imageData[i] = (byte)Math.Min(255, newValue);
            }
        }
        
        #endregion
        
        #region Main Runner
        
        public static async Task RunExamples()
        {
            Console.WriteLine("=== POC: SPAN<T> E MEMORY<T> ===\n");
            
            BasicSpanVsArray();
            StringPerformanceComparison();
            CsvParsingWithSpan();
            await MemoryAsyncOperations();
            ArrayPoolWithSpan();
            UnmanagedMemoryExample();
            MemoryMarshalExample();
            StringBuilderWithSpan();
            AlgorithmsWithSpan();
            ImageProcessingExample();
            
            Console.WriteLine("\n=== FIM DOS EXEMPLOS ===");
        }
        
        #endregion
    }
}
