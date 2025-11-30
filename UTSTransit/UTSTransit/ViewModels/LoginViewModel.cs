using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UTSTransit.Services;

namespace UTSTransit.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly TransitService _transitService;

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _statusMessage;

        [ObservableProperty]
        private bool _isBusy;

        public LoginViewModel(TransitService transitService)
        {
            _transitService = transitService;
        }

        [RelayCommand]
        public async Task Login()
        {
            if (IsBusy) return;
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "请输入邮箱和密码";
                return;
            }

            IsBusy = true;
            StatusMessage = "正在登录...";

            try
            {
                // 确保 Supabase 已初始化
                await _transitService.InitializeAsync();
                
                var success = await _transitService.LoginAsync(Email, Password);
                if (success)
                {
                    StatusMessage = "登录成功!";
                    // 导航到主页
                    await Shell.Current.GoToAsync("//MapPage");
                }
                else
                {
                    StatusMessage = "登录失败，请检查邮箱或密码";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"发生错误: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task Register()
        {
            if (IsBusy) return;
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "请输入邮箱和密码";
                return;
            }

            IsBusy = true;
            StatusMessage = "正在注册...";

            try
            {
                await _transitService.InitializeAsync();

                var success = await _transitService.RegisterAsync(Email, Password);
                if (success)
                {
                    StatusMessage = "注册成功! 请检查邮箱验证或直接登录。";
                }
                else
                {
                    StatusMessage = "注册失败";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"发生错误: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
