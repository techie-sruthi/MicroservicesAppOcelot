namespace UserService.Application.Users.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Role { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
