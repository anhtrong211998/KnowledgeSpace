using KnowledgeSpace.ViewModels.Systems;
using KnowledgeSpace.ViewModels.Validators;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace KnowledgeSpace.ViewModels.UnitTest.Systems
{
    public class RoleCreateRequestValidatorTest
    {
        private RoleCreateRequestValidator validator;
        private RoleCreateRequest request;

        /// <summary>
        /// CONSTRUCTOR TEST.
        /// </summary>
        public RoleCreateRequestValidatorTest()
        {
            request = new RoleCreateRequest()
            {
                Id = "admin",
                Name = "admin"
            };
            validator = new RoleCreateRequestValidator();
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
        /// MISS ROLEID (ROLE ID IS NULL OR EMPTY).
        /// </summary>
        [Fact]
        public void Should_Error_Result_When_Request_Miss_RoleId()
        {
            request.Id = string.Empty;
            var result = validator.Validate(request);
            Assert.False(result.IsValid);
        }

        /// <summary>
        /// MISS ROLE NAME (ROLE NAME IS NULL OR EMPTY).
        /// </summary>
        [Fact]
        public void Should_Error_Result_When_Request_Miss_RoleName()
        {
            request.Name = string.Empty;
            var result = validator.Validate(request);
            Assert.False(result.IsValid);
        }

        /// <summary>
        /// MISS ROLE ID AND ALSO ROLE NAME (ROLE IS EMPTY)
        /// </summary>
        [Fact]
        public void Should_Error_Result_When_Request_Role_Empty()
        {
            request.Id = string.Empty;
            request.Name = string.Empty;
            var result = validator.Validate(request);
            Assert.False(result.IsValid);
        }
    }
}
