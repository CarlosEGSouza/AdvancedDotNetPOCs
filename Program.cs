using System;
using System.Threading.Tasks;

namespace AdvancedDotNetPOCs
{
    /// <summary>
    /// Programa principal para executar todas as POCs de conceitos avançados do .NET
    /// 
    /// Este projeto contém exemplos práticos e teóricos para os seguintes tópicos:
    /// 1. Yield Return e Yield Break
    /// 2. Expression Trees
    /// 3. Task Schedulers
    /// 4. Span<T> e Memory<T>
    /// 5. Reflection
    /// 6. Yield Challenges (Desafios Práticos)
    /// 7. Yield Challenges - Para Você Praticar
    /// 
    /// Execute este programa para ver todos os exemplos em ação.
    /// Cada arquivo pode também ser executado individualmente.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║             POCs - CONCEITOS AVANÇADOS DO .NET               ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            if (args.Length > 0)
            {
                await RunSpecificExample(args[0]);
            }
            else
            {
                await RunAllExamples();
            }
            
            Console.WriteLine("\n\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    EXEMPLOS CONCLUÍDOS                       ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine("\nPressione qualquer tecla para sair...");
        }
        
        static async Task RunAllExamples()
        {
            Console.WriteLine("Executando TODOS os exemplos...\n");
            
            await RunExample("1", "Yield Return e Yield Break");
            await RunExample("2", "Expression Trees");
            await RunExample("3", "Task Schedulers");
            await RunExample("4", "Span<T> e Memory<T>");
            await RunExample("5", "Reflection");
        }
        
        static async Task RunSpecificExample(string choice)
        {
            var example = choice switch
            {
                "1" => "Yield Return e Yield Break",
                "2" => "Expression Trees",
                "3" => "Task Schedulers",
                "4" => "Span<T> e Memory<T>",
                "5" => "Reflection",
                "6" => "Yield - Seus Desafios",
                _ => null
            };
            
            if (example != null)
            {
                await RunExample(choice, example);
            }
            else
            {
                Console.WriteLine("Opção inválida! Use:");
                Console.WriteLine("  dotnet run          - Executar todos");
                Console.WriteLine("  dotnet run 1        - Yield");
                Console.WriteLine("  dotnet run 2        - Expression Trees");
                Console.WriteLine("  dotnet run 3        - Task Schedulers");
                Console.WriteLine("  dotnet run 4        - Span/Memory");
                Console.WriteLine("  dotnet run 5        - Reflection");
                Console.WriteLine("  dotnet run 6        - Yield - Seus Desafios");
            }
        }
        
        static async Task RunExample(string number, string name)
        {
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║  {number}. {name,-56} ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            try
            {
                switch (number)
                {
                    case "1":
                        Yield.YieldExamples.RunExamples();
                        break;
                    case "2":
                        ExpressionTrees.ExpressionTreesExamples.RunExamples();
                        break;
                    case "3":
                        await TaskSchedulers.TaskSchedulerExamples.RunExamples();
                        break;
                    case "4":
                        await SpanAndMemory.SpanAndMemoryExamples.RunExamples();
                        break;
                    case "5":
                        Reflection.ReflectionExamples.RunExamples();
                        break;
                    case "6":
                        Yield.YieldChallengesForPractice.TestarDesafios();
                        break;
                }
                
                Console.WriteLine($"\n✓ Exemplo {number} concluído com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ Erro ao executar exemplo {number}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            
            Console.WriteLine("\n" + new string('─', 64));
        }
    }
}
