using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedDotNetPOCs.Yield
{
    /// <summary>
    /// Desafios para você implementar e praticar Yield
    /// Complete cada método usando yield return/break
    /// </summary>
    public static class YieldChallengesForPractice
    {
        #region Desafio 1: Números Primos (Fácil)

        /// <summary>
        /// DESAFIO: Crie um gerador de números primos usando yield
        /// 
        /// Requisitos:
        /// - Gerar números primos começando do 2
        /// - Usar yield return para cada primo encontrado
        /// - Não deve ter limite (sequência infinita)
        /// 
        /// Exemplo de uso:
        /// var primos = GerarPrimos().Take(10);
        /// Resultado: 2, 3, 5, 7, 11, 13, 17, 19, 23, 29
        /// </summary>
        public static IEnumerable<int> GerarPrimos()
        {
            yield return 2;

            //gera os números canditados para serem primos
            int number = 3;
            while (true)
            {
                if (IsPrime(number))
                    yield return number;

                number += 2;
            }
        }

        private static bool IsPrime(int number)
        {
            //testa se o número é primo
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            int limite = (int)Math.Sqrt(number);

            for (int i = 3; i <= limite; i += 2)
            {
                if (number % i == 0)
                    return false;
            }

            return true;
        }

        #endregion

        #region Desafio 2: Filtrar Duplicados Consecutivos (Fácil)

        /// <summary>
        /// DESAFIO: Remova duplicatas consecutivas de uma sequência
        /// 
        /// Requisitos:
        /// - Retornar elementos únicos consecutivos
        /// - Usar yield return
        /// - Não criar listas intermediárias
        /// 
        /// Exemplo:
        /// Entrada: [1, 1, 2, 2, 2, 3, 1, 1, 4]
        /// Saída: [1, 2, 3, 1, 4]
        /// </summary>
        public static IEnumerable<T> RemoverDuplicatasConsecutivas<T>(IEnumerable<T> source)
        {
            if (source == null)
                yield return (T)Enumerable.Empty<T>();

            var current = source!.First();
            foreach (var item in source!)
            {
                if (!EqualityComparer<T>.Default.Equals(item, current))
                {
                    yield return current;
                    current = item;
                }
            }
            yield return current;
        }

        #endregion

        #region Desafio 3: Intercalar Sequências (Médio)

        /// <summary>
        /// DESAFIO: Intercale elementos de duas sequências
        /// 
        /// Requisitos:
        /// - Alternar entre elementos da primeira e segunda sequência
        /// - Se uma sequência terminar, continuar com a outra
        /// - Usar yield return
        /// 
        /// Exemplo:
        /// Seq1: [1, 2, 3]
        /// Seq2: [10, 20, 30, 40, 50]
        /// Resultado: [1, 10, 2, 20, 3, 30, 40, 50]
        /// </summary>
        public static IEnumerable<T> IntercalarSequencias<T>(
            IEnumerable<T> primeira,
            IEnumerable<T> segunda)
        {
            using var enum1 = primeira.GetEnumerator();
            using var enum2 = segunda.GetEnumerator();

            bool hasAny1 = enum1.MoveNext();
            bool hasAny2 = enum2.MoveNext();

            while (hasAny1 || hasAny2)
            {
                if (hasAny1)
                {
                    yield return enum1.Current;
                    hasAny1 = enum1.MoveNext();
                }

                if (hasAny2)
                {
                    yield return enum2.Current;
                    hasAny2 = enum2.MoveNext();
                }
            }
        }

        #endregion

        #region Desafio 4: Range com Passo Customizado (Médio)

        /// <summary>
        /// DESAFIO: Crie um Range que aceita start, end e step
        /// 
        /// Requisitos:
        /// - Funcionar com step positivo e negativo
        /// - Parar quando ultrapassar o limite (end)
        /// - Usar yield return
        /// - Se step for 0, lançar exceção
        /// 
        /// Exemplos:
        /// RangeCustom(0, 10, 2) -> 0, 2, 4, 6, 8, 10
        /// RangeCustom(10, 0, -2) -> 10, 8, 6, 4, 2, 0
        /// RangeCustom(1, 10, 0.5) -> 1.0, 1.5, 2.0, ..., 10.0
        /// </summary>
        public static IEnumerable<double> RangeCustom(double start, double end, double step)
        {
            if (step == 0)
                throw new ArgumentException("Step não pode ser zero.");

            if (start == end)
            {
                yield return start;
                yield break;
            }

            if ((start <= end && step < 0) || (start >= end && step > 0))
                yield break;

            if (step > 0)
            {
                for (double i = start; i <= end + double.Epsilon; i += step)
                {
                    if (i > end)
                        yield break;

                    yield return i;
                }
            }
            else
            {
                for (double i = start; i >= end - double.Epsilon; i += step)
                {
                    if (i < end)
                        yield break;

                    yield return i;
                }
            }
        }

        #endregion

        #region Desafio 5: Chunk (Médio)

        /// <summary>
        /// DESAFIO: Divida uma sequência em chunks (pedaços) de tamanho fixo
        /// Similar ao Paginate, mas retorna arrays em vez de listas
        /// 
        /// Requisitos:
        /// - Retornar arrays de tamanho exato (exceto o último)
        /// - Último chunk pode ser menor
        /// - Usar yield return
        /// - Se size <= 0, lançar ArgumentException
        /// 
        /// Exemplo:
        /// Entrada: [1, 2, 3, 4, 5, 6, 7], size = 3
        /// Saída: [[1,2,3], [4,5,6], [7]]
        /// </summary>
        public static IEnumerable<T[]> Chunk<T>(this IEnumerable<T> source, int size)
        {
            if (size <= 0)
                throw new ArgumentException("Size deve ser maior que zero.");

            T[]? chunk = null;
            int count = 0;

            foreach (var item in source)
            {
                if (chunk == null)
                    chunk = new T[size];

                chunk[count] = item;
                count++;

                if (count == size)
                {
                    yield return chunk;
                    chunk = null;
                    count = 0;
                }
            }

            if (chunk != null && count > 0)
            {
                Array.Resize(ref chunk, count);
                yield return chunk;
            }
        }

        #endregion

        #region Desafio 6: TakeUntil (Médio)

        /// <summary>
        /// DESAFIO: Retorne elementos até que uma condição seja verdadeira (incluindo o elemento)
        /// Diferente de TakeWhile que para ANTES da condição ser false
        /// 
        /// Requisitos:
        /// - Retornar elementos enquanto predicate é false
        /// - Quando predicate for true, retornar esse elemento e parar
        /// - Usar yield return e yield break
        /// 
        /// Exemplo:
        /// [1, 2, 3, 4, 5, 6].TakeUntil(x => x > 3)
        /// Resultado: [1, 2, 3, 4] (para quando encontra 4 que é > 3)
        /// </summary>
        public static IEnumerable<T> TakeUntil<T>(
            this IEnumerable<T> source,
            Func<T, bool> predicate)
        {
            foreach (var item in source)
            {
                yield return item;
                if (predicate(item))
                {
                    yield break;
                }
            }
        }

        #endregion

        #region Desafio 7: Permutações (Difícil)

        /// <summary>
        /// DESAFIO: Gere todas as permutações de uma lista
        /// 
        /// Requisitos:
        /// - Usar recursão com yield
        /// - Gerar todas as combinações possíveis
        /// - Não modificar a lista original
        /// 
        /// Exemplo:
        /// Entrada: [1, 2, 3]
        /// Saída: 
        /// [1,2,3], [1,3,2], [2,1,3], [2,3,1], [3,1,2], [3,2,1]
        /// Total: 6 permutações (3! = 6)
        /// </summary>
        public static IEnumerable<List<T>> GerarPermutacoes<T>(List<T> elementos)
        {
            if (elementos.Count == 0)
            {
                yield return new List<T>();
                yield break;
            }

            if (elementos.Count == 1)
            {
                yield return new List<T>(1) { elementos[0] };
                yield break;
            }

            for (int i = 0; i < elementos.Count; i++)
            {
                var elementoAtual = elementos[i];

                var resto = new List<T>(elementos);
                resto.RemoveAt(i);

                foreach (var permutacaoAtual in GerarPermutacoes(resto))
                {
                    var permutacaoCompleta = new List<T> { elementoAtual };
                    permutacaoCompleta.AddRange(permutacaoAtual);

                    yield return permutacaoCompleta;
                }
            }
        }

        #endregion

        #region Desafio 8: Memorização com Yield (Difícil)

        /// <summary>
        /// DESAFIO: Crie um cache lazy que só calcula valores quando pedidos
        /// 
        /// Requisitos:
        /// - Usar yield return
        /// - Cachear resultados já calculados
        /// - Só executar função se valor não estiver em cache
        /// - Suportar sequências infinitas
        /// 
        /// Exemplo de uso:
        /// var fibonacci = Memorize(FibonacciInfinite());
        /// var first10 = fibonacci.Take(10); // Calcula 10
        /// var first20 = fibonacci.Take(20); // Reutiliza os 10 primeiros, calcula mais 10
        /// </summary>
        public static IEnumerable<T> Memorize<T>(IEnumerable<T> source)
        {
            //items processados serão armazenados aqui
            //usando o conceito de closure, cache e enumerator serão mantidos entre chamadas do iterador possibilitando o cache
            var cache = new List<T>();
            var enumerator = source.GetEnumerator();

            return Interator();

            IEnumerable<T> Interator()
            {
                int index = 0;
                while (true)
                {
                    if (index < cache.Count)
                    {
                        yield return cache[index];
                    }
                    else
                        if (enumerator.MoveNext())
                        {
                            var current = enumerator.Current;
                            cache.Add(current);
                            yield return current;
                        }
                        else
                        {
                            yield break;
                        }

                    index++;
                }
            }
        }

        #endregion

        #region Desafio 9: CartesianProduct (Difícil)

        /// <summary>
        /// DESAFIO: Produto cartesiano de múltiplas sequências
        /// 
        /// Requisitos:
        /// - Gerar todas as combinações possíveis
        /// - Usar yield return
        /// - Funcionar com número variável de sequências
        /// 
        /// Exemplo:
        /// Entrada: [[1,2], [10,20], [100,200]]
        /// Saída: 
        /// [1,10,100], [1,10,200], [1,20,100], [1,20,200],
        /// [2,10,100], [2,10,200], [2,20,100], [2,20,200]
        /// </summary>
        public static IEnumerable<List<T>> ProdutoCartesiano<T>(params IEnumerable<T>[] sequences)
        {
            if (sequences.Length == 0)
            {
                yield return new List<T>();
                yield break;
            }

            if (sequences.Length == 1)
            {
                foreach (var item in sequences[0])
                {
                    yield return new List<T> { item };
                }
                yield break;
            }

            var primeira = sequences[0];
            var resto = sequences.Skip(1).ToArray();

            var cartesianoDoResto = ProdutoCartesiano(resto);

            foreach (var item in primeira)
            {
                foreach (var combinacao in cartesianoDoResto)
                {
                    var novaCombinacao = new List<T> { item };
                    novaCombinacao.AddRange(combinacao);

                    yield return novaCombinacao;
                }
            }
        }

        #endregion

        #region Desafio 10: Merge Sorted Sequences (Difícil)

        /// <summary>
        /// DESAFIO: Mescle múltiplas sequências ordenadas em uma única sequência ordenada
        /// 
        /// Requisitos:
        /// - As sequências de entrada já estão ordenadas
        /// - Resultado deve estar ordenado
        /// - Usar yield return
        /// - Não carregar tudo em memória (lazy)
        /// - Usar uma estrutura de dados eficiente (heap/priority queue)
        /// 
        /// Exemplo:
        /// Seq1: [1, 5, 9]
        /// Seq2: [2, 4, 8]
        /// Seq3: [3, 6, 7]
        /// Resultado: [1, 2, 3, 4, 5, 6, 7, 8, 9]
        /// </summary>
        public static IEnumerable<T> MergeSorted<T>(params IEnumerable<T>[] sequences)
            where T : IComparable<T>
        {
            if (sequences == null || sequences.Length == 0)
                yield break;

            var priorityQueue = new PriorityQueue<(T valor, int sequenciaIndex), T>();

            var enumerators = new IEnumerator<T>[sequences.Length];

            for (int i = 0; i < sequences.Length; i++)
            {
                enumerators[i] = sequences[i].GetEnumerator();

                if (enumerators[i].MoveNext())
                {
                    var valor = enumerators[i].Current;
                    priorityQueue.Enqueue((valor, i), valor);
                }
            }

            while (priorityQueue.Count > 0)
            {
                var (valor, sequenciaIndex) = priorityQueue.Dequeue();

                yield return valor;

                if (enumerators[sequenciaIndex].MoveNext())
                {
                    var proximoValor = enumerators[sequenciaIndex].Current;
                    priorityQueue.Enqueue((proximoValor, sequenciaIndex), proximoValor);
                }
            }

            foreach (var enumerator in enumerators)
            {
                enumerator?.Dispose();
            }
        }

        #endregion

        #region Métodos de Teste

        /// <summary>
        /// Execute este método para testar suas implementações
        /// </summary>
        public static void TestarDesafios()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
            Console.WriteLine("║  TESTE DOS DESAFIOS - Implemente e teste aqui!       ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════╝\n");

            try
            {
                // Teste Desafio 1: Primos
                Console.WriteLine("=== Testando Desafio 1: Números Primos ===");
                var primos = GerarPrimos().Take(10);
                Console.WriteLine(string.Join(", ", primos));
                // Esperado: 2, 3, 5, 7, 11, 13, 17, 19, 23, 29
                Console.WriteLine("⚠️  Implemente GerarPrimos() primeiro!\n");
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  GerarPrimos() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 2: Duplicatas
                Console.WriteLine("=== Testando Desafio 2: Remover Duplicatas ===");
                var numeros = new[] { 1, 1, 2, 2, 2, 3, 1, 1, 4 };
                var resultado = RemoverDuplicatasConsecutivas(numeros);
                Console.WriteLine(string.Join(", ", resultado));
                // Esperado: 1, 2, 3, 1, 4
                Console.WriteLine("⚠️  Implemente RemoverDuplicatasConsecutivas() primeiro!\n");
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  RemoverDuplicatasConsecutivas() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 3: Intercalar
                Console.WriteLine("=== Testando Desafio 3: Intercalar Sequências ===");
                var seq1 = new[] { 1, 2, 3 };
                var seq2 = new[] { 10, 20, 30, 40, 50 };
                var resultado = IntercalarSequencias(seq1, seq2);
                Console.WriteLine(string.Join(", ", resultado));
                // Esperado: 1, 10, 2, 20, 3, 30, 40, 50
                Console.WriteLine("⚠️  Implemente IntercalarSequencias() primeiro!\n");
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  IntercalarSequencias() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 4: Range Customizado
                Console.WriteLine("=== Testando Desafio 4: Range Customizado ===");
                var range1 = RangeCustom(0, 10, 2);
                Console.WriteLine($"0 a 10, step 2: {string.Join(", ", range1)}");
                var range2 = RangeCustom(10, 0, -2);
                Console.WriteLine($"10 a 0, step -2: {string.Join(", ", range2)}");
                var range3 = RangeCustom(1, 10, 0.5);
                Console.WriteLine($"1 a 10, step 0.5: {string.Join(", ", range3)}");
                var range4 = RangeCustom(0, 10, -1);
                Console.WriteLine($"0 a 10, step -1: {string.Join(", ", range4)}");
                Console.WriteLine("⚠️  Implemente RangeCustom() primeiro!\n");
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  RangeCustom() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 5: Chunk
                Console.WriteLine("=== Testando Desafio 5: Chunk ===");
                var numeros = Enumerable.Range(1, 7);
                var chunks = numeros.Chunk(3);
                foreach (var chunk in chunks)
                    Console.WriteLine($"[{string.Join(", ", chunk)}]");
                // Esperado: [1, 2, 3], [4, 5, 6], [7]
                Console.WriteLine("⚠️  Implemente Chunk() primeiro!\n");
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  Chunk() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 6: TakeUntil
                Console.WriteLine("=== Testando Desafio 6: TakeUntil ===");
                var resultado = new[] { 1, 2, 3, 4, 5, 6 }.TakeUntil(x => x > 3);
                Console.WriteLine(string.Join(", ", resultado));
                // Resultado: [1, 2, 3, 4] (para quando encontra 4 que é > 3)                
                Console.WriteLine("⚠️  Implemente TakeUntil() primeiro!\n");
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  TakeUntil() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 7: Permutações
                Console.WriteLine("=== Testando Desafio 7: Permutações ===");
                var resultados = GerarPermutacoes([1, 2, 3]);
                foreach (var permutacao in resultados)
                    Console.WriteLine($"[{string.Join(", ", permutacao)}]");
                //Resultado: [1,2,3], [1,3,2], [2,1,3], [2,3,1], [3,1,2], [3,2,1]
                Console.WriteLine("⚠️  Implemente GerarPermutacoes() primeiro!\n");
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  GerarPermutacoes() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 8: Memorize
                Console.WriteLine("=== Testando Desafio 8: Memorize ===");
                // Cria um gerador que mostra quando calcula
                static IEnumerable<int> GerarComLog()
                {
                    for (int i = 1; i <= 30; i++)
                    {
                        Console.WriteLine($"  [Calculando] {i}");
                        yield return i;
                    }
                }

                var memoized = Memorize(GerarComLog());
                Console.WriteLine("\n1ª iteração - Take(5):");
                var first5 = memoized.Take(5).ToList();
                Console.WriteLine($"Resultado: {string.Join(", ", first5)}");

                Console.WriteLine("\n2ª iteração - Take(10) (deve reutilizar os 5 primeiros!):");
                var first10 = memoized.Take(10).ToList();
                Console.WriteLine($"Resultado: {string.Join(", ", first10)}");

                Console.WriteLine("\n3ª iteração - Take(10) de novo (deve reutilizar TUDO!):");
                var again10 = memoized.Take(10).ToList();
                Console.WriteLine($"Resultado: {string.Join(", ", again10)}");
                Console.WriteLine("⚠️  Implemente Memorize() primeiro!\n");
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  Memorize() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 9: Produto Cartesiano
                Console.WriteLine("=== Testando Desafio 9: Produto Cartesiano ===");
                var resultados = ProdutoCartesiano([[1, 2], [10, 20], [100, 200]]);
                foreach (var resultado in resultados)
                    Console.WriteLine($"Sequência: [{string.Join(", ", resultado)}]");
                Console.WriteLine("⚠️  Implemente ProdutoCartesiano() primeiro!\n");
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  ProdutoCartesiano() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 10: MergeSorted
                Console.WriteLine("=== Testando Desafio 10: MergeSorted ===");
                var seq1 = new[] { 1, 5, 9, 11 };
                var seq2 = new[] { 2, 4, 8 };
                var seq3 = new[] { 3, 6, 7, 12, 36 };
                var resultado = MergeSorted(seq1, seq2, seq3);
                Console.WriteLine($"Resultado: {string.Join(", ", resultado)}");
                // Esperado: 1, 2, 3, 4, 5, 6, 7, 8, 9
                Console.WriteLine("⚠️  Implemente MergeSorted() primeiro!\n");
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  MergeSorted() ainda não implementado\n");
            }

            Console.WriteLine("\n✓ Continue implementando os outros desafios!");
            Console.WriteLine("Descomente os testes acima conforme implementar cada método.");
        }



        #endregion
    }
}
