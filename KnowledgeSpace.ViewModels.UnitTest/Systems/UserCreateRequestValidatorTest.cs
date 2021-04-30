using KnowledgeSpace.ViewModels.Systems;
using KnowledgeSpace.ViewModels.Validators;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace KnowledgeSpace.ViewModels.UnitTest.Systems
{
    public class UserCreateRequestValidatorTest
    {
        private UserCreateRequestValidator validator;
        private UserCreateRequest request;

        /// <summary>
        /// CONSTRUCTOR TEST.
        /// </summary>
        public UserCreateRequestValidatorTest()
        {
            request = new UserCreateRequest()
            {
                Dob = DateTime.Now,
                Email = "tedu.international@gmail.com",
                FirstName = "Test",
                LastName = "test",
                Password = "Admin@123",
                PhoneNumber = "12345",
                UserName = "test"
            };
            validator = new UserCreateRequestValidator();
        }

        /// <summary>
        /// NOT MISS ANY FIELD (THERE IS NOT ANY FIELD NULL OR EMPTY).
        /// </summary>
        [Fact]
        public void Should_Valid_Result_When_Valid_Request()
        {
            var result = validator.Validate(request);
            Assert.True(result.IsValid);
        }

        /// <summary>
        /// MISS USER NAME (NAME IS NULL OR EMPTY).
        /// </summary>
        /// <param name="userName">NULL OR EMPTY.</param>
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Error_Result_When_Miss_UserName(string userName)
        {
            request.UserName = userName;
            var result = validator.Validate(request);
            Assert.False(result.IsValid);
        }

        /// <summary>
        /// MISS LAST NAME (LAST NAME IS NULL OR EMPTY).
        /// </summary>
        /// <param name="data">NULL OR EMPTY.</param>
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Error_Result_When_Miss_LastName(string data)
        {
            request.LastName = data;
            var result = validator.Validate(request);
            Assert.False(result.IsValid);
        }

        /// <summary>
        /// MISS FIRST NAME (FIRST NAME IS NULL OR EMPTY).
        /// </summary>
        /// <param name="data">NULL OR EMPTY.</param>
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Error_Result_When_Miss_FirstName(string data)
        {
            request.FirstName = data;
            var result = validator.Validate(request);
            Assert.False(result.IsValid);
        }

        /// <summary>
        /// MISS PHONE NUMBER (PHONE NUMBER IS NULL OR EMPTY).
        /// </summary>
        /// <param name="data">NULL OR EMPTY.</param>
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Error_Result_When_Miss_PhoneNumber(string data)
        {
            request.PhoneNumber = data;
            var result = validator.Validate(request);
            Assert.False(result.IsValid);
        }

        /// <summary>
        /// PASSWORD NOT MATCH FORMAT.
        /// </summary>
        /// <param name="data">ONLY LOWER CHARACTER, ONLY NUMBER, NULL, EMPTY,NULL, 
        /// HAVE UPPER CHARACTER + LOWER CHARACTER + NUMBER BUT LENGTH IS 7 CHARACTERS.</param>
        [Theory]
        [InlineData("sdasfaf")]
        [InlineData("1234567")]
        [InlineData("Admin123")]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Error_Result_When_Password_Not_Match(string data)
        {
            request.Password = data;
            var result = validator.Validate(request);
            Assert.False(result.IsValid);
        }
    }
}
