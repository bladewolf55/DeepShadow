using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using DeepShadow;

namespace DeepShadow.UnitTests
{
    public class EntityGeneratorTests
    {
        public class GenerateEntities_Should
        {
            [Fact]
            public void GenerateMatchingEntitiesFromList()
            {
                var list = MockData.Orders();
                var actual = list.GenerateEntitiesFromList();
                Assert.Equal(MockData.OrdersExpected, actual);

            [Fact]
            public void GenerateMatchingEntitiesFromEntity()
            {
                var entity = MockData.Customer();
                var actual = entity.GenerateEntitiesFromObject();
                Assert.Equal(MockData.CustomerExpected, actual);
            }

            [Fact]
            public void GenerateEmptyListProperty()
            {
                var entity = MockData.CustomerWithoutOrders();
                var actual = entity.GenerateEntitiesFromObject();
                Assert.Contains("a1.Orders = new List<DeepShadow.UnitTests.Order>();", actual);
            }

            [Fact]
            public void GenerateMatchingStringList()
            {
                var entity = MockData.StringList();
                var actual = entity.GenerateEntitiesFromList();
                Assert.Equal(MockData.StringListExpected, actual);
            }

            [Fact]
            public void IgnoreSuppliedProperties()
            {
                var entity = MockData.ItemWithPropertiesToIgnore();
                string[] ignoreProps = { "SetKeys", "LowerCaseName" };
                var actual = entity.GenerateEntitiesFromObject(ignoreProps);
                Assert.Equal(MockData.ItemWithPropertiesToIgnoreExpected, actual);
            }
        }
    }


    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public List<string> Codes {get;set;}
        //Nav
        public List<Order> Orders { get; set; } = new List<Order>();
    }

    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderedOn { get; set; }
        public decimal? Discount { get; set; }
        //Nav
        public Customer Customer { get; set; }
    }

    public class ItemWithPropertiesToIgnore
    {
        public int ItemId { get; set; }
        public string Name { get; set; } = "Smith";
        public Action SetKeys
        {
            get { return () => { }; }
        }
        public string LowerCaseName { get { return Name.ToLower(); } set { Name = value; } }
    }


    public static class MockData
    {
        public static List<Order> Orders()
        {
            List<Order> list = new List<Order>();
            Order a1 = new Order();
            a1.OrderId = 1;
            a1.CustomerId = 101;
            a1.OrderedOn = DateTime.Parse( "1/1/2001");
            a1.Discount = 10.47m;
            Customer a2 = new Customer();
            a2.CustomerId = 101;
            a2.Name = @"Calypso\r\n""Sub""";
            a2.Codes = StringList();
            a2.Orders.Add(a1);
            a1.Customer = a2;
            Order a3 = new Order();
            a3.OrderId = 2;
            a3.OrderedOn = DateTime.Parse("5/5/2005");
            a3.Discount = null;
            a3.CustomerId = 101;
            a3.Customer = a2;
            a2.Orders.Add(a3);
            list.Add(a1);
            list.Add(a3);

            return list;
        }

        public static Customer Customer()
        {
            return Orders().Select(a => a.Customer).Distinct().Single();
        }

        public static Customer CustomerWithoutOrders()
        {
            Customer a1 = new Customer();
            a1.CustomerId = 101;
            a1.Name = @"Calypso\r\n""Sub""";
            a1.Codes = StringList();
            a1.Orders = new List<Order>();
            return a1; 
        }

        public static List<string> StringList()
        {
            List<string> a1 = new List<string>() { "a", "b", "1", "2" };
            return a1;
        }

        public static ItemWithPropertiesToIgnore ItemWithPropertiesToIgnore()
        {
            return new ItemWithPropertiesToIgnore()
            {
                ItemId = 1
            };
        }

        public static string ItemWithPropertiesToIgnoreExpected = @"DeepShadow.UnitTests.ItemWithPropertiesToIgnore a1 = new DeepShadow.UnitTests.ItemWithPropertiesToIgnore();
a1.ItemId = 1;
a1.Name = ""Smith""
";


        public static string OrdersExpected = @"List<DeepShadow.UnitTests.Order> list = new List<DeepShadow.UnitTests.Order>();

DeepShadow.UnitTests.Order a1 = new DeepShadow.UnitTests.Order();
a1.OrderId = 1;
a1.CustomerId = 101;
a1.OrderedOn = DateTime.Parse(""1/1/2001 12:00:00 AM"");
a1.Discount = Convert.ToDecimal(10.47);
DeepShadow.UnitTests.Customer a2 = new DeepShadow.UnitTests.Customer();
a2.CustomerId = 101;
a2.Name = @""Calypso\r\n""""Sub"""""";
a2.Codes = new List<String>();
a2.Codes.Add(""a"");
a2.Codes.Add(""b"");
a2.Codes.Add(""1"");
a2.Codes.Add(""2"");
a2.Orders.Add(a1);
DeepShadow.UnitTests.Order a3 = new DeepShadow.UnitTests.Order();
a3.OrderId = 2;
a3.CustomerId = 101;
a3.OrderedOn = DateTime.Parse(""5/5/2005 12:00:00 AM"");
a3.Discount = null;
a3.Customer = a2;
a2.Orders.Add(a3);
a1.Customer = a2;
list.Add(a1);

return list;
";

        public static string CustomerExpected = @"DeepShadow.UnitTests.Customer a1 = new DeepShadow.UnitTests.Customer();
a1.CustomerId = 101;
a1.Name = @""Calypso\r\n""""Sub"""""";
a1.Codes = new List<String>();
a1.Codes.Add(""a"");
a1.Codes.Add(""b"");
a1.Codes.Add(""1"");
a1.Codes.Add(""2"");
DeepShadow.UnitTests.Order a2 = new DeepShadow.UnitTests.Order();
a2.OrderId = 1;
a2.CustomerId = 101;
a2.OrderedOn = DateTime.Parse(""1/1/2001 12:00:00 AM"");
a2.Discount = Convert.ToDecimal(10.47);
a2.Customer = a1;
a1.Orders.Add(a2);
DeepShadow.UnitTests.Order a3 = new DeepShadow.UnitTests.Order();
a3.OrderId = 2;
a3.CustomerId = 101;
a3.OrderedOn = DateTime.Parse(""5/5/2005 12:00:00 AM"");
a3.Discount = null;
a3.Customer = a1;
a1.Orders.Add(a3);

return a1;
";

        public static string StringListExpected = @"List<System.String> list = new List<System.String>();

list.Add(""a"");
list.Add(""b"");
list.Add(""1"");
list.Add(""2"");

return list;
";


    }


}
