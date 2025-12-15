public class RenewPasswordDto
{
    public string OldPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
    public string RepeatNewPassword { get; set; } = null!;
}
