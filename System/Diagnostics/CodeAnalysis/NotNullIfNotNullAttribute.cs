// Decompiled with JetBrains decompiler
// Type: System.Diagnostics.CodeAnalysis.NotNullIfNotNullAttribute
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll


 
namespace System.Diagnostics.CodeAnalysis
{
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
  [ExcludeFromCodeCoverage]
  [DebuggerNonUserCode]
  internal sealed class NotNullIfNotNullAttribute : Attribute
  {
    public string ParameterName { get; }

    public NotNullIfNotNullAttribute(string parameterName) => this.ParameterName = parameterName;
  }
}
