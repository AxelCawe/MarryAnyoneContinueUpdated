// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Helpers.ShortLifeBiObjects
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using System.Collections.Generic;


 
namespace MarryAnyone.Helpers
{
  internal class ShortLifeBiObjects
  {
    public List<ShortLifeBiObject> _listO;
    protected int _delayInMicroseconde;

    public ShortLifeBiObjects(int delayInMicroseconde)
    {
      this._delayInMicroseconde = delayInMicroseconde;
      this._listO = new List<ShortLifeBiObject>();
    }

    public bool Swap(object pO, object pO2)
    {
      for (int index = 0; index < this._listO.Count; ++index)
      {
        bool flag = this._listO[index].Resolve(pO, pO2);
        if (flag)
          return false;
        if (!flag && this._listO[index].IsEmpty())
        {
          this._listO.RemoveAt(index);
          --index;
        }
      }
      this._listO.Add(new ShortLifeBiObject(this._delayInMicroseconde, pO, pO2));
      return true;
    }

    public void Done()
    {
      foreach (ShortLifeObject shortLifeObject in this._listO)
        shortLifeObject.Done();
      this._listO.Clear();
    }
  }
}
