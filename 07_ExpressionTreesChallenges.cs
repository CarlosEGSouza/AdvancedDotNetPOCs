using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualBasic;

namespace AdvancedDotNetPOCs.ExpressionTrees
{
    /// <summary>
    /// Desafios para praticar Expression Trees
    /// Complete cada método usando Expression Trees
    /// </summary>
    public static class ExpressionTreesChallenges
    {
        #region Desafio 1: Criar Operações Matemáticas (Fácil)

        /// <summary>
        /// DESAFIO: Crie uma função que gera lambdas para operações matemáticas básicas
        /// 
        /// Requisitos:
        /// - Recebe um operador ("+", "-", "*", "/")
        /// - Retorna uma lambda: (x, y) => x [operador] y
        /// - Use Expression.Add, Expression.Subtract, etc
        /// 
        /// Exemplo:
        /// var add = CriarOperacao("+");
        /// var resultado = add(10, 5); // 15
        /// </summary>
        public static Func<int, int, int> CriarOperacao(string operador)
        {
            var leftParam = Expression.Parameter(typeof(int), "x");
            var rightParam = Expression.Parameter(typeof(int), "y");

            var expressionOperator = operador switch
            {
                "+" => Expression.Add(leftParam, rightParam),
                "-" => Expression.Subtract(leftParam, rightParam),
                "*" => Expression.Multiply(leftParam, rightParam),
                "/" => Expression.Divide(leftParam, rightParam),
                _ => throw new NotSupportedException($"Operador '{operador}' não suportado")
            };

            var lambda = Expression.Lambda<Func<int, int, int>>(expressionOperator, leftParam, rightParam);

            return lambda.Compile();
        }

        #endregion

        #region Desafio 2: Property Getter Dinâmico (Fácil)

        /// <summary>
        /// DESAFIO: Crie um getter dinâmico para uma propriedade específica
        /// 
        /// Requisitos:
        /// - Recebe o nome da propriedade
        /// - Retorna uma função que lê essa propriedade
        /// - Use Expression.Property
        /// 
        /// Exemplo:
        /// var getAge = CriarGetter<Person>("Age");
        /// var age = getAge(person); // 25
        /// </summary>
        public static Func<T, object> CriarGetter<T>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T), "obj");
            var property = Expression.Property(parameter, propertyName);
            var converter = Expression.Convert(property, typeof(object));
            var lambda = Expression.Lambda<Func<T, object>>(converter, parameter);

            return lambda.Compile();
        }

        #endregion

        #region Desafio 3: Property Setter Dinâmico (Fácil/Médio)

        /// <summary>
        /// DESAFIO: Crie um setter dinâmico para uma propriedade específica
        /// 
        /// Requisitos:
        /// - Recebe o nome da propriedade
        /// - Retorna uma ação que altera essa propriedade
        /// - Use Expression.Assign
        /// 
        /// Exemplo:
        /// var setAge = CriarSetter<Person>("Age");
        /// setAge(person, 30);
        /// Console.WriteLine(person.Age); // 30
        /// </summary>
        public static Action<T, object> CriarSetter<T>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T), "obj");
            var value = Expression.Parameter(typeof(object), "value");

            var property = Expression.Property(parameter, propertyName);
            var converter = Expression.Convert(value, property.Type);

            var assign = Expression.Assign(property, converter);

            var lambda = Expression.Lambda<Action<T, object>>(assign, parameter, value);

            return lambda.Compile();
        }

        #endregion

        #region Desafio 4: Filtro Dinâmico AND/OR (Médio)

        /// <summary>
        /// DESAFIO: Combine dois predicados com operador AND ou OR
        /// 
        /// Requisitos:
        /// - Recebe dois Expression<Func<T, bool>>
        /// - Recebe um operador lógico ("AND" ou "OR")
        /// - Retorna a combinação dos predicados
        /// - Use Expression.AndAlso ou Expression.OrElse
        /// - Lembre-se: precisa unificar os parâmetros!
        /// 
        /// Exemplo:
        /// Expression<Func<Person, bool>> idadeAlta = p => p.Age > 18;
        /// Expression<Func<Person, bool>> nomeJoao = p => p.Name == "João";
        /// var filtro = CombinarFiltros(idadeAlta, nomeJoao, "AND");
        /// // Resultado: p => p.Age > 18 && p.Name == "João"
        /// </summary>
        public static Expression<Func<T, bool>> CombinarFiltros<T>(
            Expression<Func<T, bool>> filtro1,
            Expression<Func<T, bool>> filtro2,
            string operador)
        {
            //é preciso criar um novo parametro para aplicar nas duas expressões, porque sem isso não conseguimos acessar os valores corretamente
            var parameter = Expression.Parameter(typeof(T), "x");

            //o Expression.Invoke invoca a expressão informada aplicando o novo parâmetro informado
            var invoke1 = Expression.Invoke(filtro1, parameter);
            var invoke2 = Expression.Invoke(filtro2, parameter);

            var combined = operador switch
            {
                "AND" => Expression.AndAlso(invoke1, invoke2),
                "OR" => Expression.Or(invoke1, invoke2),
                _ => throw new InvalidOperationException($"Operador {operador} não suportado")
            };

            var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);

            return lambda;
        }

        #endregion

        #region Desafio 5: Construtor de Queries Dinâmicas (Médio)

        /// <summary>
        /// DESAFIO: Construa queries dinâmicas com múltiplos filtros
        /// 
        /// Requisitos:
        /// - Recebe uma lista de tuplas (propertyName, operator, value)
        /// - Combina todos os filtros com AND
        /// - Suporte operadores: "==", "!=", ">", "<", ">=", "<="
        /// 
        /// Exemplo:
        /// var filtros = new[]
        /// {
        ///     ("Age", ">", 18),
        ///     ("Name", "==", "João")
        /// };
        /// var query = ConstruirQuery<Person>(filtros);
        /// // Resultado: p => p.Age > 18 && p.Name == "João"
        /// </summary>
        public static Expression<Func<T, bool>> ConstruirQuery<T>(
            params (string property, string op, object value)[] filters)
        {
            if (filters.Length == 0)
                throw new InvalidOperationException("Informe os filtros");

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? combinedExpression = null;

            foreach (var filter in filters)
            {
                var property = Expression.Property(parameter, filter.property);
                var value = Expression.Constant(filter.value);

                var bodyExpression = filter.op switch
                {
                    "==" => Expression.Equal(property, value),
                    "!=" => Expression.NotEqual(property, value),
                    ">" => Expression.GreaterThan(property, value),
                    "<" => Expression.LessThan(property, value),
                    ">=" => Expression.GreaterThanOrEqual(property, value),
                    "<=" => Expression.LessThanOrEqual(property, value),
                    _ => throw new InvalidOperationException($"Operador {filter.op} inválido")
                };

                combinedExpression = combinedExpression == null
                    ? bodyExpression
                    : Expression.AndAlso(combinedExpression, bodyExpression);
            }

            if (combinedExpression is null)
                throw new Exception("Erro ao criar query");

            return Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
        }

        #endregion

        #region Desafio 6: Clone Profundo (Médio/Difícil)

        /// <summary>
        /// DESAFIO: Crie uma função de clonagem profunda usando Expression Trees
        /// 
        /// Requisitos:
        /// - Copiar todas as propriedades públicas com getter e setter
        /// - Criar nova instância do objeto
        /// - Use MemberInit ou Block
        /// 
        /// Exemplo:
        /// var clone = CriarClonador<Person>();
        /// var person2 = clone(person1);
        /// // person2 é uma cópia independente de person1
        /// </summary>
        public static Func<T, T> CriarClonador<T>() where T : new()
        {
            //criar um parametro de acordo com o tipo que será recebido pela Func
            var parameter = Expression.Parameter(typeof(T), "source");
            //crio uma variável para ser usada pela expressão, uma vez que o objeto destino não existe e será criado pela expressão
            var dest = Expression.Variable(typeof(T), "dest");

            var sourceProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var destProperties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                          .Where(p => p.CanWrite)
                                          .ToDictionary(p => p.Name);

            //Cria uma lista de Expression para armazenar todas as properts que serão mapeadas
            //Também inicializa a variável "dest" com uma nova instância de T
            var expressions = new List<Expression>
            {
                Expression.Assign(dest, Expression.New(typeof(T)))
            };

            foreach (var property in sourceProperties)
            {
                if (destProperties.TryGetValue(property.Name, out var destProp) && destProp.PropertyType == property.PropertyType)
                {
                    var sourceP = Expression.Property(parameter, property);
                    var destP = Expression.Property(dest, destProp);
                    var assing = Expression.Assign(destP, sourceP);
                    expressions.Add(assing);
                }
            }

            expressions.Add(dest);

            var block = Expression.Block([dest], expressions);
            return Expression.Lambda<Func<T, T>>(block, parameter).Compile();
        }

        #endregion

        #region Desafio 7: Comparador Dinâmico (Médio/Difícil)

        /// <summary>
        /// DESAFIO: Crie um comparador dinâmico por propriedade
        /// 
        /// Requisitos:
        /// - Recebe nome da propriedade
        /// - Recebe direção (true = crescente, false = decrescente)
        /// - Retorna um Comparison<T> que compara objetos por essa propriedade
        /// - Use Expression.Call com CompareTo
        /// 
        /// Exemplo:
        /// var compareByAge = CriarComparador<Person>("Age", ascending: true);
        /// pessoas.Sort(compareByAge);
        /// </summary>
        public static Comparison<T> CriarComparador<T>(string propertyName, bool ascending)
        {
            var paramX = Expression.Parameter(typeof(T), "x");
            var paramY = Expression.Parameter(typeof(T), "y");

            var propX = Expression.Property(paramX, propertyName);
            var propY = Expression.Property(paramY, propertyName);

            var compareMethodInfo = propX.Type.GetMethod("CompareTo", [propX.Type])!;
            var callCompareTo = Expression.Call(propX, compareMethodInfo, propY);

            Expression result = ascending ? callCompareTo : Expression.Multiply(callCompareTo, Expression.Constant(-1));

            return Expression.Lambda<Comparison<T>>(result, paramX, paramY).Compile();
        }

        #endregion

        #region Desafio 8: Selector de Propriedades Múltiplas (Difícil)

        /// <summary>
        /// DESAFIO: Crie um objeto anônimo com propriedades selecionadas
        /// 
        /// Requisitos:
        /// - Recebe array de nomes de propriedades
        /// - Retorna função que cria objeto dinâmico com essas propriedades
        /// - Use Expression.New com tipo anônimo ou Dictionary
        /// 
        /// Exemplo:
        /// var selector = SelecionarPropriedades<Person>("Name", "Age");
        /// var result = selector(person);
        /// // result = { Name = "João", Age = 25 }
        /// </summary>
        public static Func<T, Dictionary<string, object>> SelecionarPropriedades<T>(
            params string[] propertyNames)
        {
            // TODO: Implemente aqui
            throw new NotImplementedException();
        }

        #endregion

        #region Desafio 9: Validador de Regras (Difícil)

        /// <summary>
        /// DESAFIO: Sistema de validação com regras dinâmicas
        /// 
        /// Requisitos:
        /// - Recebe múltiplas regras de validação
        /// - Retorna função que valida objeto e retorna lista de erros
        /// - Cada regra tem: predicado + mensagem de erro
        /// - Use Expression.Condition para criar validações
        /// 
        /// Exemplo:
        /// var validador = CriarValidador<Person>(
        ///     (p => p.Age >= 18, "Idade deve ser maior que 18"),
        ///     (p => p.Name != null, "Nome é obrigatório")
        /// );
        /// var erros = validador(person);
        /// </summary>
        public static Func<T, List<string>> CriarValidador<T>(
            params (Expression<Func<T, bool>> rule, string message)[] rules)
        {
            // TODO: Implemente aqui
            throw new NotImplementedException();
        }

        #endregion

        #region Desafio 10: Builder de Objetos Fluente (Difícil)

        /// <summary>
        /// DESAFIO: Crie um builder fluente usando Expression Trees
        /// 
        /// Requisitos:
        /// - Cria uma cadeia de métodos: With("Property", value)
        /// - Cada chamada retorna o builder para encadeamento
        /// - Build() cria o objeto final com todas as propriedades
        /// - Use Dictionary para armazenar valores temporários
        /// - Use Expression.MemberInit para criar objeto final
        /// 
        /// Exemplo:
        /// var builder = new FluentBuilder<Person>()
        ///     .With("Name", "João")
        ///     .With("Age", 25)
        ///     .Build();
        /// </summary>
        public class FluentBuilder<T> where T : new()
        {
            // TODO: Implemente aqui
            private readonly Dictionary<string, object> _values = new();

            public FluentBuilder<T> With(string propertyName, object value)
            {
                throw new NotImplementedException();
            }

            public T Build()
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Desafio Bônus: Mini ORM Query Builder (Muito Difícil)

        /// <summary>
        /// DESAFIO BÔNUS: Crie um mini ORM que converte Expression Trees em SQL
        /// 
        /// Requisitos:
        /// - Where, OrderBy, Select, Take, Skip
        /// - Gerar SQL a partir das expressions
        /// - Traduzir operadores C# para SQL
        /// - Suportar joins simples
        /// 
        /// Exemplo:
        /// var query = new QueryBuilder<Person>()
        ///     .Where(p => p.Age > 18)
        ///     .OrderBy(p => p.Name)
        ///     .Select(p => new { p.Name, p.Age })
        ///     .Take(10);
        /// var sql = query.ToSql();
        /// // SELECT Name, Age FROM Person WHERE Age > 18 ORDER BY Name LIMIT 10
        /// </summary>
        public class QueryBuilder<T>
        {
            // TODO: Implemente aqui
            private Expression<Func<T, bool>> _whereClause;
            private string _orderByProperty;
            private int? _take;
            private int? _skip;

            public QueryBuilder<T> Where(Expression<Func<T, bool>> predicate)
            {
                throw new NotImplementedException();
            }

            public QueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
            {
                throw new NotImplementedException();
            }

            public QueryBuilder<T> Take(int count)
            {
                throw new NotImplementedException();
            }

            public QueryBuilder<T> Skip(int count)
            {
                throw new NotImplementedException();
            }

            public string ToSql()
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Métodos de Teste

        public static void TestarDesafios()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
            Console.WriteLine("║  DESAFIOS - EXPRESSION TREES                          ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════╝\n");

            try
            {
                // Teste Desafio 1
                Console.WriteLine("=== Desafio 1: Operações Matemáticas ===");
                var add = CriarOperacao("+");
                var sub = CriarOperacao("-");
                var mul = CriarOperacao("*");
                var div = CriarOperacao("/");
                Console.WriteLine($"10 + 5 = {add(10, 5)}");
                Console.WriteLine($"10 - 5 = {sub(10, 5)}");
                Console.WriteLine($"10 * 5 = {mul(10, 5)}");
                Console.WriteLine($"10 / 5 = {div(10, 5)}");
                Console.WriteLine();
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  CriarOperacao() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 2
                Console.WriteLine("=== Desafio 2: Property Getter ===");
                var person = new TestPerson { Name = "João", Age = 25 };
                var getName = CriarGetter<TestPerson>("Name");
                var getAge = CriarGetter<TestPerson>("Age");
                Console.WriteLine($"Nome: {getName(person)}");
                Console.WriteLine($"Idade: {getAge(person)}");
                Console.WriteLine();
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  CriarGetter() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 3
                Console.WriteLine("=== Desafio 3: Property Setter ===");
                var person = new TestPerson { Name = "João", Age = 25 };
                var setAge = CriarSetter<TestPerson>("Age");
                Console.WriteLine($"Idade antes: {person.Age}");
                setAge(person, 30);
                Console.WriteLine($"Idade depois: {person.Age}");
                Console.WriteLine();
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  CriarSetter() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 4
                Console.WriteLine("=== Desafio 4: Combinar Filtros ===");
                Expression<Func<TestPerson, bool>> filtro1 = p => p.Age > 18;
                Expression<Func<TestPerson, bool>> filtro2 = p => p.Name.StartsWith("J");
                var filtroAnd = CombinarFiltros(filtro1, filtro2, "AND");
                var filtroOr = CombinarFiltros(filtro1, filtro2, "OR");

                var person1 = new TestPerson { Name = "João", Age = 25 };
                var person2 = new TestPerson { Name = "Maria", Age = 30 };

                Console.WriteLine($"João (25) com AND: {filtroAnd.Compile()(person1)}");
                Console.WriteLine($"Maria (30) com AND: {filtroAnd.Compile()(person2)}");
                Console.WriteLine($"Maria (30) com OR: {filtroOr.Compile()(person2)}");
                Console.WriteLine();
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  CombinarFiltros() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 5
                Console.WriteLine("=== Desafio 5: Construir Query ===");
                var query = ConstruirQuery<TestPerson>(
                    ("Age", ">", 18),
                    ("Name", "==", "João")
                );
                Console.WriteLine($"Query: {query}");

                var person1 = new TestPerson { Name = "João", Age = 25 };
                var person2 = new TestPerson { Name = "João", Age = 15 };
                Console.WriteLine($"João (25): {query.Compile()(person1)}");
                Console.WriteLine($"João (15): {query.Compile()(person2)}");
                Console.WriteLine();
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  ConstruirQuery() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 6
                Console.WriteLine("=== Desafio 6: Clone Profundo ===");
                var cloner = CriarClonador<TestPerson>();
                var original = new TestPerson { Name = "João", Age = 25 };
                var clone = cloner(original);

                clone.Age = 30;
                Console.WriteLine($"Original: {original.Name}, {original.Age}");
                Console.WriteLine($"Clone: {clone.Name}, {clone.Age}");
                Console.WriteLine($"São objetos diferentes? {!ReferenceEquals(original, clone)}");
                Console.WriteLine();
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  CriarClonador() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 7
                Console.WriteLine("=== Desafio 7: Comparador Dinâmico ===");
                var people = new List<TestPerson>
                {
                    new TestPerson { Name = "Carlos", Age = 30 },
                    new TestPerson { Name = "Ana", Age = 25 },
                    new TestPerson { Name = "Bruno", Age = 35 }
                };

                var compareByName = CriarComparador<TestPerson>("Name", ascending: true);
                var sortedByName = new List<TestPerson>(people);
                sortedByName.Sort(compareByName);

                Console.WriteLine("Ordenado por Nome:");
                foreach (var p in sortedByName)
                    Console.WriteLine($"  {p.Name}, {p.Age}");
                Console.WriteLine();

                var compareByAge = CriarComparador<TestPerson>("Age", ascending: false);
                var sortedByAge = new List<TestPerson>(people);
                sortedByAge.Sort(compareByAge);

                Console.WriteLine("Ordenado por Age (desc):");
                foreach (var p in sortedByAge)
                    Console.WriteLine($"  {p.Name}, {p.Age}");
                Console.WriteLine();                
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  CriarComparador() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 8
                Console.WriteLine("=== Desafio 8: Selector de Propriedades ===");
                var person = new TestPerson { Name = "João", Age = 25 };
                var selector = SelecionarPropriedades<TestPerson>("Name", "Age");
                var result = selector(person);

                Console.WriteLine("Propriedades selecionadas:");
                foreach (var kvp in result)
                    Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
                Console.WriteLine();
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  SelecionarPropriedades() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 9
                Console.WriteLine("=== Desafio 9: Validador de Regras ===");
                var validador = CriarValidador<TestPerson>(
                    (p => p.Age >= 18, "Idade deve ser maior ou igual a 18"),
                    (p => !string.IsNullOrEmpty(p.Name), "Nome é obrigatório"),
                    (p => p.Name.Length >= 3, "Nome deve ter pelo menos 3 caracteres")
                );

                var person1 = new TestPerson { Name = "João", Age = 25 };
                var person2 = new TestPerson { Name = "Jo", Age = 15 };

                var erros1 = validador(person1);
                var erros2 = validador(person2);

                Console.WriteLine($"João (25): {erros1.Count} erros");
                Console.WriteLine($"Jo (15): {erros2.Count} erros");
                foreach (var erro in erros2)
                    Console.WriteLine($"  - {erro}");
                Console.WriteLine();
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  CriarValidador() ainda não implementado\n");
            }

            try
            {
                // Teste Desafio 10
                Console.WriteLine("=== Desafio 10: Fluent Builder ===");
                var person = new FluentBuilder<TestPerson>()
                    .With("Name", "João")
                    .With("Age", 25)
                    .Build();

                Console.WriteLine($"Person criado: {person.Name}, {person.Age}");
                Console.WriteLine();
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("⚠️  FluentBuilder ainda não implementado\n");
            }

            Console.WriteLine("\n✓ Continue implementando os desafios!");
        }

        #endregion

        #region Classe de Teste

        public class TestPerson
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        #endregion
    }
}
