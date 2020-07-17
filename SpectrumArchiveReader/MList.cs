using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SpectrumArchiveReader
{
    [Serializable, DebuggerDisplay("Count = {Count} Capacity = {Capacity}")]
    public class MList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
    {
        public const int InitialCapacity = 2; // Для максимизации скорости лучше ставить 500 - получено эмпирически при добавлении 1 000 000 элементов.
        public int Capacity;
        /// <summary>
        /// Аналог свойства Count, но не свойство, а поле. Работает быстрее свойства Count в цикле for в месте проверки конца цикла 
        /// но медленнее чем предварительная инициализация локальной переменной в for.
        /// </summary>
        public int Cnt = 0;
        /// <summary>
        /// Данные. При прямом обращении скорость выше только для структур. 
        /// Для ссылочных типов скорость при прямом обращении не возрастает - можно использовать индексатор вместо прямого доступа к Data.
        /// </summary>
        public T[] Data;
        public double GrowFactor = 2;
        public int GrowLen = 100;
        public GrowMode Grow = GrowMode.Exponential;
        [NonSerialized]
        private object syncRoot;

        public enum GrowMode
        {
            Exponential,
            Linear
        }

        /// <summary>
        /// Свойство из интерфейса IList. Вместо него лучше использовать поле Cnt.
        /// </summary>
        public int Count
        {
            get { return Cnt; }
            set { Cnt = value; }
        }

        public T this[int i]
        {
            get { return Data[i]; }
            set { Data[i] = value; }
        }

        /// <summary>
        /// Последний элемент массива. Никаких проверок на его существование не происходит. Будет выход за массив если Count == 0.
        /// Если нужна скорость, то лучше писать инлайном: Data[Cnt - 1].
        /// </summary>
        public T LastItem
        {
            get { return Data[Cnt - 1]; }
            set { Data[Cnt - 1] = value; }
        }

        /// <summary>
        /// Начальная ёмкость массива будет 2 элемента.
        /// </summary>
        public MList()
        {
            Capacity = InitialCapacity;
            Data = new T[InitialCapacity];
        }

        /// <summary>
        /// Начальная ёмкость массива будет 2 элемента.
        /// </summary>
        /// <param name="growMode"></param>
        /// <param name="growFactor"></param>
        /// <param name="growLinear"></param>
        public MList(GrowMode growMode, double growFactor, int growLinear)
        {
            Grow = growMode;
            Capacity = InitialCapacity;
            Data = new T[InitialCapacity];
            GrowFactor = growFactor;
            GrowLen = growLinear;
        }

        public MList(int capacity)
        {
            this.Capacity = capacity;
            Data = new T[capacity];
        }

        /// <summary>
        /// Создание MList с указанием массива который будет качестве лежащего в основе MList.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="count"></param>
        public MList(T[] array, int count)
        {
            AssignArray(array, count);
        }

        /// <summary>
        /// Создание MList с указанием массива который будет качестве лежащего в основе MList. Count будет равен длине массива.
        /// </summary>
        /// <param name="array"></param>
        public MList(T[] array)
        {
            AssignArray(array);
        }

        /// <summary>
        /// Создание MList с указанием массива который будет качестве лежащего в основе MList.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="growMode"></param>
        /// <param name="growFactor"></param>
        /// <param name="growLinear"></param>
        public MList(T[] array, GrowMode growMode, double growFactor, int growLinear)
        {
            AssignArray(array);
            Grow = growMode;
            GrowFactor = growFactor;
            GrowLen = growLinear;
        }

        /// <summary>
        /// Создание MList, с копированием содержимого указанного массива.
        /// </summary>
        /// <param name="arrayToBeCopied"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        public MList(T[] arrayToBeCopied, int startIndex, int count)
        {
            this.Capacity = count;
            this.Cnt = count;
            Data = new T[count];
            Array.Copy(arrayToBeCopied, startIndex, Data, 0, count);
        }

        /// <summary>
        /// Создание MList, с копированием содержимого переданного объекта MList. Копируется только Count значений. Размер массива лежащего в основе MList будет Count.
        /// </summary>
        /// <param name="arrayToBeCopied"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        public MList(MList<T> mlistToBeCopied)
        {
            this.Capacity = mlistToBeCopied.Cnt;
            this.Cnt = mlistToBeCopied.Cnt;
            Data = new T[mlistToBeCopied.Cnt];
            Array.Copy(mlistToBeCopied.Data, 0, Data, 0, mlistToBeCopied.Cnt);
        }

        /// <summary>
        /// Создание MList с копированием или клонированием содержимого указанного объекта MList.
        /// </summary>
        /// <param name="mlistToBeCopied"></param>
        /// <param name="preserveCapacity"></param>
        public MList(MList<T> mlistToBeCopied, bool preserveCapacity, bool cloneItems)
        {
            this.Capacity = preserveCapacity ? mlistToBeCopied.Capacity : mlistToBeCopied.Cnt;
            this.Cnt = mlistToBeCopied.Cnt;
            Data = new T[this.Capacity];
            if (cloneItems)
            {
                for (int i = 0; i < Cnt; i++)
                {
                    Data[i] = (T)((ICloneable)mlistToBeCopied.Data[i]).Clone();
                }
            }
            else
            {
                Array.Copy(mlistToBeCopied.Data, 0, Data, 0, mlistToBeCopied.Cnt);
            }
        }

        /// <summary>
        /// Создание MList с копированием или клонированием содержимого указанного массива.
        /// </summary>
        /// <param name="mlistToBeCopied"></param>
        /// <param name="preserveCapacity"></param>
        public MList(T[] arrayToBeCopied, bool cloneItems, int len = -1)
        {
            this.Capacity = arrayToBeCopied.Length;
            this.Cnt = len < 0 ? this.Capacity : len;
            Data = new T[this.Capacity];
            if (cloneItems)
            {
                for (int i = 0; i < Cnt; i++)
                {
                    Data[i] = (T)((ICloneable)arrayToBeCopied[i]).Clone();
                }
            }
            else
            {
                Array.Copy(arrayToBeCopied, 0, Data, 0, this.Cnt);
            }
        }

        public MList(IEnumerable<T> collection)
        {
            ICollection<T> c = collection as ICollection<T>;
            if (c != null)
            {
                int count = c.Count;
                Data = new T[count];
                c.CopyTo(Data, 0);
                this.Cnt = count;
                Capacity = count;
            }
            else
            {
                this.Cnt = 0;
                this.Capacity = InitialCapacity;
                Data = new T[InitialCapacity];
                using (IEnumerator<T> en = collection.GetEnumerator())
                {
                    while (en.MoveNext())
                    {
                        Add(en.Current);
                    }
                }
            }
        }

        /// <summary>
        /// Создание MList, с копированием содержимого указанного объекта MList. Копируется только Count значений. Размер массива лежащего в основе MList будет Count.
        /// Если mlistToBeCopied == null, то возвращает null.
        /// </summary>
        /// <param name="arrayToBeCopied"></param>
        public static MList<T> FromMList(MList<T> mlistToBeCopied)
        {
            if (mlistToBeCopied == null) return null;
            MList<T> r = new MList<T>(mlistToBeCopied.Cnt);
            r.Cnt = mlistToBeCopied.Cnt;
            r.Data = new T[mlistToBeCopied.Cnt];
            Array.Copy(mlistToBeCopied.Data, 0, r.Data, 0, mlistToBeCopied.Cnt);
            return r;
        }

        /// <summary>
        /// Установка размера массива чтобы он был не меньше min. Происходит с учетом режима роста
        /// (массив может быть умножен в длине на GrowFactor или увеличен на GrowLen).
        /// Переменная Cnt должна отражать количество имеющихся элементов, т.к. в новый массив копируется Cnt элементов.
        /// </summary>
        /// <param name="min"></param>
        public void EnsureCapacity(int min)
        {
            if (Capacity < min)
            {
                int num;
                if (Capacity == 0)
                {
                    num = InitialCapacity;
                }
                else
                {
                    num = Grow == GrowMode.Exponential ? (int)Math.Round(Capacity * GrowFactor) : Capacity + GrowLen;
                }
                //int num = (Capacity == 0) ? initialCapacity : (Capacity * 2);
                if (num < min) num = min;
                Capacity = num;
                //Capacity *= 2;  // Множитель увеличивающий следующий массив. Чем он больше, тем больше скорость добавления, но и больше лишний расход памяти.
                T[] destinationArray = new T[Capacity];
                Array.Copy(Data, 0, destinationArray, 0, Cnt);
                Data = destinationArray;
            }
        }

        /// <summary>
        /// Увеличение размера массива до размера minSize, если он меньше minSize.
        /// </summary>
        /// <param name="minSize"></param>
        /// <param name="doCopy">Флаг копирования данных из старого массива в новый в количестве Count элементов.</param>
        public void EnsureCapacityStrictly(int minSize, bool doCopy)
        {
            if (Capacity < minSize)
            {
                Capacity = minSize;
                T[] destinationArray = new T[Capacity];
                if (doCopy)
                {
                    Array.Copy(Data, 0, destinationArray, 0, Cnt);
                }
                Data = destinationArray;
            }
        }

        public void Add(T item)
        {
            if (Cnt == Capacity) EnsureCapacity(Cnt + 1);
            Data[Cnt] = item;
            Cnt++;
        }

        public void AddRef(ref T item)
        {
            if (Cnt == Capacity) EnsureCapacity(Cnt + 1);
            Data[Cnt] = item;
            Cnt++;
        }

        public void AssignArray(T[] array, int count)
        {
            Data = array;
            this.Cnt = count;
            Capacity = array.Length;
        }

        public void AssignArray(T[] array)
        {
            Data = array;
            Cnt = array.Length;
            Capacity = Cnt;
        }

        public void AssignArray(MList<T> list)
        {
            Data = list.Data;
            Cnt = list.Cnt;
        }

        /// <summary>
        /// Копирование массива из MList.
        /// </summary>
        /// <param name="source"></param>
        public void CopyArray(MList<T> source)
        {
            EnsureCapacity(source.Cnt);
            Array.Copy(source.Data, 0, Data, 0, source.Cnt);
            Cnt = source.Cnt;
        }

        public void AddRange(IEnumerable<T> collection, bool cloneItems = false)
        {
            InsertRange(Cnt, collection, cloneItems);
        }

        public int BinarySearch(T item)
        {
            return BinarySearch(0, Cnt, item, null);
        }

        public int BinarySearch(T item, IComparer<T> comparer)
        {
            return BinarySearch(0, Cnt, item, comparer);
        }

        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            //if ((index < 0) || (count < 0))
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException((index < 0) ? ExceptionArgument.index : ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            //}
            //if ((this._size - index) < count)
            //{
            //    ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            //}
            return Array.BinarySearch<T>(Data, index, count, item, comparer);
        }

        public void Clear()
        {
            Array.Clear(Data, 0, Cnt);
            Cnt = 0;
        }

        public bool Contains(T item)
        {
            if (item == null)
            {
                for (int j = 0; j < Cnt; j++)
                {
                    if (Data[j] == null)
                    {
                        return true;
                    }
                }
                return false;
            }
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < Cnt; i++)
            {
                if (comparer.Equals(Data[i], item))
                {
                    return true;
                }
            }
            return false;
        }

        public MList<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
        {
            //if (converter == null)
            //{
            //    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.converter);
            //}
            MList<TOutput> list = new MList<TOutput>(Cnt);
            for (int i = 0; i < Cnt; i++)
            {
                list.Data[i] = converter(Data[i]);
            }
            list.Cnt = Cnt;
            return list;
        }

        public bool Exists(Predicate<T> match)
        {
            return (FindIndex(match) != -1);
        }

        public T Find(Predicate<T> match)
        {
            //if (match == null)
            //{
            //    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            //}
            for (int i = 0; i < Cnt; i++)
            {
                if (match(Data[i]))
                {
                    return Data[i];
                }
            }
            return default(T);
        }

        public MList<T> FindAll(Predicate<T> match)
        {
            //if (match == null)
            //{
            //    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            //}
            MList<T> list = new MList<T>();
            for (int i = 0; i < Cnt; i++)
            {
                if (match(Data[i]))
                {
                    list.Add(Data[i]);
                }
            }
            return list;
        }

        public int FindIndex(Predicate<T> match)
        {
            return FindIndex(0, Cnt, match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            return FindIndex(startIndex, Cnt - startIndex, match);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            //if (startIndex > Count)
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
            //}
            //if ((count < 0) || (startIndex > (Count - count)))
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
            //}
            //if (match == null)
            //{
            //    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            //}
            int num = startIndex + count;
            for (int i = startIndex; i < num; i++)
            {
                if (match(Data[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public T FindLast(Predicate<T> match)
        {
            //if (match == null)
            //{
            //    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            //}
            for (int i = Cnt - 1; i >= 0; i--)
            {
                if (match(Data[i]))
                {
                    return Data[i];
                }
            }
            return default(T);
        }

        public int FindLastIndex(Predicate<T> match)
        {
            return FindLastIndex(Cnt - 1, Cnt, match);
        }

        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            return FindLastIndex(startIndex, startIndex + 1, match);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            //if (match == null)
            //{
            //    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            //}
            //if (Count == 0)
            //{
            //    if (startIndex != -1)
            //    {
            //        ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
            //    }
            //}
            //else if (startIndex >= Count)
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
            //}
            //if ((count < 0) || (((startIndex - count) + 1) < 0))
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
            //}
            int num = startIndex - count;
            for (int i = startIndex; i > num; i--)
            {
                if (match(Data[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public MList<T> GetRange(int index, int count)
        {
            //if ((index < 0) || (count < 0))
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException((index < 0) ? ExceptionArgument.index : ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            //}
            //if ((this._size - index) < count)
            //{
            //    ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            //}
            MList<T> list = new MList<T>(count);
            Array.Copy(Data, index, list.Data, 0, count);
            list.Cnt = count;
            return list;
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf<T>(this.Data, item, 0, this.Cnt);
        }

        public int IndexOfRef(ref T item)
        {
            return Array.IndexOf<T>(this.Data, item, 0, this.Cnt);
        }

        public int IndexOf(T item, int index)
        {
            //if (index > this.Count)
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
            //}
            return Array.IndexOf<T>(this.Data, item, index, this.Cnt - index);
        }

        public int IndexOf(T item, int index, int count)
        {
            //if (index > this.Count)
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
            //}
            //if ((count < 0) || (index > (this.Count - count)))
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
            //}
            return Array.IndexOf<T>(this.Data, item, index, count);
        }

        /// <summary>
        /// Вставка элемента. Если индекс отрицательный или больше числа элементов, то происходит добавление элемента в конец, аналогично функции Add.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, T item)
        {
            //if (index > this.Count)
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_ListInsert);
            //}
            if (index < 0 || index > this.Cnt) index = this.Cnt;
            if (Cnt == Capacity) EnsureCapacity(Cnt + 1);
            if (index < Cnt)
            {
                Array.Copy(Data, index, Data, index + 1, Cnt - index);
            }
            Data[index] = item;
            Cnt++;
        }

        public void InsertRange(int index, IEnumerable<T> collection, bool cloneItems = false)
        {
            //if (collection == null)
            //{
            //    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);
            //}
            //if (index > this._size)
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
            //}
            ICollection<T> is2 = collection as ICollection<T>;
            if (is2 != null)
            {
                int count = is2.Count;
                if (count > 0)
                {
                    EnsureCapacity(Cnt + count);
                    if (index < Cnt)
                    {
                        Array.Copy(Data, index, Data, index + count, Cnt - index);
                    }
                    if (this == is2)
                    {
                        if (cloneItems)
                        {
                            for (int i = 0, j = index; i < index; i++, j++)
                            {
                                Data[j] = (T)((ICloneable)Data[i]).Clone();
                            }
                            for (int k = 0, i = index + count, j = index * 2, m = Cnt - index; k < m; i++, j++, k++)
                            {
                                Data[j] = (T)((ICloneable)Data[i]).Clone();
                            }
                        }
                        else
                        {
                            Array.Copy(Data, 0, Data, index, index);
                            Array.Copy(Data, index + count, Data, index * 2, Cnt - index);
                        }
                    }
                    else
                    {
                        if (!cloneItems)
                        {
                            is2.CopyTo(Data, index);
                        }
                        else
                        {
                            IList<T> asList = collection as IList<T>;
                            if (asList != null)
                            {
                                for (int i = 0, j = index, m = asList.Count; i < m; i++, j++)
                                {
                                    Data[j] = (T)((ICloneable)asList[i]).Clone();
                                }
                            }
                            else
                            {
                                using (IEnumerator<T> enumerator = collection.GetEnumerator())
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        Insert(index++, (T)((ICloneable)enumerator.Current).Clone());
                                    }
                                }
                            }
                        }
                    }
                    Cnt += count;
                }
            }
            else
            {
                using (IEnumerator<T> enumerator = collection.GetEnumerator())
                {
                    if (cloneItems)
                    {
                        while (enumerator.MoveNext())
                        {
                            Insert(index++, (T)((ICloneable)enumerator.Current).Clone());
                        }
                    }
                    else
                    {
                        while (enumerator.MoveNext())
                        {
                            Insert(index++, enumerator.Current);
                        }
                    }
                }
            }
        }

        public void CopyTo(T[] array, int index)
        {
            Array.Copy(Data, 0, array, index, Cnt);
        }

        public void CopyTo(MList<T> dest)
        {
            dest.EnsureCapacity(Cnt);
            dest.Cnt = Cnt;
            Array.Copy(Data, 0, dest.Data, 0, Cnt);
        }

        public T[] ToArray()
        {
            T[] r = new T[Cnt];
            Array.Copy(Data, 0, r, 0, Cnt);
            return r;
        }

        public int LastIndexOf(T item)
        {
            return this.LastIndexOf(item, this.Cnt - 1, this.Cnt);
        }

        public int LastIndexOf(T item, int index)
        {
            //if (index >= this.Count)
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_Index);
            //}
            return this.LastIndexOf(item, index, index + 1);
        }

        public int LastIndexOf(T item, int index, int count)
        {
            if (this.Cnt == 0)
            {
                return -1;
            }
            //if ((index < 0) || (count < 0))
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException((index < 0) ? ExceptionArgument.index : ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            //}
            //if ((index >= this._size) || (count > (index + 1)))
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException((index >= this._size) ? ExceptionArgument.index : ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_BiggerThanCollection);
            //}
            return Array.LastIndexOf<T>(Data, item, index, count);
        }

        public bool Remove(T item)
        {
            int index = this.IndexOf(item);
            if (index >= 0)
            {
                this.RemoveAt(index);
                return true;
            }
            return false;
        }

        public int RemoveAll(Predicate<T> match)
        {
            //if (match == null)
            //{
            //    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            //}
            int index = 0;
            while ((index < this.Cnt) && !match(this.Data[index]))
            {
                index++;
            }
            if (index >= this.Cnt)
            {
                return 0;
            }
            int num2 = index + 1;
            while (num2 < this.Cnt)
            {
                while ((num2 < this.Cnt) && match(this.Data[num2]))
                {
                    num2++;
                }
                if (num2 < this.Cnt)
                {
                    this.Data[index++] = this.Data[num2++];
                }
            }
            Array.Clear(this.Data, index, this.Cnt - index);
            int num3 = this.Cnt - index;
            this.Cnt = index;
            return num3;
        }

        public void RemoveAt(int index)
        {
            //if (index >= this._size)
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException();
            //}
            this.Cnt--;
            if (index < this.Cnt)
            {
                Array.Copy(this.Data, index + 1, this.Data, index, this.Cnt - index);
            }
            Data[Cnt] = default(T);
        }

        public void RemoveRange(int index, int count, bool clear = true)
        {
            //if ((index < 0) || (count < 0))
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException((index < 0) ? ExceptionArgument.index : ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            //}
            //if ((this._size - index) < count)
            //{
            //    ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            //}
            if (count > 0)
            {
                this.Cnt -= count;
                if (index < this.Cnt)
                {
                    Array.Copy(this.Data, index + count, this.Data, index, this.Cnt - index);
                }
                if (clear) Array.Clear(this.Data, this.Cnt, count);
            }
        }

        public void Reverse()
        {
            this.Reverse(0, this.Cnt);
        }

        public void Reverse(int index, int count)
        {
            //if ((index < 0) || (count < 0))
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException((index < 0) ? ExceptionArgument.index : ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            //}
            //if ((this._size - index) < count)
            //{
            //    ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            //}
            Array.Reverse(this.Data, index, count);
        }

        public void Sort()
        {
            this.Sort(0, this.Cnt, null);
        }

        public void Sort(IComparer<T> comparer)
        {
            this.Sort(0, this.Cnt, comparer);
        }

        //public void Sort(Comparison<T> comparison)
        //{
        //    //if (comparison == null)
        //    //{
        //    //    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
        //    //}
        //    if (this.Count > 0)
        //    {
        //        IComparer<T> comparer = new Array.FunctorComparer<T>(comparison);
        //        Array.Sort<T>(this.Data, 0, this.Data, comparer);
        //    }
        //}

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            //if ((index < 0) || (count < 0))
            //{
            //    ThrowHelper.ThrowArgumentOutOfRangeException((index < 0) ? ExceptionArgument.index : ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
            //}
            //if ((this._size - index) < count)
            //{
            //    ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidOffLen);
            //}
            Array.Sort<T>(this.Data, index, count, comparer);
        }

        public void TrimExcess()
        {
            int num = (int)(Capacity * 0.9);
            if (Cnt < num)
            {
                this.Capacity = this.Cnt;
                T[] destinationArray = new T[Capacity];
                Array.Copy(Data, 0, destinationArray, 0, Cnt);
                Data = destinationArray;
            }
        }

        public void TrimExcessStrictly()
        {
            if (Cnt < Capacity)
            {
                this.Capacity = this.Cnt;
                T[] destinationArray = new T[Capacity];
                Array.Copy(Data, 0, destinationArray, 0, Cnt);
                Data = destinationArray;
            }
        }

        public bool TrueForAll(Predicate<T> match)
        {
            //if (match == null)
            //{
            //    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.match);
            //}
            for (int i = 0; i < this.Cnt; i++)
            {
                if (!match(Data[i]))
                {
                    return false;
                }
            }
            return true;
        }


        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
        {
            private MList<T> list;
            private int index;
            //private int version;
            private T current;
            internal Enumerator(MList<T> list)
            {
                this.list = list;
                this.index = 0;
                //this.version = list._version;
                this.current = default(T);
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                //MList<T> list = this.list;
                if (/*(this.version == list._version) &&*/ (this.index < list.Cnt))
                {
                    this.current = list.Data[this.index];
                    this.index++;
                    return true;
                }
                //return this.MoveNextRare();
                this.index = this.list.Cnt + 1;
                this.current = default(T);
                return false;
            }

            //private bool MoveNextRare()
            //{
            //    //if (this.version != this.list._version)
            //    //{
            //    //    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
            //    //}
            //    this.index = this.list.count + 1;
            //    this.current = default(T);
            //    return false;
            //}

            public T Current
            {
                get
                {
                    return this.current;
                }
            }
            object IEnumerator.Current
            {
                get
                {
                    if ((this.index == 0) || (this.index == (this.list.Cnt + 1)))
                    {
                        throw new Exception("ExceptionResource.InvalidOperation_EnumOpCantHappen");
                        //ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);
                    }
                    return this.Current;
                }
            }
            void IEnumerator.Reset()
            {
                //if (this.version != this.list._version)
                //{
                //    ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumFailedVersion);
                //}
                this.index = 0;
                this.current = default(T);
            }
        }

        int IList.Add(object item)
        {
            MList<T>.VerifyValueType(item);
            this.Add((T)item);
            return (this.Cnt - 1);
        }

        bool IList.Contains(object item)
        {
            return (MList<T>.IsCompatibleObject(item) && this.Contains((T)item));
        }

        int IList.IndexOf(object item)
        {
            if (MList<T>.IsCompatibleObject(item))
            {
                return this.IndexOf((T)item);
            }
            return -1;
        }

        void IList.Insert(int index, object item)
        {
            MList<T>.VerifyValueType(item);
            this.Insert(index, (T)item);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        void IList.Remove(object item)
        {
            if (MList<T>.IsCompatibleObject(item))
            {
                this.Remove((T)item);
            }
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set
            {
                MList<T>.VerifyValueType(value);
                this[index] = (T)value;
            }
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            if ((array != null) && (array.Rank != 1))
            {
                //ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
                throw new Exception();
            }
            try
            {
                Array.Copy(this.Data, 0, array, arrayIndex, this.Cnt);
            }
            catch (ArrayTypeMismatchException)
            {
                //ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                throw new Exception();
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (this.syncRoot == null)
                {
                    Interlocked.CompareExchange(ref this.syncRoot, new object(), null);
                }
                return this.syncRoot;
            }
        }

        private static void VerifyValueType(object value)
        {
            if (!MList<T>.IsCompatibleObject(value))
            {
                //ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
                throw new Exception();
            }
        }

        private static bool IsCompatibleObject(object value)
        {
            if (!(value is T) && ((value != null) || typeof(T).IsValueType))
            {
                return false;
            }
            return true;
        }
    }
}
