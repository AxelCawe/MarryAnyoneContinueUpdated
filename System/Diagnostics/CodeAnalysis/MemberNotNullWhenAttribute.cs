// Decompiled with JetBrains decompiler
// Type: System.Diagnostics.CodeAnalysis.MemberNotNullWhenAttribute
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll


 
namespace System.Diagnostics.CodeAnalysis
{
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
  [ExcludeFromCodeCoverage]
  [DebuggerNonUserCode]
  internal sealed class MemberNotNullWhenAttribute : Attribute
  {
    public bool ReturnValue { get; }

    public string[] Members { get; }

    public MemberNotNullWhenAttribute(bool returnValue, string member)
    {
      this.ReturnValue = returnValue;
      this.Members = new string[1]{ member };
    }

    public MemberNotNullWhenAttribute(bool returnValue, params string[] members)
    {
      this.ReturnValue = returnValue;
      this.Members = members;
    }
  }
}
