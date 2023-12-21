using Newtonsoft.Json.Linq;

namespace OrderHelperLib
{
    /// <summary>
    ///  数据袋，用于序列化多个数据<br/>
    ///  1. 主要目的是省流结构。<br/>
    ///  2. 可向上版本支持，所以一旦定义了数据索引就不可更改(只能增加)，未来只能透过换API(+新结构)来弃用冗余的数据<br/>
    ///  3. <see cref="Data"/>必须是<see cref="string"/>或值类型。<br/>
    ///  4. <see cref="bool"/>以<see cref="int"/>方式记录。<br/>
    /// </summary>
    public sealed class DataBag 
    {
        /// <summary>
        /// 序列化数据袋
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string SerializeWithName(string name, params object[] data)
        {
            var bag = new DataBag();
            bag.Data = data;
            bag.Size = data.Length;
            bag.DataName = name;
            return Json.Serialize(bag);
        }
        /// <summary>
        /// 序列化数据袋
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Serialize(params object[] data)
        {
            var bag = new DataBag();
            bag.Data = data;
            bag.Size = data.Length;
            bag.DataName = $"Data[{data.Length}]";
            return Json.Serialize(bag);
        }

        /// <summary>
        /// 数据袋反序列
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static DataBag? Deserialize(object? obj)
        {
            if (obj == null) return null;
            var dto = Json.Deserialize<DataBag>(obj.ToString());
            return dto;
        }

        public string DataName { get; set; }
        public object[] Data { get; set; }
        public int Size { get; set; }

        public T Get<T>(int index)
        {
            if (Data.Length <= index)
                throw new IndexOutOfRangeException($"The requested index {index} is out of range. The length of the Data array is {Data.Length}.");
            var value = Data[index];
            return Parse<T>(value);
        }

        public static T Parse<T>(object value)
        {
            var t = typeof(T);
            if (value == default) return default;

            if (value is long intValue) return (T)Convert.ChangeType(intValue, t);

            if (value is JToken token) return token.ToObject<T>();
            return (T)value;
        }
    }
}
