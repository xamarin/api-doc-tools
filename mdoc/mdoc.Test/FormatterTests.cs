using NUnit.Framework;
using System;
using System.Reflection;
using System.Linq;
using System.Text;
using mdoc.Test.SampleClasses;
using Mono.Documentation.Updater;
using Mono.Documentation;
using Mono.Documentation.Updater.Formatters.CppFormatters;
using Mono.Documentation.Updater.Formatters;

namespace mdoc.Test
{
    [TestFixture ()]
    public class FormatterTests : BasicFormatterTests<CSharpMemberFormatter>
    {
        private CSharpMemberFormatter csharpMemberFormatter = new CSharpMemberFormatter();
        protected override CSharpMemberFormatter formatter => csharpMemberFormatter;

        [Test ()]
        public void Formatters_VerifyPrivateConstructorNull ()
        {
            // this is a private constructor
            var method = GetMethod (typeof(TestClass), m => m.IsConstructor && !m.IsPublic && m.Parameters.Count () == 1);

            MemberFormatter[] formatters = new MemberFormatter[]
            {
                new CSharpFullMemberFormatter (),
                new CSharpMemberFormatter(),
                new ILMemberFormatter(),
                new ILFullMemberFormatter(),
                new VBFullMemberFormatter (),
                new VBMemberFormatter(),
                new FSharpMemberFormatter(),
                new FSharpFullMemberFormatter(), 
            };
            var sigs = formatters.Select (f => f.GetDeclaration (method));

            foreach (var sig in sigs)
                Assert.IsNull (sig);
        }

        [Test]
        public void CSharp_op_Addition () =>
            TestBinaryOp ("Addition", "+");

        [Test]
        public void CSharp_op_Subtraction () =>
            TestBinaryOp ("Subtraction", "-");

        [Test]
        public void CSharp_op_Division () =>
            TestBinaryOp ("Division", "/");

        [Test]
        public void CSharp_op_Multiplication () =>
            TestBinaryOp ("Multiply", "*");

        [Test]
        public void CSharp_op_Modulus () =>
            TestBinaryOp ("Modulus", "%");

        [Test]
        public void CSharp_op_BitwiseAnd () =>
            TestBinaryOp ("BitwiseAnd", "&");

        [Test]
        public void CSharp_op_BitwiseOr () =>
            TestBinaryOp ("BitwiseOr", "|");

        [Test]
        public void CSharp_op_ExclusiveOr () =>
            TestBinaryOp ("ExclusiveOr", "^");

        [Test]
        public void CSharp_op_LeftShift () =>
            TestBinaryOp ("LeftShift", "<<", secondType: "int");

        [Test]
        public void CSharp_op_RightShift () =>
            TestBinaryOp ("RightShift", ">>", secondType: "int");

        [Test]
        public void CSharp_op_UnsignedRightShift () =>
            TestBinaryOp("UnsignedRightShift", ">>>", secondType: "int");

        [Test]
        public void CSharp_op_CheckedAddition () =>
           TestBinaryOp("CheckedAddition", "checked +");

        [Test]
        public void CSharp_op_CheckedSubtraction () =>
           TestBinaryOp("CheckedSubtraction", "checked -");

        [Test]
        public void CSharp_op_CheckedMultiply () =>
           TestBinaryOp("CheckedMultiply", "checked *");

        [Test]
        public void CSharp_op_CheckedDivision () =>
           TestBinaryOp("CheckedDivision", "checked /");

        [Test]
        public void CSharp_op_UnaryPlus () =>
			TestUnaryOp ("UnaryPlus", "+");

		[Test]
		public void CSharp_op_UnaryNegation () =>
			TestUnaryOp ("UnaryNegation", "-");

		[Test]
		public void CSharp_op_LogicalNot () =>
			TestUnaryOp ("LogicalNot", "!");

        [Test]
        public void CSharp_op_OnesComplement () =>
            TestUnaryOp ("OnesComplement", "~");

        [Test]
        public void CSharp_op_Decrement () =>
            TestUnaryOp ("Decrement", "--");

		[Test]
		public void CSharp_op_Increment () =>
			TestUnaryOp ("Increment", "++");

        [Test]
        public void CSharp_op_True () =>
            TestUnaryOp ("True", "true", returnType: "bool");

		[Test]
		public void CSharp_op_False () =>
            TestUnaryOp ("False", "false", returnType: "bool");

        [Test]
        public void CSharp_op_CheckedIncrement () =>
           TestUnaryOp("CheckedIncrement", "checked ++");

        [Test]
        public void CSharp_op_CheckedDecrement () =>
           TestUnaryOp("CheckedDecrement", "checked --");

        [Test]
        public void CSharp_op_CheckedUnaryNegation () =>
           TestUnaryOp("CheckedUnaryNegation", "checked -");

        [Test]
        public void CSharp_op_Equality () =>
            TestComparisonOp ("Equality", "==");

        [Test]
        public void CSharp_op_Inequality () =>
            TestComparisonOp ("Inequality", "!=");

        [Test]
        public void CSharp_op_LessThan () =>
            TestComparisonOp ("LessThan", "<");

        [Test]
        public void CSharp_op_GreaterThan () =>
            TestComparisonOp ("GreaterThan", ">");

        [Test]
        public void CSharp_op_LessThanOrEqual () =>
            TestComparisonOp ("LessThanOrEqual", "<=");

        [Test]
        public void CSharp_op_GreaterThanOrEqual () =>
            TestComparisonOp ("GreaterThanOrEqual", ">=");

        [Test]
        public void CSharp_op_Implicit () =>
            TestConversionOp ("Implicit", "implicit", "TestClass", "TestClassTwo");

        [Test]
        public void CSharp_op_Implicit_inverse () =>
            TestConversionOp ("Implicit", "implicit", "TestClassTwo", "TestClass");

        [Test]
        public void CSharp_op_Explicit () =>
            TestConversionOp ("Explicit", "explicit", "int", "TestClass");

        [Test]
        public void CSharp_op_Explicit_inverse () =>
            TestConversionOp ("Explicit", "explicit", "TestClass", "int");

        [Test]
        public void CSharp_op_Explicit_checked () =>
            TestConversionOp("CheckedExplicit", "explicit", "int", "TestClass");

        [Test]
        public void CSharp_op_Explicit_checked_inverse () =>
            TestConversionOp("CheckedExplicit", "explicit", "TestClass", "int");

        [Test]
        public void CSharp_modopt () =>
            TestMod ("SomeFunc2", "public SomeClass* SomeFunc2 (SomeClass param);", returnType: "SomeClass*");

        [Test]
        public void CSharp_modreq () =>
            TestMod ("SomeFunc", "public int SomeFunc (SomeClass* param);", returnType: "int");

        [Test]
        public void CSharp_doublepointer () =>
            TestMod ("SomeFunc3", "public SomeClass** SomeFunc3 (int param);", returnType: "cppcli.SomeClass**");

        [Test]
        public void CSharp_pointerref_modreqparam() =>
            TestMod("SomeFunc4", "public int SomeFunc4 (SomeClass** param, int param2);", returnType: "int");

        [Test]
        public void DoubleMod ()
        {
            string doubledUp = "System.ValueType modopt(System.DateTime) modopt(System.Runtime.CompilerServices.IsBoxed)";
            string result = MemberFormatter.RemoveMod (doubledUp);
            Assert.AreEqual ("System.ValueType", result);
        }

        [Test]
        public void DoubleMod_Mixed ()
        {
            string doubledUp = "System.ValueType modreq(System.DateTime) modopt(System.Runtime.CompilerServices.IsBoxed)";
            string result = MemberFormatter.RemoveMod (doubledUp);
            Assert.AreEqual ("System.ValueType", result);
        }

        [Test]
        public void DoubleMod_Pointer()
        {
            string doubledUp = "System.ValueType modreq(System.DateTime) modopt(System.Runtime.CompilerServices.IsBoxed)*";
            string result = MemberFormatter.RemoveMod(doubledUp);
            Assert.AreEqual("System.ValueType*", result);
        }

        [Test]
        public void Mod_Ref()
        {
            string doubledUp = "System.ValueType& modopt(System.Runtime.CompilerServices.IsBoxed)";
            string result = MemberFormatter.RemoveMod(doubledUp);
            Assert.AreEqual("System.ValueType&", result);
        }

        [Test]
        public void Params()
        {
            var member = GetMethod (typeof(TestClass), m => m.Name == "DoSomethingWithParams");
            var sig = formatter.GetDeclaration (member);
            Assert.AreEqual ("public void DoSomethingWithParams (params int[] values);", sig);
        }

        [Test]
        public void RefProperty()
        {
            var member = GetType(typeof(SpanSpecial<>))
                .Properties.FirstOrDefault(t => t.FullName == "T& mdoc.Test.SpanSpecial`1::Item(System.Int32)");
            var formatterCsharp = new CSharpFullMemberFormatter();
            string sig = formatterCsharp.GetDeclaration(member);
            Assert.AreEqual("public ref T this[int index] { get; }", sig);
        }

        [Test]
        public void IL_RefAndOut ()
        {
            var member = GetMethod (typeof(TestClass), m => m.Name == "RefAndOut");
            var formatter = new ILFullMemberFormatter ();
            var sig = formatter.GetDeclaration (member);
            Assert.AreEqual (".method public hidebysig instance void RefAndOut(int32& a, [out] int32& b) cil managed", sig);
        }

        [Test]
        public void CreateNewGuid()
        {
            var member = GetMethod(typeof(GuidClass), m => m.Name == "CreateNewGuid");
            var sig = formatter.GetDeclaration(member);
            Assert.AreEqual("public static Guid CreateNewGuid ();", sig);
        }

        [Test]
        public void ObjectIndentical()
        {
            var member = GetMethod(typeof(GuidClass), m => m.Name == "ObjectIndentical");
            var sig = formatter.GetDeclaration(member);
            Assert.AreEqual("public bool ObjectIndentical (Guid objGuid1, Guid objGuid2);", sig);
        }

        [Test]
        public void MethodSignature_Finalize() =>
            TestMethodSignature(typeof(SomeGenericClass<>),
                "~SomeGenericClass ();",
                "Finalize");
        [Test]
        public void FuncParams()
        {
            var member = GetMethod(typeof(SomeGenericClass<>), m => m.Name == "SomeMethod4");
            var sig = formatter.GetDeclaration(member);
            Assert.AreEqual("public void SomeMethod4 (out string a, T t, object b = default);", sig);
        }

        [TestCase(typeof(ReadonlyRefClass), "Ref", "public ref int Ref ();")]
        [TestCase(typeof(ReadonlyRefClass), "ReadonlyRef", "public ref readonly int ReadonlyRef ();")]
        [TestCase(typeof(ReadonlyRefClass), "RefInAndOutMethod", "public void RefInAndOutMethod (ref int a, in int b, out int c);")]
        [TestCase(typeof(ReadonlyRefClass), "InAttributeMethod", "public void InAttributeMethod (ref int a, in int b, out int c);")]
        [TestCase(typeof(GenericRefClass<>), "Ref", "public ref T Ref ();")]
        [TestCase(typeof(GenericRefClass<>), "ReadonlyRef", "public ref readonly T ReadonlyRef ();")]
        [TestCase(typeof(GenericRefClass<>), "RefInAndOutMethod", "public void RefInAndOutMethod (ref T a, in T b, out T c);")]
        [TestCase(typeof(GenericRefClass<>), "InAttributeMethod", "public void InAttributeMethod (ref T a, in T b, out T c);")]
        public void CSharpRefReturnMethodTest(Type type, string methodName, string expectedSignature)
        {
            var member = GetMethod(type, m => m.Name == methodName);
            var actualSignature = formatter.GetDeclaration(member);
            Assert.AreEqual(expectedSignature, actualSignature);
        }

        [TestCase(typeof(SomeClass), "TestScopedParams", "public ref T TestScopedParams<T> (scoped in T p1, out T p2, scoped ref T p3);")]
        public void CSharpScopedParamsTest(Type type, string methodName, string expectedSignature)
        {
            var member = GetMethod(type, m => m.Name == methodName);
            var actualSignature = formatter.GetDeclaration(member);
            Assert.AreEqual(expectedSignature, actualSignature);
        }


        [TestCase(typeof(ReadonlyRefClass), "RefProperty", "public ref int RefProperty { get; }")]
        [TestCase(typeof(ReadonlyRefClass), "Item", "public ref readonly int this[int index] { get; }")]
        [TestCase(typeof(GenericRefClass<>), "RefProperty", "public ref T RefProperty { get; }")]
        [TestCase(typeof(GenericRefClass<>), "Item", "public ref readonly T this[int index] { get; }")]
        public void CSharpRefReturnPropertyTest(Type type, string propertyName, string expectedSignature)
        {
            var member = GetProperty(type, p => p.Name == propertyName);
            var actualSignature = formatter.GetDeclaration(member);
            Assert.AreEqual(expectedSignature, actualSignature);
        }

        [Test]
        public void CSharpInModifier()
        {
            var member = GetMethod(typeof(SomeClass), m => m.Name == "SomeMethodWithInParameterModifier");
            var formatter = new CSharpFullMemberFormatter();
            var sig = formatter.GetDeclaration(member);
            Assert.AreEqual("public void SomeMethodWithInParameterModifier (in string s);", sig);
        }

        [Test]
        public void CSharpTuple()
        {
            var member = GetMethod(typeof(NullablesAndTuples), m => m.Name == "TupleReturn");
            var formatter = new CSharpFullMemberFormatter();
            var sig = formatter.GetDeclaration(member);
            Assert.AreEqual("public (int, string) TupleReturn ();", sig);
        }

        [Test]
        public void CSharpNullable()
        {
            var member = GetMethod(typeof(NullablesAndTuples), m => m.Name == "NullableInt");
            var formatter = new CSharpFullMemberFormatter();
            var sig = formatter.GetDeclaration(member);
            Assert.AreEqual("public int? NullableInt ();", sig);
        }

        [Test]
        public void PItest()
        {
            string sig = "";
            var member  = GetType(typeof(System.Math)).Fields.FirstOrDefault(t=>t.Name=="PI");
            BindingFlags flags    = BindingFlags.NonPublic | BindingFlags.Static;
            BindingFlags flagsPub = BindingFlags.Public | BindingFlags.Static;

            Type type1 = typeof(MDocUpdater);
            MethodInfo mInfo1 = type1.GetMethod("GetFieldConstValue", flags);
            Object[] parametors1 = new Object[] { member, sig };
            mInfo1.Invoke(null, parametors1);
            sig = (string)parametors1[1];
            var piValue = "3.1415926535897931";

#if NETCOREAPP
            piValue = "3.141592653589793";
#endif //NETCOREAPP

            Assert.AreEqual(piValue, sig);
          
            Type type2 = typeof(ILFullMemberFormatter);
            sig = "";
            MethodInfo mInfo2 = type2.GetMethod("AppendFieldValue", flags);
            Object[] parametors2 = new Object[] { new StringBuilder(), member};
            sig = mInfo2.Invoke(null, parametors2).ToString();
            Assert.AreEqual($" = ({piValue})", sig);
 
            Type type3 = typeof(DocUtils);
            sig = "";                      
            MethodInfo mInfo3 = type3.GetMethod("AppendFieldValue", flagsPub);
            Object[] parametors3 = new Object[] { new StringBuilder(), member };
            mInfo3.Invoke(null, parametors3);
            sig = parametors3[0].ToString();
            Assert.AreEqual($" = {piValue}", sig);
 
            Type type4 = typeof(CppFullMemberFormatter);
            sig = "";
            MethodInfo mInfo4 = type4.GetMethod("AppendFieldValue", flags);
            Object[] parametors4 = new Object[] { new StringBuilder(), member };
            sig = mInfo4.Invoke(null, parametors4).ToString();
            Assert.AreEqual($" = {piValue}", sig);
        }

        [Test]
        public void StaticConstructor()
        {
            var member = GetMethod(
                 GetType(typeof(StaticClass)),
                 m => m.Name == ".cctor"
            );
            var formatter = new CSharpFullMemberFormatter();
            var sigCsharp = formatter.GetDeclaration(member);
            Assert.AreEqual("static StaticClass ();", sigCsharp);

            var formatterVB = new VBFullMemberFormatter();
            var sigVB = formatterVB.GetDeclaration(member);
            Assert.AreEqual("Shared Sub New ()", sigVB);

            var formatterC = new CppFullMemberFormatter();
            var sigC = formatterC.GetDeclaration(member);
            Assert.AreEqual(" static StaticClass();", sigC);
        }

        [Test]
        public void MissSignature()
        {
            var fileStreamSig = "System.Void System.IO.FileStream::.ctor(System.String,System.IO.FileMode,System.Security.AccessControl.FileSystemRights,System.IO.FileShare,System.Int32,System.IO.FileOptions,System.Security.AccessControl.FileSecurity)";

#if NETCOREAPP
            fileStreamSig = "System.Void System.IO.FileStream::.ctor(System.String,System.IO.FileMode,System.IO.FileAccess,System.IO.FileShare,System.Int32,System.IO.FileOptions)";
#endif //NETCOREAPP

            var member1 = GetMethod(typeof(System.IO.FileStream), m => m.FullName == fileStreamSig);
            var fomatter1 = new VBMemberFormatter();
            // Original return null
            var sig1 = fomatter1.GetDeclaration(member1);
            Assert.NotNull(sig1);

            var member2 = GetMethod(typeof(TestClassThree), m => m.FullName == "System.Collections.IEnumerator mdoc.Test.SampleClasses.TestClassThree::System.Collections.IEnumerable.GetEnumerator()"); ;
            var formatter2 = new FSharpFormatter(MDocUpdater.Instance.TypeMap);
            // Original return null
            var sig2 = formatter2.GetDeclaration(member2);
            Assert.NotNull(sig2);
        }
        
        [Test]
        public void ClassInterface()
        {
            // CopyTo :
            // Orignal return value: "System.Void CopyTo (T[], System.Int32, )
            var sig = "System.Void CopyTo (System.Collections.Generic.KeyValuePair`2<System.String,mdoc.Test.SampleClasses.TestClassTwo>[], System.Int32, )";
            var type = GetType(typeof(TestClassThree));
            var interFaceMembers = GetClassInterface(type);
            bool flag = interFaceMembers.ContainsKey(sig);
            Assert.AreEqual(true, flag);
        }

        [Test]
        public void CSharpRefStructTest()
        {
            var type = GetType(typeof(SampleClasses.RefStruct));
            var typeSignature = formatter.GetDeclaration(type);
            Assert.AreEqual("public ref struct RefStruct", typeSignature);
        }

        [Test]
        public void CSharpReadOnlyRefStructTest()
        {
            var type = GetType(typeof(SampleClasses.ReadOnlyRefStruct));
            var typeSignature = formatter.GetDeclaration(type);
            Assert.AreEqual("public readonly ref struct ReadOnlyRefStruct", typeSignature);
        }

        [Test]
        public void CSharpReadOnlyPropertyTest()
        {
            TestPropertySignature(typeof(SampleClasses.StructWithReadOnlyMethod), "public string Property { get; }", "Property");
        }

        [TestCase("Sum", "public readonly double Sum ();")]
        [TestCase("GetNum", "readonly int Struct_Interface_A.GetNum ();")]
        public void CSharpReadOnlyMemberStructTest(string methodName, string expectedSignature)
        {
            var method = GetMethod(typeof(SampleClasses.StructWithReadOnlyMethod), m => m.Name.Contains(methodName));
            var methodSignature = formatter.GetDeclaration(method);
            Assert.AreEqual(expectedSignature, methodSignature);
        }

        [Test]
        public void CSharpTupleNamesTypeTest()
        {
            var type = GetType(typeof(SampleClasses.TupleNamesTestClass<,>));
            var typeSignature = formatter.GetDeclaration(type);
            Assert.AreEqual("public class TupleNamesTestClass<T1,T2> : IComparable<(T1, T2)>", typeSignature);
        }

        [Test]
        public void CSharpTupleNamesPropertyTest()
        {
            var property = GetProperty(typeof(SampleClasses.TupleNamesTestClass<,>), m => m.Name == "TuplePropertyType");
            var propertySignature = formatter.GetDeclaration(property);
            Assert.AreEqual("public (int a, int b) TuplePropertyType { get; }", propertySignature);
        }

        [Test]
        public void CSharpTupleNamesFieldTest()
        {
            var field = GetField(GetType(typeof(SampleClasses.TupleNamesTestClass<,>)), "TupleField");
            var fieldSignature = formatter.GetDeclaration(field);
            Assert.AreEqual("public (int a, int b, int c) TupleField;", fieldSignature);
        }

        [TestCase("TupleMethod", "public (int a, int, int b) TupleMethod ((int, int) t1, (int b, int c, int d) t2, (int, int) t3);")]
        [TestCase("RecursiveTupleMethod", "public ((int a, long b) c, int d) RecursiveTupleMethod ((((int a, long) b, string c) d, (int e, (float f, float g) h) i, int j) t);")]
        public void CSharpTupleNamesMethodTest(string methodName, string expectedSignature)
        {
            var method = GetMethod(typeof(SampleClasses.TupleNamesTestClass<,>), m => m.Name == methodName);
            var methodSignature = formatter.GetDeclaration(method);
            Assert.AreEqual(expectedSignature, methodSignature);
        }

        [TestCase("Property1", "public int Property1 { get; set; }")]
        [TestCase("Property2", "public int Property2 { get; init; }")]
        [TestCase("Property3", "public int Property3 { get; protected init; }")]
        [TestCase("Item", "public int this[int index] { get; init; }")]
        public void CSharpInitOnlySetterTest(string propertyName, string expectedSignature)
        {
            var property = GetProperty(typeof(SampleClasses.InitOnlySetter), p => p.Name == propertyName);
            var propertySignature = formatter.GetDeclaration(property);
            Assert.AreEqual(expectedSignature, propertySignature);
        }

        [Test]
        public void CSharpNativeIntGenericTypeTest()
        {
            var type = GetType(typeof(SampleClasses.GenericNativeIntClass<>));
            var typeSignature = formatter.GetDeclaration(type);
            Assert.AreEqual("public class GenericNativeIntClass<nint>", typeSignature);
        }

        [TestCase("Method1", "public (nint, nuint) Method1 (nint a, nuint b, IntPtr c, UIntPtr d);")]
        [TestCase("Method2", "public (nint, nuint) Method2 (List<nint> a, Dictionary<int,nuint> b);")]
        [TestCase("Method3", "public (nint, nuint) Method3 ((nint, nuint) a, (nuint, IntPtr) b, (UIntPtr, string) c);")]
        [TestCase("Method4", "public (((nint a, IntPtr) b, UIntPtr c) d, (nint e, (nuint f, IntPtr g) h) i) Method4 ();")]
        public void CSharpNativeIntMethodTest(string methodName, string expectedSignature)
        {
            var method = GetMethod(typeof(SampleClasses.NativeIntClass), m => m.Name == methodName);
            var methodSignature = formatter.GetDeclaration(method);
            Assert.AreEqual(expectedSignature, methodSignature);
        }

        [TestCase("UnsafeCombine", "public static R UnsafeCombine<T1,T2,R> (delegate*<T1, T2, R> combinator, T1 left, T2 right);")]
        [TestCase("UnsafeCombine1", "public static R UnsafeCombine1<T1,T2,R> (delegate* unmanaged[Cdecl]<T1, T2, R> combinator, T1 left, T2 right);")]
        [TestCase("UnsafeCombine2", "public static R UnsafeCombine2<T1,T2,T3,R> (delegate* unmanaged[Stdcall]<ref T1, in T2, out T3, R> combinator, T1 left, T2 right, T3 outVar);")]
        [TestCase("UnsafeCombine3", "public static R UnsafeCombine3<T1,T2,R> (delegate* unmanaged[Fastcall]<T1, T2, ref R> combinator, T1 left, T2 right);")]
        [TestCase("UnsafeCombine4", "public static R UnsafeCombine4<T1,T2,R> (delegate* unmanaged[Thiscall]<T1, T2, ref readonly R> combinator, T1 left, T2 right);")]
        [TestCase("UnsafeCombine5", "public static void UnsafeCombine5 (delegate* unmanaged[Cdecl]<void> combinator);")]
        [TestCase("UnsafeCombine6", "public static void UnsafeCombine6 (delegate*<delegate* unmanaged[Fastcall]<string, int>, delegate*<string, int>> combinator);")]
        [TestCase("UnsafeCombine7", "public static delegate*<delegate* unmanaged[Thiscall]<string, int>, delegate*<string, int>> UnsafeCombine7 ();")]
        public void CSharpFuctionPointersTest(string methodName, string expectedSignature)
        {
            var method = GetMethod(typeof(SampleClasses.FunctionPointers), m => m.Name == methodName);
            var methodSignature = formatter.GetDeclaration(method);
            Assert.AreEqual(expectedSignature, methodSignature);
        }

        [TestCase("UnsafeCombine1", "public static R UnsafeCombine1<T1,T2,R> (delegate* unmanaged<T1, T2, R> combinator, T1 left, T2 right);")]
        [TestCase("UnsafeCombine2", "public static R UnsafeCombine2<T1,T2,R> (delegate* unmanaged[Cdecl, SuppressGCTransition]<T1, T2, R> combinator, T1 left, T2 right);")]
        [TestCase("UnsafeCombine3", "public static R UnsafeCombine3<T1,T2,R> (delegate* unmanaged[Stdcall, MemberFunction]<T1, T2, R> combinator, T1 left, T2 right);")]
        [TestCase("UnsafeCombine4", "public static void UnsafeCombine4 (delegate*<delegate* unmanaged[Cdecl, Fastcall]<string, int>, delegate*<string, int>> combinator);")]
        [TestCase("UnsafeCombine5", "public static delegate* unmanaged[Cdecl, Fastcall]<delegate* unmanaged[Thiscall, MemberFunction]<string, int>, delegate*<string, int>> UnsafeCombine5 ();")]
        public void CSharpFuctionPointersUnmanagedExtTest(string methodName, string expectedSignature)
        {
            var functionPointersDllPath = "../../../../external/Test/FunctionPointersTest.dll";
            TestMethodSignature(functionPointersDllPath, "FunctionPointersTest.FunctionPointers", methodName, expectedSignature);
        }

        [TestCase("StaticVirtualMembers.StaticVirtualMemberInInterface`3", "StaticVirtualMethod", "public static virtual int StaticVirtualMethod (int left, int right);")]
        [TestCase("StaticVirtualMembers.StaticVirtualMemberInInterface`3", "op_Addition", "public static abstract TResult operator + (TSelf left, TOther right);")]
        [TestCase("StaticVirtualMembers.StaticVirtualMemberInInterface`3", "op_CheckedAddition", "public static virtual TResult operator checked + (TSelf left, TOther right);")]
        [TestCase("StaticVirtualMembers.InterfaceI`1", "M", "public static abstract void M ();")]
        [TestCase("StaticVirtualMembers.InterfaceI`1", "M1", "public static virtual void M1 ();")]
        [TestCase("StaticVirtualMembers.InterfaceI`1", "op_Addition", "public static abstract T operator + (T l, T r);")]
        [TestCase("StaticVirtualMembers.InterfaceI`1", "op_Equality", "public static abstract bool operator == (T l, T r);")]
        [TestCase("StaticVirtualMembers.InterfaceI`1", "op_Inequality", "public static abstract bool operator != (T l, T r);")]
        [TestCase("StaticVirtualMembers.InterfaceI`1", "op_Implicit", "public static abstract implicit operator T (string s);")]
        [TestCase("StaticVirtualMembers.InterfaceI`1", "op_Explicit", "public static abstract explicit operator string (T t);")]
        [TestCase("StaticVirtualMembers.InterfaceI`1", "op_CheckedAddition", "public static virtual T operator checked + (T l, T r);")]
        public void CSharpStaticVirtualMethodTest(string typeFullName, string methodName, string expectedSignature)
        {
            var staticVirtualMemberDllPath = "../../../../external/Test/StaticVirtualMembers.dll";
            TestMethodSignature(staticVirtualMemberDllPath, typeFullName, methodName, expectedSignature);
        }

        [TestCase("StaticVirtualMembers.InterfaceI`1", "P", "public static abstract T P { get; set; }")]
        [TestCase("StaticVirtualMembers.InterfaceI`1", "P1", "public static virtual T P1 { get; set; }")]
        public void CSharpStaticVirtualPropertyTest(string typeFullName, string propertyName, string expectedSignature)
        {
            var staticVirtualMemberDllPath = "../../../../external/Test/StaticVirtualMembers.dll";
            TestPropertySignature(staticVirtualMemberDllPath, typeFullName, propertyName, expectedSignature);
        }

        [TestCase("StaticVirtualMembers.InterfaceI`1", "E", "static abstract event Action E;")]
        [TestCase("StaticVirtualMembers.InterfaceI`1", "E1", "static virtual event Action E1;")]
        public void CSharpStaticVirtualEventTest(string typeFullName, string eventName, string expectedSignature)
        {
            var staticVirtualMemberDllPath = "../../../../external/Test/StaticVirtualMembers.dll";
            TestEventSignature(staticVirtualMemberDllPath, typeFullName, eventName, expectedSignature);
        }

        [TestCase("StaticVirtualMembers.Derived", "M", "public static void M ();")]
        [TestCase("StaticVirtualMembers.Derived",
            "StaticVirtualMembers.StaticVirtualMemberInInterface<StaticVirtualMembers.Derived,StaticVirtualMembers.Derived,System.Int32>.M",
            "static void StaticVirtualMemberInInterface<Derived,Derived,int>.M ();")]
        [TestCase("StaticVirtualMembers.Derived",
            "StaticVirtualMembers.StaticVirtualMemberInInterface<StaticVirtualMembers.Derived,StaticVirtualMembers.Derived,System.Int32>.ToBeImplemented",
            "void StaticVirtualMemberInInterface<Derived,Derived,int>.ToBeImplemented ();")]
        [TestCase("StaticVirtualMembers.Derived",
            "StaticVirtualMembers.StaticVirtualMemberInInterface<StaticVirtualMembers.Derived,StaticVirtualMembers.Derived,System.Int32>.op_Addition",
            "static int StaticVirtualMemberInInterface<Derived,Derived,int>.operator + (Derived left, Derived right);")]
        public void CSharpStaticMethodImplementation(string typeFullName, string methodName, string expectedSignature)
        {
            var staticVirtualMemberDllPath = "../../../../external/Test/StaticVirtualMembers.dll";
            TestMethodSignature(staticVirtualMemberDllPath, typeFullName, methodName, expectedSignature);
        }

        [TestCase("StaticVirtualMembers.ClassC", "StaticVirtualMembers.InterfaceI<StaticVirtualMembers.ClassC>.P",
            "static ClassC StaticVirtualMembers.InterfaceI<StaticVirtualMembers.ClassC>.P { get; set; }")]
        public void CSharpStaticPropertyImplementation(string typeFullName, string propertyName, string expectedSignature)
        {
            var staticVirtualMemberDllPath = "../../../../external/Test/StaticVirtualMembers.dll";
            TestPropertySignature(staticVirtualMemberDllPath, typeFullName, propertyName, expectedSignature);
        }

        [TestCase("StaticVirtualMembers.ClassC", "StaticVirtualMembers.InterfaceI<StaticVirtualMembers.ClassC>.E",
            "static event Action StaticVirtualMembers.InterfaceI<StaticVirtualMembers.ClassC>.E;")]
        public void CSharpStaticEventImplementation(string typeFullName, string eventName, string expectedSignature)
        {
            var staticVirtualMemberDllPath = "../../../../external/Test/StaticVirtualMembers.dll";
            TestEventSignature(staticVirtualMemberDllPath, typeFullName, eventName, expectedSignature);
        }

        #region Helper Methods
        string RealTypeName(string name){
            switch (name) {
                case "bool": return "Boolean";
                case "int": return "Int32";
                default: return name;
            }
        }


        void TestConversionOp (string name, string type, string leftType, string rightType) {
            TestOp (name, $"public static {type} operator {(name.StartsWith("Checked") ? "checked " : "")}{leftType} ({rightType} c1);", argCount: 1, returnType: leftType);
        }

        void TestComparisonOp (string name, string op)
        {
            TestOp (name, $"public static bool operator {op} (TestClass c1, TestClass c2);", argCount: 2, returnType: "Boolean");    
        }

        void TestUnaryOp (string name, string op, string returnType = "TestClass")
        {
            TestOp (name, $"public static {returnType} operator {op} (TestClass c1);", argCount: 1, returnType: returnType);
        }

        void TestBinaryOp (string name, string op, string returnType = "TestClass", string secondType = "TestClass")
        {
            TestOp (name, $"public static {returnType} operator {op} (TestClass c1, {secondType} c2);", argCount: 2, returnType: returnType);
        }

        void TestOp (string name, string expectedSig, int argCount, string returnType = "TestClass")
        {
            var member = GetMethod (typeof(TestClass), m => m.Name == $"op_{name}" && m.Parameters.Count == argCount && m.ReturnType.Name == RealTypeName (returnType));
            var formatter = new CSharpMemberFormatter ();
            var sig = formatter.GetDeclaration (member);
            Assert.AreEqual (expectedSig, sig);
        }

        void TestMod (string name, string expectedSig, int argCount = 1, string returnType = "SomeClass")
        {
            var member = GetMethod (
                    GetType ("SampleClasses/cppcli.dll", "cppcli.SomeInterface"), 
                    m => m.Name == name
            );
            var formatter = new CSharpMemberFormatter ();
			var sig = formatter.GetDeclaration (member);
			Assert.AreEqual (expectedSig, sig);
        }
        #endregion
    }
}
