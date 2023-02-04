// Decompiled with JetBrains decompiler
// Type: System.Diagnostics.CodeAnalysis.MemberNotNullAttribute
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll


 
namespace System.Diagnostics.CodeAnalysis
{
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
  [ExcludeFromCodeCoverage]
  [DebuggerNonUserCode]
  internal sealed class MemberNotNullAttribute : Attribute
  {
    public string[] Members { get; }

    public MemberNotNullAttribute(string member) => this.Members = new string[1]
    {
      member
    };

    public MemberNotNullAttribute(params string[] members) => this.Members = members;
  }
}
