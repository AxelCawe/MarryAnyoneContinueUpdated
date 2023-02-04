// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Helpers.ShortLifeObject
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using System;


 
namespace MarryAnyone.Helpers
{
  internal class ShortLifeObject
  {
    protected object _o;
    protected DateTime _born;
    protected int _delayInMicroseconde;

    public ShortLifeObject(int delayInMicroseconde)
    {
      this._o = (object) null;
      this._born = DateTime.Now;
      this._delayInMicroseconde = delayInMicroseconde;
    }

    public bool Swap(object pO)
    {
      DateTime now = DateTime.Now;
      if (this._o == pO && (this._o != pO || now.Subtract(this._born).TotalMilliseconds <= (double) this._delayInMicroseconde))
        return false;
      this._o = pO;
      this._born = now;
      return true;
    }

    public virtual void Done() => this._o = (object) null;
  }
}
