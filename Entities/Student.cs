namespace TmsApi.Entities;

public class Student
{
    public int Id { get; set; }

    public required string RegistrationNumber { get; set; }

    public required string Name { get; set; }

    public decimal GPA { get; set; }

    public bool IsActive { get; set; } = true;

    // module 5 session 3
    public uint Version { get; set; }

    public bool IsDeleted { get; set; } = false;

    /// end of session 3
    
    public ICollection<Enrollment> Enrollments { get; set; }
        = new List<Enrollment>();

    public ICollection<Certificate> Certificates { get; set; }
        = new List<Certificate>();
}