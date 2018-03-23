
## ![](images/logo-128x128.png) an assembly to help generate unit test data from Entity Framework

DeepShadow helps solve a specific yet common use case: the developer needs unit test data from complex models, and wishes she could use actual data with some modifications.

For example, here are relatively simple parent-child domain classes. They map to SQL tables, and the application is using Entity Framework.

```csharp
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
```

You might need to test a Web API that itself calls a service. The model that comes back from the service includes related classes (i.e. Navigation Properties).

```csharp
    public class CustomersController : ApiController
    {
        ICustomerService _customerService = null;

        public CustomersController(ICustomerService customerSerivce) 
        {
            _customerService = customerService
        }
        
        //GET api/customers
        public List<CustomerViewModel> Get()
        {
            //Returns all orders for each customer
            List<Customer> customers = _customerService.GetAllCustomers();
            
            List<CustomerViewModel> model = new List<CustomerViewModel>();
            
            foreach (var customer in customers)
            {
                CustomerViewModel custViewModel = new CustomerViewModel()
                {
                    //transform to the view model
                    AllCapsName = customer.Name.ToUpper();
                    CodeList = String.Join(" ", customer.Codes);
                    Orders = customer.Orders.Select(a => new OrderViewModel() 
                    {
                      //likewise transform orders to the view model  
                    });
                }
                model.Add(custViewModel);
            }
            return Content(model);
        }
    }
```

You're not testing the service, but you need to be sure that the Customer/Order properties map correctly to the View Model properties. You don't want to find out in production you set the CustomerViewModel.CodeList property to Customer.Name.

A simplistic unit test might look like this (NOT a working example!):

```csharp
    
    [TestMethod]
    public void TestMethod1()
    {
        ICustomerService mockService = new MockCustomerService();
        mockService.TestCustomers = MockData.CustomersAndOrders;
        var controller = new CustomersController(mockService);
        var actual = controller.Get();
        Assert.AreEqual(mockService.TestCustomers.Count(), actual.Count());
    }
```

The mock service could be as simple as this, if you didn't want to use a mocking framework. Each test can set what customers/orders it wants to use.

```csharp
    
    public class CustomerSerivce: ICustomerService
    {
        public List<Customer> TestCustomers {get; set;}
        
        public List<Customer> GetAllCustomers() 
        {
            return TestCustomers;
        }
    }
    
```

So, great, but what about the actual test data? In many cases, it's OK to hand-craft what you need, like this:

```csharp
    Customer customer = new Customer()
    {
        CustomerId = 1,
        Name = "blamo",
        Codes = new List<string> {"a","b"},
        Orders = new List<Order>()
        {
            new Order()
            {
                OrderId = 11,
                CustomerId = 1,
                OrderedOn = DateTime.Parse("1/1/2001"),
                Discount = 10.00m,
                Customer = this
            }
        }
    }
```

But what if you have classes with twenty or thirty properties? And what if those properties have some domain-specific data or formats that need careful testing? 

```
Customer.ID = {820bd7ae-fed3-419f-b778-8a817b45a968}
Customer.Name = Lewellen-Taft-O'Brien Marketing, LLC.
Customer.DiscountMatrix = 15.00|13.25|10.973
etc.
```

It's both tedious and error-prone to fabricate values like that when excellent test data *right there in the database*! And, you already have code in the service to return that data. You just want to generate code to recreate the data in memory.

## Enter DeepShadow

With DeepShadow, you can potentially generate your models as simply as this:

```csharp
//This is an Entity Framework DbContext
MyDb db = new MyDb();

//REQUIRED FOR Mock Data Generation
db.Configuration.ProxyCreationEnabled = false;

//Generate a list of models
db.Customers.Take(5).GenerateEntitiesFromList(toConsole: true);

//Generate from a single entity
db.Customers.First().GenerateEntitiesFromObject(toConsole: true);
```

The output will look like this:

```csharp
List<DeepShadowDbContext.Customer> list = new List<DeepShadowDbContext.Customer>();

DeepShadowDbContext.Customer a1 = new DeepShadowDbContext.Customer();
a1.CustomerId = 1;
a1.Name = @"Nicolo Paganini";
a1.Codes = new List<String>();
a1.Codes.Add("A1");
a1.Codes.Add("A2");
a1.Codes.Add("A3");
a1.Orders = new List<DeepShadowDbContext.Order>();
list.Add(a1);
DeepShadowDbContext.Customer a2 = new DeepShadowDbContext.Customer();
a2.CustomerId = 2;
a2.Name = @"Hilary Hahn";
a2.Codes = new List<String>();
a2.Codes.Add("A1");
a2.Codes.Add("A2");
a2.Codes.Add("A3");
a2.Orders = new List<DeepShadowDbContext.Order>();
list.Add(a2);

return list;
```

You'd put the above in some kind of unit testing MockData class, which is used to set the mock service method's return value.

```csharp
public static class MockData
{
    public static List<Customer> CustomersAndOrders
    {
        get {
            List<DeepShadowDbContext.Customer> list = new List<DeepShadowDbContext.Customer>();
            //data
            return list;
        }
    }
}
```

## Masking Data
Often you'll want to retrieve data from your database, but obscure sensitive data. That's easy enough in most cases without any help from DeepShadow.

```csharp
//This is an Entity Framework DbContext
MyDb db = new MyDb();

//REQUIRED FOR Mock Data Generation
db.Configuration.ProxyCreationEnabled = false;

var employees = db.Employees.Where(a => a.Inactive == true).ToList();

//mask data
foreach (var employee in employees )
{
    employee.SSN = "111-22-3456";
    employee.DateOfBirth = DateTime.Parse("1/1/2001");
}

//Generate a list of models
employees.GenerateEntitiesFromList(toConsole: true);

```

## Complete Example

I use (and recommend) the registered version of [LINQPad](http://www.linqpad.net/) for generating the data, though you could also create a console app to do it. The code's almost exactly the same. In this example, I've created a SQL database named DeepShadowSample, and a console application with an Entity Framework DbContext named DeepShadowDb.

>**Important**  
>If you're using LINQPad, you must have a registered version in order to easily install the NuGet package. 

#### Table Schema and Data
```sql
create table Customers
(
	CustomerId int not null primary key,
	Name varchar(max)
)

create table Codes
(
	Id int identity primary key,
	Name varchar(max)
)

create table Orders
(
	OrderId int not null primary key,
	CustomerId int not null foreign key references Customers(CustomerId),
	OrderedOn datetime not null,
	Discount decimal null
)


insert Customers select 1, 'Nicolo Paganini'
insert Customers select 2, 'Jasha Heifetz'
insert Customers select 3, 'Erica Morini'
insert Customers select 4, 'Hilary Hahn'

insert Orders select 2, 1, '2001-01-01', 10
insert Orders select 3, 2, '2002-01-01', 0
insert Orders select 4, 2, '2002-02-02', 5
insert Orders select 5, 3, '2012-05-23', 0
insert Orders select 6, 3, '2012-05-23 12:00:00.000', NULL
insert Orders select 7, 3, '2012-05-24', 0
insert Orders select 9, 4, '2015-12-12', NULL
insert Orders select 10, 4, '2017-12-15', 3

insert Codes (Name) values ('A1,A2,A3')
insert Codes (Name) values ('B4,B7')
```

#### DeepShadowDbContext Console App

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace DeepShadowDbContext
{
    class Program
    {
        static void Main(string[] args)
        {            
            var service = new CustomerService(new DeepShadowDb());
            Console.WriteLine(service.GetAllCustomers().Count()); //should return 4
            Console.ReadLine();
        }
    }

    public class CustomerService
    {
        DeepShadowDb _db;

        public CustomerService(DeepShadowDb db)
        {
            _db = db;
        }

        public List<Customer> GetAllCustomers()
        {
            var customers = _db.Customers.ToList();
            var codes = _db.Codes.First();
            foreach (var customer in customers)
            {
                customer.Codes = codes.Name.Split(',').ToList();
            }
            return customers;
        }
    }

    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public List<string> Codes { get; set; }
        //Navigation
        public List<Order> Orders { get; set; } = new List<Order>();
    }

    public class Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderedOn { get; set; }
        public decimal? Discount { get; set; }
        //Navigation
        public Customer Customer { get; set; }
    }

    public class Code
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class DeepShadowDb : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<Code> Codes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().Ignore(a => a.Codes);
        }
    }
}
```
#### App.Config
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
  </configSections>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.6.1" />
  </startup>
  <connectionStrings>
    <add name="DeepShadowDb" 
         connectionString="Server=.\SQLEXPRESS2016;Database=DeepShadowSample;Trusted_Connection=True;"
         providerName="System.Data.SqlClient"/>
  </connectionStrings>
</configuration>
```


#### 1. Open LINQPad to a new C# Statements query

#### 2. Add Entity Framework connection

![](images/2018-03-23%2015_19_33.png)

![](images/2018-03-23%2015_22_32.png)

![](images/2018-03-23%2015_23_55.png)

![](images/2018-03-23%2015_27_14.png)

![](images/2018-03-23%2015_30_01.png)

#### 3. Set the query connection

![](images/2018-03-23%2015_31_44.png)

#### 4. Press F4 to open the query settings, and click Add NuGet

#### 5. Search online for DeepShadow, and add to query

![](images/2018-03-23%2015_36_34.png)

#### 6. In the Query Properties window, Additional Namespace Imports tab, click Pick from assemblies and add the `DeepShadow`  namespace.

![](images/2018-03-23%2015_42_34.png)

![](images/2018-03-23%2015_41_19.png)


#### 7. In the query window, add this code. Notice how it's essentially copy/paste from `CustomerService.GetAllCustomers`.

```csharp
DeepShadowDb _db = this;

//REQUIRED FOR Mock Data Generation
_db.Configuration.ProxyCreationEnabled = false;
//_db.Database.Connection.ConnectionString.Dump();
//////////////////

//This is the code copied from the supposed CustomerService.GetAllCustomers method
//Note that the service does some data manipulation by setting Codes from another table.

var customers = _db.Customers.ToList();
var codes = _db.Codes.First();
foreach (var customer in Customers)
{
    customer.Codes = codes.Name.Split(',').ToList();
}
//This generates the code
customers.GenerateEntitiesFromList(toConsole: true);

```

#### 8. The result

```csharp
List<DeepShadowDbContext.Customer> list = new List<DeepShadowDbContext.Customer>();

DeepShadowDbContext.Customer a1 = new DeepShadowDbContext.Customer();
a1.CustomerId = 1;
a1.Name = @"Nicolo Paganini";
a1.Codes = new List<String>();
a1.Codes.Add("A1");
a1.Codes.Add("A2");
a1.Codes.Add("A3");
a1.Orders = new List<DeepShadowDbContext.Order>();
list.Add(a1);
DeepShadowDbContext.Customer a2 = new DeepShadowDbContext.Customer();
a2.CustomerId = 2;
a2.Name = @"Jasha Heifetz";
a2.Codes = new List<String>();
a2.Codes.Add("A1");
a2.Codes.Add("A2");
a2.Codes.Add("A3");
a2.Orders = new List<DeepShadowDbContext.Order>();
list.Add(a2);
DeepShadowDbContext.Customer a3 = new DeepShadowDbContext.Customer();
a3.CustomerId = 3;
a3.Name = @"Erica Morini";
a3.Codes = new List<String>();
a3.Codes.Add("A1");
a3.Codes.Add("A2");
a3.Codes.Add("A3");
a3.Orders = new List<DeepShadowDbContext.Order>();
list.Add(a3);
DeepShadowDbContext.Customer a4 = new DeepShadowDbContext.Customer();
a4.CustomerId = 4;
a4.Name = @"Hilary Hahn";
a4.Codes = new List<String>();
a4.Codes.Add("A1");
a4.Codes.Add("A2");
a4.Codes.Add("A3");
a4.Orders = new List<DeepShadowDbContext.Order>();
list.Add(a4);

return list;
```