using System.Collections.Generic;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Linq;

namespace SUBDLab
{
    public class DBMenu
    {
        public Base CurrentBase;
        public bool IsChanged;
        public Table CurrentTable;
        public DBMenu()
        {
            IsChanged = false;
        }
        public void OpenNew()
        {
            CurrentBase = new Base();
            CurrentTable = null;
            IsChanged = true;

        }
        public int OpenBase(string Path)
        {
            try
            {
                using (StreamReader sr = new StreamReader(Path))
                {
                    Base NewBase = new Base();
                    NewBase.Path= Path;
                    string line;
                    line = sr.ReadLine();
                    int n;
                    if (!int.TryParse(line, out n)) { return 2; }
                    for (int i = 0; i < n; i++)
                    {
                        string TableName = sr.ReadLine();
                        List<Tuple<string, string>> args = new List<Tuple<string, string>>();
                        line = sr.ReadLine();
                        int m;
                        if (!int.TryParse(line, out m)) { return 2; }
                        for (int j = 0; j < m; j++)
                        {
                            line = sr.ReadLine();
                            string Type = sr.ReadLine();
                            args.Add(new Tuple<string, string>(line, Type));
                            StringField f;
                            switch (Type)
                            {
                                case "Int":
                                    f = new IntField(line);
                                    break;
                                case "Real":
                                    f = new RealField(line);
                                    break;
                                case "Char":
                                    f = new CharField(line);
                                    break;
                                case "String":
                                    f = new StringField(line);
                                    break;
                                case "Time":
                                    f = new TimeField(line);
                                    break;
                                case "TimeInterval":
                                    f = new TimeInvlField(line);
                                    break;
                                default:
                                    return 2;
                            }
                        }
                        Table t = new Table(TableName, args);
                        int l;
                        line = sr.ReadLine();
                        if (!int.TryParse(line, out l)) { return 2; }
                        for (int j = 0; j < m; j++)
                        {
                            for (int k = 0; k < l; k++)
                            {
                                line = sr.ReadLine();
                                t.Fields[j].addRow();
                                if (t.Fields[j].ChangeValue(k, line) != 0) { return 2; }
                            }
                        }
                        NewBase.Tables.Add(t);
                    }
                    CurrentBase = NewBase;
                    CurrentTable = null;
                    IsChanged = false;
                }
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }
        public int Save()
        {
            if (CurrentBase == null)
            {
                return 1;
            }
            SaveAs(CurrentBase.Path);
            IsChanged = false;
            return 0;
        }
        public int SaveAs(string Path)
        {
            try
            {
                using(FileStream fop = File.Create(Path)) { }
                CurrentBase.Path = Path;
                FileStream fs = new FileStream(Path, FileMode.Open);
                StreamWriter stream = new StreamWriter(fs);
                stream.WriteLine((CurrentBase.Tables.Count).ToString());
                foreach (Table t in CurrentBase.Tables)
                {
                    stream.WriteLine(t.Name);
                    stream.WriteLine(t.Fields.Count.ToString());
                    foreach (StringField f in t.Fields)
                    {
                        stream.WriteLine(f.Name);
                        stream.WriteLine(f.Type.ToString());
                    }
                    stream.WriteLine(t.Fields[0].Values.Count);
                    foreach (StringField f in t.Fields)
                    {
                        foreach (string s in f.Values)
                        {
                            stream.WriteLine(s);
                        }
                    }
                }
                stream.Close();
                fs.Close();
                IsChanged = false;
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("!"+ex.Message);
                return 1;
            }
        }
        public int CreateTable(string Name, List<Tuple<string, string>> NamesTypes)
        {
            foreach (Table t in CurrentBase.Tables)
            {
                if (t.Name.Equals(Name)) return 1;
            }
            CurrentBase.Tables.Add(new Table(Name, NamesTypes));
            IsChanged = true;
            return 0;
        }
        public int OpenTable(string Name)
        {
            foreach(Table t in CurrentBase.Tables)
            {
                if (t.Name.Equals(Name))
                {
                    CurrentTable = t;
                    return 0;
                }
            }
            return 1;
        }
        public int DeleteTable(string Name)
        {
            foreach(Table t in CurrentBase.Tables) if (t.Name.Equals(Name)) { CurrentBase.Tables.Remove(t); if (CurrentTable == t) CurrentTable = null; IsChanged = true; return 0; }
            return 1;
        }
        public void AddRow()
        {
            CurrentTable.AddRow();
            IsChanged = true;
        }
        public int DeleteRows(int PosRow,int num)
        {
            if (CurrentTable.Fields[0].Values.Count < PosRow + num) return 1;
            foreach (StringField f in CurrentTable.Fields)
            {
                f.Values.RemoveRange(PosRow, num);
            }
            IsChanged = true;
            return 0;
        }
        public int ChangeRowValue(int RowPos,int ColumnPos,string value)
        {
            IsChanged = true;
            return CurrentTable.Fields[ColumnPos].ChangeValue(RowPos, value);
        }
        public int Intersection(string name,List<string> Inputargs)
        {
            if (CurrentBase == null) return 1;
            if (name.Length == 0) return 2;
            foreach(Table t in CurrentBase.Tables)if(t.Name.Equals(name)) { return 3; }
            if (Inputargs.Count == 0) return 4;
            List<Table> Tablesargs = new List<Table>();

            foreach(string s in Inputargs)
            {
                bool exists = false;
                foreach(Table t in CurrentBase.Tables) if (t.Name.Equals(s)) { exists = true; Tablesargs.Add(t); break; }
                if (!exists) return 5;
            }
            for (int i = 0; i < Tablesargs.Count - 1; i++)
            {
                HashSet<Tuple<string, string>> h1 = Tablesargs[i].FieldsSet();
                bool ex = true;
                while (ex)
                {
                    ex = false;
                    foreach (Tuple<string, string> t1 in h1) foreach (Tuple<string, string> t2 in h1) if (t1 != t2 && t1.Equals(t2)) { ex = true; h1.Remove(t2); }
                }
                HashSet<Tuple<string, string>> h2 = Tablesargs[i+1].FieldsSet();
                ex = true;
                while (ex)
                {
                    ex = false;
                    foreach (Tuple<string, string> t1 in h2) foreach (Tuple<string, string> t2 in h2) if (t1 != t2 && t1.Equals(t2)) { ex = true; h2.Remove(t2); }
                }
                if (h1.Count != h2.Count) return 6;
                foreach (Tuple<string, string> t1 in h1)
                {
                    bool exists = false;
                    foreach (Tuple<string, string> t2 in h2) { if (t1.Equals(t2)) { exists = true; break; } }
                    if (!exists) return 6;
                }
            }
            List<Tuple<string, string>> NamesTypes = Tablesargs[0].FieldsSet().ToList();
            Dictionary<string, int> pos = new Dictionary<string, int>();
            for (int i = 0; i < NamesTypes.Count; i++) pos.Add(NamesTypes[i].Item1, i);
            Table newTable = new Table(name, NamesTypes);
            List<List<string>> Rows = new List<List<string>>();

            for (int i = 0; i < Tablesargs[0].Fields[0].Values.Count; i++)
            {
                List<string> Row = new List<string>(new string[NamesTypes.Count]);
                foreach (StringField f in Tablesargs[0].Fields)
                {
                    Row[pos[f.Name]] = (f.Values[i]);
                }
                Rows.Add(Row);
            }

            foreach (Table t in Tablesargs)
            {
                List<List<string>> newRows = new List<List<string>>();
                for (int i = 0; i < t.Fields[0].Values.Count;i++)
                {
                    List<string> Row=new List<string>(new string[NamesTypes.Count]);
                    foreach (StringField f in t.Fields)
                    {
                        Row[pos[f.Name]]=(f.Values[i]);
                    }
                    bool exists = false;
                    foreach (List<string> r in Rows) if (r.SequenceEqual(Row)) { exists = true; break; }
                    if (!exists) continue;
                    exists = false;
                    foreach (List<string> r in newRows) if (r.SequenceEqual(Row)) { exists = true; break; }
                    if (exists) continue;
                    newRows.Add(Row);
                }
                Rows = newRows;
            }
            foreach (List<string> r in Rows)
            {
                for (int j = 0; j < Tablesargs[0].Fields.Count; j++)
                {
                    newTable.Fields[j].Values.Add(r[j]);
                }
            }
            CurrentBase.Tables.Add(newTable);
            IsChanged = true;
            return 0;
        }
        public void Close()
        {
            if (CurrentTable == null)
            {
                CurrentBase = null;
                IsChanged = false;
            }
            else
                CurrentTable = null;

        }
    }
    public class Base
    {
        public List<Table> Tables;
        public string Path;
        public Base()
        {
            Tables = new List<Table>();
        }
    }
    public class Table
    {
        public List<StringField> Fields;
        public string Name;
        public Table(string NameVal, List<Tuple<string, string>> NamesTypes)
        {
            Name = NameVal;
            Fields = new List<StringField>();
            foreach (Tuple<string, string> nametype in NamesTypes)
            {
                switch (nametype.Item2)
                {
                    case "Int":
                        Fields.Add(new IntField(nametype.Item1));
                        break;
                    case "Real":
                        Fields.Add(new RealField(nametype.Item1));
                        break;
                    case "Char":
                        Fields.Add(new CharField(nametype.Item1));
                        break;
                    case "String":
                        Fields.Add(new StringField(nametype.Item1));
                        break;
                    case "Time":
                        Fields.Add(new TimeField(nametype.Item1));
                        break;
                    case "TimeInterval":
                        Fields.Add(new TimeInvlField(nametype.Item1));
                        break;
                }
            }
        
        }
        public void AddRow()
        {
            foreach (StringField f in Fields) f.addRow();
        }
        public HashSet<Tuple<string,string>> FieldsSet()
        {
            HashSet<Tuple<string,string>> ans= new HashSet<Tuple<string,string>>();
            foreach (StringField f in Fields) ans.Add(new Tuple<string, string>(f.Name, f.Type.ToString()));
            return ans;
        }
    }
    public enum FieldTypes
    {
        Int,
        Real,
        Char,
        String,
        Time,
        TimeInterval
    }
    public class StringField
    {
        public string Name;
        public FieldTypes Type;
        public List<string> Values=new List<string>();
        public StringField(string NameVal)
        {
            Name = NameVal;
            Type = FieldTypes.String;
        }
        public void addRow()
        {
            Values.Add("");
        }
        public virtual int ChangeValue(int Pos, string Value)
        {
            if (Pos >= Values.Count || Pos < 0)
            {
                return 2;
            }
            Values[Pos] = Value;
            return 0;
        }
    }
    public class IntField : StringField
    {
        public IntField(string NameVal) : base(NameVal)
        {
            Type = FieldTypes.Int;
        }
        public override int ChangeValue(int Pos, string Value)
        {
            if (Value.Length == 0) return 0;
            if (Pos >= Values.Count || Pos < 0)
            {
                return 2;
            }
            int n;
            if (int.TryParse(Value, out n))
            {
                Values[Pos] = Value;
                return 0;
            }
            return 1;
        }
    }
    public class RealField : StringField
    {
        public RealField(string NameVal) : base(NameVal)
        {
            Type = FieldTypes.Real;
        }
        public override int ChangeValue(int Pos, string Value)
        {
            if (Value.Length == 0) return 0;
            if (Pos >= Values.Count || Pos < 0)
            {
                return 2;
            }
            double n;
            if (double.TryParse(Value, out n))
            {
                Values[Pos] = Value;
                return 0;
            }
            return 1;
        }
    }
    public class CharField : StringField
    {
        public CharField(string NameVal) : base(NameVal)
        {
            Type = FieldTypes.Char;
        }
        public override int ChangeValue(int Pos, string Value)
        {
            if (Value.Length == 0) return 0;
            if (Pos >= Values.Count || Pos < 0)
            {
                return 2;
            }
            char n;
            if (char.TryParse(Value, out n))
            {
                Values[Pos] = Value;
                return 0;
            }
            return 1;
        }
    }
    public class TimeField : StringField
    {
        public TimeField(string NameVal) : base(NameVal)
        {
            Type = FieldTypes.Time;
        }
        public override int ChangeValue(int Pos, string Value)
        {
            if (Value.Length == 0) return 0;
            if (Pos >= Values.Count || Pos < 0)
            {
                return 2;
            }
            if (Value.Length != 5) return 1;
            string pattern = "(([0-1][0-9])|(2[0-3])):[0-5][0-9]";
            Match m = Regex.Match(Value, pattern);
            if (m.Success)
            {
                Values[Pos] = Value;
                return 0;
            }
            return 1;
        }
    }
    public class TimeInvlField : StringField
    {
        public TimeInvlField(string NameVal) : base(NameVal)
        {
            Type = FieldTypes.TimeInterval;
        }
        public override int ChangeValue(int Pos, string Value)
        {
            if (Value.Length == 0) return 0;
            if (Pos >= Values.Count || Pos < 0)
            {
                return 2;
            }
            if (Value.Length != 11) return 1;
            string pattern = "(([0-1][0-9])|(2[0-3])):[0-5][0-9]-(([0-1][0-9])|(2[0-3])):[0-5][0-9]";
            Match m = Regex.Match(Value, pattern);
            if (m.Success)
            {
                string s1 = Value.Substring(0, 5).ToUpper();
                string s2 = Value.Substring(6, 5).ToUpper();
                for(int i = 0; i < 5; i++)
                {
                    if (i == 2) continue;
                    if (s1[i] < s2[i]) break;
                    if (s1[i] > s2[i]) return 2;
                }
                Values[Pos] = Value;
                return 0;
            }
            return 1;
        }
    }
}