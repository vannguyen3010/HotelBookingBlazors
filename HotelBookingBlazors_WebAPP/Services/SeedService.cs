using HotelBookingBlazors_WebAPP.Constants;
using HotelBookingBlazors_WebAPP.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace HotelBookingBlazors_WebAPP.Services
{
    public class SeedService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public SeedService(UserManager<ApplicationUser> userManager, IUserStore<ApplicationUser> userStore,
                RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _userStore = userStore;
            _roleManager = roleManager;
            _configuration = configuration;
        }
        public async Task SeedDatabaseAsync()
        {
            //lấy email của người dùng admin từ cấu hình
            var adminUserEmail = _configuration.GetValue<string>("AdminUser:Email");

            //Kiểm tra xem người dùng admin đã tồn tại chưa
            var dbAdminUser = await _userManager.FindByEmailAsync(adminUserEmail!);
            if (dbAdminUser is not null)
                return;

            //Tạo một đối tượng người dùng mới
            var applicationUser = new ApplicationUser()
            {
                FirstName = _configuration.GetValue<string>("AdminUser:FirstName")!,
                LastName = _configuration.GetValue<string>("AdminUser:LastName")!,
                RoleName = RoleType.Admin.ToString(),
                ContactNumber = _configuration.GetValue<string>("AdminUser:ContactNumber")!,
                Designation = "Administrator"
            };

            //Thiết lập tên người dùng và email cho người dùng mới
            await _userStore.SetUserNameAsync(applicationUser, adminUserEmail, default);
            var emailStore = (IUserEmailStore<ApplicationUser>)_userStore;
            await emailStore.SetEmailAsync(applicationUser, adminUserEmail, default);

            //Tạo người dùng mới trong hệ thống
            var result = await _userManager.CreateAsync(applicationUser, _configuration.GetValue<string>("AdminUser:Password")!);
            //Nếu việc tạo người dùng không thành công
            if (!result.Succeeded)
            {
                var errors = string.Join(Environment.NewLine, result.Errors.Select(x => x.Description));
                throw new Exception($"Errors in creating user. {errors}");
            }

            //Kiểm tra xem vai trò admin đã tồn tại chưa, nếu chưa thì tạo các vai trò
            if (await _roleManager.FindByNameAsync(RoleType.Admin.ToString()) is null)
            {
                foreach (var roleName in Enum.GetNames<RoleType>())
                {
                    var role = new IdentityRole
                    {
                        Name = roleName,
                    };
                    await _roleManager.CreateAsync(role);
                }
            }

            //Gán quyền admin cho người dùng
            result = await _userManager.AddToRoleAsync(applicationUser, RoleType.Admin.ToString());
            if(!result.Succeeded)
            {
                var erros = string.Join(Environment.NewLine, result.Errors.Select(x => x.Description));
                throw new Exception($"Errors in adding user to Admin role. {erros}");
            }
        }
    }
}


