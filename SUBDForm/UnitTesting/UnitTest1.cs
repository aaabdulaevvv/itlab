using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SUBDLab;
using System.Collections.Generic;

namespace UnitTesting
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            IntField TestField = new IntField("Test");
            TestField.addRow();
            Assert.AreEqual(TestField.ChangeValue(0, "-100"), 0);
        }
        [TestMethod]
        public void TestMethod2()
        {
            RealField TestField=new RealField("Test");
            TestField.addRow();
            Assert.AreEqual(TestField.ChangeValue(1, "5,5"), 2);
        }
        [TestMethod]
        public void TestMethod3()
        {
            TimeField TestField=new TimeField("Test");
            TestField.addRow();
            Assert.AreEqual(TestField.ChangeValue(0, "25:40"), 1);
        }
        [TestMethod]
        public void TestMethod4()
        {
            TimeInvlField TestField=new TimeInvlField("Test");
            TestField.addRow();
            Assert.AreEqual(TestField.ChangeValue(0, "19:00-22:45"), 0);
        }
        [TestMethod]
        public void TestMethod5()
        {
            DBMenu menu = new DBMenu();
            menu.CurrentBase = new Base();
            List<Tuple<string,string>> NamesTypes=new List<Tuple<string,string>>();
            NamesTypes.Add(new Tuple<string, string>("1", "Int"));
            menu.CreateTable("Table1", NamesTypes);
            Assert.AreEqual(menu.CreateTable("Table1", NamesTypes), 1);
        }
        [TestMethod]
        public void TestMethod6()
        {
            DBMenu menu = new DBMenu();
            menu.CurrentBase = new Base();
            List<Tuple<string, string>> NamesTypes = new List<Tuple<string, string>>();
            NamesTypes.Add(new Tuple<string, string>("1", "String"));
            NamesTypes.Add(new Tuple<string, string>("2", "Time"));
            menu.CreateTable("Table1", NamesTypes);
            menu.CreateTable("Table2", NamesTypes);
            menu.OpenTable("Table1");
            menu.AddRow(); menu.AddRow();
            menu.ChangeRowValue(0, 0, "Alice"); menu.ChangeRowValue(0, 1, "19:00");
            menu.ChangeRowValue(1, 0, "Bob"); menu.ChangeRowValue(1, 1, "20:00");
            menu.OpenTable("Table2");
            menu.AddRow(); menu.AddRow(); menu.AddRow();
            menu.ChangeRowValue(0, 0, "Alice"); menu.ChangeRowValue(0, 1, "19:00");
            menu.ChangeRowValue(1, 0, "Bob"); menu.ChangeRowValue(1, 1, "08:00");
            menu.ChangeRowValue(2, 0, "Charles"); menu.ChangeRowValue(2, 1, "20:00");
            List<string> s = new List<string>(); s.Add("Table1"); s.Add("Table2");
            menu.Intersection("Table3",s);
            menu.OpenTable("Table3");
            Assert.AreEqual(menu.CurrentTable.Fields[0].Values.Count, 1);
        }
    }
}
