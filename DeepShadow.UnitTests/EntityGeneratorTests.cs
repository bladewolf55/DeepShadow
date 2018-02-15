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
            public void GeneratesMatchingEntitiesFromList()
            {
                var list = MockData.Orders();
                var actual = list.GenerateEntitiesFromList();
                Assert.Equal(MockData.OrdersExpected, actual);
            }

            [Fact]
            public void GeneratesMatchingEntitiesFromEntity()
            {
                var entity = MockData.Customer();
                var actual = entity.GenerateEntitiesFromObject();
                Assert.Equal(MockData.CustomerExpected, actual);
            }
        }
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
            Customer a2 = new Customer();
            a2.CustomerId = 101;
            a2.Name = "Calypso";
            a2.Orders.Add(a1);
            a1.Customer = a2;
            Order a3 = new Order();
            a3.OrderId = 2;
            a3.OrderedOn = DateTime.Parse("5/5/2005");
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

        public static string OrdersExpected = @"List<DeepShadow.UnitTests.Order> list = new List<DeepShadow.UnitTests.Order>();

DeepShadow.UnitTests.Order a1 = new DeepShadow.UnitTests.Order();
a1.OrderId = 1;
a1.CustomerId = 101;
a1.OrderedOn = DateTime.Parse(""1/1/2001 12:00:00 AM"");
DeepShadow.UnitTests.Customer a2 = new DeepShadow.UnitTests.Customer();
a2.CustomerId = 101;
a2.Name = ""Calypso"";
a2.Orders.Add(a1);
DeepShadow.UnitTests.Order a3 = new DeepShadow.UnitTests.Order();
a3.OrderId = 2;
a3.CustomerId = 101;
a3.OrderedOn = DateTime.Parse(""5/5/2005 12:00:00 AM"");
a3.Customer = a2;
a2.Orders.Add(a3);
a1.Customer = a2;
list.Add(a1);

return list;
";

        public static string CustomerExpected = @"DeepShadow.UnitTests.Customer a1 = new DeepShadow.UnitTests.Customer();
a1.CustomerId = 101;
a1.Name = ""Calypso"";
DeepShadow.UnitTests.Order a2 = new DeepShadow.UnitTests.Order();
a2.OrderId = 1;
a2.CustomerId = 101;
a2.OrderedOn = DateTime.Parse(""1/1/2001 12:00:00 AM"");
a2.Customer = a1;
a1.Orders.Add(a2);
DeepShadow.UnitTests.Order a3 = new DeepShadow.UnitTests.Order();
a3.OrderId = 2;
a3.CustomerId = 101;
a3.OrderedOn = DateTime.Parse(""5/5/2005 12:00:00 AM"");
a3.Customer = a1;
a1.Orders.Add(a3);
list.Add(a1);

return a1;
";
    }


    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        //Nav
        public List<Order> Orders { get; set; } = new List<Order>();
    }

    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderedOn { get; set; }
        //Nav
        public Customer Customer { get; set; }
    }


}
