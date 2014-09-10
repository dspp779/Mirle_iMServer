using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Mirle.iMServer.Model
{
    /// <summary>
    /// 資料的抽象型別，為站位資料與點位資料之基底型別
    /// 實作值改變通知介面、相等比較介面、大小比較介面
    /// </summary>
    public abstract class AbstractData : INotifyPropertyChanged, IComparable, IEquatable<AbstractData> 
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyValueChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        // 需實作搜尋方法
        public abstract bool Contains(string[] keywords);

        // 檢查來源是否包含特定字串
        protected static bool Contains(string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        public virtual int CompareTo(object obj)
        {
            return 0;
        }

        public override int GetHashCode()
        {
            return this.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this == obj;
        }

        public virtual bool Equals(AbstractData obj)
        {
            return this == obj;
        }

    }
}
