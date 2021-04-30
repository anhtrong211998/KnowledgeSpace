using KnowledgeSpace.BackendServer.Controllers;
using KnowledgeSpace.BackendServer.UnitTest.Extensions;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using MockQueryable.Moq;
using KnowledgeSpace.ViewModels;

namespace KnowledgeSpace.BackendServer.UnitTest.ControllerTests
{
    public class RolesControllerTest
    {
        #region MOCK AND CONSTRUCTOR 
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;

        private List<IdentityRole> _roleSources;

        /// <summary>
        /// CONSTRUCTOR TEST.
        /// </summary>
        public RolesControllerTest()
        {
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);

            _roleSources = new List<IdentityRole>()
                            {
                                new IdentityRole("test1"),
                                new IdentityRole("test2"),
                                new IdentityRole("test3"),
                                new IdentityRole("test4")
                            };
        }

        #endregion

        /// <summary>
        /// TEST CONSTRUCTOR CONTROLLER.
        /// </summary>
        [Fact]
        public void ShouldCreateInstance_NotNull_Ok()
        {
            var rolesController = new RolesController(_mockRoleManager.Object);
            Assert.NotNull(rolesController);
        }

        #region TEST GET ALL METHOD
        /// <summary>
        /// GET ALL ROLES SUCCESS.
        /// </summary>
        /// <returns>COUNT OF LIST ROLE IS GREATER THAN 0.</returns>
        [Fact]
        public async Task GetRoles_HasData_ReturnSuccess()
        {
            _mockRoleManager.Setup(x => x.Roles)
                .Returns(_roleSources.AsQueryable().BuildMock().Object);
            var roleController = new RolesController(_mockRoleManager.Object);
            var result = await roleController.GetAll();
            var okResult = result as OkObjectResult;
            var roleVms = okResult.Value as IEnumerable<RoleVm>;
            Assert.True(roleVms.Count() > 0);
        }

        /// <summary>
        /// GET ALL ROLES RETURN EXCEPTION.
        /// </summary>
        /// <returns>EXCEPTION.</returns>
        [Fact]
        public async Task GetRoles_ThrowException_Failed()
        {
            _mockRoleManager.Setup(x => x.Roles)
                .Throws<Exception>();
            var roleController = new RolesController(_mockRoleManager.Object);

            await Assert.ThrowsAnyAsync<Exception>(async () =>
                                    await roleController.GetAll());
        }
        #endregion

        #region TEST PAGINATION.
        /// <summary>
        /// PAGINATION WITH NO FILTER (KEYWORD IS NULL).
        /// </summary>
        /// <returns>TOTAL RECORD IS 4 AND TOTAL ROW IS 2.</returns>
        [Fact]
        public async Task GetRolesPagin_NotFilter_ReturnSuccess()
        {
            _mockRoleManager.Setup(x => x.Roles)
                .Returns(_roleSources.AsQueryable().BuildMock().Object);
            var roleController = new RolesController(_mockRoleManager.Object);
            var result = await roleController.GetRolesPagin(null,1,2);
            var okResult = result as OkObjectResult;
            var roleVms = okResult.Value as Pagination<RoleVm>;

            Assert.Equal(4, roleVms.TotalRecords);
            Assert.Equal(2, roleVms.Items.Count);
        }

        /// <summary>
        /// PAGINATION WITH FILTER (KEYWORD IS test3).
        /// </summary>
        /// <returns>TOTAL RECORDS IS 1 AND TOTAL ROWS IS SINGLE</returns>
        [Fact]
        public async Task GetRolesPagin_HasFilter_ReturnSuccess()
        {
            _mockRoleManager.Setup(x => x.Roles)
                .Returns(_roleSources.AsQueryable().BuildMock().Object);
            var roleController = new RolesController(_mockRoleManager.Object);
            var result = await roleController.GetRolesPagin("test3", 1, 2);
            var okResult = result as OkObjectResult;
            var roleVms = okResult.Value as Pagination<RoleVm>;

            Assert.Equal(1, roleVms.TotalRecords);
            Assert.Single(roleVms.Items);
        }

        /// <summary>
        /// PAGINATION RETURN EXCEPTION.
        /// </summary>
        /// <returns>EXCEPTION.</returns>
        [Fact]
        public async Task GetRolesPagin_ThrowException_Failed()
        {
            _mockRoleManager.Setup(x => x.Roles)
                .Throws<Exception>();
            var roleController = new RolesController(_mockRoleManager.Object);

            await Assert.ThrowsAnyAsync<Exception>(async () =>
                                    await roleController.GetRolesPagin(null,1,2));
        }
        #endregion

        #region TEST GET ROLE WITH ID
        /// <summary>
        /// GET ROLE BY ID SUCCESS.
        /// </summary>
        /// <returns>RESULT NOT NULL.</returns>
        [Fact]
        public async Task GetById_HasData_ReturnSuccess()
        {
            _mockRoleManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityRole()
                {
                    Id = "test1",
                    Name = "Test 1"
                }) ;

            var roleController = new RolesController(_mockRoleManager.Object);
            var result = await roleController.GetById("test1");
            var okResult = result as OkObjectResult;

            Assert.NotNull(okResult);

            var roleVm = okResult.Value as RoleVm;
            Assert.Equal("Test 1",roleVm.Name);
        }

        /// <summary>
        /// GET ROLE BY ID RETURN EXCEPTION.
        /// </summary>
        /// <returns>EXCEPTION.</returns>
        [Fact]
        public async Task GetById_ThrowException_Failed()
        {
            _mockRoleManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .Throws<Exception>();
            var roleController = new RolesController(_mockRoleManager.Object);

            await Assert.ThrowsAnyAsync<Exception>(async () =>
                                    await roleController.GetById("test1"));
        }
        #endregion

        #region TEST POST ROLE
        /// <summary>
        /// CREATE NEW ROLE AND RETURN SUCCESS.
        /// </summary>
        /// <returns>RESULT IS NOT NULL, AND HTTP STATUS IS 201.</returns>
        [Fact]
        public async Task PostRole_ValidInput_Success()
        {
            _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
                            .ReturnsAsync(IdentityResult.Success);

            var rolesController = new RolesController(_mockRoleManager.Object);

            var result = await rolesController.PostRole(new RoleCreateRequest()
            {
                Id = "test",
                Name = "test"
            });

            Assert.NotNull(result);
            Assert.IsType<CreatedAtActionResult>(result);
        }

        /// <summary>
        /// CREATE NEW ROLE FAILED.
        /// </summary>
        /// <returns>RESULT NOT NULL AND HTTP STATUS IS 400.</returns>
        [Fact]
        public async Task PostRole_ValidInput_Failed()
        {
            _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError[] { }));

            var rolesController = new RolesController(_mockRoleManager.Object);

            var result = await rolesController.PostRole(new RoleCreateRequest()
            {
                Id = "test",
                Name = "test"
            });

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
        }
        #endregion

        #region TEST PUT ROLE
        /// <summary>
        /// UPDATE ROLE SUCCESS.
        /// </summary>
        /// <returns>RESULT IS NOT NULL AND HTTP STATUS IS 204.</returns>
        [Fact]
        public async Task PutRole_ValidInput_Success()
        {
            _mockRoleManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityRole()
                {
                    Id = "test",
                    Name = "test"
                });

            _mockRoleManager.Setup(x => x.UpdateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Success);

            var roleController = new RolesController(_mockRoleManager.Object);

            var result = await roleController.PutRole("test", new RoleCreateRequest()
            {
                Id = "test",
                Name = "test"
            });

            Assert.NotNull(result);
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// UPDATE ROLE FAILED.
        /// </summary>
        /// <returns>RESULT IS NOT NULL AND HTTP STATUS IS 400.</returns>
        [Fact]
        public async Task PutRole_ValidInput_Failed()
        {
            _mockRoleManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityRole()
                {
                    Id = "test",
                    Name = "test"
                });

            _mockRoleManager.Setup(x => x.UpdateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError[] { })
                );

            var roleController = new RolesController(_mockRoleManager.Object);

            var result = await roleController.PutRole("test", new RoleCreateRequest()
            {
                Id = "test",
                Name = "test"
            });

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
        }
        #endregion

        #region TEST DELETE ROLE
        /// <summary>
        /// DELETE ROLE SUCCESS.
        /// </summary>
        /// <returns>HTTP STATUS IS 200.</returns>
        [Fact]
        public async Task Delete_ValidInput_Success()
        {
            _mockRoleManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityRole()
                {
                    Id = "test",
                    Name = "test"
                });

            _mockRoleManager.Setup(x => x.DeleteAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Success);

            var roleController = new RolesController(_mockRoleManager.Object);

            var result = await roleController.DeleteRole("test");

            Assert.IsType<OkObjectResult>(result);
        }

        /// <summary>
        /// DELETE ROLE FAILED
        /// </summary>
        /// <returns>HTTP STATUS IS 400.</returns>
        [Fact]
        public async Task Delete_ValidInput_Failed()
        {
            _mockRoleManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new IdentityRole()
                {
                    Id = "test",
                    Name = "test"
                });

            _mockRoleManager.Setup(x => x.DeleteAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Failed(
                    new IdentityError[] { })
                );

            var roleController = new RolesController(_mockRoleManager.Object);

            var result = await roleController.DeleteRole("test");

            Assert.IsType<BadRequestObjectResult>(result);
        }
        #endregion

    }
}
