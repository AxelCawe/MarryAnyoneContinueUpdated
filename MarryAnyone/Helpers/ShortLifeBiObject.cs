// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Helpers.ShortLifeBiObject
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using System;


 
namespace MarryAnyone.Helpers
{
  internal class ShortLifeBiObject : ShortLifeObject
  {
    protected object _o2;

    public ShortLifeBiObject(int delayInMicroseconde)
      : base(delayInMicroseconde)
    {
      this._o2 = (object) null;
    }

    public ShortLifeBiObject(int delayInMicroseconde, object o, object o2)
      : base(delayInMicroseconde)
    {
      this._o = o;
      this._o2 = o2;
    }

    public bool Swap(object pO, object pO2)
    {
      DateTime now = DateTime.Now;
      if (this._o == pO && this._o2 == pO2 && (this._o != pO || this._o2 != pO2 || now.Subtract(this._born).TotalMilliseconds <= (double) this._delayInMicroseconde))
        return false;
      this._o = pO;
      this._born = now;
      return true;
    }

    public bool Resolve(object pO, object pO2)
    {
      DateTime now = DateTime.Now;
      bool flag = now.Subtract(this._born).TotalMilliseconds <= (double) this._delayInMicroseconde;
      if (((this._o != pO ? 0 : (this._o2 == pO2 ? 1 : 0)) & (flag ? 1 : 0)) != 0)
      {
        this._born = now;
        return true;
      }
      if (!flag)
        this.Done();
      return false;
    }

    public bool IsEmpty() => this._o == null && this._o2 == null;

    public override void Done()
    {
      this._o2 = (object) null;
      base.Done();
    }
  }
}
