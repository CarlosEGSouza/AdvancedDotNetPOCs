# POCs - Conceitos Avançados do .NET

Este projeto contém Proof of Concepts (POCs) para aprofundamento em conceitos avançados do .NET.

## 📚 Tópicos Abordados

### 1. **Yield Return e Yield Break** ([01_YieldExamples.cs](AdvancedDotNetPOCs/01_YieldExamples.cs))
- Lazy evaluation e deferred execution
- Diferença entre listas tradicionais e yield
- Sequências infinitas (Fibonacci)
- Yield break para interrupção condicional
- Performance em grandes datasets
- Navegação recursiva em árvores
- Pipeline de transformações
- Paginação lazy

**Conceitos-chave:**
- State machine gerada pelo compilador
- IEnumerable e iteradores
- Economia de memória
- Composição de queries (similar ao LINQ)

### 2. **Expression Trees** ([02_ExpressionTreesExamples.cs](AdvancedDotNetPOCs/02_ExpressionTreesExamples.cs))
- Construção manual de Expression Trees
- Análise de lambda expressions
- Visitor Pattern para modificação
- Query builder dinâmico
- Property accessors de alta performance
- Mapper dinâmico (AutoMapper-like)
- SQL query generator (LINQ to SQL simplificado)

**Conceitos-chave:**
- Representação de código como dados (AST)
- Base do Entity Framework e LINQ
- Geração dinâmica de código
- Performance superior ao Reflection

### 3. **Task Schedulers** ([03_TaskSchedulersExamples.cs](AdvancedDotNetPOCs/03_TaskSchedulersExamples.cs))
- TaskScheduler padrão vs customizados
- LimitedConcurrencyLevelTaskScheduler (controle de recursos)
- PriorityTaskScheduler (priorização de tarefas)
- QueuedTaskScheduler (FIFO garantido)
- ThreadPerTaskScheduler (isolamento de threads)
- Rate limiting para chamadas de API
- Child tasks e observability

**Conceitos-chave:**
- Controle fino de concorrência
- Thread pool customizado
- Isolamento de recursos
- Debugging e profiling

### 4. **Span<T> e Memory<T>** ([04_SpanAndMemoryExamples.cs](AdvancedDotNetPOCs/04_SpanAndMemoryExamples.cs))
- Span vs Array (zero-copy operations)
- Performance em operações com strings
- CSV parsing eficiente
- Memory para operações assíncronas
- ArrayPool para reduzir GC pressure
- Interoperabilidade com código nativo
- MemoryMarshal para cenários avançados
- Algoritmos eficientes
- Image processing

**Conceitos-chave:**
- Ref struct (stack-only)
- Zero-copy semantics
- Redução de alocações
- Interop com unmanaged code

### 5. **Reflection** ([05_ReflectionExamples.cs](AdvancedDotNetPOCs/05_ReflectionExamples.cs))
- Type inspection e metadados
- Criação dinâmica de instâncias
- Invocação dinâmica de métodos
- Trabalho com propriedades e campos privados
- Custom attributes
- Assembly loading e type discovery
- Manipulação de tipos genéricos
- Performance comparison (Reflection vs Emit vs Expression)
- Sistema de plugins
- Object cloner (deep copy)

**Conceitos-chave:**
- Metadados em runtime
- Activator e dynamic invocation
- Attributes customizados
- Reflection.Emit para geração de IL
- Arquiteturas extensíveis

## 🚀 Como Executar

### Executar todos os exemplos:
```bash
cd AdvancedDotNetPOCs
dotnet run
```

### Executar um exemplo específico:
```bash
dotnet run 1    # Yield
dotnet run 2    # Expression Trees
dotnet run 3    # Task Schedulers
dotnet run 4    # Span/Memory
dotnet run 5    # Reflection
```

### Compilar o projeto:
```bash
dotnet build
```

## 📊 Estrutura do Projeto

```
AdvancedDotNetPOCs/
├── 01_YieldExamples.cs              # POC de Yield
├── 02_ExpressionTreesExamples.cs    # POC de Expression Trees
├── 03_TaskSchedulersExamples.cs     # POC de Task Schedulers
├── 04_SpanAndMemoryExamples.cs      # POC de Span<T> e Memory<T>
├── 05_ReflectionExamples.cs         # POC de Reflection
├── Program.cs                        # Programa principal
├── AdvancedDotNetPOCs.csproj        # Arquivo de projeto
└── README.md                         # Este arquivo
```

## 🎯 Objetivos de Aprendizado

### Para cada tópico, você irá aprender:

1. **Teoria**: Conceitos fundamentais e como funciona internamente
2. **Prática**: Exemplos de código executáveis e comentados
3. **Performance**: Comparações e benchmarks quando aplicável
4. **Casos de Uso**: Aplicações práticas e reais
5. **Best Practices**: Quando usar e quando evitar

## 💡 Dicas de Estudo

1. **Execute os exemplos**: Rode cada exemplo e observe a saída
2. **Modifique o código**: Experimente variações para entender melhor
3. **Use o debugger**: Coloque breakpoints e observe o comportamento
4. **Leia os comentários**: Cada exemplo tem documentação detalhada
5. **Compare performance**: Use os benchmarks para entender impacto
6. **Pesquise mais**: Use os conceitos como ponto de partida para pesquisas

## 📈 Próximos Passos

Após dominar estes conceitos, considere estudar:

- **Source Generators**: Geração de código em compile-time
- **Minimal APIs**: ASP.NET Core moderno
- **gRPC**: Comunicação de alta performance
- **Channels**: Producer-consumer patterns
- **System.Threading.Channels**: Async streams
- **IAsyncEnumerable**: Async iterators
- **ValueTask**: Otimizações de performance
- **Unsafe Code**: Ponteiros e manipulação direta de memória

## 🔗 Recursos Adicionais

### Documentação Oficial:
- [Microsoft Docs - Yield](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/yield)
- [Expression Trees](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/expression-trees/)
- [Task Schedulers](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskscheduler)
- [Span<T>](https://docs.microsoft.com/en-us/dotnet/api/system.span-1)
- [Reflection](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/reflection)

## 📝 Notas

- Todos os exemplos foram testados com .NET 8.0
- Alguns exemplos usam `unsafe` code (requer AllowUnsafeBlocks)
- Os benchmarks são indicativos e podem variar conforme o hardware
- Código é didático, algumas simplificações foram feitas para clareza

## 🤝 Contribuindo

Este é um projeto de estudo pessoal, mas sugestões são bem-vindas:
- Adicione novos exemplos
- Melhore a documentação
- Corrija bugs ou otimize código
- Sugira novos tópicos avançados

## ✅ Checklist de Domínio

Marque conforme você dominar cada tópico:

- [ ] Entendo quando e como usar `yield return`
- [ ] Sei criar e manipular Expression Trees
- [ ] Posso implementar custom Task Schedulers
- [ ] Domino o uso de Span<T> para otimização
- [ ] Sei usar Reflection de forma eficiente
- [ ] Entendo as implicações de performance de cada técnica
- [ ] Consigo aplicar estes conceitos em projetos reais

## 📜 Licença

Este projeto é para fins educacionais. Sinta-se livre para usar e modificar.

---