using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AdvancedDotNetPOCs.ExpressionTrees
{
    /// <summary>
    /// POC sobre Expression Trees
    /// 
    /// CONCEITOS:
    /// - Expression Trees: Representam código como dados (AST - Abstract Syntax Tree)
    /// - Permitem análise, modificação e compilação de código em runtime
    /// - Base do LINQ to SQL, Entity Framework, AutoMapper
    /// - Lambda Expressions podem ser convertidas em Expression Trees
    /// 
    /// BENEFÍCIOS:
    /// - Geração dinâmica de código
    /// - Tradução de queries para SQL/outras linguagens
    /// - Criação de DSLs (Domain Specific Languages)
    /// - Reflexão avançada com performance
    /// - Validações e mapeamentos dinâmicos
    /// </summary>
    public class ExpressionTreesExamples
    {
        #region Exemplo 1: Básico - Construindo Expression Trees Manualmente
        
        /// <summary>
        /// Cria uma expression tree manualmente: (x, y) => x + y
        /// </summary>
        public static void BasicExpressionTree()
        {
            Console.WriteLine("--- Exemplo 1: Expression Tree Básica ---");
            
            // Parâmetros
            ParameterExpression paramX = Expression.Parameter(typeof(int), "x");
            ParameterExpression paramY = Expression.Parameter(typeof(int), "y");
            
            // Operação: x + y
            BinaryExpression body = Expression.Add(paramX, paramY);
            
            // Lambda: (x, y) => x + y
            Expression<Func<int, int, int>> expression = 
                Expression.Lambda<Func<int, int, int>>(body, paramX, paramY);
            
            Console.WriteLine($"Expression: {expression}");
            
            // Compilar e executar
            Func<int, int, int> compiledFunc = expression.Compile();
            int result = compiledFunc(10, 20);
            
            Console.WriteLine($"Resultado: 10 + 20 = {result}");
        }
        
        #endregion
        
        #region Exemplo 2: Análise de Lambda Expressions
        
        /// <summary>
        /// Analisa uma lambda expression e extrai informações
        /// </summary>
        public static void AnalyzeLambdaExpression()
        {
            Console.WriteLine("\n--- Exemplo 2: Analisando Lambda Expression ---");
            
            Expression<Func<Person, bool>> expression = p => p.Age > 18 && p.Name.StartsWith("J");
            
            Console.WriteLine($"Expression completa: {expression}");
            Console.WriteLine($"Body Type: {expression.Body.NodeType}");
            Console.WriteLine($"Parameters: {string.Join(", ", expression.Parameters.Select(p => p.Name))}");
            
            // Navegar pela árvore
            if (expression.Body is BinaryExpression binaryExpr)
            {
                Console.WriteLine($"\nOperador binário: {binaryExpr.NodeType}");
                Console.WriteLine($"Lado esquerdo: {binaryExpr.Left}");
                Console.WriteLine($"Lado direito: {binaryExpr.Right}");
            }
        }
        
        #endregion
        
        #region Exemplo 3: Visitor Pattern - Modificando Expression Trees
        
        /// <summary>
        /// Visitor para substituir parâmetros em uma expression tree
        /// </summary>
        public class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParameter;
            private readonly ParameterExpression _newParameter;
            
            public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
            {
                _oldParameter = oldParameter;
                _newParameter = newParameter;
            }
            
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == _oldParameter ? _newParameter : base.VisitParameter(node);
            }
        }
        
        /// <summary>
        /// Combina duas expressions com operador AND
        /// </summary>
        public static Expression<Func<T, bool>> CombineExpressions<T>(
            Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            
            var replacer1 = new ParameterReplacer(expr1.Parameters[0], parameter);
            var replacer2 = new ParameterReplacer(expr2.Parameters[0], parameter);
            
            var body1 = replacer1.Visit(expr1.Body);
            var body2 = replacer2.Visit(expr2.Body);
            
            var combinedBody = Expression.AndAlso(body1, body2);
            
            return Expression.Lambda<Func<T, bool>>(combinedBody, parameter);
        }
        
        public static void ModifyExpressionTree()
        {
            Console.WriteLine("\n--- Exemplo 3: Modificando Expression Trees ---");
            
            Expression<Func<Person, bool>> expr1 = p => p.Age > 18;
            Expression<Func<Person, bool>> expr2 = p => p.Name.Length > 3;
            
            Console.WriteLine($"Expressão 1: {expr1}");
            Console.WriteLine($"Expressão 2: {expr2}");
            
            var combined = CombineExpressions(expr1, expr2);
            Console.WriteLine($"Expressão combinada: {combined}");
            
            // Testar
            var person = new Person { Name = "João", Age = 25 };
            var result = combined.Compile()(person);
            Console.WriteLine($"Resultado para {person.Name} (idade {person.Age}): {result}");
        }
        
        #endregion
        
        #region Exemplo 4: Query Builder Dinâmico
        
        /// <summary>
        /// Constrói queries dinâmicas usando Expression Trees
        /// Similar ao que o Entity Framework faz
        /// </summary>
        public class DynamicQueryBuilder<T>
        {
            public static Expression<Func<T, bool>> BuildPredicate(
                string propertyName,
                string operation,
                object value)
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                var property = Expression.Property(parameter, propertyName);
                var constant = Expression.Constant(value);
                
                Expression comparison = operation switch
                {
                    "==" => Expression.Equal(property, constant),
                    "!=" => Expression.NotEqual(property, constant),
                    ">" => Expression.GreaterThan(property, constant),
                    "<" => Expression.LessThan(property, constant),
                    ">=" => Expression.GreaterThanOrEqual(property, constant),
                    "<=" => Expression.LessThanOrEqual(property, constant),
                    "Contains" => Expression.Call(property, typeof(string).GetMethod("Contains", [typeof(string)])!, constant),
                    _ => throw new NotSupportedException($"Operação {operation} não suportada")
                };
                
                return Expression.Lambda<Func<T, bool>>(comparison, parameter);
            }
        }
        
        public static void DynamicQueryBuilderExample()
        {
            Console.WriteLine("\n--- Exemplo 4: Query Builder Dinâmico ---");
            
            var people = new List<Person>
            {
                new Person { Name = "João", Age = 25, Email = "joao@example.com" },
                new Person { Name = "Maria", Age = 30, Email = "maria@example.com" },
                new Person { Name = "Pedro", Age = 20, Email = "pedro@example.com" },
                new Person { Name = "Ana", Age = 35, Email = "ana@example.com" }
            };
            
            // Query dinâmica 1: Age > 25
            var predicate1 = DynamicQueryBuilder<Person>.BuildPredicate("Age", ">", 25);
            Console.WriteLine($"Query 1: {predicate1}");
            var result1 = people.AsQueryable().Where(predicate1).ToList();
            Console.WriteLine($"Resultado: {string.Join(", ", result1.Select(p => p.Name))}");
            
            // Query dinâmica 2: Name contains "Jo"
            var predicate2 = DynamicQueryBuilder<Person>.BuildPredicate("Name", "Contains", "Jo");
            Console.WriteLine($"\nQuery 2: {predicate2}");
            var result2 = people.AsQueryable().Where(predicate2).ToList();
            Console.WriteLine($"Resultado: {string.Join(", ", result2.Select(p => p.Name))}");
        }
        
        #endregion
        
        #region Exemplo 5: Property Accessor Generator (Performance)
        
        /// <summary>
        /// Gera acessores de propriedades usando Expression Trees
        /// Muito mais rápido que Reflection tradicional
        /// </summary>
        public class PropertyAccessor<T>
        {
            private static readonly Dictionary<string, Func<T, object>> _getters = new();
            private static readonly Dictionary<string, Action<T, object>> _setters = new();
            
            public static Func<T, object> GetPropertyGetter(string propertyName)
            {
                if (_getters.TryGetValue(propertyName, out var getter))
                    return getter;
                
                var parameter = Expression.Parameter(typeof(T), "obj");
                var property = Expression.Property(parameter, propertyName);
                var convert = Expression.Convert(property, typeof(object));
                var lambda = Expression.Lambda<Func<T, object>>(convert, parameter);
                
                getter = lambda.Compile();
                _getters[propertyName] = getter;
                
                return getter;
            }
            
            public static Action<T, object> GetPropertySetter(string propertyName)
            {
                if (_setters.TryGetValue(propertyName, out var setter))
                    return setter;
                
                var parameter = Expression.Parameter(typeof(T), "obj");
                var valueParameter = Expression.Parameter(typeof(object), "value");
                
                var property = Expression.Property(parameter, propertyName);
                var convert = Expression.Convert(valueParameter, property.Type);
                var assign = Expression.Assign(property, convert);
                var lambda = Expression.Lambda<Action<T, object>>(assign, parameter, valueParameter);
                
                setter = lambda.Compile();
                _setters[propertyName] = setter;
                
                return setter;
            }
        }
        
        public static void PropertyAccessorExample()
        {
            Console.WriteLine("\n--- Exemplo 5: Property Accessor (Performance) ---");
            
            var person = new Person { Name = "João", Age = 25 };
            
            // Getter usando Expression Tree
            var nameGetter = PropertyAccessor<Person>.GetPropertyGetter("Name");
            var name = nameGetter(person);
            Console.WriteLine($"Nome (via Expression): {name}");
            
            // Setter usando Expression Tree
            var ageSetter = PropertyAccessor<Person>.GetPropertySetter("Age");
            ageSetter(person, 30);
            Console.WriteLine($"Idade após set (via Expression): {person.Age}");
            
            // Benchmark básico
            Console.WriteLine("\nComparação de Performance:");
            var iterations = 1_000_000;
            
            // Reflection tradicional
            var prop = typeof(Person).GetProperty("Name");
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var _ = prop.GetValue(person);
            }
            sw.Stop();
            Console.WriteLine($"Reflection: {sw.ElapsedMilliseconds}ms");
            
            // Expression Tree compilada
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var _ = nameGetter(person);
            }
            sw.Stop();
            Console.WriteLine($"Expression Tree: {sw.ElapsedMilliseconds}ms");
            
            // Acesso direto (baseline)
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var _ = person.Name;
            }
            sw.Stop();
            Console.WriteLine($"Acesso direto: {sw.ElapsedMilliseconds}ms");
        }
        
        #endregion
        
        #region Exemplo 6: Mapper Dinâmico (AutoMapper-like)
        
        /// <summary>
        /// Implementação simplificada de um mapper usando Expression Trees
        /// </summary>
        public class SimpleMapper<TSource, TDestination> where TDestination : new()
        {
            private static Func<TSource, TDestination> _compiledMapper;
            
            public static TDestination Map(TSource source)
            {
                if (_compiledMapper == null)
                    _compiledMapper = CreateMapper();
                
                return _compiledMapper(source);
            }
            
            private static Func<TSource, TDestination> CreateMapper()
            {
                var sourceParam = Expression.Parameter(typeof(TSource), "source");
                var destVariable = Expression.Variable(typeof(TDestination), "dest");
                
                var expressions = new List<Expression>
                {
                    Expression.Assign(destVariable, Expression.New(typeof(TDestination)))
                };
                
                // Mapear propriedades com mesmo nome
                var sourceProps = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var destProps = typeof(TDestination).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite)
                    .ToDictionary(p => p.Name);
                
                foreach (var sourceProp in sourceProps)
                {
                    if (destProps.TryGetValue(sourceProp.Name, out var destProp) && 
                        destProp.PropertyType == sourceProp.PropertyType)
                    {
                        var sourceProperty = Expression.Property(sourceParam, sourceProp);
                        var destProperty = Expression.Property(destVariable, destProp);
                        var assign = Expression.Assign(destProperty, sourceProperty);
                        expressions.Add(assign);
                    }
                }
                
                expressions.Add(destVariable);
                
                var block = Expression.Block(new[] { destVariable }, expressions);
                var lambda = Expression.Lambda<Func<TSource, TDestination>>(block, sourceParam);
                
                return lambda.Compile();
            }
        }
        
        public static void MapperExample()
        {
            Console.WriteLine("\n--- Exemplo 6: Mapper Dinâmico ---");
            
            var person = new Person { Name = "João", Age = 25, Email = "joao@example.com" };
            
            var dto = SimpleMapper<Person, PersonDto>.Map(person);
            
            Console.WriteLine($"Person: Name={person.Name}, Age={person.Age}");
            Console.WriteLine($"PersonDto: Name={dto.Name}, Age={dto.Age}");
        }
        
        #endregion
        
        #region Exemplo 7: SQL Query Generator (LINQ to SQL simplificado)
        
        /// <summary>
        /// Converte Expression Trees em SQL queries
        /// Demonstra como ORMs como Entity Framework funcionam
        /// </summary>
        public class SqlQueryGenerator
        {
            public static string GenerateSelect<T>(Expression<Func<T, bool>> predicate)
            {
                var tableName = typeof(T).Name;
                var whereClause = GenerateWhereClause(predicate.Body);
                
                return $"SELECT * FROM {tableName} WHERE {whereClause}";
            }
            
            private static string GenerateWhereClause(Expression expression)
            {
                return expression switch
                {
                    BinaryExpression binary => GenerateBinaryExpression(binary),
                    MemberExpression member => member.Member.Name,
                    ConstantExpression constant => FormatValue(constant.Value),
                    MethodCallExpression method => GenerateMethodCall(method),
                    _ => throw new NotSupportedException($"Expressão não suportada: {expression.NodeType}")
                };
            }
            
            private static string GenerateBinaryExpression(BinaryExpression binary)
            {
                var left = GenerateWhereClause(binary.Left);
                var right = GenerateWhereClause(binary.Right);
                
                var op = binary.NodeType switch
                {
                    ExpressionType.Equal => "=",
                    ExpressionType.NotEqual => "!=",
                    ExpressionType.GreaterThan => ">",
                    ExpressionType.LessThan => "<",
                    ExpressionType.GreaterThanOrEqual => ">=",
                    ExpressionType.LessThanOrEqual => "<=",
                    ExpressionType.AndAlso => "AND",
                    ExpressionType.OrElse => "OR",
                    _ => throw new NotSupportedException($"Operador não suportado: {binary.NodeType}")
                };
                
                return $"({left} {op} {right})";
            }
            
            private static string GenerateMethodCall(MethodCallExpression method)
            {
                if (method.Method.Name == "StartsWith")
                {
                    var obj = GenerateWhereClause(method.Object);
                    var arg = GenerateWhereClause(method.Arguments[0]);
                    return $"{obj} LIKE {arg.TrimEnd('\'')}%'";
                }
                
                if (method.Method.Name == "Contains")
                {
                    var obj = GenerateWhereClause(method.Object);
                    var arg = GenerateWhereClause(method.Arguments[0]);
                    return $"{obj} LIKE '%{arg.Trim('\'')}%'";
                }
                
                throw new NotSupportedException($"Método não suportado: {method.Method.Name}");
            }
            
            private static string FormatValue(object value)
            {
                return value switch
                {
                    string s => $"'{s}'",
                    int i => i.ToString(),
                    bool b => b ? "1" : "0",
                    _ => value?.ToString() ?? "NULL"
                };
            }
        }
        
        public static void SqlGeneratorExample()
        {
            Console.WriteLine("\n--- Exemplo 7: SQL Query Generator ---");
            
            Expression<Func<Person, bool>> predicate1 = p => p.Age > 18;
            var sql1 = SqlQueryGenerator.GenerateSelect(predicate1);
            Console.WriteLine($"Query 1: {sql1}");
            
            Expression<Func<Person, bool>> predicate2 = p => p.Age > 18 && p.Name.StartsWith("J");
            var sql2 = SqlQueryGenerator.GenerateSelect(predicate2);
            Console.WriteLine($"Query 2: {sql2}");
            
            Expression<Func<Person, bool>> predicate3 = p => p.Email.Contains("example");
            var sql3 = SqlQueryGenerator.GenerateSelect(predicate3);
            Console.WriteLine($"Query 3: {sql3}");
        }
        
        #endregion
        
        #region Main Runner
        
        public static void RunExamples()
        {
            Console.WriteLine("=== POC: EXPRESSION TREES ===\n");
            
            BasicExpressionTree();
            AnalyzeLambdaExpression();
            ModifyExpressionTree();
            DynamicQueryBuilderExample();
            PropertyAccessorExample();
            MapperExample();
            SqlGeneratorExample();
            
            Console.WriteLine("\n=== FIM DOS EXEMPLOS ===");
        }
        
        #endregion
    }
    
    #region Classes de Suporte
    
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }
    }
    
    public class PersonDto
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
    
    #endregion
}
