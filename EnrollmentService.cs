public class EnrollmentService
{
    // Optional delegate extension
    public Action<Student>? Listener { get; set; }

    public EnrollmentRecord ProcessRegistration(
        Student? student,
        Course? course)
    {
        // Guard clauses
        if (student is null)
            throw new ArgumentNullException(nameof(student));

        if (course is null)
            throw new ArgumentNullException(nameof(course));

        // Custom domain exception
        if (course.EnrolledCount >= course.Capacity)
            throw new CapacityReachedException(course.Code);

        // GPA classification
        string standing = student.GPA switch
        {
            >= 3.5m => "Honors",
            >= 2.5m => "Good Standing",
            _ => "Academic Warning"
        };

        Console.WriteLine(
            $"{student.Name} is in {standing}."
        );

        return new EnrollmentRecord(
            student.Id,
            course.Code,
            DateTime.UtcNow
        );
    }

    // Optional extension activity
    public void FinalizeEnrollment(Student s)
    {
        Console.WriteLine("Persisting to database...");

        Listener?.Invoke(s);
    }
}