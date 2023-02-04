// Decompiled with JetBrains decompiler
// Type: System.Diagnostics.CodeAnalysis.DoesNotReturnIfAttribute
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

namespace System.Diagnostics.CodeAnalysis
{
  [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
  [ExcludeFromCodeCoverage]
  [DebuggerNonUserCode]
  internal sealed class DoesNotReturnIfAttribute : Attribute
  {
    public bool ParameterValue { get; }

    public DoesNotReturnIfAttribute(bool parameterValue) => this.ParameterValue = parameterValue;
  }
}
