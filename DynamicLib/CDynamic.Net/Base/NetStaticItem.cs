using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net
{
    public class NetStatisticItem
    {
        public string Name { get; set; }

        public double Value { get; set; }

        public double Value2 { get; set; }

        public double Value3 { get; set; }

        public double Value4 { get; set; }

        public double Value5 { get; set; }

        public double Value6 { get; set; }

        public double Value7 { get; set; }

        public double Value8 { get; set; }
    }

    public class NetStatisticValueColumnItem
    {
        public string Unit { get; set; }

        public string Desc { get; set; }

        public string ColumnName { get; set; }
    }

    public class NetStatisticGroup
    {
        public NetStatisticGroup()
        {
            Items = new List<NetStatisticItem>();
            Columns = new List<NetStatisticValueColumnItem>();
        }

        public List<NetStatisticValueColumnItem> Columns { get; set; }

        public NetStatisticValueColumnItem AddColumn(string name, string unit)
        {
             NetStatisticValueColumnItem c = new  NetStatisticValueColumnItem() { ColumnName = name, Unit = unit };
             Columns.Add(c);

             return c;
        }

        public string GroupName { get; set; }

        public List<NetStatisticItem> Items { get; set; }

        public NetStatisticItem AddItem(string name, double value)
        {
            NetStatisticItem item = new NetStatisticItem() { Name = name, Value = value };
            Items.Add(item);
            return item;
        }
    }


    public class NetStatistic : List<NetStatisticGroup>
    {
        public NetStatistic()
        {
        }


        public NetStatisticGroup AddGroup(string name, bool isCommon)
        {
            NetStatisticGroup g = new NetStatisticGroup() { GroupName = name };
            this.Add(g);
            if (isCommon)
            {
                g.AddColumn("值", "");
            }

            return g;
        }

    }

    
}
