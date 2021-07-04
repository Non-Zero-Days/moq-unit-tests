## C# Moq Unit Tests

*A unit test attempts to verify the functionality of a single unit of work. Given a class method with inputs and measurable outputs a unit test aims to verify the correctness of part of the method without peeking into private state management such as a database.*

### Prerequisites:

[.NET 5.0](https://dotnet.microsoft.com/download/dotnet/5.0)
[Visual Studio 2022 Preview](https://visualstudio.microsoft.com/vs/preview/vs2022//)

### Loose Agenda:
Learn about the Moq unit testing framework in .NET

Identify scenarios worth unit testing

### Step by Step

#### Setup testing framework

Create a directory for today's exercise and navigate to it in a terminal instance.

We're going to leverage the code created in [this exercise](https://github.com/Non-Zero-Days/clean-architecture) in order to bootstrap our effort. We will copy the `src` directory from that repository into today's exercise's directory.

From a terminal instance in the root directory, run `dotnet new sln` to create an empty solution. 

Add the `src.csproj` to the solution by running  `dotnet sln add src/src.csproj`

Create a new .NET unit test project in it's own directory by running `dotnet new mstest -o test`

Add the `test.csproj` to the solution by running  `dotnet sln add test/test.csproj`

Add a reference from the test project to the src project by running `dotnet add test/test.csproj reference src/src.csproj`

Add a reference to Moq to the test.csproj by running `dotnet add test/test.csproj package moq`

Now let's open our solution in Visual Studio 2022 Preview.

#### Create a test without Moq

In Visual Studio, open `test/UnitTest1.cs`

Back in the [C# Unit Tests exercise](https://github.com/Non-Zero-Days/csharp-unit-tests#arrange-act-assert) we talked about Arrange/Act/Assert. We're going to leverage that practice in order to test the src/Core/ContactService class.

We want to make sure that our `Create` method is validating for a null input, so let's rename the `TestMethod1` method to `ContactService_Create_Throws_InvalidOperationException_For_NullInput`. 

If the ContactService is working as intended then we should expect the code to throw an InvalidOperationException. As such, just above the TestMethod let's add `[ExpectedException(typeof(InvalidOperationException))]`. 

The Arrange step will be creating our repository, service and input contact of null. 

The Act step will be calling the service.Create method.

The Assert step is handled by our ExpectedException attribute. If an InvalidOperationException is not thrown then the test will fail.

Our method now looks like
```
[TestMethod]
[ExpectedException(typeof(InvalidOperationException))]
public void ContactService_Create_Throws_InvalidOperationException_For_NullInput()
{
    // Arrange
    var repository = new ContactRepository();
    var service = new ContactService(repository);
    Contact inputContact = null;

    // Act
    service.Create(inputContact);
}
```

Open `View > Test Explorer` or press `Ctrl+E`, `T` on the keyboard to open the Test Explorer window. Run all tests in this window and note that the test should succeed.

#### Create a test with Moq

Note that in the previous steps we created a test but required a dependency on the exact implementation details of infrastructure code (the ContactRepository.) If this repository was connected to a database then we would have potential to actually call the database. In the spirit of only testing a single unit of work, we want to replace the specific infrastructure implementation with a mocked implementation of the infrastructure contract. 

We can adjust the method we just created with Moq by writing:
```
[TestMethod]
[ExpectedException(typeof(InvalidOperationException))]
public void ContactService_Create_Throws_InvalidOperationException_For_NullInput()
{
    // Arrange
    var repository = new Mock<IContactRepository>();
    var service = new ContactService(repository.Object);
    Contact inputContact = null;

    // Act
    service.Create(inputContact);
}
```

Note that we've removed the need for the using declaration `using src.Infrastructure;` and instead have `using Moq;`

#### Moq Setup

For the immediate our ContactService.Retrieve method just forwards the call to our Repository. Let's create a new test method to verify that the result from the repository is returned through the service. In order to do this we will need to use Moq's Setup method.

```
[TestMethod]
public void ContactService_Retrieve_Returns_Contact_From_Repository()
{
    // Arrange
    var repository = new Mock<IContactRepository>();
    var service = new ContactService(repository.Object);
    var contactName = "Pete";
    var expectedContact = new Contact()
    {
        Name = contactName,
        Number = "5031234567",
        Type = "Person"
    };
    repository.Setup(repo => repo.Retrieve(contactName)).Returns(expectedContact);

    // Act
    var actualContact = service.Retrieve(contactName);

    // Assert
    Assert.IsNotNull(actualContact);
    Assert.AreEqual(actualContact.Name, expectedContact.Name);
    Assert.AreEqual(actualContact.Number, expectedContact.Number);
    Assert.AreEqual(actualContact.Type, expectedContact.Type);
}
```

We can run this test to verify that each property received matches each property expected.

Note that we call `repository.Setup(repo => repo.Retrieve(contactName)).Returns(expectedContact);` in order to instruct our Mock object on how to behave if it receives a very specific name input `contactName`. If we don't care about the specific input name we can instead use `It.IsAny<string>()` to instruct our Mock object to return the expectedContact regardless of input string. Note that if we changed our code to the following it would work as well

```
[TestMethod]
public void ContactService_Retrieve_Returns_Contact_From_Repository()
{
    // Arrange
    var repository = new Mock<IContactRepository>();
    var service = new ContactService(repository.Object);
    var expectedContact = new Contact()
    {
        Name = "Pete",
        Number = "5031234567",
        Type = "Person"
    };
    repository.Setup(repo => repo.Retrieve(It.IsAny<string>())).Returns(expectedContact);

    // Act
    var actualContact = service.Retrieve(expectedContact.Name);

    // Assert
    Assert.IsNotNull(actualContact);
    Assert.AreEqual(actualContact.Name, expectedContact.Name);
    Assert.AreEqual(actualContact.Number, expectedContact.Number);
    Assert.AreEqual(actualContact.Type, expectedContact.Type);
}
```

#### Verify a dependency call

In the above test we expect that the repository Retrieve method is being called and mock behavior accordingly. If we wanted to additionally check that this method is called as many times as we expect then we can do so in the Act section with Verify. Let's change our `ContactService_Retrieve_Returns_Contact_From_Repository` code to the following.

```
[TestMethod]
public void ContactService_Retrieve_Returns_Contact_From_Repository()
{
    // Arrange
    var repository = new Mock<IContactRepository>();
    var service = new ContactService(repository.Object);
    var expectedContact = new Contact()
    {
        Name = "Pete",
        Number = "5031234567",
        Type = "Person"
    };
    repository.Setup(repo => repo.Retrieve(It.IsAny<string>())).Returns(expectedContact);

    // Act
    var actualContact = service.Retrieve(expectedContact.Name);

    // Assert
    Assert.IsNotNull(actualContact);
    Assert.AreEqual(actualContact.Name, expectedContact.Name);
    Assert.AreEqual(actualContact.Number, expectedContact.Number);
    Assert.AreEqual(actualContact.Type, expectedContact.Type);

    repository.Verify(repo => repo.Retrieve(It.IsAny<string>()), Times.Once);
}
```

Running this test now verifies that the Retrieve method is only called once. Note that if we change the `Times.Once` to `Times.Exactly(3)` then the test will fail.

### Additional Documentation

- [Moq Quickstart](https://github.com/Moq/moq4/wiki/Quickstart)
- [Unit Testing Library](https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.testtools.unittesting)
