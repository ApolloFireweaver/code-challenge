using challenge.Controllers;
using challenge.Data;
using challenge.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using code_challenge.Tests.Integration.Extensions;

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using code_challenge.Tests.Integration.Helpers;
using System.Text;

namespace code_challenge.Tests.Integration
{
    [TestClass]
    public class EmployeeControllerTests
    {
        private static HttpClient _httpClient;
        private static TestServer _testServer;

        [ClassInitialize]
        public static void InitializeClass(TestContext context)
        {
            _testServer = new TestServer(WebHost.CreateDefaultBuilder()
                .UseStartup<TestServerStartup>()
                .UseEnvironment("Development"));

            _httpClient = _testServer.CreateClient();
        }

        [ClassCleanup]
        public static void CleanUpTest()
        {
            _httpClient.Dispose();
            _testServer.Dispose();
        }

        [TestMethod]
        public void CreateEmployee_Returns_Created()
        {
            // Arrange
            var employee = new Employee()
            {
                Department = "Complaints",
                FirstName = "Debbie",
                LastName = "Downer",
                Position = "Receiver",
            };

            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newEmployee = response.DeserializeContent<Employee>();
            Assert.IsNotNull(newEmployee.EmployeeId);
            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
            Assert.AreEqual(employee.Department, newEmployee.Department);
            Assert.AreEqual(employee.Position, newEmployee.Position);
        }

        [TestMethod]
        public void GetEmployeeById_Returns_Ok()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var expectedFirstName = "John";
            var expectedLastName = "Lennon";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var employee = response.DeserializeContent<Employee>();
            Assert.AreEqual(expectedFirstName, employee.FirstName);
            Assert.AreEqual(expectedLastName, employee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_Ok()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f",
                Department = "Engineering",
                FirstName = "Pete",
                LastName = "Best",
                Position = "Developer VI",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var putRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var putResponse = putRequestTask.Result;
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);
            var newEmployee = putResponse.DeserializeContent<Employee>();

            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_NotFound()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "Invalid_Id",
                Department = "Music",
                FirstName = "Sunny",
                LastName = "Bono",
                Position = "Singer/Song Writer",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void DirectReportsBasic()
        {
            var leader = new Employee()
            {
                EmployeeId = "03aa1462-ffa9-4978-404b-7c001562cf6f",
                Department = "Songwriter",
                FirstName = "Brendan",
                LastName = "Urie",
                Position = "Singer/Song Writer"
            };
            var underling1 = new Employee()
            {
                EmployeeId = "03aa1462-ffa9-5c39-404b-7c001562cf6f",
                Department = "Band",
                FirstName = "Ryan",
                LastName = "Ross",
                Position = "Guitar"
            };
            var underling2 = new Employee()
            {
                EmployeeId = "03aa1462-e32a-4978-404b-7c001562cf6f",
                Department = "Band",
                FirstName = "Spencer",
                LastName = "Smith",
                Position = "Drums"
            };
            leader.DirectReports.Add(underling1);
            leader.DirectReports.Add(underling2);

            var requestContent = new JsonSerialization().ToJson(leader);

            // Execute
            var putRequestTask = _httpClient.PutAsync($"api/employee/{leader.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var putResponse = putRequestTask.Result;

            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);

            var reportTask = _httpClient.PutAsync($"api/employee/GetReportingStructure/{leader.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));

            var reportResponse = reportTask.Result;

            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);

            var reportObject = reportResponse.DeserializeContent<ReportingStructure>();

            Assert.IsNotNull(reportObject);

            Assert.AreEqual(reportObject.numberOfReports, 2);
        }

        [TestMethod]
        public void DirectReportsAdvanced()
        {
            var leader = new Employee()
            {
                EmployeeId = "03aa1462-ffa9-4978-404b-7c001562cf6f",
                Department = "Songwriter",
                FirstName = "Brendan",
                LastName = "Urie",
                Position = "Singer/Song Writer"
            };
            var underling1 = new Employee()
            {
                EmployeeId = "03aa1462-ffa9-5c39-404b-7c001562cf6f",
                Department = "Band",
                FirstName = "Ryan",
                LastName = "Ross",
                Position = "Guitar"
            };
            var underling2 = new Employee()
            {
                EmployeeId = "03aa1462-e32a-4978-404b-7c001562cf6f",
                Department = "Band",
                FirstName = "Spencer",
                LastName = "Smith",
                Position = "Drums"
            };
            var minion1 = new Employee()
            {
                EmployeeId = "03bc1462-e32a-4978-404b-7c001562cf6f",
                Department = "Band",
                FirstName = "Dan",
                LastName = "Pawlovich",
                Position = "Drums"
            };
            var minion2 = new Employee()
            {
                EmployeeId = "029c1462-e32a-4978-404b-7c001562cf6f",
                Department = "Band",
                FirstName = "Dallon",
                LastName = "Weekes",
                Position = "Guitar"
            };
            var minion3 = new Employee()
            {
                EmployeeId = "03bc1462-e32a-4978-404b-7c001672cf6f",
                Department = "Band",
                FirstName = "Jon",
                LastName = "Walker",
                Position = "Guitar"
            };
            leader.DirectReports.Add(underling1);
            leader.DirectReports.Add(underling2);
            underling1.DirectReports.Add(minion2);
            underling1.DirectReports.Add(minion3);
            underling2.DirectReports.Add(minion1);

            var requestContent = new JsonSerialization().ToJson(leader);

            // Execute
            var putRequestTask = _httpClient.PutAsync($"api/employee/{leader.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var putResponse = putRequestTask.Result;

            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);

            var reportTask = _httpClient.PutAsync($"api/employee/GetReportingStructure/{leader.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));

            var reportResponse = reportTask.Result;

            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);

            var reportObject = reportResponse.DeserializeContent<ReportingStructure>();

            Assert.IsNotNull(reportObject);

            Assert.AreEqual(reportObject.numberOfReports, 5);
        }

        [TestMethod]
        public void BasicCompensationTests()
        {
            var compensation = new Compensation();
            compensation.employee = new Employee()
            {
                EmployeeId = "03aa1462-ffa9-4978-404b-7c001562cf6f",
                Department = "Songwriter",
                FirstName = "Brendan",
                LastName = "Urie",
                Position = "Singer/Song Writer"
            };
            compensation.salary = 92500.00;
            compensation.effectiveDate = DateTime.UtcNow;

            var requestContent = new JsonSerialization().ToJson(compensation);
            var putRequestTask = _httpClient.PostAsync($"api/employee/CreateCompensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var putResponse = putRequestTask.Result;

            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);

            var getRequestTask = _httpClient.PutAsync($"api/employee/getCompensation/{compensation.employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var getResponse = putRequestTask.Result;

            var savedCompensation = getResponse.DeserializeContent<Compensation>();
        }
    }
}
