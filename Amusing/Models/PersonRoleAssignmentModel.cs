namespace Amusing.Models;

public class PersonRoleAssignmentModel
{
    public uint PersonId { get; set; }
    public uint GroupId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
}
