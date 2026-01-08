using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace AdvancedDotNetPOCs.Reflection
{
    /// <summary>
    /// POC sobre Reflection
    /// 
    /// CONCEITOS:
    /// - Reflection: Inspeção e manipulação de metadados em runtime
    /// - Type, MethodInfo, PropertyInfo, FieldInfo, etc.
    /// - Activator: Criação dinâmica de instâncias
    /// - Attributes: Metadados customizados
    /// - Emit: Geração de código IL em runtime
    /// - Expression Trees: Alternativa performática ao Reflection
    /// 
    /// BENEFÍCIOS:
    /// - Análise de tipos em runtime
    /// - Criação dinâmica de objetos
    /// - Invocação dinâmica de métodos
    /// - Plugins e arquiteturas extensíveis
    /// - Serialização customizada
    /// - Dependency Injection containers
    /// 
    /// DESVANTAGENS:
    /// - Performance overhead significativo
    /// - Type safety perdida
    /// - Dificuldade de debugging
    /// - Problemas com AOT (Ahead-of-Time compilation)
    /// </summary>
    public class ReflectionExamples
    {
        #region Exemplo 1: Básico - Type Inspection
        
        /// <summary>
        /// Inspeção básica de tipos usando Reflection
        /// </summary>
        public static void BasicTypeInspection()
        {
            Console.WriteLine("--- Exemplo 1: Type Inspection ---");
            
            Type personType = typeof(Person);
            
            Console.WriteLine($"Nome: {personType.Name}");
            Console.WriteLine($"Full Name: {personType.FullName}");
            Console.WriteLine($"Namespace: {personType.Namespace}");
            Console.WriteLine($"É classe? {personType.IsClass}");
            Console.WriteLine($"É abstrato? {personType.IsAbstract}");
            Console.WriteLine($"É sealed? {personType.IsSealed}");
            
            // Propriedades
            Console.WriteLine("\nPropriedades:");
            var properties = personType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                Console.WriteLine($"  - {prop.Name}: {prop.PropertyType.Name} " +
                                $"(CanRead: {prop.CanRead}, CanWrite: {prop.CanWrite})");
            }
            
            // Métodos
            Console.WriteLine("\nMétodos:");
            var methods = personType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                var parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                Console.WriteLine($"  - {method.ReturnType.Name} {method.Name}({parameters})");
            }
            
            // Construtores
            Console.WriteLine("\nConstrutores:");
            var constructors = personType.GetConstructors();
            foreach (var ctor in constructors)
            {
                var parameters = string.Join(", ", ctor.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
                Console.WriteLine($"  - {personType.Name}({parameters})");
            }
        }
        
        #endregion
        
        #region Exemplo 2: Criação Dinâmica de Instâncias
        
        /// <summary>
        /// Diferentes formas de criar instâncias dinamicamente
        /// </summary>
        public static void DynamicInstanceCreation()
        {
            Console.WriteLine("\n--- Exemplo 2: Criação Dinâmica de Instâncias ---");
            
            Type personType = typeof(Person);
            
            // Método 1: Activator.CreateInstance
            var person1 = (Person)Activator.CreateInstance(personType);
            Console.WriteLine($"Activator.CreateInstance: {person1}");
            
            // Método 2: Activator com parâmetros
            var person2 = (Person)Activator.CreateInstance(personType, "João", 25);
            Console.WriteLine($"Activator com parâmetros: {person2}");
            
            // Método 3: ConstructorInfo.Invoke
            var constructor = personType.GetConstructor(new[] { typeof(string), typeof(int) });
            var person3 = (Person)constructor.Invoke(new object[] { "Maria", 30 });
            Console.WriteLine($"ConstructorInfo.Invoke: {person3}");
            
            // Método 4: Criar tipo genérico
            Type genericListType = typeof(List<>);
            Type listOfInts = genericListType.MakeGenericType(typeof(int));
            var list = (System.Collections.IList)Activator.CreateInstance(listOfInts);
            list.Add(1);
            list.Add(2);
            list.Add(3);
            Console.WriteLine($"Lista genérica criada: {string.Join(", ", list.Cast<int>())}");
        }
        
        #endregion
        
        #region Exemplo 3: Invocação Dinâmica de Métodos
        
        /// <summary>
        /// Invocar métodos dinamicamente usando Reflection
        /// </summary>
        public static void DynamicMethodInvocation()
        {
            Console.WriteLine("\n--- Exemplo 3: Invocação Dinâmica de Métodos ---");
            
            var person = new Person("João", 25);
            Type type = person.GetType();
            
            // Invocar método sem parâmetros
            var celebrateMethod = type.GetMethod("CelebrateBirthday");
            celebrateMethod.Invoke(person, null);
            Console.WriteLine($"Idade após aniversário: {person.Age}");
            
            // Invocar método com parâmetros
            var greetMethod = type.GetMethod("Greet");
            var result = (string)greetMethod.Invoke(person, new object[] { "Olá" });
            Console.WriteLine($"Resultado do Greet: {result}");
            
            // Invocar método privado
            var privateMethod = type.GetMethod("SecretMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            var secret = (string)privateMethod.Invoke(person, null);
            Console.WriteLine($"Método privado: {secret}");
            
            // Invocar método estático
            var staticMethod = type.GetMethod("GetTypeName", BindingFlags.Public | BindingFlags.Static);
            var typeName = (string)staticMethod.Invoke(null, null);
            Console.WriteLine($"Método estático: {typeName}");
        }
        
        #endregion
        
        #region Exemplo 4: Propriedades e Campos
        
        /// <summary>
        /// Trabalhar com propriedades e campos dinamicamente
        /// </summary>
        public static void PropertiesAndFields()
        {
            Console.WriteLine("\n--- Exemplo 4: Propriedades e Campos ---");
            
            var person = new Person("João", 25);
            Type type = person.GetType();
            
            // Ler propriedade
            var nameProp = type.GetProperty("Name");
            var name = (string)nameProp.GetValue(person);
            Console.WriteLine($"Nome lido via reflection: {name}");
            
            // Escrever propriedade
            nameProp.SetValue(person, "João Silva");
            Console.WriteLine($"Nome após SetValue: {person.Name}");
            
            // Trabalhar com campo privado
            var privateField = type.GetField("_internalId", BindingFlags.NonPublic | BindingFlags.Instance);
            if (privateField != null)
            {
                var internalId = privateField.GetValue(person);
                Console.WriteLine($"Campo privado _internalId: {internalId}");
                
                privateField.SetValue(person, Guid.NewGuid());
                Console.WriteLine($"Novo _internalId: {privateField.GetValue(person)}");
            }
            
            // Indexer (propriedades indexadas)
            var employee = new Employee { Name = "Carlos" };
            employee["Department"] = "IT";
            employee["Level"] = "Senior";
            
            var indexer = typeof(Employee).GetProperty("Item");
            var dept = (string)indexer.GetValue(employee, new object[] { "Department" });
            Console.WriteLine($"Indexer via reflection: Department = {dept}");
        }
        
        #endregion
        
        #region Exemplo 5: Custom Attributes
        
        /// <summary>
        /// Trabalhar com atributos customizados
        /// </summary>
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method)]
        public class DocumentationAttribute : Attribute
        {
            public string Description { get; set; }
            public string Author { get; set; }
            public string Version { get; set; }
            
            public DocumentationAttribute(string description)
            {
                Description = description;
            }
        }
        
        [AttributeUsage(AttributeTargets.Property)]
        public class ValidateAttribute : Attribute
        {
            public int MinLength { get; set; }
            public int MaxLength { get; set; }
            public bool Required { get; set; }
        }
        
        [Documentation("Representa um produto no sistema", Author = "João", Version = "1.0")]
        public class Product
        {
            [Validate(Required = true, MinLength = 3, MaxLength = 100)]
            [Documentation("Nome do produto")]
            public string Name { get; set; }
            
            [Validate(Required = true)]
            [Documentation("Preço do produto")]
            public decimal Price { get; set; }
            
            [Documentation("Valida o produto", Author = "João")]
            public bool Validate()
            {
                return !string.IsNullOrEmpty(Name) && Price > 0;
            }
        }
        
        public static void CustomAttributesExample()
        {
            Console.WriteLine("\n--- Exemplo 5: Custom Attributes ---");
            
            Type productType = typeof(Product);
            
            // Atributos na classe
            var classAttrs = productType.GetCustomAttributes<DocumentationAttribute>();
            foreach (var attr in classAttrs)
            {
                Console.WriteLine($"Classe: {attr.Description} (Autor: {attr.Author}, Versão: {attr.Version})");
            }
            
            // Atributos nas propriedades
            Console.WriteLine("\nPropriedades com validação:");
            var properties = productType.GetProperties();
            foreach (var prop in properties)
            {
                var validateAttr = prop.GetCustomAttribute<ValidateAttribute>();
                if (validateAttr != null)
                {
                    Console.WriteLine($"  {prop.Name}:");
                    Console.WriteLine($"    Required: {validateAttr.Required}");
                    Console.WriteLine($"    MinLength: {validateAttr.MinLength}");
                    Console.WriteLine($"    MaxLength: {validateAttr.MaxLength}");
                }
                
                var docAttr = prop.GetCustomAttribute<DocumentationAttribute>();
                if (docAttr != null)
                {
                    Console.WriteLine($"    Doc: {docAttr.Description}");
                }
            }
            
            // Validador dinâmico baseado em atributos
            var product = new Product { Name = "PC", Price = 1000 };
            var validationErrors = ValidateObject(product);
            
            if (validationErrors.Any())
            {
                Console.WriteLine("\nErros de validação:");
                foreach (var error in validationErrors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }
            else
            {
                Console.WriteLine("\nProduto válido!");
            }
        }
        
        private static List<string> ValidateObject(object obj)
        {
            var errors = new List<string>();
            var type = obj.GetType();
            
            foreach (var prop in type.GetProperties())
            {
                var validateAttr = prop.GetCustomAttribute<ValidateAttribute>();
                if (validateAttr == null) continue;
                
                var value = prop.GetValue(obj);
                
                if (validateAttr.Required && value == null)
                {
                    errors.Add($"{prop.Name} é obrigatório");
                }
                
                if (value is string strValue)
                {
                    if (validateAttr.MinLength > 0 && strValue.Length < validateAttr.MinLength)
                    {
                        errors.Add($"{prop.Name} deve ter pelo menos {validateAttr.MinLength} caracteres");
                    }
                    
                    if (validateAttr.MaxLength > 0 && strValue.Length > validateAttr.MaxLength)
                    {
                        errors.Add($"{prop.Name} deve ter no máximo {validateAttr.MaxLength} caracteres");
                    }
                }
            }
            
            return errors;
        }
        
        #endregion
        
        #region Exemplo 6: Assembly Loading e Type Discovery
        
        /// <summary>
        /// Carregar assemblies e descobrir tipos dinamicamente
        /// </summary>
        public static void AssemblyLoadingExample()
        {
            Console.WriteLine("\n--- Exemplo 6: Assembly Loading ---");
            
            // Assembly atual
            var currentAssembly = Assembly.GetExecutingAssembly();
            Console.WriteLine($"Assembly atual: {currentAssembly.GetName().Name}");
            
            // Todos os tipos no assembly
            var types = currentAssembly.GetTypes()
                .Where(t => t.Namespace == "AdvancedDotNetPOCs.Reflection")
                .Take(5);
            
            Console.WriteLine("\nTipos neste namespace:");
            foreach (var type in types)
            {
                Console.WriteLine($"  - {type.Name}");
            }
            
            // Encontrar tipos que implementam interface
            var interfaceType = typeof(IProcessor);
            var implementingTypes = currentAssembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t));
            
            Console.WriteLine($"\nTipos que implementam {interfaceType.Name}:");
            foreach (var type in implementingTypes)
            {
                Console.WriteLine($"  - {type.Name}");
                
                // Criar instância e executar
                var instance = (IProcessor)Activator.CreateInstance(type);
                instance.Process("Teste");
            }
            
            // Encontrar tipos com atributo específico
            var documentedTypes = currentAssembly.GetTypes()
                .Where(t => t.GetCustomAttribute<DocumentationAttribute>() != null);
            
            Console.WriteLine("\nTipos com DocumentationAttribute:");
            foreach (var type in documentedTypes)
            {
                var attr = type.GetCustomAttribute<DocumentationAttribute>();
                Console.WriteLine($"  - {type.Name}: {attr.Description}");
            }
        }
        
        #endregion
        
        #region Exemplo 7: Generic Type Manipulation
        
        /// <summary>
        /// Trabalhar com tipos genéricos via Reflection
        /// </summary>
        public static void GenericTypeManipulation()
        {
            Console.WriteLine("\n--- Exemplo 7: Generic Types ---");
            
            // Criar tipo genérico fechado
            Type openType = typeof(Repository<>);
            Type closedType = openType.MakeGenericType(typeof(Person));
            
            var repository = Activator.CreateInstance(closedType);
            
            // Invocar método genérico
            var addMethod = closedType.GetMethod("Add");
            addMethod.Invoke(repository, new object[] { new Person("João", 25) });
            addMethod.Invoke(repository, new object[] { new Person("Maria", 30) });
            
            var getAllMethod = closedType.GetMethod("GetAll");
            var items = getAllMethod.Invoke(repository, null);
            
            Console.WriteLine($"Itens no repositório: {items}");
            
            // Trabalhar com métodos genéricos
            var helperType = typeof(GenericHelper);
            var genericMethod = helperType.GetMethod("ProcessList");
            
            // Fechar método genérico com tipo específico
            var specificMethod = genericMethod.MakeGenericMethod(typeof(int));
            specificMethod.Invoke(null, new object[] { new List<int> { 1, 2, 3, 4, 5 } });
            
            // Verificar se tipo é genérico
            Console.WriteLine($"\nList<int> é genérico? {typeof(List<int>).IsGenericType}");
            Console.WriteLine($"Definição genérica: {typeof(List<int>).GetGenericTypeDefinition()}");
            Console.WriteLine($"Argumentos genéricos: {string.Join(", ", typeof(List<int>).GetGenericArguments().Select(t => t.Name))}");
        }
        
        #endregion
        
        #region Exemplo 8: Performance Comparison
        
        /// <summary>
        /// Comparação de performance entre diferentes abordagens
        /// </summary>
        public static void PerformanceComparison()
        {
            Console.WriteLine("\n--- Exemplo 8: Performance Comparison ---");
            
            var person = new Person("João", 25);
            const int iterations = 1_000_000;
            
            // 1. Acesso direto
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var _ = person.Name;
            }
            sw.Stop();
            Console.WriteLine($"Acesso direto: {sw.ElapsedMilliseconds}ms");
            
            // 2. Reflection tradicional
            var prop = typeof(Person).GetProperty("Name");
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var _ = prop.GetValue(person);
            }
            sw.Stop();
            Console.WriteLine($"Reflection (GetValue): {sw.ElapsedMilliseconds}ms");
            
            // 3. Reflection com delegate compilado
            var compiled = CreatePropertyGetter<Person, string>("Name");
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var _ = compiled(person);
            }
            sw.Stop();
            Console.WriteLine($"Compiled Expression: {sw.ElapsedMilliseconds}ms");
            
            // 4. Emit (código IL gerado)
            var emitted = CreatePropertyGetterEmit<Person, string>("Name");
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                var _ = emitted(person);
            }
            sw.Stop();
            Console.WriteLine($"Emit (IL gerado): {sw.ElapsedMilliseconds}ms");
        }
        
        private static Func<TObject, TProperty> CreatePropertyGetter<TObject, TProperty>(string propertyName)
        {
            var param = System.Linq.Expressions.Expression.Parameter(typeof(TObject), "obj");
            var property = System.Linq.Expressions.Expression.Property(param, propertyName);
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<TObject, TProperty>>(property, param);
            return lambda.Compile();
        }
        
        private static Func<TObject, TProperty> CreatePropertyGetterEmit<TObject, TProperty>(string propertyName)
        {
            var method = new DynamicMethod(
                name: "Get_" + propertyName,
                returnType: typeof(TProperty),
                parameterTypes: new[] { typeof(TObject) },
                m: typeof(ReflectionExamples).Module,
                skipVisibility: true);
            
            var il = method.GetILGenerator();
            var propInfo = typeof(TObject).GetProperty(propertyName);
            var getMethod = propInfo.GetGetMethod();
            
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, getMethod);
            il.Emit(OpCodes.Ret);
            
            return (Func<TObject, TProperty>)method.CreateDelegate(typeof(Func<TObject, TProperty>));
        }
        
        #endregion
        
        #region Exemplo 9: Plugin System
        
        /// <summary>
        /// Sistema de plugins usando Reflection
        /// </summary>
        public interface IPlugin
        {
            string Name { get; }
            string Version { get; }
            void Execute();
        }
        
        public class PluginLoader
        {
            public static List<IPlugin> LoadPlugins(Assembly assembly)
            {
                var plugins = new List<IPlugin>();
                var pluginType = typeof(IPlugin);
                
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && pluginType.IsAssignableFrom(t));
                
                foreach (var type in types)
                {
                    try
                    {
                        var plugin = (IPlugin)Activator.CreateInstance(type);
                        plugins.Add(plugin);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao carregar plugin {type.Name}: {ex.Message}");
                    }
                }
                
                return plugins;
            }
        }
        
        public class SamplePlugin : IPlugin
        {
            public string Name => "Sample Plugin";
            public string Version => "1.0.0";
            
            public void Execute()
            {
                Console.WriteLine($"Executando {Name} v{Version}");
            }
        }
        
        public class AnotherPlugin : IPlugin
        {
            public string Name => "Another Plugin";
            public string Version => "2.0.0";
            
            public void Execute()
            {
                Console.WriteLine($"Executando {Name} v{Version}");
            }
        }
        
        public static void PluginSystemExample()
        {
            Console.WriteLine("\n--- Exemplo 9: Plugin System ---");
            
            var assembly = Assembly.GetExecutingAssembly();
            var plugins = PluginLoader.LoadPlugins(assembly);
            
            Console.WriteLine($"Plugins carregados: {plugins.Count}");
            
            foreach (var plugin in plugins)
            {
                plugin.Execute();
            }
        }
        
        #endregion
        
        #region Exemplo 10: Object Cloner (Deep Copy)
        
        /// <summary>
        /// Clonador de objetos usando Reflection (deep copy)
        /// </summary>
        public static class ObjectCloner
        {
            public static T Clone<T>(T source)
            {
                if (source == null) return default;
                
                var type = source.GetType();
                
                // Tipos primitivos e strings
                if (type.IsPrimitive || type == typeof(string))
                    return source;
                
                // Arrays
                if (type.IsArray)
                {
                    var elementType = type.GetElementType();
                    var sourceArray = source as Array;
                    var clonedArray = Array.CreateInstance(elementType, sourceArray.Length);
                    
                    for (int i = 0; i < sourceArray.Length; i++)
                    {
                        clonedArray.SetValue(Clone(sourceArray.GetValue(i)), i);
                    }
                    
                    return (T)(object)clonedArray;
                }
                
                // Objetos
                var cloned = Activator.CreateInstance(type);
                
                // Clonar campos
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    var value = field.GetValue(source);
                    var clonedValue = Clone(value);
                    field.SetValue(cloned, clonedValue);
                }
                
                // Clonar propriedades
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead && p.CanWrite);
                
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(source);
                    var clonedValue = Clone(value);
                    prop.SetValue(cloned, clonedValue);
                }
                
                return (T)cloned;
            }
        }
        
        public static void ObjectClonerExample()
        {
            Console.WriteLine("\n--- Exemplo 10: Object Cloner ---");
            
            var original = new Person("João", 25)
            {
                Address = new Address { Street = "Rua A", City = "São Paulo" }
            };
            
            var cloned = ObjectCloner.Clone(original);
            
            Console.WriteLine($"Original: {original.Name}, {original.Address.City}");
            Console.WriteLine($"Clonado: {cloned.Name}, {cloned.Address.City}");
            
            // Modificar o clone não afeta o original
            cloned.Name = "Maria";
            cloned.Address.City = "Rio de Janeiro";
            
            Console.WriteLine($"\nApós modificar clone:");
            Console.WriteLine($"Original: {original.Name}, {original.Address.City}");
            Console.WriteLine($"Clonado: {cloned.Name}, {cloned.Address.City}");
        }
        
        #endregion
        
        #region Main Runner
        
        public static void RunExamples()
        {
            Console.WriteLine("=== POC: REFLECTION ===\n");
            
            BasicTypeInspection();
            DynamicInstanceCreation();
            DynamicMethodInvocation();
            PropertiesAndFields();
            CustomAttributesExample();
            AssemblyLoadingExample();
            GenericTypeManipulation();
            PerformanceComparison();
            PluginSystemExample();
            ObjectClonerExample();
            
            Console.WriteLine("\n=== FIM DOS EXEMPLOS ===");
        }
        
        #endregion
    }
    
    #region Classes de Suporte
    
    public class Person
    {
        private Guid _internalId = Guid.NewGuid();
        
        public string Name { get; set; }
        public int Age { get; set; }
        public Address Address { get; set; }
        
        public Person()
        {
            Name = "Unknown";
            Age = 0;
        }
        
        public Person(string name, int age)
        {
            Name = name;
            Age = age;
        }
        
        public void CelebrateBirthday()
        {
            Age++;
        }
        
        public string Greet(string greeting)
        {
            return $"{greeting}, meu nome é {Name}!";
        }
        
        private string SecretMethod()
        {
            return "Este é um método privado!";
        }
        
        public static string GetTypeName()
        {
            return "Person";
        }
        
        public override string ToString()
        {
            return $"Person(Name={Name}, Age={Age})";
        }
    }
    
    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
    }
    
    public class Employee
    {
        public string Name { get; set; }
        private Dictionary<string, string> _data = new Dictionary<string, string>();
        
        public string this[string key]
        {
            get => _data.ContainsKey(key) ? _data[key] : null;
            set => _data[key] = value;
        }
    }
    
    public interface IProcessor
    {
        void Process(string data);
    }
    
    public class StringProcessor : IProcessor
    {
        public void Process(string data)
        {
            Console.WriteLine($"    StringProcessor: {data.ToUpper()}");
        }
    }
    
    public class DataProcessor : IProcessor
    {
        public void Process(string data)
        {
            Console.WriteLine($"    DataProcessor: [{data.Length} caracteres]");
        }
    }
    
    public class Repository<T>
    {
        private List<T> _items = new List<T>();
        
        public void Add(T item)
        {
            _items.Add(item);
        }
        
        public List<T> GetAll()
        {
            return _items;
        }
        
        public override string ToString()
        {
            return $"Repository<{typeof(T).Name}> com {_items.Count} itens";
        }
    }
    
    public static class GenericHelper
    {
        public static void ProcessList<T>(List<T> items)
        {
            Console.WriteLine($"Processando lista de {typeof(T).Name} com {items.Count} itens");
        }
    }
    
    #endregion
}
