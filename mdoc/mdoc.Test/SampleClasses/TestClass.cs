using System;
namespace mdoc.Test.SampleClasses
{
    public class TestClass
    {
        public TestClass () { }
        // private constructor
        TestClass (TestPrivateClass arg) { }

        // Unary Operators
		public static TestClass operator + (TestClass c1) { return new TestClass (); }
        public static TestClass operator - (TestClass c1) { return new TestClass (); }
        public static TestClass operator ! (TestClass c1) { return new TestClass (); }
        public static TestClass operator ~ (TestClass c1) { return new TestClass (); }
        public static TestClass operator ++ (TestClass c1) { return new TestClass (); }
        public static TestClass operator -- (TestClass c1) { return new TestClass (); }
        public static TestClass operator checked ++ (TestClass c1) { return new TestClass (); }
        public static TestClass operator checked -- (TestClass c1) { return new TestClass (); }
        public static TestClass operator checked - (TestClass c1) { return new TestClass (); }

        // Binary Operators
        public static TestClass operator + (TestClass c1, TestClass c2) { return new TestClass (); }
		public static TestClass operator - (TestClass c1, TestClass c2) {return new TestClass (); }
		public static TestClass operator / (TestClass c1, TestClass c2) { return new TestClass (); } 
        public static TestClass operator * (TestClass c1, TestClass c2) { return new TestClass (); }
		public static TestClass operator % (TestClass c1, TestClass c2) { return new TestClass (); }
        public static TestClass operator & (TestClass c1, TestClass c2) { return new TestClass (); }
        public static TestClass operator | (TestClass c1, TestClass c2) { return new TestClass (); }
        public static TestClass operator ^ (TestClass c1, TestClass c2) { return new TestClass (); }
        public static TestClass operator << (TestClass c1, int c2) { return new TestClass (); }
        public static TestClass operator >> (TestClass c1, int c2) { return new TestClass (); }
        public static TestClass operator >>> (TestClass c1, int c2) { return new TestClass(); }
        public static TestClass operator checked * (TestClass c1, TestClass c2) { return new TestClass(); }
        public static TestClass operator checked / (TestClass c1, TestClass c2) { return new TestClass(); }
        public static TestClass operator checked + (TestClass c1, TestClass c2) { return new TestClass(); }
        public static TestClass operator checked - (TestClass c1, TestClass c2) { return new TestClass(); }

        // Comparison Operators
        public static bool operator true (TestClass c1) { return false; }
		public static bool operator false (TestClass c1) { return false; }
		public static bool operator == (TestClass c1, TestClass c2) { return true; }
		public static bool operator != (TestClass c1, TestClass c2) { return true; }
        public static bool operator < (TestClass c1, TestClass c2) { return true; }
        public static bool operator > (TestClass c1, TestClass c2) { return true; }
        public static bool operator <= (TestClass c1, TestClass c2) { return true; }
        public static bool operator >= (TestClass c1, TestClass c2) { return true; }

        // Conversion Operators
        public static implicit operator TestClassTwo (TestClass c1) { return new TestClassTwo (); }
        public static implicit operator TestClass (TestClassTwo c1) { return new TestClass (); }
        public static explicit operator int (TestClass c1) { return 0; }
        public static explicit operator TestClass (int c1) { return new TestClass (); }
        public static explicit operator checked TestClass (int c1) { return new TestClass (); }
        public static explicit operator checked int (TestClass c1) { return 0; }

        public void DoSomethingWithParams (params int[] values) { }
        public void RefAndOut (ref int a, out int b) { b = 1; }
	}
}
