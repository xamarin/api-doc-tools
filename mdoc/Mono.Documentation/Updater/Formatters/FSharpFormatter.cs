using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using mdoc.Mono.Documentation.Util;
using Mono.Cecil;
using Mono.Collections.Generic;
using Mono.Documentation.Util;

namespace Mono.Documentation.Updater
{
    public class FSharpFormatter : MemberFormatter
    {
        private static readonly Dictionary<string, string> operators = new Dictionary<string, string>()
        {
              { "op_Nil"                   ,"[]"       },
              { "op_Cons"                  ,"::"       },
              { "op_Addition"              ,"+"        },
              { "op_Subtraction"           ,"-"        },
              { "op_Multiply"              ,"*"        },
              { "op_Division"              ,"/"        },
              { "op_Append"                ,"@"        },
              { "op_Concatenate"           ,"^"        },
              { "op_Modulus"               ,"%"        },
              { "op_BitwiseAnd"            ,"&&&"      },
              { "op_BitwiseOr"             ,"|||"      },
              { "op_ExclusiveOr"           ,"^^^"      },
              { "op_LeftShift"             ,"<<<"      },
              { "op_LogicalNot"            ,"~~~"      },
              { "op_RightShift"            ,">>>"      },
              { "op_UnaryPlus"             ,"~+"       },
              { "op_UnaryNegation"         ,"~-"       },
              { "op_Equality"              ,"="        },
              { "op_LessThanOrEqual"       ,"<="       },
              { "op_GreaterThanOrEqual"    ,">="       },
              { "op_LessThan"              ,"<"        },
              { "op_GreaterThan"           ,">"        },
              { "op_Dynamic"               ,"?"        },
              { "op_DynamicAssignment"     ,"?<-"      },
              { "op_PipeRight"             ,"|>"       },
              { "op_PipeRight2"            ,"||>"      },
              { "op_PipeRight3"            ,"|||>"     },
              { "op_PipeLeft"              ,"<|"       },
              { "op_PipeLeft2"             ,"<||"      },
              { "op_PipeLeft3"             ,"<|||"     },
              { "op_Dereference"           ,"!"        },
              { "op_ComposeRight"          ,">>"       },
              { "op_ComposeLeft"           ,"<<"       },
              { "op_Quotation"             ,"<@ @>"    },
              { "op_QuotationUntyped"      ,"<@@ @@>"  },
              { "op_AdditionAssignment"    ,"+="       },
              { "op_SubtractionAssignment" ,"-="       },
              { "op_MultiplyAssignment"    ,"*="       },
              { "op_DivisionAssignment"    ,"/="       },
              { "op_Range"                 ,".."       },
              { "op_RangeStep"             ,".. .."    },
        };

        // Other combinations of operator characters that are not listed here can be used as operators and have names
        // that are made up by concatenating names for the individual characters from the following table.
        // For example, +! becomes op_PlusBang
        private static readonly Dictionary<string, string> combinatedOperators = new Dictionary<string, string>()
        {
            {"Greater" , ">"},
            {"Less"    , "<"},
            {"Plus"    , "+"},
            {"Minus"   , "-"},
            {"Multiply", "*"},
            {"Equals"  , "="},
            {"Twiddle" , "~"},
            {"Percent" , "%"},
            {"Dot"     , "."},
            {"Amp"     , "&"},
            {"Bar"     , "|"},
            {"At"      , "@"},
            {"Hash"    , "#"},
            {"Hat"     , "^"},
            {"Bang"    , "!"},
            {"Qmark"   , "?"},
            {"Divide"  , "/"},
            {"Colon"   , ":"},
            {"LParen"  , "("},
            {"Comma"   , ","},
            {"RParen"  , ")"},
            {"LBrack"  , "["},
            {"RBrack"  , "]"},
        };

        private static readonly Dictionary<string, string> typeAbbreviations = new Dictionary<string, string>()
        {
            {"System.Boolean", "bool"},
            {"System.Byte", "byte"},
            {"System.SByte", "sbyte"},
            {"System.Int16", "int16"},
            {"System.UInt16", "uint16"},
            {"System.Int32", "int"},
            {"System.UInt32", "uint32"},
            {"System.Int64", "int64"},
            {"System.UInt64", "uint64"},
            {"System.IntPtr", "nativeint"},
            {"System.UIntPtr", "unativeint"},
            {"System.Char", "char"},
            {"System.String", "string"},
            {"System.Decimal", "decimal"},
            {"System.Void", "unit"},
            {"System.Single", "single"},// Can be float32
            {"System.Double", "float"},// Can be double
            {"System.Object", "obj"},
            {"Microsoft.FSharp.Core.Unit", "unit"},
            {"Microsoft.FSharp.Core.FSharpOption`1", "option"},
            {"System.Collections.Generic.IEnumerable`1", "seq"},
            {"Microsoft.FSharp.Core.FSharpRef`1", "ref"}
        };

        private static readonly HashSet<string> fSharpPrefixes = new HashSet<string>()
        {
            "Microsoft.FSharp.Collections.FSharp",
            "Microsoft.FSharp.Core.FSharp",
            "Microsoft.FSharp.Control.FSharp",
            "Microsoft.FSharp.Data.FSharp",
            "Microsoft.FSharp.Linq.FSharp",
            "Microsoft.FSharp.NativeInterop.FSharp",
            "Microsoft.FSharp.Quotations.FSharp",
            "Microsoft.FSharp.Reflection.FSharp"
        };

        private static readonly HashSet<string> ignoredValueTypeInterfaces = new HashSet<string>()
        {
            "System.IEquatable`1",
            "System.Collections.IStructuralEquatable",
            "System.IComparable`1",
            "System.IComparable",
            "System.Collections.IStructuralComparable",
        };

        private static readonly HashSet<string> IgnoredMethodOverrideNames = new HashSet<string>()
        {
            "CompareTo",
            "Equals",
            "GetHashCode",
            "ToString"
        };

        private GenericParameterState genericParameterState = GenericParameterState.None;

        protected string GetFSharpType(TypeReference type)
        {
            string typeToCompare = type.FullName;

            var fullName = type.IsGenericInstance ? type.GetElementType().FullName : type.FullName;
            if (typeAbbreviations.ContainsKey(fullName))
            {
                typeToCompare = typeAbbreviations[fullName];
            }
            else
            {
                var prefixToRemove = fSharpPrefixes.FirstOrDefault(i => fullName.StartsWith(i));
                if (prefixToRemove != null)
                {
                    typeToCompare = base.AppendTypeName(new StringBuilder(), type.FullName.Replace(prefixToRemove, "")).ToString();
                }

                if (type.IsGenericParameter)
                {
                    typeToCompare = GetGenericName(type.Name);
                }
            }
            return typeToCompare == type.FullName ? null : typeToCompare;
        }

        protected override StringBuilder AppendTypeName(StringBuilder buf, TypeReference type, DynamicParserContext context)
        {
            string fSharpType = GetFSharpType(type);
            if (fSharpType != null)
            {
                return buf.Append(fSharpType);
            }

            if (type.IsGenericParameter)
            {
                return buf.Append(GetGenericName(type.Name));
            }

            return base.AppendTypeName(buf, type, context);
        }

        protected override string GetTypeDeclaration(TypeDefinition type)
        {
            string visibility = GetTypeVisibility(type.Attributes);
            if (visibility == null)
                return null;

            StringBuilder buf = new StringBuilder();

            AppendAttributes(type, buf);

            if (IsModule(type))
            {
                AppendModuleDeclaraion(buf, type);
                return buf.ToString();
            }
            if (IsDiscriminatedUnionCase(type))
            {
                AppendDiscriminatedUnionCase(buf, type);
                return buf.ToString();
            }

            buf.Append($"type {visibility}{GetTypeName(type)} =");

            if (IsRecord(type))
            {
                AppendRecordLabels(type, buf);
                AppendInterfaces(type, buf);
                AppendMethodsAndConstructors(type, buf, includeConstructors:false);
                AppendProperties(type, buf);
                
                return buf.ToString();
            }

            if (IsDiscriminatedUnion(type))
            {
                AppendDULableDeclarations(type, buf);
                AppendInterfaces(type, buf);
                AppendMethodsAndConstructors(type, buf, includeConstructors:false);
                AppendProperties(type, buf);

                return buf.ToString();
            }
            if (type.IsEnum)
            {
                foreach (var field in type.Fields.Where(f => !f.IsRuntimeSpecialName))
                {
                    buf.Append($"{GetLineEnding()}{Consts.Tab}| {field.Name} = {field.Constant}");
                }

                return buf.ToString();
            }

            if (DocUtils.IsDelegate(type))
            {
                buf.Append(" delegate of");
                buf.Append(" ");
                MethodDefinition invoke = type.GetMethod("Invoke");
                AppendFunctionSignature(buf, invoke);
                return buf.ToString();
            }

            if (type.IsValueType)
            {
                buf.Append($"{GetLineEnding()}{Consts.Tab}struct");
            }
            else if (type.IsInterface)
            {
                buf.Append($"{GetLineEnding()}{Consts.Tab}interface");
            }

            AppendBaseType(buf, type);
            AppendInterfaces(type, buf);
            AppendMethodsAndConstructors(type, buf);
            AppendProperties(type, buf);

            if (type.IsValueType || type.IsInterface)
            {
                buf.Append($"{GetLineEnding()}{Consts.Tab}end");
            }

            // List it as an empty class if it doesn't have anything visible or callable (and it's a class).
            if (!type.IsValueType && !type.HasInterfaces && !type.HasMethods && !type.HasProperties)
            {
                buf.Append("class end");
            }

            return buf.ToString();
        }

        private void AppendDULableDeclarations(TypeDefinition type, StringBuilder buf)
        {
            string ToParamString(ParameterDefinition p)
            {
                // No name applied to type arg. Example:
                //
                // type Bar =
                //     | Baz of double
                if (p.Name.StartsWith("item", StringComparison.OrdinalIgnoreCase))
                {
                    return $"{GetTypeName(p.ParameterType)}";
                }
                else
                {
                    // Substring off the first char, since the names are compiled with a '_' character
                    return $"{p.Name.Substring(1)}: {GetTypeName(p.ParameterType)}";
                }
            }

            string GetName(MethodDefinition label)
            {
                var name = label.Name;

                if (name.StartsWith("New"))
                {
                    return name.Substring(3);
                }
                else if (name.StartsWith("get_"))
                {
                    return name.Substring(4);
                }
                else
                {
                    return name;
                }
            }

            var labels = type.Methods.Where(p => GetFSharpFlags(p.CustomAttributes).Any(f => f == SourceConstructFlags.UnionCase));

            foreach (var label in labels)
            {
                buf.Append($"{GetLineEnding()}{Consts.Tab}| {GetName(label)}");

                if (label.HasParameters)
                {
                    buf.Append(" of ");
                    var paramStr = string.Join(" * ", label.Parameters.Select(p => ToParamString(p)));
                    buf.Append(paramStr);
                }
            }
        }

        private void AppendRecordLabels(TypeDefinition type, StringBuilder buf)
        {
            var labels = type.Properties.Where(p => GetFSharpFlags(p.CustomAttributes).Any(f => f == SourceConstructFlags.Field)).ToList();

            var first = labels.First(); // You cannot define an empty record in F#

            string GetLabelString(PropertyDefinition p)
            {
                if (p.SetMethod != null && p.SetMethod.IsPublic)
                {
                    return $"mutable {p.Name}";
                }
                else
                {
                    return p.Name;
                }

            }
            buf.Append($"{GetLineEnding()}{Consts.Tab}" + "{" + $" {GetLabelString(first)} : {GetTypeName(first.PropertyType)}");

            // Example: type R = { IntVal: int }
            if (labels.Count == 1)
            {
                buf.Append(" }");
            }
            else
            {
                // At least one more label, but we handle the last label differently than the rest.
                //
                // Example:
                // type R =
                //    { IntVal : int
                //      StringVal : string }
                //
                // Note that there is a trailing `}` rather than a newline
                var last = labels.Last();
                var lastString = $"{GetLineEnding()}{Consts.Tab}  {GetLabelString(last)} : {GetTypeName(last.PropertyType)}" + " }";

                // Only a begin and end label.
                if (labels.Count == 2)
                {
                    buf.Append(lastString);
                }
                else
                {

                    // And now we fill in the middle!
                    var middle = labels.Skip(1).Take(labels.Count - 2);
                    foreach (var label in middle)
                    {
                        buf.Append($"{GetLineEnding()}{Consts.Tab}  {GetLabelString(label)} : {GetTypeName(label.PropertyType)}");
                    }

                    buf.Append(lastString);
                }
            }
        }

        private void AppendProperties(TypeDefinition type, StringBuilder buf)
        {
            if (type.HasProperties)
            {
                var props = type.Properties.Where(p => DocUtils.IsIgnored(p)).OrderBy(p => p.FullName);

                if (IsRecord(type) || IsDiscriminatedUnion(type))
                {
                    props = props.Where(p => !GetFSharpFlags(p.CustomAttributes).Any(ca => ca == SourceConstructFlags.Field))
                                 .OrderBy(p => p.FullName);
                }

                foreach (var prop in props)
                {
                    if (prop is null || string.IsNullOrEmpty(prop.Name)) continue;

                    var lineEnd = GetLineEnding();
                    var tab = type.IsValueType || type.IsInterface ? Consts.Tab + Consts.Tab : Consts.Tab;
                    var propDecl = GetPropertyDeclaration(prop);

                    if (string.IsNullOrWhiteSpace(propDecl)) continue;

                    buf.Append($"{lineEnd}{tab}{propDecl}");
                }
            }
        }

        private void AppendMethodsAndConstructors(TypeDefinition type, StringBuilder buf, bool includeConstructors = true)
        {
            if (type.HasMethods)
            {
                var filtered = type.Methods.Where(m => DocUtils.IsIgnored(m) && !m.IsGetter && !m.IsSetter);
                if (filtered is null) return;

                if (includeConstructors)
                {
                    var ctors = filtered.Where(m => m.IsConstructor);

                    // There is constructor metadata here for interfaces, but we don't want to actually show it
                    if (!type.IsInterface && !(ctors is null))
                    {
                        foreach (var ctor in ctors.OrderBy(c => c.Name))
                        {
                            if (ctor is null || string.IsNullOrEmpty(ctor.Name)) continue;

                            var lineEnd = GetLineEnding();
                            var tab = type.IsValueType ? Consts.Tab + Consts.Tab : Consts.Tab;
                            var ctorDecl = GetConstructorDeclaration(ctor);

                            if (string.IsNullOrWhiteSpace(ctorDecl)) continue;

                            buf.Append($"{lineEnd}{tab}{ctorDecl}");
                        }
                    }
                }

                var meths = filtered.Where(m => !m.IsConstructor);

                if (!(meths is null))
                {
                    foreach (var meth in meths.OrderBy(m => m.Name))
                    {
                        if (meth is null || string.IsNullOrEmpty(meth.Name)) continue;

                        if (IgnoredMethodOverrideNames.Contains(meth.Name))
                        {
                            continue;
                        }

                        var lineEnd = GetLineEnding();
                        var tab = type.IsValueType || type.IsInterface ? Consts.Tab + Consts.Tab : Consts.Tab;
                        var methDecl = GetMethodDeclaration(meth);

                        if (string.IsNullOrWhiteSpace(methDecl)) continue;

                        buf.Append($"{lineEnd}{tab}{methDecl}");
                    }
                }
            }
        }

        private void AppendInterfaces(TypeDefinition type, StringBuilder buf)
        {
            if (type.HasInterfaces)
            {
                foreach (var interfaceImplementation in type.Interfaces.OrderBy(iface => iface.InterfaceType.Name))
                {
                    var resolvedInterface = interfaceImplementation.InterfaceType.Resolve();

                    if (type.IsValueType
                        && ignoredValueTypeInterfaces.Any(i => interfaceImplementation.InterfaceType.FullName.StartsWith(i))
                        || (resolvedInterface != null && resolvedInterface.IsNotPublic))
                        continue;

                    var tab = type.IsValueType || type.IsInterface ? Consts.Tab + Consts.Tab : Consts.Tab;
                    buf.Append($"{GetLineEnding()}{tab}interface ");
                    AppendTypeName(buf, GetTypeName(interfaceImplementation.InterfaceType));
                }
            }
        }

        private static void AppendAttributes(TypeDefinition type, StringBuilder buf)
        {
            if (!IsModule(type) && !(IsRecord(type) || IsDiscriminatedUnion(type)) && !type.IsEnum)
            {
                if (type.IsAbstract && !type.IsInterface)
                {
                    buf.Append($"[<AbstractClass>]{GetLineEnding()}");
                }

                if (type.IsSealed)
                {
                    buf.Append($"[<Sealed>]{GetLineEnding()}");
                }
            }

            if (type.IsValueType && (IsDiscriminatedUnion(type) || IsRecord(type)))
            {
                buf.Append($"[<Struct>]{GetLineEnding()}");
            }
        }

        private void AppendDiscriminatedUnionCase(StringBuilder buf, TypeDefinition type)
        {
            buf.Append(GetName(type));
            buf.Append(" : ");

            var constructor = type.Methods.First(i => i.Name == ".ctor");
            AppendParameters(buf, constructor, constructor.Parameters);
            buf.Append(" -> ");
            buf.Append(GetName(type.DeclaringType));
        }

        private void AppendBaseType(StringBuilder buf, TypeDefinition type)
        {
            TypeReference basetype = type.BaseType;
            if (basetype != null && basetype.FullName == "System.Object" || type.IsValueType)
                basetype = null;

            if (basetype != null)
            {
                buf.Append($"{GetLineEnding()}{Consts.Tab}inherit ");
                buf.Append(GetName(basetype));
            }
        }

        private void AppendFunctionSignature(StringBuilder buf, MethodDefinition method)
        {
            bool isTuple = method.Parameters.Count == 1 && IsTuple(method.Parameters[0].ParameterType);

            if (isTuple)
            {
                buf.Append("(");
            }

            AppendParameters(buf, method, method.Parameters);

            if (isTuple)
            {
                buf.Append(")");
            }

            buf.Append(" -> ");

            if (method.IsConstructor)
            {
                buf.Append(GetTypeName(method.DeclaringType));
            }
            else
            {
                if (IsFSharpFunction(method.ReturnType))
                {
                    buf.Append("(");
                }

                buf.Append(GetTypeName(method.ReturnType));

                if (IsFSharpFunction(method.ReturnType))
                {
                    buf.Append(")");
                }
            }
        }

        private string GetTypeKind(TypeDefinition type)
        {
            if (type.IsInterface)
            {
                return "interface";
            }
            if (type.IsValueType)
            {
                return "struct";
            }
            if (DocUtils.IsDelegate(type))
            {
                return "delegate of";
            }
            return "class";
        }

        protected override StringBuilder AppendGenericType(StringBuilder buf, TypeReference type, DynamicParserContext context, bool appendGeneric = true)
        {
            List<TypeReference> decls = DocUtils.GetDeclaringTypes(
                   type is GenericInstanceType ? type.GetElementType() : type);
            List<TypeReference> genArgs = GetGenericArguments(type);
            int argIdx = 0;
            int displayedInParentArguments = 0;
            bool insertNested = false;
            foreach (var decl in decls)
            {
                TypeReference declDef = decl.Resolve() ?? decl;
                if (insertNested)
                {
                    buf.Append(NestedTypeSeparator);
                }
                insertNested = true;

                bool isTuple = IsTuple(type)
                               // If this is a declaration of `Tuple` type
                               // treat it as an ordinary type
                               && !(type is TypeDefinition);
                bool isFSharpFunction = IsFSharpFunction(type);
                if (!isTuple && !isFSharpFunction)
                    AppendTypeName(buf, declDef, context);

                int argumentCount = DocUtils.GetGenericArgumentCount(declDef);
                int notYetDisplayedArguments = argumentCount - displayedInParentArguments;
                displayedInParentArguments = argumentCount;// nested TypeReferences have parents' generic arguments, but we shouldn't display them
                if (notYetDisplayedArguments > 0)
                {
                    if (!isTuple && !isFSharpFunction)
                        buf.Append(GenericTypeContainer[0]);
                    if (isFSharpFunction && genericParameterState == GenericParameterState.WithinTuple)
                        buf.Append("(");
                    var origState = MemberFormatterState;
                    var genericParameterStateOrigState = genericParameterState;
                    MemberFormatterState = MemberFormatterState.WithinGenericTypeParameters;
                    genericParameterState = isTuple ? GenericParameterState.WithinTuple : GenericParameterState.None;

                    for (int i = 0; i < notYetDisplayedArguments; ++i)
                    {
                        if (i > 0)
                            buf.Append(isTuple ? " * " : isFSharpFunction ? " -> " : ", ");
                        var genArg = genArgs[argIdx++];
                        var genericParameter = genArg as GenericParameter;
                        if (genericParameter != null && IsFlexibleType(genericParameter))
                        {
                            buf.Append("#");// replace genericParameter which is a flexible type with its constraint type
                            _AppendTypeName(buf, genericParameter.Constraints[0], context);
                        }
                        else
                        {
                            _AppendTypeName(buf, genArg, context);
                        }
                    }
                    MemberFormatterState = origState;
                    genericParameterState = genericParameterStateOrigState;

                    if (MemberFormatterState == MemberFormatterState.None)
                    {
                        AppendConstraints(buf,
                            genArgs.GetRange(0, notYetDisplayedArguments)
                                .SafeCast<GenericParameter>()
                                .ToList());
                    }
                    if (!isTuple && !isFSharpFunction)
                        buf.Append(GenericTypeContainer[1]);
                    if (isFSharpFunction && genericParameterState == GenericParameterState.WithinTuple)
                        buf.Append(")");
                }
            }
            return buf;
        }

        private void AppendModuleDeclaraion(StringBuilder buf, TypeDefinition type)
        {
            buf.Append("module ");
            buf.Append(GetModuleName(type));
        }

        private string GetModuleName(TypeDefinition type)
        {
            var moduleName = GetTypeName(type);
            const string moduleKeyWord = "Module";
            if (moduleName.EndsWith(moduleKeyWord))
            {
                moduleName = moduleName.Substring(0, moduleName.Length - moduleKeyWord.Length);
            }
            return moduleName;
        }

        protected override StringBuilder AppendGenericTypeConstraints(StringBuilder buf, TypeReference type)
        {
            return buf;
        }

        private void AppendConstraints(StringBuilder buf, IList<GenericParameter> genArgs)
        {
            var origMemberFormatterState = MemberFormatterState;
            MemberFormatterState = MemberFormatterState.WithinGenericTypeParameters;
            List<string> constraintStrings = new List<string>();

            foreach (GenericParameter genArg in genArgs.Where(i => !IsFlexibleType(i)))
            {
                GenericParameterAttributes attrs = genArg.Attributes;
                IList<TypeReference> constraints = genArg.Constraints;
                if (attrs == GenericParameterAttributes.NonVariant && constraints.Count == 0)
                    continue;

                bool isref = (attrs & GenericParameterAttributes.ReferenceTypeConstraint) != 0;
                bool isvt = (attrs & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;
                bool isnew = (attrs & GenericParameterAttributes.DefaultConstructorConstraint) != 0;

                if (!isref && !isvt && !isnew && constraints.Count == 0)
                    continue;

                var genericName = GetGenericName(genArg.Name);
                if (isref)
                {
                    // May be "not struct". We don't know it here
                    constraintStrings.Add($"{genericName} : null");
                }
                else if (isvt)
                {
                    constraintStrings.Add($"{genericName} : struct");
                }
                if (constraints.Count > 0 && !isvt)
                {
                    foreach (var typeReference in constraints)
                    {
                        constraintStrings.Add($"{genericName} :> {GetTypeName(typeReference)}");
                    }
                }
                if (isnew && !isvt)
                {
                    constraintStrings.Add($"{genericName} : (new : unit -> {GetTypeName(genArg)})");
                }
            }

            if (constraintStrings.Count > 0)
            {
                buf.Append($" (requires {string.Join(" and ", constraintStrings)})");
            }
            MemberFormatterState = origMemberFormatterState;
        }

        protected override string GetConstructorDeclaration(MethodDefinition constructor)
        {
            StringBuilder buf = new StringBuilder();
            if (AppendVisibility(buf, constructor) == null)
                return null;

            buf.Append("new : ");
            AppendFunctionSignature(buf, constructor);

            return buf.ToString();
        }

        protected override string GetMethodDeclaration(MethodDefinition method)
        {
            var visibilityBuf = new StringBuilder();
            if (AppendVisibility(visibilityBuf, method) == null)
                return null;

            string visibility = visibilityBuf.ToString();
            StringBuilder buf = new StringBuilder();
            var kind = GetMethodKind(method);
            switch (kind)
            {
                case FSharpMethodKind.InModule:
                    AppendModuleMethod(buf, method);
                    break;
                case FSharpMethodKind.Static:
                    AppendStaticMethod(buf, method, visibility);
                    break;
                case FSharpMethodKind.Override:
                    AppendOverrideMethod(buf, method, visibility);
                    break;
                case FSharpMethodKind.Abstract:
                    AppendAbstractMethod(buf, method, visibility);
                    break;
                case FSharpMethodKind.Virtual:
                    AppendVirtualMethod(buf, method, visibility);
                    break;
                case FSharpMethodKind.Common:
                    AppendCommonMethod(buf, method, visibility);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return buf.ToString();
        }

        private FSharpMethodKind GetMethodKind(MethodDefinition method)
        {
            if (IsModule(method.DeclaringType))
                return FSharpMethodKind.InModule;
            if (method.IsStatic)
                return FSharpMethodKind.Static;
            if (IsOverride(method))
                return FSharpMethodKind.Override;
            if (method.IsAbstract)
                return FSharpMethodKind.Abstract;
            if (method.IsVirtual)
                return FSharpMethodKind.Virtual;

            return FSharpMethodKind.Common;
        }

        private void AppendModuleMethod(StringBuilder buf, MethodDefinition method)
        {
            if (!IsOperator(method))
            {
                buf.Append($"{GetModuleName(method.DeclaringType)}.");
            }
            AppendMethodDeclarationEnding(buf, method);
        }

        private void AppendVirtualMethod(StringBuilder buf, MethodDefinition method, string visibility)
        {
            AppendAbstractMethod(buf, method, visibility);
            buf.Append(GetLineEnding());
            AppendOverrideMethod(buf, method, visibility);
        }

        private void AppendStaticMethod(StringBuilder buf, MethodDefinition method, string visibility)
        {
            buf.Append("static member ");
            buf.Append(visibility);
            AppendMethodDeclarationEnding(buf, method);
        }

        private void AppendAbstractMethod(StringBuilder buf, MethodDefinition method, string visibility)
        {
            buf.Append("abstract member ");
            buf.Append(visibility);
            AppendMethodDeclarationEnding(buf, method);
        }

        private void AppendOverrideMethod(StringBuilder buf, MethodDefinition method, string visibility)
        {
            buf.Append("override ");
            buf.Append(visibility);
            buf.Append("this.");
            AppendMethodDeclarationEnding(buf, method);
        }

        private void AppendCommonMethod(StringBuilder buf, MethodDefinition method, string visibility)
        {
            buf.Append("member ");
            buf.Append(visibility);
            buf.Append("this.");
            AppendMethodDeclarationEnding(buf, method);
        }

        private void AppendMethodDeclarationEnding(StringBuilder buf, MethodDefinition method)
        {
            AppendMethodName(buf, method);
            buf.Append(" : ");
            AppendFunctionSignature(buf, method);
            AppendConstraints(buf, method.GenericParameters);
        }

        protected override StringBuilder AppendMethodName(StringBuilder buf, MethodDefinition method)
        {
            if (IsOperator(method))
            {
                // this is an operator
                if (TryAppendOperatorName(buf, method))
                    return buf;

                return base.AppendMethodName(buf, method);
            }

            var compilationSourceNameAttribute = method.CustomAttributes.FirstOrDefault
                (i => i.AttributeType.FullName == "Microsoft.FSharp.Core.CompilationSourceNameAttribute");
            if (compilationSourceNameAttribute != null)
                return buf.Append(compilationSourceNameAttribute.ConstructorArguments.First().Value);

            return buf.Append(method.Name);
        }

        protected override StringBuilder AppendGenericMethodConstraints(StringBuilder buf, MethodDefinition method)
        {
            return buf;
        }

        protected override StringBuilder AppendRefTypeName(StringBuilder buf, TypeReference type, DynamicParserContext context)
        {
            return buf;
        }

        protected override StringBuilder AppendModifiers(StringBuilder buf, MethodDefinition method)
        {
            return buf;
        }

        private bool IsOverride(MethodDefinition method)
        {
            return IsOverride(method.DeclaringType, method);
        }

        private bool IsOverride(TypeDefinition type, MethodDefinition method)
        {
            if (type == null || type.BaseType == null)
                return false;
            var baseType = type.BaseType.Resolve();
            if (baseType != null && baseType.Methods.Any(i => i.Name == method.Name))
                return true;
            return IsOverride(type.BaseType.Resolve(), method);
        }

        protected override StringBuilder AppendGenericMethod(StringBuilder buf, MethodDefinition method)
        {
            return buf;
        }

        protected void AppendRecordParameter(StringBuilder buf, PropertyDefinition property)
        {
            bool hasParameterName = !string.IsNullOrEmpty(property.Name);
            if (property.SetMethod != null && property.SetMethod.IsPublic)
                buf.Append("mutable ");
            if (hasParameterName)
            {
                buf.Append(property.Name.TrimEnd('@'));
            }

            if (property.PropertyType.FullName != "System.Object")
            {
                var typeName = GetTypeName(property.PropertyType, new DynamicParserContext(property));
                if (hasParameterName)
                    buf.Append(" : ");
                buf.Append(typeName);
            }
        }

        protected override StringBuilder AppendParameters(StringBuilder buf, MethodDefinition method, IList<ParameterDefinition> parameters)
        {
            var curryBorders = GetCurryBorders(method);

            if (parameters.Count > 0)
            {
                bool isExtensionMethod = IsExtensionMethod(method);

                // If it's an extension method, first parameter is ignored
                if (!isExtensionMethod)
                {
                    AppendParameter(buf, parameters[0]);
                }

                for (int i = 1; i < parameters.Count; ++i)
                {
                    if (!(isExtensionMethod && i == 1))
                    {
                        if (curryBorders.Contains(i))
                        {
                            buf.Append(" -> ");
                        }
                        else
                        {
                            buf.Append(" * ");
                        }
                    }

                    AppendParameter(buf, parameters[i]);
                }
            }
            else
            {
                buf.Append("unit");
            }

            return buf;
        }

        private void AppendParameter(StringBuilder buf, ParameterDefinition parameter)
        {
            bool isFSharpFunction = IsFSharpFunction(parameter.ParameterType);
            var typeName = GetTypeName(parameter.ParameterType, new DynamicParserContext(parameter));

            // Tooltips in VS ignore name-less params or those with the discard character.
            if (string.IsNullOrWhiteSpace(parameter.Name) || parameter.Name.StartsWith("_"))
            {
                if (isFSharpFunction)
                {
                    buf.Append($"({typeName})");
                }
                else
                {
                    buf.Append($"{typeName}");
                }
            }
            else
            {
                if (isFSharpFunction)
                {
                    buf.Append($"{parameter.Name}:({typeName})");
                }
                else
                {
                    buf.Append($"{parameter.Name}:{typeName}");
                }
            }
        }

        protected override string GetPropertyDeclaration(PropertyDefinition property)
        {
            string getVisible = null;
            if (DocUtils.IsAvailablePropertyMethod(property.GetMethod))
                getVisible = AppendVisibility(new StringBuilder(), property.GetMethod)?.ToString();
            string setVisible = null;
            if (DocUtils.IsAvailablePropertyMethod(property.SetMethod))
                setVisible = AppendVisibility(new StringBuilder(), property.SetMethod)?.ToString();

            if (setVisible == null && getVisible == null)
                return null;

            bool isField = GetFSharpFlags(property.CustomAttributes).Any(i => i == SourceConstructFlags.Field);
            bool isRecordOrDU = IsRecord(property.DeclaringType) || IsDiscriminatedUnion(property.DeclaringType);
            StringBuilder buf = new StringBuilder();

            if (IsModule(property.DeclaringType))
            {
                buf.Append($"{GetName(property.DeclaringType)}.");
            }
            else
            {
                if (isField && !isRecordOrDU)
                    buf.Append("val ");
                else
                    buf.Append("member this.");
            }

            buf.Append(DocUtils.GetPropertyName(property, NestedTypeSeparator));
            if (property.Parameters.Count != 0)
            {
                buf.Append("(");
                AppendParameters(buf, property.GetMethod ?? property.SetMethod, property.Parameters);
                buf.Append(")");
            }
            buf.Append(" : ");
            buf.Append(GetTypeName(property.PropertyType));

            if (getVisible != null && setVisible != null)
            {
                buf.Append(" with get, set");
            }
            return buf.ToString();
        }

        protected override string GetFieldDeclaration(FieldDefinition field)
        {
            TypeDefinition declType = field.DeclaringType;
            if (declType.IsEnum && field.Name == "value__")
                return null; // This member of enums aren't documented.

            var visibility = GetFieldVisibility(field);
            if (visibility == null)
                return null;
            var buf = new StringBuilder();

            if (declType.IsEnum)
            {
                buf.Append(field.Name);
                if (field.IsLiteral)
                {
                    buf.Append($" = {field.Constant}");
                }
                return buf.ToString();
            }
            if (field.IsStatic && !field.IsLiteral)
                buf.Append(" static");

            buf.Append("val mutable");
            if (!string.IsNullOrEmpty(visibility))
                buf.Append(" ");
            buf.Append(visibility);
            buf.Append(" ");
            buf.Append(field.Name);
            buf.Append(" : ");
            buf.Append(GetTypeName(field.FieldType, new DynamicParserContext(field)));

            return buf.ToString();
        }

        protected override string GetEventDeclaration(EventDefinition e)
        {
            StringBuilder buf = new StringBuilder();
            StringBuilder visibilityBuf = new StringBuilder();
            if (AppendVisibility(visibilityBuf, e.AddMethod) == null)
            {
                return null;
            }

            buf.Append("member this.");
            if (visibilityBuf.Length > 0)
                buf.Append(visibilityBuf).Append(' ');
            buf.Append(e.Name).Append(" : ");
            buf.Append(GetTypeName(e.EventType, new DynamicParserContext(e.AddMethod.Parameters[0]))).Append(' ');

            return buf.ToString();
        }

        private static IEnumerable<SourceConstructFlags> GetFSharpFlags(Collection<CustomAttribute> customAttributes)
        {
            foreach (var attribute in customAttributes)
            {
                if (attribute.AttributeType.Name.Contains("CompilationMapping"))
                {
                    var sourceConstructFlags = attribute.ConstructorArguments.Where(
                        i => i.Type.FullName == "Microsoft.FSharp.Core.SourceConstructFlags");
                    foreach (var customAttributeArgument in sourceConstructFlags)
                    {
                        var constructFlags = (SourceConstructFlags)customAttributeArgument.Value;
                        yield return constructFlags;
                    }
                }
            }
        }

        private static string GetGenericName(string name)
        {
            var trimmedName = name.TrimStart('T');
            if (trimmedName.Length == 0 || !Regex.IsMatch(trimmedName, @"^[a-zA-Z]+$"))
                trimmedName = name;
            return $"\'{trimmedName}";
        }

        /// <summary>
        /// Get sequencing of curried arguments
        /// </summary>
        /// <returns>Positions between arguments which are curried</returns>
        protected HashSet<int> GetCurryBorders(MethodDefinition method)
        {
            var compilationArgumentCounts = GetCustomAttribute(method.CustomAttributes,
                "CompilationArgumentCountsAttribute");

            int[] curryCounts = { };
            if (compilationArgumentCounts != null)
            {
                var customAttributeArguments =
                    (CustomAttributeArgument[])compilationArgumentCounts.ConstructorArguments[0].Value;
                curryCounts = customAttributeArguments.Select(i => (int)i.Value).ToArray();
            }
            var curryBorders = new HashSet<int>();
            int sum = 0;
            foreach (var curryCount in curryCounts)
            {
                sum += curryCount;
                curryBorders.Add(sum);
            }
            return curryBorders;
        }

        private CustomAttribute GetCustomAttribute(Collection<CustomAttribute> customAttributes, string name)
        {
            return customAttributes.SingleOrDefault(i => i.AttributeType.Name == name);
        }

        protected bool TryAppendOperatorName(StringBuilder buf, MethodDefinition method)
        {
            if (!IsOperator(method))
                return false;
            if (operators.ContainsKey(method.Name))
            {
                buf.Append($"( {operators[method.Name]} )");
                return true;
            }

            if (TryAppendCombinatedOperandName(buf, method))
                return true;

            return false;
        }

        private static bool TryAppendCombinatedOperandName(StringBuilder buf, MethodDefinition method)
        {
            var oldName = method.Name.Remove(0, 3);
            var newName = new StringBuilder();
            bool found;
            do
            {
                found = false;
                foreach (var op in combinatedOperators)
                {
                    if (oldName.StartsWith(op.Key))
                    {
                        oldName = oldName.Remove(0, op.Key.Length);
                        newName.Append(op.Value);
                        found = true;
                    }
                }
            } while (found);

            if (oldName.Length == 0)
            {
                buf.Append($"( {newName} )");
                return true;
            }
            return false;
        }

        protected override StringBuilder AppendPointerTypeName(StringBuilder buf, TypeReference type, DynamicParserContext context)
        {
            TypeSpecification spec = type as TypeSpecification;
            buf.Append("nativeptr<");
            _AppendTypeName(buf, spec != null ? spec.ElementType : type.GetElementType(), context);
            buf.Append(">");
            return buf;
        }

        #region "Is" methods
        private static bool IsOperator(MethodDefinition method)
        {
            return method.Name.StartsWith("op_", StringComparison.Ordinal);
        }

        private static bool IsFSharpFunction(TypeReference type)
        {
            return type.FullName.StartsWith("Microsoft.FSharp.Core.FSharpFunc`");
        }

        private static bool IsTuple(TypeReference type)
        {
            return type.FullName.StartsWith("System.Tuple");
        }

        private bool IsFlexibleType(GenericParameter genericParameter)
        {
            return genericParameter.Constraints.Count == 1 && GetFSharpType(genericParameter.Constraints[0].GetElementType()) != null;
        }

        private static bool IsModule(TypeDefinition type)
        {
            var fSharpFlags = GetFSharpFlags(type.CustomAttributes);
            return fSharpFlags.Any(i => i == SourceConstructFlags.Module);
        }

        private static bool IsDiscriminatedUnion(TypeDefinition type)
        {
            var fSharpFlags = GetFSharpFlags(type.CustomAttributes);
            return fSharpFlags.Any(i => i == SourceConstructFlags.SumType);
        }

        private static bool IsDiscriminatedUnionCase(TypeDefinition type)
        {
            return type.DeclaringType != null && IsDiscriminatedUnion(type.DeclaringType);
        }

        private static bool IsRecord(TypeDefinition type)
        {
            var fSharpFlags = GetFSharpFlags(type.CustomAttributes);
            return fSharpFlags.Any(i => i == SourceConstructFlags.RecordType);
        }

        protected static bool IsExtensionMethod(MethodDefinition method)
        {
            var firstParameter = method.Parameters.FirstOrDefault();
            return firstParameter != null && firstParameter.Name == "this";
        }
        #endregion

        #region Visibility
        protected override StringBuilder AppendVisibility(StringBuilder buf, MethodDefinition method)
        {
            if (method.IsPublic
                || method.IsFamily
                || method.IsFamilyOrAssembly)
                return buf.Append("");
            return null;
        }

        private static string GetTypeVisibility(TypeAttributes ta)
        {
            switch (ta & TypeAttributes.VisibilityMask)
            {
                case TypeAttributes.Public:
                case TypeAttributes.NestedPublic:
                    return "";
            }
            return null;
        }

        private static string GetFieldVisibility(FieldDefinition field)
        {
            if (field.IsPublic
                || field.IsFamily
                || field.IsFamilyOrAssembly)
            {
                return "";
            }
            return null;
        }
        #endregion

        #region Supported
        public override bool IsSupported(TypeReference tref)
        {
            if (tref.DeclaringType != null
                && IsDiscriminatedUnion(tref.DeclaringType.Resolve())
                && tref.Name == "Tags")
            {
                return false;
            }

            return true;
        }

        public override bool IsSupported(MemberReference mref)
        {
            if (mref.DeclaringType != null && IsDiscriminatedUnion(mref.DeclaringType.Resolve()))
            {
                var property = mref as PropertyDefinition;
                if (property?.GetMethod != null)
                {
                    var fSharpFlags = GetFSharpFlags(property.GetMethod.CustomAttributes);
                    if (fSharpFlags.Any(i => i == SourceConstructFlags.UnionCase))// For descriminated unions show only properties with UnionCase attribute
                        return true;
                }
                return false;
            }
            switch (mref)
            {
                case MethodDefinition method:
                    return !(method.HasCustomAttributes && method.CustomAttributes.Any(
                                 ca => ca.GetDeclaringType() ==
                                       "System.Diagnostics.Contracts.ContractInvariantMethodAttribute"
                                       || ca.GetDeclaringType() ==
                                       Consts.CompilerGeneratedAttribute))
                            && AppendVisibility(new StringBuilder(), method) != null;
            }

            return true;
        }
        #endregion

        #region Private types
        // Copied from F# Core
        private enum SourceConstructFlags
        {
            /// <summary>Indicates that the compiled entity has no relationship to an element in F# source code.</summary>
            None,
            /// <summary>Indicates that the compiled entity is part of the representation of an F# union type declaration.</summary>
            SumType,
            /// <summary>Indicates that the compiled entity is part of the representation of an F# record type declaration.</summary>
            RecordType,
            /// <summary>Indicates that the compiled entity is part of the representation of an F# class or other object type declaration.</summary>
            ObjectType,
            /// <summary>Indicates that the compiled entity is part of the representation of an F# record or union case field declaration.</summary>
            Field,
            /// <summary>Indicates that the compiled entity is part of the representation of an F# exception declaration.</summary>
            Exception,
            /// <summary>Indicates that the compiled entity is part of the representation of an F# closure.</summary>
            Closure,
            /// <summary>Indicates that the compiled entity is part of the representation of an F# module declaration.</summary>
            Module,
            /// <summary>Indicates that the compiled entity is part of the representation of an F# union case declaration.</summary>
            UnionCase,
            /// <summary>Indicates that the compiled entity is part of the representation of an F# value declaration.</summary>
            Value,
            /// <summary>The mask of values related to the kind of the compiled entity.</summary>
            KindMask = 31,
            /// <summary>Indicates that the compiled entity had private or internal representation in F# source code.</summary>
            NonPublicRepresentation
        }

        private enum GenericParameterState
        {
            None,
            WithinTuple
        }

        private enum FSharpMethodKind
        {
            InModule,
            Static,
            Override,
            Abstract,
            Virtual,
            Common,
        }
        #endregion
    }
}
