using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public abstract class Key : IModelledKey
    {
        protected Key(IEnumerable<IModelledColumn> columns, DatabaseKeyType keyType)
        {
            if (columns == null || columns.Empty() || columns.AnyNull())
                throw new ArgumentNullException(nameof(columns));

            Columns = columns;
            KeyType = keyType;
        }

        public IEnumerable<IModelledColumn> Columns { get; }

        public DatabaseKeyType KeyType { get; }

        public PropertyInfo Property { get; set; }

        public class Primary : Key
        {
            public Primary(params IModelledColumn[] columns)
                : this(columns as IEnumerable<IModelledColumn>)
            {
            }

            public Primary(IEnumerable<IModelledColumn> columns)
                : base(columns, DatabaseKeyType.Primary)

            {
                var nullableColumns = columns
                    .Where(c => c.IsNullable)
                    .Select(c => c.Property.Name);

                if (nullableColumns.Any())
                    throw new ArgumentException("Nullable columns cannot be members of a primary key. Nullable column properties: " + nullableColumns.Join(", "), nameof(columns));
            }
        }

        public class Unique : Key
        {
            public Unique(params IModelledColumn[] columns)
                : this(columns as IEnumerable<IModelledColumn>)
            {
            }

            public Unique(IEnumerable<IModelledColumn> columns)
                : base(columns, DatabaseKeyType.Unique)
            {
            }
        }

        // not to be used directly, only required to make implementing Key.Foreign<T> a lot easier
        public abstract class ForeignKey : Key
        {
            protected ForeignKey(IEnumerable<IModelledColumn> columns)
                : base(columns, DatabaseKeyType.Foreign) { }

            public abstract Type TargetType { get; }

            public abstract Func<object, Key> KeySelector { get; }
        }

        public class Foreign<T> : ForeignKey where T : class, new()
        {
            public Foreign(Func<T, Key> keySelector, params IModelledColumn[] columns)
                : this(keySelector, columns as IEnumerable<IModelledColumn>) { }

            public Foreign(Func<T, Key> keySelector, IEnumerable<IModelledColumn> columns)
                : base(columns)
            {
                _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            }

            public override Type TargetType { get; } = typeof(T);

            public override Func<object, Key> KeySelector
            {
                get
                {
                    if (Property == null)
                        throw new ArgumentException($"The { nameof(Property) } property must be set before calling { nameof(KeySelector) }.", nameof(Property));

                    var targetProp = GetTargetProperty();
                    return obj =>
                    {
                        var result = _keySelector(obj as T);
                        result.Property = targetProp;
                        return result;
                    };
                }
            }

            // Rather ugly, but this is where the magic happens.
            // Intended to parse what the selector function really points to so that we can bind
            // a PropertyInfo object on the resulting key before we use it later via reflection.
            private PropertyInfo GetTargetProperty()
            {
                var sourceType = Property.DeclaringType;
                var sourceAsm = sourceType.GetTypeInfo().Assembly;
                if (!AssemblyCache.ContainsKey(sourceAsm))
                    AssemblyCache[sourceAsm] = AssemblyDefinition.ReadAssembly(sourceAsm.Location);

                var sourceAsmDefinition = AssemblyCache[sourceAsm];
                var sourceTypeDefinition = sourceAsmDefinition.MainModule.GetType(sourceType.FullName);
                var sourceProperty = sourceTypeDefinition.Properties.Single(p => p.Name == Property.Name);
                var sourcePropInstructions = sourceProperty.GetMethod.Body.Instructions;
                var fnInstruction = sourcePropInstructions.FirstOrDefault(i => i.OpCode.Code == Code.Ldftn);
                if (fnInstruction == null)
                    throw new ArgumentException(
                        "Could not find function pointer instruction in the get method of the source property " +
                        $"{ Property.DeclaringType.FullName }.{ Property.Name }. " +
                        "Is the key selector method a simple lambda expression?"
                    );

                var fnOperand = fnInstruction.Operand as MethodDefinition;
                if (fnOperand == null)
                    throw new ArgumentException(
                        "Expected to find a method definition associated with a function pointer instruction but could not find one for " +
                        $"{ Property.DeclaringType.FullName }.{ Property.Name }."
                    );

                var operandInstructions = fnOperand.Body.Instructions;
                var bodyCallInstr = operandInstructions.FirstOrDefault(i => i.OpCode.Code == Code.Callvirt);
                if (bodyCallInstr == null)
                    throw new ArgumentException(
                        "Could not find virtual call instruction in the key selector function that was provided to " +
                        $"{ Property.DeclaringType.FullName }.{ Property.Name }. " +
                        "Is the key selector method a simple lambda expression?"
                    );

                var bodyMethodDef = bodyCallInstr.Operand as MethodDefinition;
                if (bodyMethodDef == null)
                    throw new ArgumentException("Expected to find a method definition associated with the virtual call instruction but could not find one in the key selector.");

                var targetPropertyName = bodyMethodDef.Name;
                var targetProp = TargetType.GetTypeInfo().GetProperties().SingleOrDefault(p => p.GetGetMethod().Name == targetPropertyName);
                if (targetProp == null)
                    throw new ArgumentException($"Expected to find a property named { targetPropertyName } in { TargetType.FullName } but could not find one.");

                return targetProp;
            }

            private static IDictionary<Assembly, AssemblyDefinition> AssemblyCache { get; } = new Dictionary<Assembly, AssemblyDefinition>();

            private readonly Func<T, Key> _keySelector;
        }
    }
}
