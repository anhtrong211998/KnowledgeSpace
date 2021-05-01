using KnowledgeSpace.ViewModels.Systems;
using KnowledgeSpace.ViewModels.Validators;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace KnowledgeSpace.ViewModels.UnitTest.Systems
{
    public class FunctionCreateRequestValidatorTest
    {
        private FunctionCreateRequestValidator validator;
        private FunctionCreateRequest request;

        /// <summary>
        /// CONSTRUCTOR TEST.
        /// </summary>
        public FunctionCreateRequestValidatorTest()
        {
            request = new FunctionCreateRequest()
            {
                Id = "test6",
                ParentId = null,
                Name = "test6",
                SortOrder = 6,
                Url = "/test6"
            };
            validator = new FunctionCreateRequestValidator();
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
        /// MISS FUNCTION ID (KEY).
        /// </summary>
        /// <param name="data">NULL OR EMPTY.</param>
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Error_Result_When_Miss_Id(string data)
        {
            request.Id = data;
            var result = validator.Validate(request);
            Assert.False(result.IsValid);
        }

        /// <summary>
        /// MISS FUNCTION NAME.
        /// </summary>
        /// <param name="data">NULL OR EMPTY.</param>
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Error_Result_When_Miss_Name(string data)
        {
            request.Name = data;
            var result = validator.Validate(request);
            Assert.False(result.IsValid);
        }

        /// <summary>
        /// MISS URL.
        /// </summary>
        /// <param name="data">NULL OR EMPTY.</param>
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Should_Error_Result_When_Miss_Url(string data)
        {
            request.Url = data;
            var result = validator.Validate(request);
            Assert.False(result.IsValid);
        }
    }
}
