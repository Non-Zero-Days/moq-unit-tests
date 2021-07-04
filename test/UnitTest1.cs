using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using src.Core;

using System;

namespace test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ContactService_Create_Throws_InvalidOperationException_For_NullInput()
        {
            // Arrange
            var repository = new Mock<IContactRepository>();
            var service = new ContactService(repository.Object);
            Contact contact = null;

            // Act
            service.Create(contact);

            // Assert
        }

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
            Assert.AreEqual(expectedContact.Name, actualContact.Name);
            Assert.AreEqual(expectedContact.Number, actualContact.Number);
            Assert.AreEqual(expectedContact.Type, actualContact.Type);
            repository.Verify(repo => repo.Retrieve(It.IsAny<string>()), Times.Once);
        }
    }
}
