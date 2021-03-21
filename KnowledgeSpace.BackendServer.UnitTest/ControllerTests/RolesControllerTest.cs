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
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;

        private List<IdentityRole> _roleSources;

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
        //// Test contructor
        [Fact]
        public void ShouldCreateInstance_NotNull_Ok()
        {
            var rolesController = new RolesController(_mockRoleManager.Object);
            Assert.NotNull(rolesController);
        }

        #region Test for Getall method
        //// get success
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

        //// get throw exception
        [Fact]
        public async Task GetRoles_ThrowException_Failed()
        {
            _mockRoleManager.Setup(x => x.Roles)
                .Throws<Exception>();
            var roleController = new RolesController(_mockRoleManager.Object);

            await Assert.ThrowsAnyAsync<Exception>(async () =>
                                    await roleController.GetAll());
        }
        #endregion Test for Getall method

        #region Test for GetPagin method
        //// get with no filter success
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

        //// get with filter success
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
        //// get throw exception
        [Fact]
        public async Task GetRolesPagin_ThrowException_Failed()
        {
            _mockRoleManager.Setup(x => x.Roles)
                .Throws<Exception>();
            var roleController = new RolesController(_mockRoleManager.Object);

            await Assert.ThrowsAnyAsync<Exception>(async () =>
                                    await roleController.GetRolesPagin(null,1,2));
        }
        #endregion Test for GetPagin method

        #region Test for get by id method
        //// Get success
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

        //// get throw exception
        [Fact]
        public async Task GetById_ThrowException_Failed()
        {
            _mockRoleManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .Throws<Exception>();
            var roleController = new RolesController(_mockRoleManager.Object);

            await Assert.ThrowsAnyAsync<Exception>(async () =>
                                    await roleController.GetById("test1"));
        }
        #endregion end test for get by id method

        #region Test for Post method
        //// Post success
        [Fact]
        public async Task PostRole_ValidInput_Success()
        {
            _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
                            .ReturnsAsync(IdentityResult.Success);

            var rolesController = new RolesController(_mockRoleManager.Object);

            var result = await rolesController.PostRole(new RoleVm()
            {
                Id = "test",
                Name = "test"
            });

            Assert.NotNull(result);
            Assert.IsType<CreatedAtActionResult>(result);
        }

        //// Post failed
        [Fact]
        public async Task PostRole_ValidInput_Failed()
        {
            _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError[] { }));

            var rolesController = new RolesController(_mockRoleManager.Object);

            var result = await rolesController.PostRole(new RoleVm()
            {
                Id = "test",
                Name = "test"
            });

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
        }
        #endregion Test for Post method

        #region Test for Update 
        //// update success.
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

            var result = await roleController.PutRole("test", new RoleVm()
            {
                Id = "test",
                Name = "test"
            });

            Assert.NotNull(result);
            Assert.IsType<NoContentResult>(result);
        }

        //// Update failed
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

            var result = await roleController.PutRole("test", new RoleVm()
            {
                Id = "test",
                Name = "test"
            });

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
        }
        #endregion end test for Update method

        #region Test for Delete
        //// delete success.
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

        //// Delete failed
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
        #endregion End test for delete

    }
}
