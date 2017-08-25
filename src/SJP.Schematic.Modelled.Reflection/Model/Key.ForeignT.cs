using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public abstract partial class Key : IModelledKey
    {
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
                var sourceProperty = sourceTypeDefinition.Properties.SingleOrDefault(p => p.Name == Property.Name && !p.HasParameters);
                if (sourceProperty == null)
                    throw new ArgumentException(
                        $"Could not find the source property { Property.DeclaringType.FullName }.{ Property.Name }. Check that assemblies are up to date.",
                        $"{ Property.DeclaringType.FullName }.{ Property.Name }."
                    );

                var sourcePropInstructions = sourceProperty.GetMethod.Body.Instructions;
                var fnInstruction = sourcePropInstructions.FirstOrDefault(i => i.OpCode.Code == Code.Ldftn);
                if (fnInstruction == null)
                    throw new ArgumentException(
                        "Could not find function pointer instruction in the get method of the source property " +
                        $"{ Property.DeclaringType.FullName }.{ Property.Name }. " +
                        "Is the key selector method a simple lambda expression?",
                        $"{ Property.DeclaringType.FullName }.{ Property.Name }."
                    );

                var fnOperand = fnInstruction.Operand as MethodDefinition;
                if (fnOperand == null)
                    throw new ArgumentException(
                        "Expected to find a method definition associated with a function pointer instruction but could not find one for " +
                        $"{ Property.DeclaringType.FullName }.{ Property.Name }.",
                        $"{ Property.DeclaringType.FullName }.{ Property.Name }."
                    );

                var operandInstructions = fnOperand.Body.Instructions;
                var bodyCallInstr = operandInstructions.FirstOrDefault(i => i.OpCode.Code == Code.Callvirt || i.OpCode.Code == Code.Call);
                if (bodyCallInstr == null)
                    throw new ArgumentException(
                        "Could not find call or virtual call instruction in the key selector function that was provided to " +
                        $"{ Property.DeclaringType.FullName }.{ Property.Name }. " +
                        "Is the key selector method a simple lambda expression?",
                        $"{ Property.DeclaringType.FullName }.{ Property.Name }."
                    );

                var bodyMethodDef = bodyCallInstr.Operand as MethodDefinition;
                if (bodyMethodDef == null)
                    throw new ArgumentException("Expected to find a method definition associated with the call or virtual call instruction but could not find one in the key selector.");

                var targetPropertyName = bodyMethodDef.Name;
                var targetProp = TargetType.GetTypeInfo().GetProperties().SingleOrDefault(p => p.GetGetMethod().Name == targetPropertyName && p.GetIndexParameters().Length == 0);
                if (targetProp == null)
                    throw new ArgumentException(
                        $"Expected to find a property named { targetPropertyName } in { TargetType.FullName } but could not find one.",
                        $"{ Property.DeclaringType.FullName }.{ Property.Name }."
                    );

                return targetProp;
            }

            private static IDictionary<Assembly, AssemblyDefinition> AssemblyCache { get; } = new Dictionary<Assembly, AssemblyDefinition>();

            private readonly Func<T, Key> _keySelector;
        }
    }
}
