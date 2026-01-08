using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedDotNetPOCs.TaskSchedulers
{
    /// <summary>
    /// POC sobre Task Schedulers
    /// 
    /// CONCEITOS:
    /// - TaskScheduler: Controla quando e onde as Tasks são executadas
    /// - TaskScheduler.Default: Usa o ThreadPool
    /// - TaskScheduler.FromCurrentSynchronizationContext(): Para UI threads
    /// - Custom Schedulers: Controle total sobre execução
    /// 
    /// BENEFÍCIOS:
    /// - Controle de concorrência (limitar threads)
    /// - Priorização de tarefas
    /// - Isolamento de recursos
    /// - Debugging e profiling
    /// - Testes mais determinísticos
    /// </summary>
    public class TaskSchedulerExamples
    {
        #region Exemplo 1: TaskScheduler Padrão
        
        public static async Task DefaultTaskSchedulerExample()
        {
            Console.WriteLine("--- Exemplo 1: TaskScheduler Padrão ---");
            Console.WriteLine($"Thread Principal: {Thread.CurrentThread.ManagedThreadId}");
            
            var tasks = Enumerable.Range(1, 5).Select(async i =>
            {
                await Task.Delay(100);
                Console.WriteLine($"Task {i} executando na thread {Thread.CurrentThread.ManagedThreadId}");
                return i;
            });
            
            await Task.WhenAll(tasks);
            
            Console.WriteLine($"TaskScheduler.Current: {TaskScheduler.Current.GetType().Name}");
            Console.WriteLine($"TaskScheduler.Default: {TaskScheduler.Default.GetType().Name}");
        }
        
        #endregion
        
        #region Exemplo 2: LimitedConcurrencyLevelTaskScheduler
        
        /// <summary>
        /// Custom TaskScheduler que limita o número de tarefas concorrentes
        /// Útil para controlar uso de recursos (DB connections, API calls, etc)
        /// </summary>
        public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
        {
            private readonly int _maxDegreeOfParallelism;
            private readonly LinkedList<Task> _tasks = new LinkedList<Task>();
            private int _delegatesQueuedOrRunning = 0;
            
            public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
            {
                if (maxDegreeOfParallelism < 1)
                    throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism));
                
                _maxDegreeOfParallelism = maxDegreeOfParallelism;
            }
            
            protected sealed override void QueueTask(Task task)
            {
                lock (_tasks)
                {
                    _tasks.AddLast(task);
                    if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                    {
                        ++_delegatesQueuedOrRunning;
                        NotifyThreadPoolOfPendingWork();
                    }
                }
            }
            
            private void NotifyThreadPoolOfPendingWork()
            {
                ThreadPool.UnsafeQueueUserWorkItem(_ =>
                {
                    try
                    {
                        while (true)
                        {
                            Task item;
                            lock (_tasks)
                            {
                                if (_tasks.Count == 0)
                                {
                                    --_delegatesQueuedOrRunning;
                                    break;
                                }
                                
                                item = _tasks.First.Value;
                                _tasks.RemoveFirst();
                            }
                            
                            TryExecuteTask(item);
                        }
                    }
                    finally
                    {
                        // Se houver mais tarefas, notifica outra thread
                        lock (_tasks)
                        {
                            if (_tasks.Count > 0 && _delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                            {
                                ++_delegatesQueuedOrRunning;
                                NotifyThreadPoolOfPendingWork();
                            }
                        }
                    }
                }, null);
            }
            
            protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                if (!taskWasPreviouslyQueued)
                    return TryExecuteTask(task);
                
                return false;
            }
            
            protected sealed override IEnumerable<Task> GetScheduledTasks()
            {
                bool lockTaken = false;
                try
                {
                    Monitor.TryEnter(_tasks, ref lockTaken);
                    if (lockTaken)
                        return _tasks.ToArray();
                    else
                        throw new NotSupportedException();
                }
                finally
                {
                    if (lockTaken)
                        Monitor.Exit(_tasks);
                }
            }
            
            public override int MaximumConcurrencyLevel => _maxDegreeOfParallelism;
        }
        
        public static async Task LimitedConcurrencyExample()
        {
            Console.WriteLine("\n--- Exemplo 2: Limited Concurrency Scheduler ---");
            
            var scheduler = new LimitedConcurrencyLevelTaskScheduler(2); // Máximo 2 tarefas simultâneas
            var factory = new TaskFactory(scheduler);
            
            var stopwatch = Stopwatch.StartNew();
            var tasks = Enumerable.Range(1, 6).Select(i =>
                factory.StartNew(() =>
                {
                    var threadId = Thread.CurrentThread.ManagedThreadId;
                    Console.WriteLine($"[{stopwatch.ElapsedMilliseconds}ms] Task {i} iniciada na thread {threadId}");
                    Thread.Sleep(500); // Simula trabalho
                    Console.WriteLine($"[{stopwatch.ElapsedMilliseconds}ms] Task {i} finalizada na thread {threadId}");
                    return i;
                })
            );
            
            await Task.WhenAll(tasks);
            stopwatch.Stop();
            
            Console.WriteLine($"\nTempo total: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine("Observe que apenas 2 tarefas executam simultaneamente!");
        }
        
        #endregion
        
        #region Exemplo 3: PriorityTaskScheduler
        
        /// <summary>
        /// TaskScheduler que executa tarefas com base em prioridade
        /// </summary>
        public class PriorityTaskScheduler : TaskScheduler
        {
            private readonly int _maxDegreeOfParallelism;
            private readonly ConcurrentDictionary<Task, int> _taskPriorities = new();
            private readonly object _lock = new object();
            private readonly SortedSet<PriorityTask> _tasks = new SortedSet<PriorityTask>(new PriorityTaskComparer());
            private int _delegatesQueuedOrRunning = 0;
            
            public PriorityTaskScheduler(int maxDegreeOfParallelism = -1)
            {
                _maxDegreeOfParallelism = maxDegreeOfParallelism == -1
                    ? Environment.ProcessorCount
                    : maxDegreeOfParallelism;
            }
            
            public void QueueTask(Task task, int priority)
            {
                _taskPriorities[task] = priority;
                QueueTask(task);
            }
            
            protected override void QueueTask(Task task)
            {
                var priority = _taskPriorities.GetOrAdd(task, 0);
                
                lock (_lock)
                {
                    _tasks.Add(new PriorityTask(task, priority));
                    
                    if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                    {
                        ++_delegatesQueuedOrRunning;
                        NotifyThreadPoolOfPendingWork();
                    }
                }
            }
            
            private void NotifyThreadPoolOfPendingWork()
            {
                ThreadPool.UnsafeQueueUserWorkItem(_ =>
                {
                    try
                    {
                        while (true)
                        {
                            PriorityTask priorityTask;
                            lock (_lock)
                            {
                                if (_tasks.Count == 0)
                                {
                                    --_delegatesQueuedOrRunning;
                                    break;
                                }
                                
                                priorityTask = _tasks.Max; // Maior prioridade
                                _tasks.Remove(priorityTask);
                            }
                            
                            TryExecuteTask(priorityTask.Task);
                        }
                    }
                    finally
                    {
                        lock (_lock)
                        {
                            if (_tasks.Count > 0 && _delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                            {
                                ++_delegatesQueuedOrRunning;
                                NotifyThreadPoolOfPendingWork();
                            }
                        }
                    }
                }, null);
            }
            
            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;
            
            protected override IEnumerable<Task> GetScheduledTasks()
            {
                lock (_lock)
                {
                    return _tasks.Select(pt => pt.Task).ToArray();
                }
            }
            
            public override int MaximumConcurrencyLevel => _maxDegreeOfParallelism;
            
            private class PriorityTask
            {
                public Task Task { get; }
                public int Priority { get; }
                
                public PriorityTask(Task task, int priority)
                {
                    Task = task;
                    Priority = priority;
                }
            }
            
            private class PriorityTaskComparer : IComparer<PriorityTask>
            {
                public int Compare(PriorityTask x, PriorityTask y)
                {
                    var priorityComparison = x.Priority.CompareTo(y.Priority);
                    return priorityComparison != 0 ? priorityComparison : x.Task.Id.CompareTo(y.Task.Id);
                }
            }
        }
        
        public static async Task PrioritySchedulerExample()
        {
            Console.WriteLine("\n--- Exemplo 3: Priority Task Scheduler ---");
            
            var scheduler = new PriorityTaskScheduler(maxDegreeOfParallelism: 1);
            var factory = new TaskFactory(scheduler);
            
            var tasks = new List<Task>();
            
            // Agendar tarefas com diferentes prioridades
            for (int i = 1; i <= 5; i++)
            {
                int taskId = i;
                int priority = 6 - i; // Prioridades: 5, 4, 3, 2, 1
                
                var task = new Task(() =>
                {
                    Console.WriteLine($"Executando Task {taskId} (Prioridade: {priority})");
                    Thread.Sleep(200);
                });
                
                scheduler.QueueTask(task, priority);
                task.Start(scheduler);
                tasks.Add(task);
            }
            
            await Task.WhenAll(tasks);
            Console.WriteLine("Observe que as tarefas foram executadas por prioridade!");
        }
        
        #endregion
        
        #region Exemplo 4: QueuedTaskScheduler (FIFO)
        
        /// <summary>
        /// TaskScheduler que garante execução em ordem FIFO (First In, First Out)
        /// </summary>
        public class QueuedTaskScheduler : TaskScheduler
        {
            private readonly ConcurrentQueue<Task> _tasks = new ConcurrentQueue<Task>();
            private int _isProcessing = 0;
            
            protected override void QueueTask(Task task)
            {
                _tasks.Enqueue(task);
                
                if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) == 0)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(_ => ProcessQueue(), null);
                }
            }
            
            private void ProcessQueue()
            {
                try
                {
                    while (_tasks.TryDequeue(out Task task))
                    {
                        TryExecuteTask(task);
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _isProcessing, 0);
                    
                    // Verifica se há novas tarefas
                    if (!_tasks.IsEmpty && Interlocked.CompareExchange(ref _isProcessing, 1, 0) == 0)
                    {
                        ThreadPool.UnsafeQueueUserWorkItem(_ => ProcessQueue(), null);
                    }
                }
            }
            
            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;
            
            protected override IEnumerable<Task> GetScheduledTasks() => _tasks.ToArray();
            
            public override int MaximumConcurrencyLevel => 1;
        }
        
        public static async Task QueuedSchedulerExample()
        {
            Console.WriteLine("\n--- Exemplo 4: Queued (FIFO) Task Scheduler ---");
            
            var scheduler = new QueuedTaskScheduler();
            var factory = new TaskFactory(scheduler);
            
            var tasks = Enumerable.Range(1, 5).Select(i =>
                factory.StartNew(() =>
                {
                    Console.WriteLine($"Executando Task {i} na ordem");
                    Thread.Sleep(100);
                })
            );
            
            await Task.WhenAll(tasks);
            Console.WriteLine("Todas as tarefas foram executadas em ordem FIFO!");
        }
        
        #endregion
        
        #region Exemplo 5: ThreadPerTaskScheduler
        
        /// <summary>
        /// TaskScheduler que cria uma thread dedicada para cada tarefa
        /// Útil para tarefas de longa duração ou que precisam de isolamento
        /// </summary>
        public class ThreadPerTaskScheduler : TaskScheduler
        {
            protected override void QueueTask(Task task)
            {
                var thread = new Thread(() =>
                {
                    TryExecuteTask(task);
                })
                {
                    IsBackground = true,
                    Name = $"TaskThread-{task.Id}"
                };
                
                thread.Start();
            }
            
            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                return TryExecuteTask(task);
            }
            
            protected override IEnumerable<Task> GetScheduledTasks()
            {
                return Enumerable.Empty<Task>();
            }
        }
        
        public static async Task ThreadPerTaskExample()
        {
            Console.WriteLine("\n--- Exemplo 5: Thread Per Task Scheduler ---");
            
            var scheduler = new ThreadPerTaskScheduler();
            var factory = new TaskFactory(scheduler);
            
            var tasks = Enumerable.Range(1, 3).Select(i =>
                factory.StartNew(() =>
                {
                    var thread = Thread.CurrentThread;
                    Console.WriteLine($"Task {i}: Thread ID={thread.ManagedThreadId}, Name={thread.Name}");
                    Thread.Sleep(200);
                })
            );
            
            await Task.WhenAll(tasks);
            Console.WriteLine("Cada tarefa executou em sua própria thread!");
        }
        
        #endregion
        
        #region Exemplo 6: Caso de Uso Real - Rate Limiting API Calls
        
        /// <summary>
        /// Exemplo prático: Limitar chamadas de API para respeitar rate limits
        /// </summary>
        public class ApiClient
        {
            private readonly LimitedConcurrencyLevelTaskScheduler _scheduler;
            private readonly TaskFactory _factory;
            
            public ApiClient(int maxConcurrentRequests)
            {
                _scheduler = new LimitedConcurrencyLevelTaskScheduler(maxConcurrentRequests);
                _factory = new TaskFactory(_scheduler);
            }
            
            public Task<string> CallApiAsync(string endpoint, int id)
            {
                return _factory.StartNew(() =>
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Chamando API: {endpoint}/{id}");
                    Thread.Sleep(500); // Simula chamada HTTP
                    var result = $"Response from {endpoint}/{id}";
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Resposta recebida: {endpoint}/{id}");
                    return result;
                });
            }
        }
        
        public static async Task RateLimitingExample()
        {
            Console.WriteLine("\n--- Exemplo 6: Rate Limiting API Calls ---");
            
            var apiClient = new ApiClient(maxConcurrentRequests: 2);
            
            var stopwatch = Stopwatch.StartNew();
            
            // Fazer 10 chamadas, mas apenas 2 simultâneas
            var calls = Enumerable.Range(1, 10)
                .Select(i => apiClient.CallApiAsync("/users", i))
                .ToList();
            
            var results = await Task.WhenAll(calls);
            
            stopwatch.Stop();
            Console.WriteLine($"\nTotal de chamadas: {results.Length}");
            Console.WriteLine($"Tempo total: {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"Tempo médio por chamada: {stopwatch.ElapsedMilliseconds / results.Length}ms");
        }
        
        #endregion
        
        #region Exemplo 7: Child Tasks e TaskScheduler
        
        public static async Task ChildTasksExample()
        {
            Console.WriteLine("\n--- Exemplo 7: Child Tasks com Custom Scheduler ---");
            
            var scheduler = new LimitedConcurrencyLevelTaskScheduler(2);
            var factory = new TaskFactory(scheduler);
            
            var parentTask = factory.StartNew(() =>
            {
                Console.WriteLine($"Parent task iniciada na thread {Thread.CurrentThread.ManagedThreadId}");
                
                var childTasks = Enumerable.Range(1, 4).Select(i =>
                    Task.Factory.StartNew(() =>
                    {
                        Console.WriteLine($"  Child {i} na thread {Thread.CurrentThread.ManagedThreadId}");
                        Thread.Sleep(300);
                    }, CancellationToken.None, TaskCreationOptions.AttachedToParent, scheduler)
                ).ToArray();
                
                Task.WaitAll(childTasks);
                Console.WriteLine("Parent task finalizada");
            });
            
            await parentTask;
        }
        
        #endregion
        
        #region Exemplo 8: TaskScheduler Observability
        
        /// <summary>
        /// TaskScheduler com logging e métricas para observability
        /// </summary>
        public class ObservableTaskScheduler : TaskScheduler
        {
            private readonly TaskScheduler _innerScheduler;
            private int _queuedCount = 0;
            private int _executedCount = 0;
            
            public int QueuedCount => _queuedCount;
            public int ExecutedCount => _executedCount;
            
            public ObservableTaskScheduler(TaskScheduler innerScheduler)
            {
                _innerScheduler = innerScheduler ?? TaskScheduler.Default;
            }
            
            protected override void QueueTask(Task task)
            {
                Interlocked.Increment(ref _queuedCount);
                Console.WriteLine($"[Observable] Task {task.Id} enfileirada. Total: {_queuedCount}");
                
                Task.Factory.StartNew(() =>
                {
                    var sw = Stopwatch.StartNew();
                    TryExecuteTask(task);
                    sw.Stop();
                    
                    Interlocked.Increment(ref _executedCount);
                    Console.WriteLine($"[Observable] Task {task.Id} executada em {sw.ElapsedMilliseconds}ms. Total executadas: {_executedCount}");
                }, CancellationToken.None, TaskCreationOptions.None, _innerScheduler);
            }
            
            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => false;
            
            protected override IEnumerable<Task> GetScheduledTasks() => Enumerable.Empty<Task>();
        }
        
        public static async Task ObservabilityExample()
        {
            Console.WriteLine("\n--- Exemplo 8: Observable Task Scheduler ---");
            
            var innerScheduler = new LimitedConcurrencyLevelTaskScheduler(2);
            var observableScheduler = new ObservableTaskScheduler(innerScheduler);
            var factory = new TaskFactory(observableScheduler);
            
            var tasks = Enumerable.Range(1, 5).Select(i =>
                factory.StartNew(() =>
                {
                    Thread.Sleep(200);
                    return i * i;
                })
            );
            
            await Task.WhenAll(tasks);
            
            Console.WriteLine($"\nResumo:");
            Console.WriteLine($"  Tarefas enfileiradas: {observableScheduler.QueuedCount}");
            Console.WriteLine($"  Tarefas executadas: {observableScheduler.ExecutedCount}");
        }
        
        #endregion
        
        #region Main Runner
        
        public static async Task RunExamples()
        {
            Console.WriteLine("=== POC: TASK SCHEDULERS ===\n");
            
            await DefaultTaskSchedulerExample();
            await LimitedConcurrencyExample();
            await PrioritySchedulerExample();
            await QueuedSchedulerExample();
            await ThreadPerTaskExample();
            await RateLimitingExample();
            await ChildTasksExample();
            await ObservabilityExample();
            
            Console.WriteLine("\n=== FIM DOS EXEMPLOS ===");
        }
        
        #endregion
    }
}
