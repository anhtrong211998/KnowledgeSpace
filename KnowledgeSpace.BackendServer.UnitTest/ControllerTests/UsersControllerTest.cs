using KnowledgeSpace.BackendServer.Controllers;
using KnowledgeSpace.BackendServer.Models;
using KnowledgeSpace.BackendServer.Models.Entities;
using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MockQueryable.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace KnowledgeSpace.BackendServer.UnitTest.ControllerTests
{
    public class UsersControllerTest
    {
        #region MOCK AND CONSTRUCTOR
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private KnowledgeSpaceContext _context;

        private List<User> _userSources = new List<User>(){
                             new User("1","test1","Test 1","LastTest 1","test1@gmail.com","001111",DateTime.Now),
                             new User("2","test2","Test 2","LastTest 2","test2@gmail.com","001111",DateTime.Now),
                             new User("3","test3","Test 3","LastTest 3","test3@gmail.com","001111",DateTime.Now),
                             new User("4","test4","Test 4","LastTest 4","test4@gmail.com","001111",DateTime.Now),
                        };

        /// <summary>
        /// CONSTRUCTOR TEST.
        /// </summary>
        public UsersControllerTest()
        {
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(userStore.Object,
                null, null, null, null, null, null, null, null);

            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);

            _context = new InMemoryDbContextFactory().GetApplicationDbContext();
        }
        #endregion

        /// <summary>
        /// TEST CONSTRUCTOR CONTROLLER. 
        /// </summary>
        [Fact]
        public void ShouldCreateInstance_NotNull_Success()
        {
            var usersController = new UsersController(_mockUserManager.Object, _mockRoleManager.Object, _context);
            Assert.NotNull(usersController);
        }

        #region TEST GET ALL METHOD
        /// <summary>
        /// GET ALL USERS SUCCESS.
        /// </summary>
        /// <returns>COUNT OF LIST USERS IS GREATER THAN 0.</returns>
        [Fact]
        public async Task GetUsers_HasData_ReturnSuccess()
        {
            _mockUserManager.Setup(x => x.Users)
                .Returns(_userSources.AsQueryable().BuildMock().Object);
            var usersController = new UsersController(_mockUserManager.Object, _mockRoleManager.Object, _context);
            var result = await usersController.GetUsers();
            var okResult = result as OkObjectResult;
            var UserVms = okResult.Value as IEnumerable<UserVm>;
            Assert.True(UserVms.Count() > 0);
        }

        /// <summary>
        /// GET ALL USERS RETURN EXCEPTION.
        /// </summary>
        /// <returns>EXCEPTION.</returns>
        [Fact]
        public async Task GetUsers_ThrowException_Failed()
        {
            _mockUserManager.Setup(x => x.Users).Throws<Exception>();

            var usersController = new UsersController(_mockUserManager.Object, _mockRoleManager.Object, _context);

            await Assert.ThrowsAnyAsync<Exception>(async () => await usersController.GetUsers());
        }
        #endregion

        #region TEST PAGINATION
        /// <summary>
        /// PAGINATION WITH NO FILTER (KEYWORD IS NULL).
        /// </summary>
        /// <returns>TOTAL RECORDS IS 4 AND TOTAL ROWS IS 2.</returns>
        [Fact]
        public async Task GetUsersPaging_NoFilter_ReturnSuccess()
        {
            _mockUserManager.Setup(x => x.Users)
                .Returns(_userSources.AsQueryable().BuildMock().Object);

            var usersController = new UsersController(_mockUserManager.Object, _mockRoleManager.Object, _context);
            var result = await usersController.GetUsersPaging(null, 1, 2);
            var okResult = result as OkObjectResult;
            var UserVms = okResult.Value as Pagination<UserVm>;
            Assert.Equal(4, UserVms.TotalRecords);
            Assert.Equal(2, UserVms.Items.Count);
        }

        /// <summary>
        /// PAGINATION WITH FILTER (KEYWORD IS test3).
        /// </summary>
        /// <returns>TOTAL RECORDS IS 1 AND TOTAL ROW IS SINGLE.</returns>
        [Fact]
        public async Task GetUsersPaging_HasFilter_ReturnSuccess()
        {
            _mockUserManager.Setup(x => x.Users)
                .Returns(_userSources.AsQueryable().BuildMock().Object);

            var usersController = new UsersController(_mockUserManager.Object, _mockRoleManager.Object, _context);
            var result = await usersController.GetUsersPaging("test3", 1, 2);
            var okResult = result as OkObjectResult;
            var UserVms = okResult.Value as Pagination<UserVm>;
            Assert.Equal(1, UserVms.TotalRecords);
            Assert.Single(UserVms.Items);
        }

        /// <summary>
        /// PAGINATION RETURN EXCEPTION.
        /// </summary>
        /// <returns>EXCEPTION.</returns>
        [Fact]
        public async Task GetUsersPaging_ThrowException_Failed()
        {
            _mockUserManager.Setup(x => x.Users).Throws<Exception>();

            var usersController = new UsersController(_mockUserManager.Object, _mockRoleManager.Object, _context);

            await Assert.ThrowsAnyAsync<Exception>(async () => await usersController.GetUsersPaging(null, 1, 1));
        }
        #endregion

        #region TEST GET USER WITH ID
        /// <summary>
        /// GET USER BY ID SUCCESS.
        /// </summary>
        /// <returns>RESULT IS NOT NULL.</returns>
        [Fact]
        public async Task GetById_HasData_ReturnSuccess()
        {
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new User()
                {
                    UserName = "test1"
                });
            var usersController = new UsersController(_mockUserManager.Object, _mockRoleManager.Object, _context);
            var result = await usersController.GetById("test1");
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var userVm = okResult.Value as UserVm;

            Assert.Equal("test1", userVm.UserName);
        }

        /// <summary>
        /// GET USER BY ID RETURN EXCRPTION.
        /// </summary>
        /// <returns>EXCEPTION.</returns>
        [Fact]
        public async Task GetById_ThrowException_Failed()
        {
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).Throws<Exception>();

            var usersController = new UsersController(_mockUserManager.Object, _mockRoleManager.Object, _context);

            await Assert.ThrowsAnyAsync<Exception>(async () => await usersController.GetById("test1"));
        }
        #endregion

        #region TEST POST USER
        /// <summary>
        /// CREATE NEW USER SUCCESS.
        /// </summary>
        /// <returns>RESULT IS NOT NULL AND HTTP STATUS IS 201.</returns>
        [Fact]
        public async Task PostUser_ValidInput_Success()
        {
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new User()
                {
                    UserName = "test"
                });

            var usersController = new UsersController(_mockUserManager.Object, _mockRoleManager.Object, _context);
            var result = await usersController.PostUser(new UserCreateRequest()
            {
                UserName = "test"
            });

            Assert.NotNull(result);
            Assert.IsType<CreatedAtActionResult>(result);
        }

        /// <summary>
        /// CREATE NEW USER FAILED.
        /// </summary>
        /// <returns>RESULT IS NOT NULL AND HTTP STATUS IS 400.</returns>
        [Fact]
        public async Task PostUser_ValidInput_Failed()
        {
            _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError[] { }));

            var usersController = new UsersController(_mockUserManager.Object, _mockRoleManager.Object, _context);
            var result = await usersController.PostUser(new UserCreateRequest()
            {
                UserName = "test"
            });

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
        }
        #endregion

        #region TEST PUT USER
        /// <summary>
        /// UPDATE USER SUCCESSS.
        /// </summary>
        /// <returns>RESULT NOT NULL AND HTTP STATUS IS 204.</returns>
        [Fact]
        public async Task PutUser_ValidInput_Success()
        {
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
               .ReturnsAsync(new User()
               {
                   UserName = "test1"
               });

            _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);
            var usersController = new UsersController(_mockUserManager.Object, _mockRoleManager.Object, _context);
            var result = await usersController.PutUser("test", new UserCreateRequest()
            {
                FirstName = "test2"
            });

            Assert.NotNull(result);
            Assert.IsType<NoContentResult>(result);
        }

        /// <summary>
        /// UPDATE USER FAILED.
        /// </summary>
        /// <returns>RESULT NOT NULL AND HTTP STATUS IS 400.</returns>
        [Fact]
        public async Task PutUser_ValidInput_Failed()
        {
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
             .ReturnsAsync(new User()
             {
                 UserName = "test1"
             });

            _mockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError[] { }));

            var usersController = new UsersController(_mockUserManager.Object, _mockRoleManager.Object, _context);
            var result = await usersController.PutUser("test", new UserCreateRequest()
            {
                UserName = "test1"
            });

            Assert.NotNull(result);
            Assert.IsType<BadRequestObjectResult>(result);
        }
        #endregion

        #region TEST DELETE USER
        /// <summary>
        /// DELETE USER SUCCESS.
        /// </summary>
        /// <returns>HTTP STATUS IS 200.</returns>
        [Fact]
        public async Task DeleteUser_ValidInput_Success()
        {
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
               .ReturnsAsync(new User()
               {
                   UserName = "test1"
               });

            _mockUserManager.Setup(x => x.DeleteAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);
            var usersController = new UsersController(_mockUserManager.Object, _mockRoleManager.Object, _context);
            var result = await usersController.DeleteUser("test");
            Assert.IsType<OkObjectResult>(result);
        }

        /// <summary>
        /// DELETE USER FAILED.
        /// </summary>
        /// <returns>HTTP STATUS IS 400.</returns>
        [Fact]
        public async Task DeleteUser_ValidInput_Failed()
        {
            _mockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
             .ReturnsAsync(new User()
             {
                 UserName = "test1"
             });

            _mockUserManager.Setup(x => x.DeleteAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError[] { }));

            var usersController = new UsersController(_mockUserManager.Object, _mockRoleManager.Object, _context);
            var result = await usersController.DeleteUser("test");
            Assert.IsType<BadRequestObjectResult>(result);
        }
        #endregion
    }
}
