using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AdvancedDotNetPOCs.Yield
{
    /// <summary>
    /// POC sobre yield return e yield break
    /// 
    /// CONCEITOS:
    /// - yield return: Retorna cada elemento de forma lazy (sob demanda)
    /// - yield break: Interrompe a iteração
    /// - Deferred Execution: O código só executa quando iterado
    /// - State Machine: Compilador gera uma máquina de estados
    /// 
    /// BENEFÍCIOS:
    /// - Economia de memória (não cria lista completa)
    /// - Performance em grandes datasets
    /// - Composição de queries (LINQ)
    /// - Código mais limpo e legível
    /// </summary>
    public static class YieldExamples
    {
        #region Exemplo 1: Básico - Diferença entre Lista e Yield
        
        /// <summary>
        /// Abordagem tradicional: Cria toda a lista em memória
        /// </summary>
        public static List<int> GetNumbersTraditional(int max)
        {
            Console.WriteLine("[Traditional] Iniciando geração de números...");
            var numbers = new List<int>();
            
            for (int i = 1; i <= max; i++)
            {
                Console.WriteLine($"[Traditional] Gerando número: {i}");
                numbers.Add(i * i);
            }
            
            Console.WriteLine("[Traditional] Todos os números gerados!");
            return numbers;
        }
        
        /// <summary>
        /// Abordagem com yield: Gera números sob demanda (lazy)
        /// </summary>
        public static IEnumerable<int> GetNumbersWithYield(int max)
        {
            Console.WriteLine("[Yield] Iniciando geração de números...");
            
            for (int i = 1; i <= max; i++)
            {
                Console.WriteLine($"[Yield] Gerando número: {i}");
                yield return i * i;
            }
            
            Console.WriteLine("[Yield] Iteração completa!");
        }
        
        #endregion
        
        #region Exemplo 2: Fibonacci Infinito
        
        /// <summary>
        /// Sequência de Fibonacci infinita usando yield
        /// Demonstra como yield permite trabalhar com sequências infinitas
        /// </summary>
        public static IEnumerable<long> FibonacciSequence()
        {
            long previous = 0;
            long current = 1;
            
            yield return previous;
            yield return current;
            
            while (true)
            {
                long next = previous + current;
                yield return next;
                previous = current;
                current = next;
            }
        }
        
        #endregion
        
        #region Exemplo 3: Yield Break - Interrupção Condicional
        
        /// <summary>
        /// Demonstra o uso de yield break para interromper a iteração
        /// </summary>
        public static IEnumerable<int> GetNumbersUntilCondition(int max, Func<int, bool> stopCondition)
        {
            for (int i = 1; i <= max; i++)
            {
                if (stopCondition(i))
                {
                    Console.WriteLine($"Condição de parada atingida em {i}");
                    yield break; // Interrompe a iteração
                }
                
                yield return i;
            }
        }
        
        #endregion
        
        #region Exemplo 4: Performance - Grande Volume de Dados
        
        /// <summary>
        /// Processamento de grande volume com yield (economia de memória)
        /// </summary>
        public static IEnumerable<DataRecord> ProcessLargeDataset(int count)
        {
            for (int i = 0; i < count; i++)
            {
                // Simula leitura de dados de uma fonte externa
                var record = new DataRecord
                {
                    Id = i,
                    Timestamp = DateTime.Now,
                    Data = $"Record_{i}",
                    ProcessedAt = DateTime.Now
                };
                
                // Processa o registro
                record.IsProcessed = true;
                
                yield return record;
                
                // Não mantém todos os registros em memória!
            }
        }
        
        #endregion
        
        #region Exemplo 5: Árvore de Diretórios (Recursivo com Yield)
        
        /// <summary>
        /// Navegação recursiva em árvore usando yield
        /// Demonstra yield em métodos recursivos
        /// </summary>
        public static IEnumerable<TreeNode> TraverseTree(TreeNode root)
        {
            if (root == null)
                yield break;
            
            // Retorna o nó atual
            yield return root;
            
            // Recursivamente retorna os filhos
            if (root.Children != null)
            {
                foreach (var child in root.Children)
                {
                    foreach (var node in TraverseTree(child))
                    {
                        yield return node;
                    }
                }
            }
        }
        
        #endregion
        
        #region Exemplo 5.1: ENTENDENDO YIELD - Pausa e Continuação
        
        /// <summary>
        /// Demonstra VISUALMENTE como yield return PAUSA e CONTINUA (não sai da função!)
        /// Este exemplo mostra o fluxo de execução linha por linha
        /// </summary>
        public static IEnumerable<string> DemonstrateYieldFlow()
        {
            Console.WriteLine("  [Método] Linha 1: Início do método");
            
            Console.WriteLine("  [Método] Linha 2: Antes do primeiro yield");
            yield return "Primeiro"; // PAUSA aqui, retorna "Primeiro", salva estado
            Console.WriteLine("  [Método] Linha 3: CONTINUOU após primeiro yield!");
            
            Console.WriteLine("  [Método] Linha 4: Antes do segundo yield");
            yield return "Segundo"; // PAUSA aqui, retorna "Segundo", salva estado
            Console.WriteLine("  [Método] Linha 5: CONTINUOU após segundo yield!");
            
            Console.WriteLine("  [Método] Linha 6: Antes do terceiro yield");
            yield return "Terceiro"; // PAUSA aqui, retorna "Terceiro", salva estado
            Console.WriteLine("  [Método] Linha 7: CONTINUOU após terceiro yield!");
            
            Console.WriteLine("  [Método] Linha 8: Fim do método - sem mais yields");
            // Aqui termina a iteração
        }
        
        /// <summary>
        /// Demonstra yield return em método recursivo com logs detalhados
        /// Mostra que yield return NÃO sai da função recursiva
        /// </summary>
        public static IEnumerable<string> TraverseTreeWithLogs(TreeNode root, string indent = "")
        {
            if (root == null)
                yield break;
            
            Console.WriteLine($"{indent}[TraverseTree] Entrando no nó: {root.Value}");
            
            Console.WriteLine($"{indent}[TraverseTree] YIELD RETURN do nó: {root.Value}");
            yield return root.Value; // PAUSA aqui, mas a função NÃO termina
            
            Console.WriteLine($"{indent}[TraverseTree] CONTINUOU após yield do nó: {root.Value}");
            
            // Processa filhos (se existirem)
            if (root.Children != null)
            {
                Console.WriteLine($"{indent}[TraverseTree] Processando {root.Children.Length} filho(s) de {root.Value}");
                
                foreach (var child in root.Children)
                {
                    Console.WriteLine($"{indent}[TraverseTree] Chamando recursivamente para filho: {child.Value}");
                    
                    // Cada yield return dos filhos também pausa e continua
                    foreach (var childValue in TraverseTreeWithLogs(child, indent + "  "))
                    {
                        yield return childValue;
                    }
                    
                    Console.WriteLine($"{indent}[TraverseTree] Voltou da recursão de: {child.Value}");
                }
            }
            
            Console.WriteLine($"{indent}[TraverseTree] Finalizando nó: {root.Value}");
        }
        
        #endregion
        
        #region Exemplo 6: Pipeline de Transformações
        
        /// <summary>
        /// Demonstra composição de operações com yield (similar ao LINQ)
        /// </summary>
        public static IEnumerable<T> Filter<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    yield return item;
                }
            }
        }
        
        public static IEnumerable<TResult> Transform<TSource, TResult>(
            this IEnumerable<TSource> source, 
            Func<TSource, TResult> transformer)
        {
            foreach (var item in source)
            {
                yield return transformer(item);
            }
        }
        
        #endregion
        
        #region Exemplo 7: Paginação Lazy
        
        /// <summary>
        /// Implementação de paginação lazy usando yield
        /// Útil para APIs e grandes conjuntos de dados
        /// </summary>
        public static IEnumerable<List<T>> Paginate<T>(IEnumerable<T> source, int pageSize)
        {
            var page = new List<T>(pageSize);
            
            foreach (var item in source)
            {
                page.Add(item);
                
                if (page.Count == pageSize)
                {
                    yield return page;
                    page = new List<T>(pageSize);
                }
            }
            
            // Retorna a última página parcial, se existir
            if (page.Count > 0)
            {
                yield return page;
            }
        }
        
        #endregion
        
        #region Exemplo 8: Quando Usar (e Quando NÃO Usar) Yield
        
        /// <summary>
        /// Guia prático: Quando yield É vantajoso
        /// </summary>
        public static void WhenToUseYield()
        {
            Console.WriteLine("\n\n=== QUANDO USAR YIELD ===\n");
            
            // CENÁRIO 1: Grande volume + Pode parar cedo ✅
            Console.WriteLine("✅ CENÁRIO 1: Grande volume + Early exit");
            Console.WriteLine("Procurar primeiro número divisível por 7 em 1 milhão:");
            
            var sw = Stopwatch.StartNew();
            var firstMatch = GenerateMillionNumbers().FirstOrDefault(n => n % 7 == 0);
            sw.Stop();
            Console.WriteLine($"   Yield: {firstMatch} encontrado em {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"   (gerou apenas ~7 números, não 1 milhão!)");
            
            // CENÁRIO 2: Múltiplas iterações ❌
            Console.WriteLine("\n❌ CENÁRIO 2: Múltiplas iterações (RUIM para yield)");
            var sequence = GenerateSmallSequence().ToList(); // Materializa de uma vez
            
            sw.Restart();
            var sum1 = sequence.Sum();
            var max1 = sequence.Max();
            var min1 = sequence.Min();
            sw.Stop();
            var listTime = sw.ElapsedMilliseconds;
            
            Console.WriteLine($"   Com List (materializada): {listTime}ms para 3 operações");
            
            sw.Restart();
            var lazySequence = GenerateSmallSequence();
            var sum2 = lazySequence.Sum();        // Gera todos
            var max2 = lazySequence.Max();        // Gera todos DE NOVO
            var min2 = lazySequence.Min();        // Gera todos DE NOVO (3x!)
            sw.Stop();
            
            Console.WriteLine($"   Com Yield (lazy): {sw.ElapsedMilliseconds}ms para 3 operações");
            Console.WriteLine($"   ⚠️  Yield gerou a sequência 3 VEZES!");
            
            // CENÁRIO 3: Pipeline de transformações ✅
            Console.WriteLine("\n✅ CENÁRIO 3: Pipeline (composição lazy)");
            sw.Restart();
            var pipelineResult = Enumerable.Range(1, 1_000_000)
                .Where(n => n % 2 == 0)
                .Select(n => n * n)
                .Where(n => n > 1000)
                .Take(5)  // Para logo!
                .ToList();
            sw.Stop();
            Console.WriteLine($"   Pipeline lazy: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"   (processou apenas ~50 números para obter 5 resultados)");
            
            // CENÁRIO 4: Precisa ordenar/agrupar ❌
            Console.WriteLine("\n❌ CENÁRIO 4: Operações que precisam de tudo (ordenar)");
            Console.WriteLine("   OrderBy, GroupBy, Reverse = precisa TODOS os dados");
            Console.WriteLine("   Nestes casos, yield não ajuda (vai materializar tudo mesmo)");
            
            // CENÁRIO 5: Processamento de arquivo grande ✅
            Console.WriteLine("\n✅ CENÁRIO 5: Arquivo grande (linha por linha)");
            Console.WriteLine("   ReadLines com yield: apenas 1 linha em memória por vez");
            Console.WriteLine("   ReadAllLines sem yield: arquivo inteiro na memória");
        }
        
        private static IEnumerable<int> GenerateMillionNumbers()
        {
            for (int i = 1; i <= 1_000_000; i++)
            {
                yield return i;
            }
        }
        
        private static IEnumerable<int> GenerateSmallSequence()
        {
            for (int i = 1; i <= 100; i++)
            {
                // Simula processamento custoso
                System.Threading.Thread.Sleep(1);
                yield return i;
            }
        }
        
        /// <summary>
        /// Demonstração: Yield vs List - Uso de memória
        /// </summary>
        public static void MemoryComparison()
        {
            Console.WriteLine("\n\n=== YIELD vs LIST: MEMÓRIA ===\n");
            
            const int count = 1_000_000;
            
            // Abordagem 1: List (tudo em memória)
            Console.WriteLine("❌ List: Criando 1 milhão de objetos...");
            var beforeList = GC.GetTotalMemory(true);
            
            var list = new List<DataRecord>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new DataRecord 
                { 
                    Id = i, 
                    Data = $"Record_{i}",
                    Timestamp = DateTime.Now 
                });
            }
            
            var afterList = GC.GetTotalMemory(false);
            var listMemory = (afterList - beforeList) / 1024 / 1024;
            Console.WriteLine($"   Memória usada: ~{listMemory}MB");
            Console.WriteLine($"   Todos os {count:N0} objetos em memória!");
            
            list = null;
            GC.Collect();
            
            // Abordagem 2: Yield (sob demanda)
            Console.WriteLine("\n✅ Yield: Processando 1 milhão sob demanda...");
            var beforeYield = GC.GetTotalMemory(true);
            
            var processed = 0;
            foreach (var record in ProcessLargeDataset(count).Take(10))
            {
                processed++;
                // Processa apenas 10
            }
            
            var afterYield = GC.GetTotalMemory(false);
            var yieldMemory = (afterYield - beforeYield) / 1024;
            Console.WriteLine($"   Memória usada: ~{yieldMemory}KB");
            Console.WriteLine($"   Processados: {processed} de {count:N0}");
            Console.WriteLine($"   Economia: {listMemory * 1024 - yieldMemory}KB!");
        }
        
        /// <summary>
        /// Resumo: Decisão rápida - Usar yield ou não?
        /// </summary>
        public static void YieldDecisionGuide()
        {
            Console.WriteLine("\n\n╔═══════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           GUIA DE DECISÃO: USAR YIELD?                    ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");
            
            Console.WriteLine("✅ USE YIELD quando:");
            Console.WriteLine("   • Grande volume de dados (> 1000 itens)");
            Console.WriteLine("   • Pode NÃO precisar de todos os itens (Take, First, Any)");
            Console.WriteLine("   • Iteração única ou poucas iterações");
            Console.WriteLine("   • Pipeline de transformações (Where, Select)");
            Console.WriteLine("   • Leitura de arquivos/streams linha por linha");
            Console.WriteLine("   • Sequências potencialmente infinitas");
            Console.WriteLine("   • Quer economizar memória");
            
            Console.WriteLine("\n❌ NÃO use yield quando:");
            Console.WriteLine("   • Pequeno volume (< 100 itens)");
            Console.WriteLine("   • Vai iterar MÚLTIPLAS vezes (materializar com ToList!)");
            Console.WriteLine("   • Precisa de OrderBy, GroupBy, Reverse (precisa tudo)");
            Console.WriteLine("   • Precisa de acesso aleatório (indexação)");
            Console.WriteLine("   • Geração dos itens é trivial (sem custo)");
            Console.WriteLine("   • Vai armazenar em cache de qualquer forma");
            
            Console.WriteLine("\n💡 DICA: Em caso de dúvida, meça!");
            Console.WriteLine("   Use Stopwatch e GC.GetTotalMemory() para comparar.\n");
        }
        
        #endregion
        
        #region Exemplo de Uso - Main
        
        public static void RunExamples()
        {
            Console.WriteLine("=== POC: YIELD RETURN ===\n");
            
            // Exemplo 1: Comparação Traditional vs Yield
            Console.WriteLine("--- Exemplo 1: Traditional vs Yield ---");
            Console.WriteLine("\nTraditional (cria lista completa):");
            var traditional = GetNumbersTraditional(5);
            Console.WriteLine("Consumindo os primeiros 3:");
            foreach (var num in traditional.Take(3))
            {
                Console.WriteLine($"Consumindo: {num}");
            }
            
            Console.WriteLine("\n\nYield (lazy evaluation):");
            var lazy = GetNumbersWithYield(5);
            Console.WriteLine("Consumindo os primeiros 3:");
            foreach (var num in lazy.Take(3))
            {
                Console.WriteLine($"Consumindo: {num}");
            }
            
            // Exemplo 2: Fibonacci
            Console.WriteLine("\n\n--- Exemplo 2: Fibonacci Infinito ---");
            var fibonacci = FibonacciSequence().Take(10);
            Console.WriteLine(string.Join(", ", fibonacci));
            
            // Exemplo 3: Yield Break
            Console.WriteLine("\n\n--- Exemplo 3: Yield Break ---");
            var numbersUntil = GetNumbersUntilCondition(20, n => n > 7);
            Console.WriteLine($"Números gerados: {string.Join(", ", numbersUntil)}");
            
            // Exemplo 4: Performance
            Console.WriteLine("\n\n--- Exemplo 4: Performance em Grande Volume ---");
            var stopwatch = Stopwatch.StartNew();
            
            var largeDataset = ProcessLargeDataset(1_000_000);
            var firstTen = largeDataset.Take(10).ToList();
            
            stopwatch.Stop();
            Console.WriteLine($"Processados 10 registros de 1M em {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"Primeiro registro: {firstTen.First().Data}");
            
            // Exemplo 5: Árvore
            Console.WriteLine("\n\n--- Exemplo 5: Travessia de Árvore ---");
            var tree = new TreeNode("Root", new[]
            {
                new TreeNode("Child1", new[]
                {
                    new TreeNode("GrandChild1"),
                    new TreeNode("GrandChild2")
                }),
                new TreeNode("Child2")
            });
            
            var allNodes = TraverseTree(tree);
            Console.WriteLine("Nós da árvore:");
            foreach (var node in allNodes)
            {
                Console.WriteLine($"  - {node.Value}");
            }
            
            // Exemplo 5.1: ENTENDENDO YIELD - Fluxo de Execução
            Console.WriteLine("\n\n--- Exemplo 5.1: ENTENDENDO YIELD (Pausa e Continuação) ---");
            Console.WriteLine("\n>>> IMPORTANTE: yield return PAUSA a função, não termina! <<<\n");
            
            Console.WriteLine("Demonstração 1: Fluxo Linear");
            Console.WriteLine("────────────────────────────");
            var sequence = DemonstrateYieldFlow();
            
            Console.WriteLine("\n[Chamador] Começando iteração...\n");
            foreach (var item in sequence)
            {
                Console.WriteLine($"[Chamador] Recebi: '{item}' (método pausado, aguardando próximo MoveNext)\n");
            }
            Console.WriteLine("[Chamador] Iteração completa!\n");
            
            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine("Demonstração 2: Fluxo Recursivo (Árvore com Logs)");
            Console.WriteLine(new string('=', 70) + "\n");
            
            var smallTree = new TreeNode("A",
            [
                new TreeNode("B"),
                new TreeNode("C",
                [
                    new TreeNode("D")
                ])
            ]);
            
            Console.WriteLine("[Chamador] Começando travessia da árvore...\n");
            foreach (var nodeName in TraverseTreeWithLogs(smallTree))
            {
                Console.WriteLine($">>> [Chamador] RECEBEU: {nodeName} <<<\n");
            }
            Console.WriteLine("[Chamador] Travessia completa!");
            
            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine("CONCLUSÃO:");
            Console.WriteLine("- yield return PAUSA a execução (não sai da função)");
            Console.WriteLine("- Quando próximo item é solicitado, CONTINUA de onde parou");
            Console.WriteLine("- Funciona perfeitamente com recursão!");
            Console.WriteLine("- O compilador cria uma State Machine nos bastidores");
            Console.WriteLine(new string('=', 70));
            
            // Exemplo 6: Pipeline
            Console.WriteLine("\n\n--- Exemplo 6: Pipeline de Transformações ---");
            var numbers = Enumerable.Range(1, 20)
                .Filter(n => n % 2 == 0)           // Pares
                .Transform(n => n * n)             // Quadrado
                .Filter(n => n > 50)               // Maiores que 50
                .Take(5);
            
            Console.WriteLine($"Pipeline result: {string.Join(", ", numbers)}");
            
            // Exemplo 7: Paginação
            Console.WriteLine("\n\n--- Exemplo 7: Paginação Lazy ---");
            var items = Enumerable.Range(1, 25);
            var pages = Paginate(items, 10);
            
            int pageNumber = 1;
            foreach (var page in pages)
            {
                Console.WriteLine($"Página {pageNumber}: {string.Join(", ", page)}");
                pageNumber++;
            }
            
            // Exemplo 8: Quando usar yield
            WhenToUseYield();
            MemoryComparison();
            YieldDecisionGuide();
            
            Console.WriteLine("\n=== FIM DOS EXEMPLOS ===");
        }
        
        #endregion
    }
    
    #region Classes de Suporte
    
    public class DataRecord
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Data { get; set; }
        public DateTime ProcessedAt { get; set; }
        public bool IsProcessed { get; set; }
    }
    
    public class TreeNode
    {
        public string Value { get; set; }
        public TreeNode[] Children { get; set; }
        
        public TreeNode(string value, TreeNode[] children = null)
        {
            Value = value;
            Children = children;
        }
    }
    
    #endregion
}
