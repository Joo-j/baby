using System.Collections.Generic;

namespace Supercent.Util
{
    public struct BufferedValue<T>
    {
        T value;
        public T Value
        {
            set
            {
                if (EqualityComparer<T>.Default.Equals(this.value, value))
                    return;

                this.value = value;
                IsChanged = true;
            }
            get => value;
        }

        public bool IsChanged { private set; get; }



        public BufferedValue(T value)
        {
            this.value = value;
            IsChanged = true;
        }

        public T Confirm()
        {
            IsChanged = false;
            return value;
        }

        public override string ToString() => $"{value}, {IsChanged}";
    }
}