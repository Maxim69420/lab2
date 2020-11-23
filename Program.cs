using System;
using System.Numerics;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;

namespace lab1v2
{
    struct DataItem
    {
        public Vector2 cord { get; set; }
        public Complex complex { get; set; }
        public DataItem(Vector2 x, Complex y)
        {
            cord = new Vector2(x.X, x.Y);
            complex = new Complex(0, 0);
            complex = y;
        }
        public override string ToString() // virtual
        {
            return cord.ToString() + complex.ToString();
        }
        public string ToString(string format) // virtual
        {
            return cord.ToString(format) + complex.ToString(format);
        }
    }
    struct Grid1D
    {
        public float step { get; set; }
        public int count_node { get; set; }
        public Grid1D(float x, int y)
        {
            step = x;
            count_node = y;
        }
        public Grid1D(Grid1D obj)
        {
            step = obj.step;
            count_node = obj.count_node;
        }
        public override string ToString()
        {
            return step.ToString() + " " + count_node.ToString() + "\n";
        }
        public string ToString(string format)
        {
            return step.ToString(format) + " " + count_node.ToString() + "\n";
        }
    }
    abstract class V2Data
    {
        public string data { get; set; }
        public double period { get; set; }
        public V2Data(string data = "Data", double period = 2)
        {
            this.data = data;
            this.period = period;
        }
        public abstract IEnumerable<Vector2> GetCoords();
        public abstract IEnumerable<DataItem> GetDataItem();
        public abstract Complex[] NearAverage(float eps);
        public abstract string ToLongString();
        public abstract string ToLongString(string format);
        public virtual string ToString(string format)
        {
            return "(Data: " + data + "; frequency: " + period.ToString(format) + ")\n";
        }
        public override string ToString()
        {
            return "(Data: " + data + "; frequency: " + period.ToString() + ")\n";
        }
    }
    class V2DataOnGrid : V2Data, IEnumerable<DataItem>
    {
        public Grid1D[] OXY { get; set; }
        public Complex[,] complices { get; set; }
        public IEnumerator GetEnumerator()
        {
            Vector2 coord = new Vector2(0.0f, 0.0f);
            float x_step = OXY[0].step, y_step = OXY[1].step;
            DataItem data_item;
            for (int j = 0; j < OXY[1].count_node; j++)
            {
                for (int i = 0; i < OXY[0].count_node; i++)
                {
                    data_item = new DataItem(coord, complices[i, j]);
                    yield return data_item;
                    coord.X += x_step;
                }
                coord.Y += y_step;
                coord.X = 0;
            }
        }

        IEnumerator<DataItem> IEnumerable<DataItem>.GetEnumerator()
        {
            Vector2 coord = new Vector2(0.0f, 0.0f);
            float x_step = OXY[0].step, y_step = OXY[1].step;
            DataItem data_item;
            for (int j = 0; j < OXY[1].count_node; j++)
            {
                for (int i = 0; i < OXY[0].count_node; i++)
                {
                    data_item = new DataItem(coord, complices[i, j]);
                    yield return data_item;
                    coord.X += x_step;
                }
                coord.Y += y_step;
                coord.X = 0;
            }
        }
        public V2DataOnGrid(string filename)
        {
            StreamReader get = new StreamReader(filename);
            try
            {
                data = get.ReadLine();
                period = double.Parse(get.ReadLine());
                //string str1 = get.ReadLine();
                //base.data = str1;
                //int per1 = int.Parse(get.ReadLine());
                //base.period = per1;
                Grid1D[] OXY1 = new Grid1D[2];
                int x1 = int.Parse(get.ReadLine());
                float x2 = float.Parse(get.ReadLine());
                OXY1[0] = new Grid1D(x2, x1);
                int y1 = int.Parse(get.ReadLine());
                float y2 = float.Parse(get.ReadLine());
                OXY1[1] = new Grid1D(y2, y1);
                complices = new Complex[x1, y1];
                OXY = OXY1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
            finally
            {
                if (get != null)
                    get.Dispose();
            }
        }
        public override IEnumerable<Vector2> GetCoords()
        {
            yield return new Vector2(0f, 0f);
        }
        public override IEnumerable<DataItem> GetDataItem()
        {
            V2DataCollection temp = (V2DataCollection)this;
            return temp.GetDataItem();
        }
        public V2DataOnGrid(Grid1D x, Grid1D y, string z, double w) : base(z, w)
        {
            OXY = new Grid1D[2];
            complices = new Complex[x.count_node, y.count_node];
            OXY[0] = x;
            OXY[1] = y;
        }
        public void InitRandom(double minValue, double maxValue)
        {
            Random rand = new Random();
            for (int i = 0; i < OXY[0].count_node; i++)
            {
                for (int j = 0; j < OXY[1].count_node; j++)
                {
                    double real = rand.NextDouble() * (maxValue - minValue) + minValue;
                    double imaginary = rand.NextDouble() * (maxValue - minValue) + minValue;
                    Complex data = new Complex(real, imaginary);
                    complices[i, j] = data;
                }
            }
        }
        public static explicit operator V2DataCollection(V2DataOnGrid x)
        {
            V2DataCollection ans = new V2DataCollection(x.data, x.period);
            ans.dataItems = new List<DataItem>();
            float X = 0;
            float Y = 0;
            DataItem I;
            Vector2 coords;
            for (int i = 0; i < x.OXY[0].count_node; i++)
            {
                for (int j = 0; j < x.OXY[1].count_node; j++)
                {
                    coords = new Vector2(X, Y);
                    I = new DataItem(coords, x.complices[i, j]);
                    ans.dataItems.Add(I);
                    X += x.OXY[0].step;
                }
                Y += x.OXY[1].step;
                X = 0;
            }
            return ans;
        }
        public override Complex[] NearAverage(float eps)
        {
            float mean = 0;
            for (int i = 0; i < OXY[0].count_node; i++)
                for (int j = 0; j < OXY[1].count_node; j++)
                    mean += (float)complices[i, j].Real;

            mean = mean / ((float)OXY[0].count_node * (float)OXY[1].count_node);
            List<Complex> arr = new List<Complex>();
            for (int i = 0; i < OXY[0].count_node; i++)
                for (int j = 0; j < OXY[1].count_node; j++)
                    if (((float)complices[i, j].Real < mean + eps) && ((float)complices[i, j].Real > mean - eps))
                        arr.Add(complices[i, j]);
            return arr.ToArray();
        }
        public override string ToString()
        {
            return "DataOnGrid" + " " + base.ToString()
                + OXY[0].count_node.ToString() + " " + OXY[0].step.ToString() + "\n"
                + OXY[1].count_node.ToString() + " " + OXY[1].step.ToString() + "\n";
        }
        public override string ToString(string format)
        {
            return "DataOnGrid" + " " + base.ToString()
                + OXY[0].count_node.ToString() + " " + OXY[0].step.ToString(format) + "\n"
                + OXY[1].count_node.ToString() + " " + OXY[1].step.ToString(format) + "\n";
        }
        public override string ToLongString()
        {
            string S = ToString();
            float X = 0;
            float Y = 0;
            for (int i = 0; i < OXY[0].count_node; i++)
            {
                for (int j = 0; j < OXY[1].count_node; j++)
                {
                    S = S + "(" + X.ToString() + "; " + Y.ToString() + ") "
                        + complices[i, j].ToString() + "\n";
                    X += OXY[0].step;
                }
                Y += OXY[1].step;
                X = 0;
            }
            return S;
        }
        public override string ToLongString(string format)
        {
            string S = ToString(format);
            float X = 0;
            float Y = 0;
            for (int i = 0; i < OXY[0].count_node; i++)
            {
                for (int j = 0; j < OXY[1].count_node; j++)
                {
                    S = S + "(" + X.ToString(format) + "; " + Y.ToString(format) + ") "
                        + complices[i, j].ToString(format) + "\n";
                    X += OXY[0].step;
                }
                Y += OXY[1].step;
                X = 0;
            }
            return S;
        }
    }
    class V2DataCollection : V2Data, IEnumerable<DataItem>
    {
        public List<DataItem> dataItems { get; set; }
        public V2DataCollection(string x, double y) : base(x, y)
        {
            dataItems = new List<DataItem>();
        }
        public override IEnumerable<Vector2> GetCoords()
        {
            foreach (DataItem item in dataItems)
                yield return item.cord;
        }
        public override IEnumerable<DataItem> GetDataItem()
        {
            foreach (DataItem item in dataItems)
                yield return item;
        }
        public IEnumerator GetEnumerator()
        {
            return dataItems.GetEnumerator();
        }
        IEnumerator<DataItem> IEnumerable<DataItem>.GetEnumerator()
        {
            return dataItems.GetEnumerator();
        }
        public void InitRandom(int nItems, float xmax, float ymax, double minValue, double maxValue)
        {
            Random rand = new Random();
            for (int i = 0; i < nItems; i++)
            {
                double real = rand.NextDouble() * (maxValue - minValue) + minValue;
                double imaginary = rand.NextDouble() * (maxValue - minValue) + minValue;
                Complex data = new Complex(real, imaginary);
                float x = (float)rand.NextDouble() * xmax;
                float y = (float)rand.NextDouble() * ymax;
                Vector2 coords = new Vector2(x, y);
                DataItem item = new DataItem(coords, data);
                dataItems.Add(item);
            }
        }
        public override Complex[] NearAverage(float eps)
        {
            float mean = 0;
            for (int i = 0; i < dataItems.Count; i++)
                mean += (float)dataItems[i].complex.Real;
            mean = mean / dataItems.Count;
            List<Complex> ans = new List<Complex>();
            for (int i = 0; i < dataItems.Count; i++)
            {
                if (((float)dataItems[i].complex.Real < (mean + eps)) &&
                    ((float)dataItems[i].complex.Real > (mean - eps)))
                {
                    ans.Add(dataItems[i].complex);
                }
            }
            return ans.ToArray();
        }
        public override string ToString()
        {
            return "DataCollection " + base.ToString();
        }
        public override string ToString(string format)
        {
            return "DataCollection " + base.ToString(format);
        }
        public override string ToLongString(string format)
        {
            string str = ToString(format);
            for (int i = 0; i < dataItems.Count; i++)
            {
                str = str + "(" + dataItems[i].cord.X.ToString(format) + "; "
                    + dataItems[i].cord.Y.ToString(format) + ") ("
                    + dataItems[i].complex.Real.ToString(format) + ", "
                    + dataItems[i].complex.Imaginary.ToString(format) + ")\n";
            }
            return str;
        }
        public override string ToLongString()
        {
            string str = ToString();
            for (int i = 0; i < dataItems.Count; i++)
            {
                str = str + "(" + dataItems[i].cord.X.ToString() + "; "
                    + dataItems[i].cord.Y.ToString() + ") ("
                    + dataItems[i].complex.Real.ToString() + ", "
                    + dataItems[i].complex.Imaginary.ToString() + ")\n";
            }
            return str;
        }
    }
    class V2MainCollection : IEnumerable<V2Data>
    {
        private List<V2Data> V2Datas;
        public int Count
        {
            get { return V2Datas.Count; }
        }
        public double GetAverage
        {
            get { return V2Datas.Average<V2Data>(x => x.GetAverage()); }
        }
        public IEnumerator<V2Data> GetEnumerator()
        {
            return V2Datas.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return V2Datas.GetEnumerator();
        }
        public DataItem GetNearAverage
        {
            get
            {
                Console.WriteLine("GetNearAverage = {GetAverage}");

                IEnumerable<DataItem> query =
                     from data in V2Datas
                     from item in data.GetDataItem()
                     orderby (Math.Abs(item.complex.Magnitude - GetAverage)) ascending
                     select item;

                return query.First();
            }
        }
        public IEnumerable<Vector2> GetValue
        {
            get
            {
                Console.WriteLine("V2DataCollection coordinations: ");

                IEnumerable<IEnumerable<Vector2>> query =
                    from data in V2Datas
                    where data.GetType().Equals(typeof(V2DataCollection))
                    select data.GetCoords();

                return query.SelectMany(x => x);
            }
        }

        public void Add(V2Data item)
        {
            V2Datas.Add(item);
        }
        public bool Remove(string id, double w)
        {
            int baseCount = V2Datas.Count;

            V2Datas.RemoveAll(x => (x.data == id) && (x.period == w));

            return baseCount != V2Datas.Count;
        }
        public void AddDefaults()
        {
            V2Datas = new List<V2Data>();
            Grid1D x1 = new Grid1D(1, 3);

            V2DataOnGrid d1 = new V2DataOnGrid(x1, x1, "Grid", 2);
            d1.InitRandom(1, 5);
            V2Datas.Add(d1);

            V2DataCollection d2 = new V2DataCollection("Collection", 1.5);
            d2.InitRandom(3, 5, 5, 4.5, 6.9);
            V2Datas.Add(d2);

            Grid1D x2 = new Grid1D(1, 3);
            V2DataOnGrid d3 = new V2DataOnGrid(x2, x2, "Grid 2", 0.5);
            d3.InitRandom(1, 4);
            V2Datas.Add(d3);
        }
        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < V2Datas.Count; i++)
                str += " " + V2Datas[i].ToString();
            return str;
        }
        public string ToString(string format)
        {
            string str = "";
            for (int i = 0; i < V2Datas.Count; i++)
                str += " " + V2Datas[i].ToString(format);
            return str;
        }
        public string ToLongString(string format = "V2")
        {
            string str = " ";
            for (int i = 0; i < V2Datas.Count; i++)
                str += V2Datas[i].ToLongString(format) + "\n";
            return str;
        }
    }
    class MainClass
    {
        public static void Main(string[] args)
        {
            /*Console.WriteLine("Task 1\n");
            Grid1D x1 = new Grid1D((float)1.5, 3);
            V2DataOnGrid d1 = new V2DataOnGrid(x1, x1, "6", 2);
            d1.InitRandom(1, 5);
            Console.WriteLine(d1.ToLongString());
            V2DataCollection d2 = (V2DataCollection)d1;
            Console.WriteLine(d2.ToLongString());

            Console.WriteLine("Task 2\n");
            V2MainCollection coll = new V2MainCollection();

            coll.AddDefaults();
            foreach (V2Data item in coll)
            {
                Console.WriteLine(item.ToLongString() + "\n");
            }

            Console.WriteLine("Task 3\n");
            Complex[] ans;
            foreach (V2Data item in coll)
            {
                ans = item.NearAverage((float)0.5);
                for (int i = 0; i < ans.Length; i++)
                {
                    Console.WriteLine(ans[i].ToString());
                }
                Console.WriteLine("\n");
            }*/
            try
            {
                Console.WriteLine("--------initializing V2DataOnGrid from text file--------\n");
                V2DataOnGrid DOG = new V2DataOnGrid("filename.txt");
                Console.WriteLine(DOG.ToLongString("N1"));
                Console.WriteLine("--------------------------------------------------------\n");

                V2MainCollection MC = new V2MainCollection();
                MC.AddDefaults();
                Console.WriteLine(MC.ToLongString());

                foreach (Vector2 coord in MC.GetValue)
                {
                    Console.WriteLine(coord);
                }
                Console.WriteLine("\n");

                Console.WriteLine(MC.GetNearAverage.ToString("N2"));
            }
            catch (Exception e)
            {
                Console.WriteLine("--------<ERROR: " + e.Message + ">--------");
                return;
            }
        }
    }
}
  
