namespace auth_service.Entities;

public sealed class Role
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
