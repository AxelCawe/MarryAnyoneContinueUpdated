// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Behaviors.EnumerableHelper
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using System.Collections.Generic;
using System.Linq;



namespace MarryAnyone.Behaviors
{
  public static class EnumerableHelper
  {
    public static T Random<T>(this IEnumerable<T> input, System.Random random) => input == null || input.Count<T>() == 0 ? default (T) : input.ElementAt<T>(random.Next(input.Count<T>()));
  }
}
