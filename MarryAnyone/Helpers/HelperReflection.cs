// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Helpers.HelperReflection
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using System.Reflection;


 
namespace MarryAnyone.Helpers
{
  internal static class HelperReflection
  {
    public static string Properties(object o, string sep, BindingFlags flag)
    {
      string str1 = (string) null;
      if ((flag & BindingFlags.Instance) != BindingFlags.Default)
      {
        foreach (PropertyInfo property in o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
          string str2;
          try
          {
            str2 = string.Format("{0} ?= {1}", (object) property.Name, property.GetValue(o, (object[]) null));
          }
          catch
          {
            str2 = string.Format("{0} READ ERROR", (object) property.Name);
          }
          str1 = str1 != null ? str1 + sep + str2 : str2;
        }
      }
      if ((flag & BindingFlags.Static) != BindingFlags.Default)
      {
        foreach (PropertyInfo property in o.GetType().GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
        {
          string str3;
          try
          {
            str3 = string.Format("static {0} ?= {1}", (object) property.Name, property.GetValue((object) null, (object[]) null));
          }
          catch
          {
            str3 = string.Format("{0} READ ERROR", (object) property.Name);
          }
          str1 = str1 != null ? str1 + sep + str3 : str3;
        }
      }
      return str1;
    }
  }
}
